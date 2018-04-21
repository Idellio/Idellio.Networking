using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Idellio.Networking.MonoBehaviours
{
    public abstract class BaseNetworkBehaviour : MonoBehaviour
    {
        /// <summary>
        /// Are we on a server?
        /// </summary>
        public bool isServer
        {
            get {
                if (BaseNetworkServer.Instance == null)
                    return false;
                return BaseNetworkServer.Instance._Hosting;
            }
        }

        /// <summary>
        /// Are we on a client?
        /// </summary>
        public bool isClient
        {
            get
            {
                if (BaseNetworkClient.Instance == null)
                    return false;
                return BaseNetworkClient.Instance._Connected;
            }
        }

        void Awake()
        {
            if (isServer)
            {
                BaseNetworkServer.RegisterMonobehaviour(this);
            }
            if (isClient)
            {
                BaseNetworkClient.RegisterMonobehaviour(this);
            }
        }
    }
}
