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
        public int TickRate = 32;
        public ushort MaxPacketSize = 4096;

        internal bool _Initialized { get; set; }
        internal bool _Hosting { get; set; }

        public static BaseNetworkServer Instance { get; private set; }

        private int _HostId { get; set; }
        private float _TimeSinceLastTick { get; set; }

        private GlobalConfig _GlobalConfig { get; set; }
        private ConnectionConfig _ConnectionConfig { get; set; }
        private HostTopology _HostTopology { get; set; }


        private List<BaseNetworkBehaviour> _NetworkBehaviours { get; set; }


        void Awake()
        {
            Initialize();
            _TimeSinceLastTick = 0f;
        }

        void Update()
        {
            if (_Initialized)
            {
                if (_Hosting)
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
            if (_Initialized) throw new Exception("[IDNS-Server] Error: Already initialized.");

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

            _NetworkBehaviours = new List<BaseNetworkBehaviour>();
            Instance = this;

            _Initialized = true;
        }

        /// <summary>
        /// Start listening
        /// </summary>
        /// <param name="port">Port for the server to listen on.</param>
        public void StartServer(int port)
        {
            if (_Hosting || !_Initialized) throw new Exception("[IDNS-Server] Error Starting Server.");

            _HostTopology = new HostTopology(_ConnectionConfig, 100);
            _HostId = NetworkTransport.AddHost(_HostTopology, port);
            _Hosting = true;

            Debug.Log($"[IDNS-Server] Listening on port {port}.");
        }

        private void ServerTick()
        {
            if (_TimeSinceLastTick >= (1 / (float)TickRate)) {
                if (!_Hosting || !_Initialized) throw new Exception("[IDNS-Server] Error Running Server Tick.");
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
                            break;
                        case NetworkEventType.DataEvent:       //3
                            break;
                        case NetworkEventType.DisconnectEvent: //4
                            break;
                    }
                }
            }
        }

        internal void RegisterMonobehaviour(BaseNetworkBehaviour baseNetworkBehaviour)
        {
            Debug.Log("[IDNS-Server] Monobehaviour Registered.");
        }
    }
}
