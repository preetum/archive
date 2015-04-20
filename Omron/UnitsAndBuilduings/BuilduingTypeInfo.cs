using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnitsAndBuilduings
{
    public class ResourceCostInfo//many of this will constitute the overall cost
    {
        public string Resource;//resource type
        public float Cost;//# of resources
    }

    public class BuilduingTypeInfo
    {
        public BuilduingTypeInfo() { }

        public int Health;
        public ResourceCostInfo[] Cost;
        public float SightRange;
        public string Type;//The actual class which will be used
        public string[] SubTypes;//the unit names factory can build go here
        public ResourceCostInfo[][] SubTypeCosts;//each array of ResourceCostInfo corresponds to each unit
        public float[] ActionTimes;//each time corresponds to each unit in order
        public int[] ActionTypes;//0-build unit, 1-unlock building, 2-unlock upgrade
        public string[] UnlocksNeeded;
        public PolygonInfo Polygon;//collision data
        public DamageTypeInfo Defense;
        public AnimationTypeInfo BuildAnimation, WorkAnimation, IdleAnimation;
        public float WorkNeeded; 
        public AttackTypeInfo[] Attacks;//list of attack the unit can do
        public SpawnAttackTypeInfo[] SpawnAttacks;//list of spaw attacks unit can do
        public bool CanRotate;
    }
}
