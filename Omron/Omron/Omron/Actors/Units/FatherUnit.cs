using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UnitsAndBuilduings;

using Omron.Framework;
using Omron.AI;

namespace Omron.Actors
{
    public abstract class FatherUnit : DynamicActor
    {
        public Animation IdleAnimation, MoveAnimation, WorkAnimation, MeleeAnimation, RangedAnimation;

        public override Animation GetAnim(AnimationType type)
        {
            switch (type)
            {
                case AnimationType.Idle:
                    return IdleAnimation;
                case AnimationType.MeleeAttack:
                    return MeleeAnimation;
                case AnimationType.Move:
                    return MoveAnimation;
                case AnimationType.RangedAttack:
                    return RangedAnimation;
                case AnimationType.Work:
                    return WorkAnimation;
                default:
                    //throw new KeyNotFoundException("bad byte?");
                    return null;
            }
        }
        public override AnimationType GetAnimType(Animation anim)
        {
            if (anim == IdleAnimation)
                return AnimationType.Idle;
            if (anim == MoveAnimation)
                return AnimationType.Move;
            if (anim == WorkAnimation)
                return AnimationType.Work;
            if (anim == MeleeAnimation)
                return AnimationType.MeleeAttack;
            if (anim == RangedAnimation)
                return AnimationType.RangedAttack;

            return AnimationType.Idle;
        }

        public float MaxSpeed;

        public FatherUnit(UnitTypeInfo info, string type, Vector2 pos) 
            :base(UnitConverter.CreatePolygon(info.Polygon, pos))
        {
            this.Position = polygon.Center;
            this.Inertia = info.Inertia;
            fullHp = info.Health;
            Health = info.Health;
            Defense = new Damage(info.Defense);

            if ((info.Attacks.Length > 0) && !info.Attacks[0].Null)
                mele = new ArealAttack(info.Attacks[0], this);
            if (info.SpawnAttacks.Length != 0)
            {
                spAtk = new SpawnAttack(info.SpawnAttacks[0], this);
            }

            this.Type = type;
            this.workPerSec = info.WorkPower;
            this.workPd = info.WorkPeriod;

            this.MaxSpeed = info.Speed;
            this.AI = UnitConverter.CreateAI(info.AIType, this, MaxSpeed);
            this.CollisionClass = UnitConverter.CreateCollisionType(info.CollisionType);
           
            IdleAnimation = UnitConverter.CreateAnimation(polygon, info.IdleAnimation);
            MoveAnimation = UnitConverter.CreateAnimation(polygon, info.MoveAnimation);
            WorkAnimation = UnitConverter.CreateAnimation(polygon, info.WorkAnimation);
            MeleeAnimation = UnitConverter.CreateAnimation(polygon, info.MeleeAnimation);
            RangedAnimation = UnitConverter.CreateAnimation(polygon, info.RangedAnimation);

            SetBaseAnimation(IdleAnimation);

            this.SightRange = info.SightRange;

            unitMenu = new ActorMenu(this);
            unitMenu.DisplayBar = true;

            IsRotatable = info.Rotatable;

            DrawDepth = Omron.Framework.DrawPriority.UnitDepth;
        }

        public bool ShouldBuild
        {
            get
            {
                return (workPerSec  != 0);
            }
        }


        public bool ShouldAttack
        {
            get
            {
                return !(((MeleeAttack == null) || (MeleeAttack.Damage.IsZero())) &&
                    (RangedAttack == null));
            }
        }

        public void SetBaseAnimationType(AnimationType type)
        {
            switch (type)
            {
                case AnimationType.Idle:
                    SetBaseAnimation(IdleAnimation);
                    break;

            }
        }

        public IUnitAI AI;
        float workPerSec;
        float workPd;
        float fullHp;
        ActorMenu unitMenu;

        public bool IsRotatable;

        public override ActorMenu Menu
        {
            get
            {
                return unitMenu;
            }
        }

        public void Track(Vector2 target)
        {
            if (AI != null)
                AI.SetTarget(target);
        }

        public void MayhamTrack(Vector2 target)
        {
            if (AI != null)
                AI.SetTargetAggressive(target);
        }

        public override float HealthRatio
        {
            get { return Health / fullHp; }
        }

        void updateMenu()
        {
            unitMenu.Info = "Health: " + Health + "/" + fullHp;
            unitMenu.BarValue = HealthRatio;
        }

        public override void MenuNeedsUpdate() { }

        public override void UpdateFast(GameTime gameTime)
        {
            base.UpdateFast(gameTime);
            updateMenu();
            if ((this.ActualVelocity.Length() > 0.4*this.MaxSpeed))
            {
                SetBaseAnimation(MoveAnimation);
            }
            else
            {
                SetBaseAnimation(IdleAnimation);
            }
            if (AI != null)
                AI.UpdateMotionFast(gameTime);
        }
        public override void UpdateSlow(GameTime gameTime)
        {
            base.UpdateSlow(gameTime);

            if (AI != null)
                AI.UpdateSlow(gameTime);
        }

        ArealAttack mele;
        SpawnAttack spAtk;
        public ArealAttack MeleeAttack { get { return mele; } }
        public SpawnAttack RangedAttack { get { return spAtk; } }

        public override void ReadInUpdateData(Lidgren.Network.NetIncomingMessage im)
        {
            base.ReadInUpdateData(im);
            updateMenu();
        }

        public virtual float WorkPower
        {
            get { return workPerSec; }
        }
        public virtual float WorkPeriod
        {
            get { return workPd; }
        }

        public abstract void IssueMeleeAttack(Vector2 loc);
        public abstract void IssueRangedAttack(Vector2 loc);

        public abstract void IssueMeleeAttack(Actor target);
        public abstract void IssueRangedAttack(Actor target);

        public virtual void IssueWork(FatherBuilding build)
        {
            build.OnWorkedOn(WorkPower);

            PushAnimation(WorkAnimation);
        }

    }
}
