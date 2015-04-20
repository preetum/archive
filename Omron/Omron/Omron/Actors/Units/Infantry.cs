using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UnitsAndBuilduings;

using Omron.Framework;

namespace Omron.Actors
{
    public class Infantry : FatherUnit
    {//basic infantry unit. ********ranged atack then Melee attack
        public Infantry(UnitTypeInfo info, string type, Vector2 loc)
            : base(info, type, loc)
        {
        }

        public override void IssueMeleeAttack(Actor target)
        {
            if (MeleeAttack != null)
            {
                PushAnimation(MeleeAnimation);

                ArealAttack atk = MeleeAttack.GetInstance(Position);
                Vector2 dir = Vector2.Normalize((target.Position - this.Position));
                atk.AreaOfAffect.Rotation = MathHelper.GetAngle(dir);
                atk.AreaOfAffect.Center += dir * atk.AreaOfAffect.Width / 2f;
                stage.InflictAttack(atk);
                //stage.PushEffect(new Effects.RectEffect(atk.AreaOfAffect as RectPoly));

                Effects.GraphicsEffect effect = MeleeAttack.CreateEffect(0.3f, target, atk.Period);
                if (effect != null)
                    stage.PushEffect(effect);
            }
        }

        public override void IssueRangedAttack(Actor target)
        {
            this.IssueRangedAttack(target.Position);
        }

        public override void IssueMeleeAttack(Vector2 loc)
        {
            if (MeleeAttack != null)
            {
                PushAnimation(MeleeAnimation);

                ArealAttack atk = MeleeAttack.GetInstance(loc);
                stage.InflictAttack(atk);

                Effects.GraphicsEffect effect = MeleeAttack.CreateEffect(0.3f, null, 0.0f);
                if (effect != null)
                    stage.PushEffect(effect);
            }
        }

        public override void IssueRangedAttack(Vector2 loc)
        {
            if (RangedAttack != null)
            {
                PushAnimation(RangedAnimation);

                int NUM = RangedAttack.Number; //number of arrows to fire
                float span = this.MaxRadius; //span arrow which to distribute arrows (perpendicular to the direction to the target -- essentially space out the arrows across this span)

                Vector2 targDir = Vector2.Normalize(loc - this.Position);
                Vector2 launchPos = this.Position + 0.5f * this.MaxRadius * targDir;

                if (NUM == 1)
                {
                    Actor shot = UnitConverter.CreateActor(RangedAttack.Type, launchPos, this.Faction);
                    ((FatherUnit)shot).Track(loc);
                    stage.AddActor(shot);
                }
                else
                {
                    for (int i = 0; i < NUM; i++)
                    {

                        float x = (float)i * (span / (NUM - 1)) - span / 2f;

                        Vector2 perpDisp = x * MathHelper.Perpen(targDir);

                        Actor shot = UnitConverter.CreateActor(RangedAttack.Type, launchPos + perpDisp, this.Faction);
                        ((FatherUnit)shot).Track(loc);
                        stage.AddActor(shot);
                    }
                }

            }
        }
    }
}
