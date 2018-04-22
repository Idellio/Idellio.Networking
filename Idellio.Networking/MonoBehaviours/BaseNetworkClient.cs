using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Idellio.Networking.MonoBehaviours
{
    public abstract class BaseNetworkClient : MonoBehaviour
    {
        [Header("Settings")]
        public int TickRate = 64;
        public ushort MaxPacketSize = 65535;

        internal bool Initialized { get; private set; }
        internal bool Connected { get; private set; }

        public static BaseNetworkClient Instance { get; private set; }

        private int _HostId { get; set; }
        private int _ConnectionId { get; set; }
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
            if (Initialized)
            {
                if (Connected)
                {
                    ClientTick();
                    _TimeSinceLastTick += Time.deltaTime;
                }
            }
        }

        /// <summary>
        /// Initialize IDNS
        /// </summary>
        private void Initialize()
        {
            if (Initialized) throw new Exception("[IDNS-Client] Error: Already initialized.");

            //Setup globalconfig
            _GlobalConfig = new GlobalConfig();
            _GlobalConfig.MaxPacketSize = MaxPacketSize;
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

            Initialized = true;
        }

        /// <summary>
        /// Connect to a server
        /// </summary>
        /// <param name="ip">Ip to connect to.</param>
        /// <param name="port">Port to connect to.</param>
        public void Connect(string ip, int port)
        {
            if (Connected || !Initialized) throw new Exception("[IDNS-Server] Error Starting Server.");

            _HostTopology = new HostTopology(_ConnectionConfig, 1);
            _HostId = NetworkTransport.AddHost(_HostTopology, 0); //0 Tells the LLAPI to pick a random port

            byte error;
            _ConnectionId = NetworkTransport.Connect(_HostId, ip, port, 0, out error);
            Connected = true;
            Debug.Log($"[IDNS-Client] Connecting to server @{ip}:{port}.....");
        }

        private void ClientTick()
        {
            if (_TimeSinceLastTick >= (1 / (float)TickRate))
            {
                if (!Connected || !Initialized) throw new Exception("[IDNS-Client] Error Running Server Tick.");
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
                _TimeSinceLastTick = 0f;
            }
        }

        internal static void RegisterMonobehaviour(BaseNetworkBehaviour baseNetworkBehaviour)
        {
            Debug.Log("[IDNS-Client] Monobehaviour Registered.");
        }
    }
}
