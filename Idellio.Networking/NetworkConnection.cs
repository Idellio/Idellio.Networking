using Idellio.Networking.Enums;
using Idellio.Networking.MonoBehaviours;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Idellio.Networking
{
    public class NetworkConnection
    {
        /// <summary>
        /// Ping of this client
        /// </summary>
        public float Ping
        {
            get
            {
                if (!BaseNetworkServer.Instance.Hosting)
                    return -1;
                return BaseNetworkServer.GetPing(this);
            }
        }

        /// <summary>
        /// LLAPI Connection Id
        /// </summary>
        public int ConnectionId { get; private set; }

        /// <summary>
        /// Id to use for storing the auth id of the connection
        /// </summary>
        public string AuthId { get; set; }

        /// <summary>
        /// Current state of this connection
        /// </summary>
        public ConnectionState State { get; set; }

        public NetworkConnection(int ConnectionId, string AuthId = null)
        {
            this.ConnectionId = ConnectionId;
            this.AuthId = AuthId;
        }
    }
}
