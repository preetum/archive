using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Omron.Framework
{
    public struct Damage
    {
        public Damage(UnitsAndBuilduings.DamageTypeInfo info) 
        {
            Slashing = info.Slashing;
            Blunt = info.Blunt;
            Pierce = info.Pierce;
            Mining = info.Mining;
            Blight = info.Blight;

        }
        
        public bool IsZero()
        {
            return (Slashing == 0) && (Blight == 0) && (Pierce == 0) && (Blight == 0);
        }

        public float Slashing, Blunt, Pierce,Blight, Mining;
    }
}
