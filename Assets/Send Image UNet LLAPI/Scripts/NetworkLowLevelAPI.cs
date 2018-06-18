using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Runtime.Serialization;
using System;
using System.Text;

public class NetworkLowLevelAPI : MonoBehaviour
{
    public static NetworkLowLevelAPI instance = null;

    [Header("Server")]
    public GameObject server;
    public GameObject buttonServer;

    public Text ipText;

    public int sceneServer = 1;

    [Header("Client")]
    public GameObject client;
    public GameObject buttonClient;

    public InputField ipField;

    public int sceneClient = 2;
    
    [Header("Network")]
    public uint timeOutSec = 86400; // = 24h

    private bool isInit = false;

    private int myReliableChannelId;
    private int socketId;

    private string socketIP = "127.0.0.1";
    private int socketPort = 8888;

    private int connectionId;

    Dictionary <string, byte[]> imageBytes = null;

    void Awake()
    {
        //Check if instance already exists
        if (instance == null)
        {
            //if not, set instance to this
            instance = this;
        }
        //If instance already exists and it's not this:
        else if (instance != this)
        {
            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
            Destroy(gameObject);
        }
        //Sets this to not be destroyed when reloading scene
        //DontDestroyOnLoad(gameObject);

        imageBytes = new Dictionary<string, byte[]>();
    }

    /// <summary>
    /// Initialize the Network system
    /// </summary>
    void Init()
    {
        isInit = true;

        // Shutdown before to init a new session
        NetworkTransport.Shutdown();

        NetworkTransport.Init();
        ConnectionConfig config = new ConnectionConfig();
        config.DisconnectTimeout = (timeOutSec * 1000);

        myReliableChannelId = config.AddChannel(QosType.AllCostDelivery);

        int maxConnections = 10000;
        HostTopology topology = new HostTopology(config, maxConnections);

        socketId = NetworkTransport.AddHost(topology, socketPort);
        Debug.Log("Socket Open. SocketId is: " + socketId);        
    }

    /// <summary>
    /// Connect client to server
    /// </summary>
    public void Connect()
    {
        // Disconnect before to connect a new session
        Disconnect();

        byte error;
        connectionId = NetworkTransport.Connect(socketId, socketIP, socketPort, 0, out error);
        Debug.Log("Connected to server. ConnectionId: " + connectionId);
    }

    /// <summary>
    /// Disconnect client to server
    /// </summary>
    public void Disconnect()
    {
        byte error;
        NetworkTransport.Disconnect(socketId, connectionId, out error);
        Debug.Log("Disconnect to server. ConnectionId: " + connectionId);
    }

    /// <summary>
    /// Send a socket client to server
    /// </summary>
    /// <param name="imageBytes">A part on the image in bytes</param>
    /// <param name="header">Type of data content</param>
    public void SendSocketMessage(byte[] imageBytes, string header)
    {
        byte error;
        byte[] buffer = new byte[SaveFile.HEADER_LENGTH];
        Stream stream = new MemoryStream(buffer);
        BinaryFormatter formatter = new BinaryFormatter();
        formatter.Serialize(stream, header);

        //int bufferSize = 1024;
        int bufferSize = imageBytes.Length + buffer.Length; // = 4095 + 1024

        byte[] newImageBytes = new byte[bufferSize];

        for (int i = 0; i < buffer.Length; i++)
        {
            newImageBytes[i] = buffer[i];
        }

        for (int i = buffer.Length; i < newImageBytes.Length; i++)
        {
            newImageBytes[i] = imageBytes[i - buffer.Length];
        }

        NetworkTransport.Send(socketId, connectionId, myReliableChannelId, newImageBytes, bufferSize, out error);
    }

    private void Update()
    {
        if (isInit)
        {
            int recHostId;
            int recConnectionId;
            int recChannelId;
            byte[] recBuffer = new byte[SaveFile.IMAGE_LENGTH + SaveFile.HEADER_LENGTH];
            int bufferSize = SaveFile.IMAGE_LENGTH + SaveFile.HEADER_LENGTH;
            int dataSize;
            byte error;
            NetworkEventType recNetworkEvent = NetworkTransport.Receive(out recHostId, out recConnectionId, out recChannelId, recBuffer, bufferSize, out dataSize, out error);

            switch (recNetworkEvent)
            {
                case NetworkEventType.Nothing:
                    break;
                case NetworkEventType.ConnectEvent:
                    Debug.Log("NetworkEventType : ConnectEvent");
                    break;
                case NetworkEventType.DataEvent:

                    byte[] header = new byte[SaveFile.HEADER_LENGTH];

                    for (int i = 0; i < header.Length; i++)
                    {
                        header[i] = recBuffer[i];
                    }

                    Stream stream = new MemoryStream(header);
                    BinaryFormatter formatter = new BinaryFormatter();
                    string header_imageID = formatter.Deserialize(stream) as string;
                    
                    string header_type = header_imageID.Split(';')[0]; // header
                    header_imageID = header_imageID.Split(';')[1]; // ID

                    Debug.Log("NetworkEventType : DataEvent = " + header_imageID);
                    Debug.Log("Header Type : " + header_type);

                    if (header_type == SaveFile.HEADER_IMAGE_UPDATE)
                    {
                        byte[] newImageBytes = null;

                        if (!imageBytes.TryGetValue(header_imageID, out newImageBytes))
                        {
                            newImageBytes = new byte[dataSize - SaveFile.HEADER_LENGTH];
                            for (int i = 0; i < newImageBytes.Length; i++)
                            {
                                newImageBytes[i] = recBuffer[i + SaveFile.HEADER_LENGTH];
                            }
                            //Debug.Log("newBytes " + finalImageBytes.Length + " + 0");

                            imageBytes.Add(header_imageID, newImageBytes);
                        }
                        else
                        {
                            newImageBytes = new byte[imageBytes[header_imageID].Length + dataSize - SaveFile.HEADER_LENGTH];
                            //Debug.Log("newBytes " + finalImageBytes.Length +" + "+ dataSize);

                            for (int i = 0; i < imageBytes[header_imageID].Length; i++)
                            {
                                newImageBytes[i] = imageBytes[header_imageID][i];
                            }

                            for (int i = imageBytes[header_imageID].Length; i < newImageBytes.Length; i++)
                            {
                                newImageBytes[i] = recBuffer[i + SaveFile.HEADER_LENGTH - imageBytes[header_imageID].Length];
                            }

                            imageBytes[header_imageID] = newImageBytes;
                        }
                    }

                    if (header_type == SaveFile.HEADER_IMAGE_END)
                    {
                        //Debug.Log("bytes lenght actual : " + imageBytes[imageID].Length);

                        ServerManager server = (ServerManager)GameObject.FindObjectOfType(typeof(ServerManager));
                        if (server != null)
                            server.SaveNewImage(imageBytes[header_imageID]);
                    }

                    break;
                case NetworkEventType.DisconnectEvent:
                    Debug.Log("NetworkEventType : DisconnectEvent");
                    break;
            }
        }
    }

    /// <summary>
    /// Start Server side
    /// </summary>
    public void StartServer()
    {
        server.SetActive(true);
        client.SetActive(false);

        buttonServer.SetActive(false);

        Init();

        // show server ip
        if (ipText != null)
            ipText.text = "IP : " + Network.player.ipAddress;

        Debug.Log("SERVER Start");
        SceneManager.LoadScene(sceneServer, LoadSceneMode.Additive);
    }

    /// <summary>
    /// Start Client side
    /// </summary>
    public void StartClient()
    {
        server.SetActive(false);
        client.SetActive(true);

        buttonClient.SetActive(false);

        socketIP = ipField.text;

        Init();

        Debug.Log("CLIENT Start");
        SceneManager.LoadScene(sceneClient, LoadSceneMode.Additive);
    }

    public void Button_Quit()
    {
        Application.Quit();
    }
}
