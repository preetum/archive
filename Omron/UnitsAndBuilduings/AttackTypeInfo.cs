using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnitsAndBuilduings
{
    public class AttackTypeInfo
    {
        public bool Null;//used if attack is not anything. ex: archer units must have two attacks, but in a specific unit mele may be null
        public DamageTypeInfo Damage;
        public PolygonInfo AreaOfEffect;
        public float MaxRange;
        public float Period;
        public EffectTypeInfo Effect;
    }
}
