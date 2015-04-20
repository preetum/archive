using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnitsAndBuilduings
{
    public class SpawnAttackTypeInfo
    {
        public SpawnAttackTypeInfo() { }

        public string SpawnActor; //the unit that will be spawened when shot
        public float Period;
        public float MaxRange;
        public int Number;
    }
}
