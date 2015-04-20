using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UnitsAndBuilduings;

using Omron.Framework;
using Omron.Framework.Networking;
using Omron.AI;

namespace Omron.Actors
{
    public abstract class FatherBuilding : Actor
    {

        public Animation IdleAnimation, WorkAnimation, BuildAnimation;

        public override Animation GetAnim(AnimationType type)
        {
            switch (type)
            {
                case AnimationType.Idle:
                    return IdleAnimation;
                case AnimationType.Work:
                    return WorkAnimation;
                case AnimationType.Build:
                    return BuildAnimation;
                default:
                    //throw new KeyNotFoundException("bad byte?");
                    return null;
            }
        }
        public override AnimationType GetAnimType(Animation anim)
        {
            if (anim == IdleAnimation)
                return AnimationType.Idle;
            if (anim == WorkAnimation)
                return AnimationType.Work;
            if (anim == BuildAnimation)
                return AnimationType.Build;

            return AnimationType.Idle;
        }

        public override void MenuNeedsUpdate() { }

        public FatherBuilding(BuilduingTypeInfo info, string type, Vector2 pos)
            :base(UnitConverter.CreatePolygon(info.Polygon, pos))
        {
            this.Position = polygon.Center;
            fullHp = info.Health;
            Defense = new Damage(info.Defense);
            //SpriteData = ResourceManager.CreateSpData(info.DrawInfo, polygon);
            this.Type = type;
            this.wNeed = info.WorkNeeded;
            wDone = 0;

            this.SightRange = info.SightRange;

            IdleAnimation = UnitConverter.CreateAnimation(polygon, info.IdleAnimation);
            BuildAnimation = UnitConverter.CreateAnimation(polygon, info.BuildAnimation);
            WorkAnimation = UnitConverter.CreateAnimation(polygon, info.WorkAnimation);
            SetBaseAnimation(BuildAnimation);
            BuildAnimation.Stop();
            buildMenu = new ActorMenu(this);
            buildMenu.DisplayBar = true;
            Health = 1;

            DrawDepth = Omron.Framework.DrawPriority.BuildingDepth;
            this.Attacked += new AttackedEventHandler(changeAnim);
        }

        void changeAnim(ArealAttack damage, Actor attacker)
        {
            if (IsComplete)
            {
                if (this.Health > 0)
                    this.IdleAnimation.GoToFrameRatio(1 - this.HealthRatio);
            }
        }

        public IBuildingAI AI;

        float wNeed;
        protected float wDone;
        protected ActorMenu buildMenu;
        protected float fullHp;

        public virtual ArealAttack MeleeAttack { get { return null; } }
        public virtual SpawnAttack RangedAttack { get { return null; } }
        public virtual void IssueMeleeAttack(Vector2 loc) { }
        public virtual void IssueRangedAttack(Vector2 loc) { }

        public override void WriteOutUpdateData(Lidgren.Network.NetOutgoingMessage om)
        {
            base.WriteOutUpdateData(om);

            om.Write(this.WorkDone);
        }
        public override void ReadInUpdateData(Lidgren.Network.NetIncomingMessage im)
        {
            base.ReadInUpdateData(im);

            wDone = im.ReadFloat();
            updateMenu();
            if ((wDone == wNeed) && (!builtCalled))
            {
                builtCalled = true;
                OnUpdateMenu(this);
            }
        }

        bool builtCalled = false;

        public override ActorMenu Menu
        {
            get
            {
                return buildMenu;
            }
        }

        public float WorkNeeded
        {
            get { return wNeed; }
        }
        public float WorkDone
        {
            get { return wDone; }
        }
        public float WorkLeft
        {
            get { return wNeed - wDone; }
        }
        public float Completion
        {
            get { return wDone / wNeed; }
        }
        public bool IsComplete
        {
            get { return wDone >= wNeed; }
        }

        public void OnWorkedOn(float work)
        {
            if (IsComplete)
                return;
            wDone += work;
            Health += work / wNeed * (fullHp - 1);
            if (Health > fullHp) Health = fullHp;
            updateMenu();
            BuildAnimation.GoToFrameRatio(Completion);
            if (wDone >= wNeed)
            {//goes here when construction is done! lolB
                SetBaseAnimation(IdleAnimation);
                IdleAnimation.Stop();
                wDone = wNeed;
                OnUpdateMenu(this);
                return;
            }
        }
        public void AutoComplete()
        {
            this.OnWorkedOn(this.WorkNeeded);
        }
        public override float HealthRatio
        {
            get { return Health / fullHp; }
        }

        void updateMenu()
        {
            if (!IsComplete)
                buildMenu.BarValue = Completion;
            else
                buildMenu.BarValue = Health / fullHp;
            buildMenu.Info = "Health: " + Health + "/" + fullHp;
        }

        public override void UpdateSlow(GameTime gameTime)
        {
            base.UpdateSlow(gameTime);
            updateMenu();

            if (AI != null && IsComplete)
                AI.UpdateSlow(gameTime);
        }
        public override void UpdateFast(GameTime gameTime)
        {
            base.UpdateFast(gameTime);

            if (AI != null && IsComplete)
                AI.UpdateFast(gameTime);
        }
    }
}
