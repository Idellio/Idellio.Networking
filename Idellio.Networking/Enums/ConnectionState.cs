using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Idellio.Networking.Enums
{
    public enum ConnectionState
    {
        Connecting,
        Authorizing,
        Authorized,
        SyncingScene,
        SyncingObjects,
        Connected
    }
}
