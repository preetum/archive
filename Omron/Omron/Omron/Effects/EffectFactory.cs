using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Omron.Effects
{
    public static class EffectFactory
    {
        public static GraphicsEffect CreateEffect(string type)
        {
            switch (type)
            {
                case "Explosion":
                    return new Explosion();
                case "TractorBeam":
                    return new TractorBeam();
                default:
                    //throw new Exception("no such effect");
                    return null;
            }
        }
    }
}
