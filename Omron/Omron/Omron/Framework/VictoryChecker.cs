using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Omron.Framework
{
    public enum CheckTime
    {
        Timer,
        Death,
        Creation
    }

    public delegate bool FactionWonDelegate(World w, Faction f);

    public class VictoryChecker
    {
        public CheckTime CheckTime;

        public float TimerTime;

        public FactionWonDelegate FactionWon;
        public FactionWonDelegate FactionLost;
    }
}
