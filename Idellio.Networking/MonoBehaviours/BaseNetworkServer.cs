using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Idellio.Networking.MonoBehaviours
{
    public abstract class BaseNetworkServer : MonoBehaviour
    {
        [Header("Settings")]
        public int TickRate = 64;
        public ushort MaxPacketSize = 65535;

        internal bool Initialized { get; private set; }
        internal bool Hosting { get; private set; }

        public static BaseNetworkServer Instance { get; private set; }

        private int _HostId { get; set; }
        private float _TimeSinceLastTick { get; set; }

        private GlobalConfig _GlobalConfig { get; set; }
        private ConnectionConfig _ConnectionConfig { get; set; }
        private HostTopology _HostTopology { get; set; }


        private List<BaseNetworkBehaviour> _NetworkBehaviours { get; set; }
        private List<NetworkConnection> _Connections { get; set; }


        void Awake()
        {
            Initialize();
            _TimeSinceLastTick = 0f;
        }

        void Update()
        {
            if (Initialized)
            {
                if (Hosting)
                {
                    ServerTick();
                    _TimeSinceLastTick += Time.deltaTime;
                }
            }
        }


        /// <summary>
        /// Initialize IDNS
        /// </summary>
        private void Initialize()
        {
            if (Initialized) throw new Exception("[IDNS-Server] Error: Already initialized.");

            //Setup globalconfig
            _GlobalConfig = new GlobalConfig();
            _GlobalConfig.MaxPacketSize = MaxPacketSize;
            _GlobalConfig.ReactorModel = ReactorModel.FixRateReactor;
            _GlobalConfig.ReactorMaximumReceivedMessages = 4096;
            _GlobalConfig.ReactorMaximumSentMessages = 4096;
            NetworkTransport.Init(_GlobalConfig);

            //Setup Channels
            _ConnectionConfig = new ConnectionConfig();
            _ConnectionConfig.AddChannel(QosType.Unreliable);
            _ConnectionConfig.AddChannel(QosType.UnreliableFragmented);
            _ConnectionConfig.AddChannel(QosType.UnreliableSequenced);
            _ConnectionConfig.AddChannel(QosType.Reliable);
            _ConnectionConfig.AddChannel(QosType.ReliableFragmented);
            _ConnectionConfig.AddChannel(QosType.ReliableSequenced);
            _ConnectionConfig.AddChannel(QosType.StateUpdate);
            _ConnectionConfig.AddChannel(QosType.ReliableStateUpdate);
            _ConnectionConfig.AddChannel(QosType.AllCostDelivery);
            _ConnectionConfig.AddChannel(QosType.UnreliableFragmentedSequenced);
            _ConnectionConfig.AddChannel(QosType.ReliableFragmentedSequenced);

            Instance = this;

            Initialized = true;
        }

        /// <summary>
        /// Start listening
        /// </summary>
        /// <param name="port">Port for the server to listen on.</param>
        public void StartServer(int port)
        {
            if (Hosting || !Initialized) throw new Exception("[IDNS-Server] Error Starting Server.");

            _HostTopology = new HostTopology(_ConnectionConfig, 100);
            _HostId = NetworkTransport.AddHost(_HostTopology, port);
            _NetworkBehaviours = new List<BaseNetworkBehaviour>();
            _Connections = new List<NetworkConnection>();
            Hosting = true;

            Debug.Log($"[IDNS-Server] Listening on port {port}.");
        }

        private void ServerTick()
        {
            if (_TimeSinceLastTick >= (1 / (float)TickRate)) {
                if (!Hosting || !Initialized) throw new Exception("[IDNS-Server] Error Running Server Tick.");
                byte err;
                int connectionId;
                int channelId;
                byte[] recBuffer = new byte[MaxPacketSize];
                int bufferSize = MaxPacketSize;
                int dataSize;
                byte error;
                int size = NetworkTransport.GetIncomingMessageQueueSize(_HostId, out err) + 1;
                for (int i = 0; i < size; i++)
                {
                    NetworkEventType recData = NetworkTransport.ReceiveFromHost(_HostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);
                    switch (recData)
                    {
                        case NetworkEventType.Nothing:         //1
                            break;
                        case NetworkEventType.ConnectEvent:    //2
                            _Connections.Add(new NetworkConnection(connectionId));
                            break;
                        case NetworkEventType.DataEvent:       //3
                            break;
                        case NetworkEventType.DisconnectEvent: //4
                            break;
                    }
                }
                _TimeSinceLastTick = 0f;
            }
        }

        internal static void RegisterMonobehaviour(BaseNetworkBehaviour baseNetworkBehaviour)
        {
            Debug.Log("[IDNS-Server] Monobehaviour Registered.");
        }

        /// <summary>
        /// Get the networkconnection associated with the specified connection id.
        /// </summary>
        /// <param name="ConnectionId"></param>
        /// <returns></returns>
        public NetworkConnection GetNetworkConnection(int ConnectionId)
        {
            NetworkConnection connection = null;
            foreach(var conn in _Connections)
            {
                if (conn.ConnectionId == ConnectionId)
                    connection = conn;
            }

            return connection;
        }

        /// <summary>
        /// Get the networkconnection associated with the specif auth id.
        /// </summary>
        /// <param name="AuthId"></param>
        /// <returns></returns>
        public NetworkConnection GetNetworkConnection(string AuthId)
        {
            NetworkConnection connection = null;
            foreach (var conn in _Connections)
            {
                if (conn.AuthId == AuthId)
                    connection = conn;
            }

            return connection;
        }

        /// <summary>
        /// Get the ping for a specified networkconnection (Round trip)
        /// </summary>
        /// <param name="client">Client you want to get the ping for</param>
        /// <returns>Round trip ping in milliseconds</returns>
        public static int GetPing(NetworkConnection client)
        {
            byte err;
            return NetworkTransport.GetCurrentRTT(Instance._HostId, client.ConnectionId, out err);
        }
    }
}
