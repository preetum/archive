using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Omron.Framework
{
    public class SpawnAttack
    {
        public SpawnAttack(UnitsAndBuilduings.SpawnAttackTypeInfo info, Actor atk)
        {
            Attacker = atk;
            Type = info.SpawnActor;
            Range = info.MaxRange;
            Period = info.Period;
            Number = info.Number;
        }

        public string Type;

        public Actor Attacker;
        public float Range;
        public float Period;
        public int Number;
    }

    public class ArealAttack
    {
        public ArealAttack(UnitsAndBuilduings.AttackTypeInfo info, Actor atk)
            : this(info, Microsoft.Xna.Framework.Vector2.Zero)
        {
            Attacker = atk;
            baseInfo = info;
        }
        UnitsAndBuilduings.AttackTypeInfo baseInfo;

        public ArealAttack(UnitsAndBuilduings.AttackTypeInfo info, Microsoft.Xna.Framework.Vector2 loc)
        {
            AreaOfAffect = Omron.Actors.UnitConverter.CreatePolygon(info.AreaOfEffect, loc);
            Damage = new Damage(info.Damage);
            Range = info.MaxRange;
            Period = info.Period;
            EffectData = Actors.UnitConverter.CreateEffectData(info.Effect);
        }

        public Effects.GraphicsEffect CreateEffect(float rad, Actor targ, float lifeTime)
        {
            switch (EffectData.Type)
            {
                case "Explosion":
                    return new Effects.Explosion(Attacker.Position, rad, EffectData.Image);
                case "TractorBeam":
                    if (targ == null)
                        return null;
                    return new Effects.TractorBeam(Attacker, targ, lifeTime, AreaOfAffect.Height, EffectData.Image);
                case "RectEffect":
                    RectPoly rect = new RectPoly(Attacker.Position, targ.Position);
                    rect.Rotation = MathHelper.GetAngle(targ.Position - Attacker.Position);
                    rect.Center = (targ.Position + Attacker.Position) / 2;
                    rect.Height = AreaOfAffect.Height;
                    rect.Width = AreaOfAffect.Width;
                    return new Effects.RectEffect(rect);
                default:
                    return null;
            }
        }

        public ArealAttack GetInstance(Microsoft.Xna.Framework.Vector2 loc)
        {
            AreaOfAffect.Center = loc;
            ArealAttack newAtk = new ArealAttack(baseInfo, loc);
            newAtk.Attacker = Attacker;
            return newAtk;
        }

        public Effects.EffectData EffectData;
        public float Range;
        public IPolygon AreaOfAffect;
        public Damage Damage;
        public Actor Attacker;
        public float Period;
    }
}
