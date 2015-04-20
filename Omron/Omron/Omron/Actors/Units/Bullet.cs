using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

using UnitsAndBuilduings;
using Omron.Framework;
using Omron.Effects;

namespace Omron.Actors
{
    class Bullet : FatherUnit
    {
        public Bullet(UnitsAndBuilduings.UnitTypeInfo info, string type, Vector2 loc)
            : base(info, type, loc)
        {
            explosion = info.Attacks[0].Effect.Image;
        }

        string explosion;

        public override void IssueRangedAttack(Vector2 loc) { }

        public override void IssueMeleeAttack(Vector2 loc)
        {
            if (MeleeAttack != null)
            {
                ArealAttack atk = MeleeAttack.GetInstance(loc);
                stage.InflictAttack(atk);

                Effects.GraphicsEffect effect = MeleeAttack.CreateEffect(MeleeAttack.AreaOfAffect.MaxRadius, null, 0.0f);
                if (effect != null)
                    stage.PushEffect(effect);
            }
        }

        public override void IssueMeleeAttack(Actor target)
        {
            this.IssueMeleeAttack(target.Position);
        }

        public override void IssueRangedAttack(Actor target)
        {
            this.IssueRangedAttack(target.Position);
        }
    }
}
