using System;
using UnityEngine;
using System.Text;
using Aws.GameLift.Realtime;
using Aws.GameLift.Realtime.Event;
using Aws.GameLift.Realtime.Types;

/**
 * An example client that wraps the GameLift Realtime client SDK
 * 
 * You can redirect logging from the SDK by setting up the LogHandler as such:
 * ClientLogger.LogHandler = (x) => Console.WriteLine(x);
 *
 */
public class RealTimeClient
{
    public Aws.GameLift.Realtime.Client Client { get; private set; }
    public bool OnCloseReceived { get; private set; }
    // An opcode defined by client and your server script that represents a custom message type
    private const int MY_TEST_OP_CODE = 10;

    //public RealTimeClient(SceneHandlerService sceneHandlerService) {
    //    _sceneHandlerService = sceneHandlerService;
    //}
    /// <summary>
    /// Initialize a client for GameLift Realtime and connects to a player session.
    /// </summary>
    /// <param name="endpoint">The endpoint for the GameLift Realtime server to connect to</param>
    /// <param name="tcpPort">The TCP port for the GameLift Realtime server</param>
    /// <param name="localUdpPort">Local Udp listen port to use</param>
    /// <param name="playerSessionId">The player session Id in use - from CreatePlayerSession</param>
    /// <param name="connectionPayload"></param>
    /// 
    public void init(string endpoint, int tcpPort, int localUdpPort,ConnectionType connectionType, string playerSessionId, byte[] connectionPayload) {

        Debug.Log("Entered RealtimeClient");
        this.OnCloseReceived = false;

        ClientConfiguration clientConfiguration = new ClientConfiguration(){ ConnectionType = connectionType };

        Client = new Aws.GameLift.Realtime.Client(clientConfiguration);
        Client.ConnectionOpen += new EventHandler(OnOpenEvent);
        Client.ConnectionClose += new EventHandler(OnCloseEvent);
        Client.GroupMembershipUpdated += new EventHandler<GroupMembershipEventArgs>(OnGroupMembershipUpdate);
        Client.DataReceived += new EventHandler<DataReceivedEventArgs>(OnDataReceived);

        ConnectionToken token = new ConnectionToken(playerSessionId, connectionPayload);
        Client.Connect(endpoint, tcpPort, localUdpPort, token);
    }

    public void Disconnect() {
        if (Client.Connected) {
            Client.Disconnect();
        }
    }

    public bool IsConnected() {
        return Client.Connected;
    }

    /// <summary>
    /// Example of sending to a custom message to the server.
    /// 
    /// Server could be replaced by known peer Id etc.
    /// </summary>
    /// <param name="intent">Choice of delivery intent ie Reliable, Fast etc. </param>
    /// <param name="payload">Custom payload to send with message</param>
    public void SendMessage(DeliveryIntent intent, string payload) {
        Client.SendMessage(Client.NewMessage(MY_TEST_OP_CODE)
            .WithDeliveryIntent(intent)
            .WithTargetPlayer(Constants.PLAYER_ID_SERVER)
            .WithPayload(StringToBytes(payload)));
    }

    public void SendMessage(DeliveryIntent intent, int opcode, string payload) {
        Debug.Log("SendMessage with opcode");
        Client.SendMessage(Client.NewMessage(opcode)
            .WithDeliveryIntent(intent)
            .WithTargetPlayer(Constants.PLAYER_ID_SERVER)
            .WithPayload(StringToBytes(payload)));
    }

    /**
     * Handle connection open events
     */
    public void OnOpenEvent(object sender, EventArgs e) {
    }

    /**
     * Handle connection close events
     */
    public void OnCloseEvent(object sender, EventArgs e) {
        OnCloseReceived = true;
    }

    /**
     * Handle Group membership update events 
     */
    public void OnGroupMembershipUpdate(object sender, GroupMembershipEventArgs e) {
    }

    /**
     *  Handle data received from the Realtime server 
     */
    public virtual void OnDataReceived(object sender, DataReceivedEventArgs e) {
        switch (e.OpCode) {
            // handle message based on OpCode
            default:
            break;
        }
    }

    /**
     * Helper method to simplify task of sending/receiving payloads.
     */
    public static byte[] StringToBytes(string str) {
        return Encoding.UTF8.GetBytes(str);
    }

    /**
     * Helper method to simplify task of sending/receiving payloads.
     */
    public static string BytesToString(byte[] bytes) {
        return Encoding.UTF8.GetString(bytes);
    }
}