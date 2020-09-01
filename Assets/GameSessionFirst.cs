using System;
using UnityEngine;
using Aws.GameLift.Realtime.Types;
using Amazon;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using Amazon.CognitoIdentity;
using System.Text;
using System.Net;
using System.Net.Sockets;
//using Zenject;

// This data structure is returned by the client service when a game match is found
[System.Serializable]
public class PlayerSessionObject
{
    public string PlayerSessionId;
    public string PlayerId;
    public string GameSessionId;
    public string FleetId;
    public string CreationTime;
    public string Status;
    public string IpAddress;
    public string Port;
}

public class GameSessionFirst
{
    private RealTimeClient _realTimeClient;
    private byte[] connectionPayload = new Byte[64];
    private static readonly IPEndPoint DefaultLoopbackEndpoint = new IPEndPoint(IPAddress.Loopback, port: 0);

    public GameSessionFirst(RealTimeClient realTimeClient) {
        _realTimeClient = realTimeClient;
        setupMatch();
    }

    public void playerAction(int opcode, string data) {
        _realTimeClient.SendMessage(DeliveryIntent.Fast, opcode, data);
    }


    async void setupMatch() {

        // Initialize the Amazon Cognito credentials provider
        CognitoAWSCredentials credentials = new CognitoAWSCredentials(
            "ap-northeast-1:c97ce189-f243-4877-9d26-061f42783dca", // Identity pool ID
            RegionEndpoint.APNortheast1 // Region
        );

        AmazonLambdaClient client = new AmazonLambdaClient(credentials, RegionEndpoint.APNortheast1);
        InvokeRequest request = new InvokeRequest {
            FunctionName = "TestLamba1",
            InvocationType = InvocationType.RequestResponse
        };

        var response = await client.InvokeAsync(request);

        if (response.FunctionError == null) {
            if (response.StatusCode == 200) {
                var payload = Encoding.ASCII.GetString(response.Payload.ToArray()) + "\n";
                var playerSessionObj = JsonUtility.FromJson<PlayerSessionObject>(payload);
                if (playerSessionObj.FleetId == null) {
                    Debug.Log($"Error in Lambda: {payload}");
                } else {
                    joinMatch(playerSessionObj.IpAddress, playerSessionObj.Port, playerSessionObj.PlayerSessionId);
                }
            }
        } else {
            Debug.LogError(response.FunctionError);
        }

    }

    void joinMatch(string playerSessionDns, string playerSessionPort, string playerSessionId) {
        Debug.Log($"[client] Attempting to connect to server dns: {playerSessionDns} Tcp port:{playerSessionPort} Player Session Id:{playerSessionId}");
        int localPort = GetAvailablePort();
        _realTimeClient.init(playerSessionDns, Int32.Parse(playerSessionPort), localPort, ConnectionType.RT_OVER_WS_UDP_UNSECURED, playerSessionId, connectionPayload);
    }

    public static int GetAvailablePort(){

        using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)) {
            socket.Bind(DefaultLoopbackEndpoint);
            return ((IPEndPoint)socket.LocalEndPoint).Port;
        }
    }

}
