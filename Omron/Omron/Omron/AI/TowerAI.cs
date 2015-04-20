using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Omron;
using Omron.Actors;
using Omron.Framework;
using Omron.Helpers;

namespace Omron.AI
{
    public class TowerAI : IBuildingAI
    {
        FatherBuilding tower;
        BuildingAIState state;

        Actor engagement;

        Timer rangeTimer, meleeTimer;
        public BuildingAIState State { get { return state; } }


        public TowerAI(FatherBuilding tower)
        {
            this.tower = tower;
            state = BuildingAIState.Idleing;

            if (tower.RangedAttack != null)
            {
                rangeTimer = new Timer(tower.RangedAttack.Period, true);
                rangeTimer.Triggered += new TimerEventHandler(rangeTimer_Triggered);
            }
            if (tower.MeleeAttack != null)
            {
                meleeTimer = new Timer(tower.MeleeAttack.Period, true);
                meleeTimer.Triggered += new TimerEventHandler(meleeTimer_Triggered);
            }
        }

        void meleeTimer_Triggered(GameTime gameTime)
        {
            tower.IssueMeleeAttack(engagement.Position);
        }

        void rangeTimer_Triggered(GameTime gameTime)
        {
            tower.IssueRangedAttack(engagement.Position);
        }

        public void Engage(Actor actor)
        {
            engagement = actor;
        }

        HashSet<Actor> visibleActors = new HashSet<Actor>();

        protected IEnumerable<Actor> queryRadiusAll(float rad)
        {
            if (rad > tower.SightRange) throw new Exception("can't see that far!");

            return from otherActor in visibleActors
                   where withinRad(otherActor, rad)
                   select otherActor;
        }
        void updateSight()
        {
            var feelTiles = tower.GetCurrentSightUV().Select(uv => tower.stage.TileGrid[uv]);
            var potCollides = from tile in feelTiles
                              from otherActor in tile.ProximityActors.CloneToList()
                              where otherActor != tower
                              select otherActor;
            visibleActors = new HashSet<Actor>(potCollides);
        }
        protected bool withinRad(Actor otherActor, float rad)
        {
            return (otherActor.Position - tower.Position).Length() - otherActor.MaxRadius < rad;
        }
        public void UpdateSlow(GameTime gameTime)
        {
            updateSight();
        }
        public void UpdateFast(GameTime gameTime)
        {

            if (rangeTimer != null)
                rangeTimer.Update(gameTime);
            if (meleeTimer != null)
                meleeTimer.Update(gameTime);

            switch (state)
            {
                case BuildingAIState.Idleing:

                    if (engagement == null)
                        engagement = queryRadiusAll(tower.SightRange).FirstOrDefault(actor => tower.Faction.GetRelationship(actor.Faction) == FactionRelationship.Hostile && !actor.IsDead);

                    if (engagement == null)
                        break;


                    if (tower.MeleeAttack != null && withinRad(engagement, tower.MeleeAttack.Range))
                    {
                        state = BuildingAIState.EngagingMelee;
                    }
                    else if (tower.RangedAttack != null && withinRad(engagement, tower.RangedAttack.Range))
                    {
                        state = BuildingAIState.EngagingRange;
                    }


                    break;
                case BuildingAIState.EngagingMelee:

                    if (engagement == null)
                        state = BuildingAIState.Idleing; //should never be hit, but just in case

                    if (!engagement.IsDead)
                    {
                        if (withinRad(engagement, tower.MeleeAttack.Range))
                            meleeTimer.Start();
                        else
                        {
                            meleeTimer.Pause();
                            state = BuildingAIState.Idleing;
                        }
                    }
                    else
                    {
                        //killed engagement
                        meleeTimer.Pause();
                        engagement = null;

                        state = BuildingAIState.Idleing;
                    }


                    break;
                case BuildingAIState.EngagingRange:

                    if (engagement == null)
                        state = BuildingAIState.Idleing; //should never be hit, but just in case

                    if (!engagement.IsDead)
                    {
                        if (withinRad(engagement, tower.RangedAttack.Range))
                            rangeTimer.Start();
                        else
                        {
                            rangeTimer.Pause();
                            state = BuildingAIState.Idleing;
                        }
                    }
                    else
                    {
                        //killed engagement
                        rangeTimer.Pause();
                        engagement = null;

                        state = BuildingAIState.Idleing;
                    }


                    break;
            }
        }
    }
}
