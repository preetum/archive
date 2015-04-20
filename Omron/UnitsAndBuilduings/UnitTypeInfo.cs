using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnitsAndBuilduings
{
    public class RectangleInfo
    {
        public RectangleInfo() { }
        public float X, Y, Width, Height;
    }

    public class UnitTypeInfo
    {//load from xml
        public UnitTypeInfo() { }

        public string Type;
        public float Speed;
        public float Health;
        public float Inertia;
        public float SightRange;
        public AttackTypeInfo[] Attacks;//list of attack the unit can do
        public SpawnAttackTypeInfo[] SpawnAttacks;//list of spawn attacks
        public DamageTypeInfo Defense;
        public float AttackPeriod;
        public PolygonInfo Polygon;
        public AnimationTypeInfo IdleAnimation, MoveAnimation, WorkAnimation, MeleeAnimation, RangedAnimation;
        public string AIType;
        public string CollisionType;//normal or isolated(may collide with crap but other crap does not collide with it)
        public float WorkPower;//# of work done per thing
        public float WorkPeriod;//period of work
        public bool Rotatable;
    }
}
