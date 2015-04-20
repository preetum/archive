using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Lidgren.Network;
using Lidgren.Network.Xna;

namespace Omron.Framework.Networking
{
    /// <summary>
    /// actors marked IFullSynchronize will be constantly sent in the server update loop
    /// </summary>
    public interface IFullSynchronize
    {
        void WriteOutUpdateData(NetOutgoingMessage om);
        void ReadInUpdateData(NetIncomingMessage im);
    }

    public interface INetworkInitialize
    {
        void WriteOutInitialData(NetOutgoingMessage om);
        void ReadInInitialData(NetIncomingMessage im);
    }

    /// <summary>
    /// updates whenever invalid = true. sets invalid=false after update
    /// </summary>
    public interface IChangeUpdate
    {
        bool IsInvalid { get; set; }

        void WriteOutChangeUpdateData(NetOutgoingMessage om);
        void ReadInChangeUpdateData(NetIncomingMessage im);
    }

    public interface IPushUpdate
    {
        void ReadInPushUpdateData(NetIncomingMessage im);
    }
}
