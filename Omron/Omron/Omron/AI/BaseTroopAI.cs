using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Omron.Actors;
using Omron.Framework;
using Omron;
using Omron.Helpers;

namespace Omron.AI
{
    public abstract class BaseTroopAI : IUnitAI
    {
        protected FatherUnit actor;
        protected Vector2 postarget;
        protected float maxSpeed;
        protected UnitAIState state;
        public UnitAIState State { get { return state; } }

        protected bool targSet = false;

        Timer rangeTimer, meleeTimer, workTimer;
        protected Actor engagement;

        /// <summary>
        /// true if unit pursues even outside LOS. pursues and engages until engagement dead
        /// </summary>
        bool forceEngage = false;

        /// <summary>
        /// if aggressive, then unit will pursue any unit en route to destination. default false.
        /// </summary>
        public bool aggressive = false;

        HashSet<Actor> visibleActors = new HashSet<Actor>();

        public BaseTroopAI(FatherUnit actor, float maxSpeed)
        {
            this.actor = actor;
            this.maxSpeed = maxSpeed;
            state = UnitAIState.Holding;

            visibleActors = new HashSet<Actor>();

            if (actor.RangedAttack != null)
            {
                rangeTimer = new Timer(actor.RangedAttack.Period, true);
                rangeTimer.Triggered += new TimerEventHandler(rangeTimer_Triggered);
            }
            if (actor.MeleeAttack != null)
            {
                meleeTimer = new Timer(actor.MeleeAttack.Period, true);
                meleeTimer.Triggered += new TimerEventHandler(meleeTimer_Triggered);
            }
            workTimer = new Timer(actor.WorkPeriod, true);
            workTimer.Triggered += new TimerEventHandler(workTimer_Triggered);
        }

        void workTimer_Triggered(GameTime gameTime)
        {
            actor.IssueWork(engagement as FatherBuilding);
        }

        void meleeTimer_Triggered(GameTime gameTime)
        {
            actor.IssueMeleeAttack(engagement);
        }
        void rangeTimer_Triggered(GameTime gameTime)
        {
            actor.IssueRangedAttack(engagement);
        }
        public void Build(FatherBuilding building)
        {
            ClearEngagement();

            engagement = building;
            state = UnitAIState.Building;
            forceEngage = true;
            aggressive = false;
        }
        public void ClearEngagement()
        {
            if (meleeTimer != null)
                meleeTimer.Pause();
            if (rangeTimer != null)
                rangeTimer.Pause();
            workTimer.Pause();

            state = UnitAIState.Tracking;
            engagement = null;
        }
        public void Engage(Actor enemy)
        {
            engagement = enemy;
            state = UnitAIState.Pursuing;
            forceEngage = true;
        }
        public virtual void SetTarget(Vector2 target)
        {
            ClearEngagement();

            this.postarget = target;
            state = UnitAIState.Tracking;
            aggressive = false;

            targSet = true;
        }
        public virtual void SetTargetAggressive(Vector2 target)
        {
            ClearEngagement();

            this.postarget = target;
            state = UnitAIState.Tracking;
            aggressive = true;

            targSet = true;
        }

        protected abstract void hold(Vector2 targ);
        protected abstract void track(Vector2 targ);
        protected abstract void surround(Vector2 targ, float rad);

        protected virtual bool withinThresh(Vector2 targ)
        {
            float dThresh = 0.05f * maxSpeed;
            return ((targ - actor.Position).Length() < dThresh);
        }
        protected IEnumerable<Actor> queryRadiusAll(float rad)
        {
            //if (rad > actor.SightRange) throw new Exception("can't see that far!");

            return from otherActor in visibleActors
                   where withinRad(otherActor, rad)
                   select otherActor;
        }
        protected IEnumerable<Actor> queryRadiusBuildings(float rad)
        {
            if (rad > actor.SightRange) throw new Exception("can't see that far!");

            return from otherActor in visibleActors
                   where otherActor is FatherBuilding && withinRad(otherActor, rad)
                   select (FatherBuilding)otherActor;
        }
        protected IEnumerable<FatherUnit> queryRadiusUnits(float rad)
        {
            if (rad > actor.SightRange) throw new Exception("can't see that far!");

            return from otherActor in visibleActors
                   where otherActor is FatherUnit && withinRad(otherActor, rad)
                   select (FatherUnit)otherActor;
        }
        void updateSight()
        {
            var feelTiles = actor.GetCurrentSightUV().Select(uv => actor.stage.TileGrid[uv]);
            var potCollides = from tile in feelTiles
                              from otherActor in tile.ProximityActors.CloneToList()
                              where otherActor != actor
                              select otherActor;
            visibleActors = new HashSet<Actor>(potCollides);
        }
        protected bool withinRad(Actor otherActor, float rad)
        {
            return (otherActor.Position - actor.Position).Length() - otherActor.MaxRadius < rad;
        }
        /// <summary>
        /// true if otherActor is within the actor's rad + extendRad
        /// </summary>
        /// <param name="otherActor"></param>
        /// <param name="extendRad"></param>
        /// <returns></returns>
        protected bool withinExtendRad(Actor otherActor, float extendRad)
        {
            return withinRad(otherActor, actor.MaxRadius + extendRad);
        }

        /// <summary>
        /// return to default behavior, having completed an engagement or pursuit
        /// </summary>
        protected virtual void returnTrackToDefault()
        {
            if (forceEngage)
            {
                //if unit was forced to engage, stay at position of death
                SetTarget(actor.Position);
            }
            else
            {
                //else, set state to aggressive and attempt to return to postarget (since aggro=true, unit will engage anything enroute)
                SetTargetAggressive(postarget);
            }
            state = UnitAIState.Tracking;
            forceEngage = false;
        }
        void pauseAttackTimers()
        {
            if (rangeTimer != null)
                rangeTimer.Pause();
            if (meleeTimer != null)
                meleeTimer.Pause();
        }
        public virtual void UpdateSlow(GameTime gameTime)
        {
            updateSight();
        }
        public virtual void UpdateMotionFast(GameTime gameTime)
        {
            actor.DesiredVelocity = Vector2.Zero; //clear velocity

            if (rangeTimer != null)
                rangeTimer.Update(gameTime);
            if (meleeTimer != null)
                meleeTimer.Update(gameTime);
            workTimer.Update(gameTime);

            if (!targSet)
            {
                SetTarget(actor.Position);
            }



            switch (state)
            {
                case UnitAIState.Tracking:
                    if (withinThresh(postarget))
                    {
                        state = UnitAIState.Holding;
                        break;
                    }

                    if (aggressive)
                    {
                        if (engagement == null)
                            engagement = queryRadiusUnits(actor.SightRange).Where(unit => actor.Faction.GetRelationship(unit.Faction) == FactionRelationship.Hostile).OrderBy(unit => (actor.Position - unit.Position).Length() - unit.MaxRadius).FirstOrDefault();

                        if (engagement != null)
                        {
                            state = UnitAIState.Pursuing;
                            forceEngage = false;
                            break;
                        }
                    }

                    track(postarget);

                    break;

                case UnitAIState.Holding:

                    if (engagement == null)
                        engagement = queryRadiusUnits(actor.SightRange).Where(unit => actor.Faction.GetRelationship(unit.Faction) == FactionRelationship.Hostile).OrderBy(unit => (actor.Position - unit.Position).Length() - unit.MaxRadius).FirstOrDefault();
                    if (engagement == null)
                        engagement = queryRadiusBuildings(actor.SightRange).Where(unit => actor.Faction.GetRelationship(unit.Faction) == FactionRelationship.Hostile).FirstOrDefault();

                    //TODO: if holdGroundSteadfast, FIRE but do not move

                    if (engagement != null)
                    {
                        if (actor.ShouldAttack)
                        {
                            state = UnitAIState.Pursuing;
                            forceEngage = false;
                            break;
                        }
                        else
                        {
                            //todo: flee!!
                        }
                    }

                    //if no hostile units in sight, search for buildings unfinished
                    engagement = queryRadiusAll(actor.SightRange).Where(building => building is FatherBuilding && building.Faction == actor.Faction && !(building as FatherBuilding).IsComplete).OrderBy(building => (actor.Position - building.Position).Length() - building.MaxRadius).FirstOrDefault();
                    if (engagement != null)
                    {
                        if (actor.ShouldBuild)
                        {
                            state = UnitAIState.Building;
                            break;
                        }
                        else
                            engagement = null;
                    }

                    hold(postarget);

                    break;

                case UnitAIState.Pursuing:

                    pauseAttackTimers();

                    if (engagement == null)
                    {
                        returnTrackToDefault(); //this should never get called? but sometimes it does. what is happening.
                        break;
                    }

                    if (!engagement.IsDead && (forceEngage || withinExtendRad(engagement, actor.SightRange)))
                    {
                        if (actor.MeleeAttack != null && withinExtendRad(engagement, actor.MeleeAttack.Range))
                        {
                            meleeTimer.Start();
                            surround(engagement.Position, actor.MeleeAttack.Range * 0.8f);
                        }
                        else if (actor.RangedAttack != null && withinExtendRad(engagement, actor.RangedAttack.Range))
                        {
                            rangeTimer.Start();
                            surround(engagement.Position, actor.RangedAttack.Range * 0.8f);
                        }
                        else
                            track(engagement.Position); //if no attacks able to launch, pursue the engagement

                    }
                    else
                    {
                        //lost the unit, or unit dead
                        pauseAttackTimers();

                        engagement = null;

                        returnTrackToDefault();
                    }
                    break;

                case UnitAIState.Building:
                    var build = (engagement as FatherBuilding);
                    float BUILDRAD = 0.3f;

                    if (!build.IsComplete && !build.IsDead)
                    {                     

                        if (withinExtendRad(build, BUILDRAD))
                        {
                            surround(build.Position, 0f);
                            workTimer.Start();
                        }
                        else
                        {
                            track(build.Position);
                        }
                    }
                    else
                    {
                        workTimer.Pause();
                        SetTarget(actor.Position); //stay at current position
                    }



                    break;
            }
        }
    }
}
