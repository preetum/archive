using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

using Omron.Framework;
using Omron.Helpers;
using Omron.Actors;
using Omron.Effects;
using Omron.Framework.Networking;

using Lidgren.Network;

namespace Omron
{

    public delegate void TileOverlapEventHandler(Actor actor);
    public delegate void TileMainEventHandler(Actor actor, Tile prevOrNextTile);

    public delegate void ActorRemoved(Actor actor);
    public delegate void ActorAdded(Actor actor);

    public delegate void EffectPushed(GraphicsEffect effect);
    public delegate void UpdatePushed(Actor actor, NetOutgoingMessage update);

    public abstract class Tile
    {
        Vector2 _posScreen;
        Point _posUV;
        float _sideLen;

        public Vector2 Position { get { return _posScreen; } }
        public Point PositionUV { get { return _posUV; } }
        public float SideLen { get { return _sideLen; } }

        public HashSet<Actor> ProximityActors;

        public HexPoly BoundingHex;

        #region tile_events
        public event TileOverlapEventHandler ProximityEntered;
        public event TileOverlapEventHandler ProximityExited;
        /// <summary>
        /// called when the position (usually cent) of an actor enters the tile
        /// </summary>
        public event TileMainEventHandler TileEntered;
        public event TileMainEventHandler TileExited;

        public void OnProximityEnter(Actor actor)
        {
            if (ProximityEntered != null)
                ProximityEntered(actor);
        }
        public void OnProximityExit(Actor actor)
        {
            if (ProximityExited != null)
                ProximityExited(actor);
        }
        public void OnMainEnter(Actor actor, Tile prev)
        {
            if (TileEntered != null)
                TileEntered(actor, prev);
        }
        public void OnMainExit(Actor actor, Tile dest)
        {
            if (TileExited != null)
                TileExited(actor, dest);
        }
        #endregion tile_events

        public abstract float TerrainCost { get; }

        public Tile(Point posUV, Vector2 posScreen, float sideLen)
        {
            _posUV = posUV;
            _posScreen = posScreen;
            _sideLen = sideLen;

            ProximityActors = new HashSet<Actor>();
            BoundingHex = new HexPoly(Position, SideLen, 0.0f);
        }

    }
    public class DirtTile : Tile
    {
        public override float TerrainCost { get { return 10; } }

        public DirtTile(Point posUV, Vector2 posScreen, float sideLen)
            : base(posUV, posScreen, sideLen)
        {
        }

    }

    public class World
    {
        public HexGrid<Tile> TileGrid;

        public Faction Faction;

        List<Actor> actors;

        List<GraphicsEffect> effects;

        public NetServer server;

        public event ActorRemoved NotifyActorRemoved;
        void onActorRemoved(Actor act)
        {
            if (NotifyActorRemoved != null)
                NotifyActorRemoved(act);
        }
        public event ActorRemoved NotifyActorAdded;
        void onActorAdded(Actor act)
        {
            if (NotifyActorAdded != null)
                NotifyActorAdded(act);
        }

        public event EffectPushed NotifyEffectPushed;
        void onEffectPushed(GraphicsEffect effect)
        {
            if (NotifyEffectPushed != null)
                NotifyEffectPushed(effect);
        }

        public event UpdatePushed NotifyUpdatePushed;
        public void PushUpdateActor(Actor actor, NetOutgoingMessage update)
        {
            if (NotifyUpdatePushed != null)
                NotifyUpdatePushed(actor, update);
        }
        public NetOutgoingMessage CreateServerOM()
        {
            if (server != null)
                return server.CreateMessage();
            return null;
        }

        public IEnumerable<Actor> GetActors()
        {
            return actors.CloneToList();
        }

        public World(int usize, int vsize, float hexSideLen)
        {
            initGrid(usize, vsize, hexSideLen);
            actors = new List<Actor>();
            effects = new List<GraphicsEffect>();
            Faction = new Faction((byte)MathHelper.GetUniqueID(), this);
            idRegister = new Dictionary<ushort, Actor>();
        }
        void initGrid(int usize, int vsize, float hexSideLen)
        {
            TileGrid = new HexGrid<Tile>(usize, vsize, hexSideLen);
            for (int u = 0; u < TileGrid.U_length; u++)
                for (int v = 0; v < TileGrid.V_length; v++)
                {
                    Point uv = new Point(u, v);
                    TileGrid[u, v] = new DirtTile(uv, TileGrid.UVToScreen(uv), hexSideLen);
                }
        }

        Dictionary<ushort, Actor> idRegister;
        ushort id = 0;

        public Actor GetActorByID(ushort id)
        {
            if (idRegister.ContainsKey(id))
                return idRegister[id];
            else
                return null;
        }

        ushort getUnusedId()
        {
            return id++;
        }

        public void PushEffect(GraphicsEffect effect)
        {
            effect.world = this;
            lock (effects)
                effects.Add(effect);

            onEffectPushed(effect);
        }
        public IEnumerable<GraphicsEffect> GetEffects()
        {
            return effects.CloneToList();
        }

        /// <summary>
        /// adds an actor to world WITH A NEW UNIQUE ID!
        /// </summary>
        /// <param name="actor"></param>
        public void AddActor(Actor actor)
        {
            AddActor(actor, getUnusedId());
        }
        public void AddActor(Actor actor, ushort ID)
        {
            _addActor(actor, ID);
        }
        public void RemoveActor(Actor actor)
        {
            _removeActor(actor);
        }

        void _addActor(Actor actor, ushort ID)
        {
            actor.stage = this;
            actor.Init();
            actor.ActorID = ID;
            idRegister.Add(actor.ActorID, actor);
            lock (actors)
                actors.Add(actor);

            lock (actor.Faction.Actors)
                actor.Faction.Actors.Add(actor);

            onActorAdded(actor);
            

            if (actor.CollisionClass == CollisionClass.Normal)
            {
                foreach (Tile tile in actor.GetProximityOverlaps())
                {
                    lock (tile.ProximityActors)
                        tile.ProximityActors.Add(actor);
                }

            }
        }
        void _removeActor(Actor actor)
        {
            idRegister.Remove(actor.ActorID);
            lock (actors)
                actors.Remove(actor);

            lock (actor.Faction.Actors)
                actor.Faction.Actors.Remove(actor);

            onActorRemoved(actor);

            if (actor.CollisionClass == CollisionClass.Normal)
            {
                foreach (Tile tile in actor.GetProximityOverlaps())
                {
                    lock (tile.ProximityActors)
                        tile.ProximityActors.Remove(actor);
                }

            }
        }

        public void MoveActor(DynamicActor actor, Vector2 newPos, float newRot)
        {
            if (actor.CollisionClass == CollisionClass.IsolatedNoPersist)
            {
                actor.Position = newPos;
                actor.Rotation = newRot;
                throw new Exception("you really shouldn't be doing this");
                return;
            }  

            moveActor_overlaps(actor, newPos, newRot);
        }

        /// <summary>
        /// move an actor, updating ActorOverlaps for all affected tiles
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="newPos"></param>
        void moveActor_overlaps(DynamicActor actor, Vector2 newPos, float newRot)
        {
            var initUV = actor.GetPosUV();
            var initProximity = actor.GetProximityOverlaps();
            actor.Position = newPos;
            actor.Rotation = newRot;
            var finalUV = actor.GetPosUV();
            var finalProximity = actor.GetProximityOverlaps();

            //main events
            if (initUV != finalUV)
            {
                var initTile = TileGrid[initUV];
                var finalTile = TileGrid[finalUV];
                initTile.OnMainExit(actor, finalTile);
                finalTile.OnMainEnter(actor, initTile);
            }

            //overlap events
            //note: could be optimized
            foreach (Tile ftile in finalProximity)
            {
                if ( !initProximity.Contains(ftile) )
                {
                    lock (ftile.ProximityActors)
                    {
                        ftile.ProximityActors.Add(actor);
                        ftile.OnProximityEnter(actor);
                    }
                }
            }
            foreach (Tile itile in initProximity)
            {
                if (! finalProximity.Contains(itile) )
                {
                    lock (itile.ProximityActors)
                    {
                        itile.ProximityActors.Remove(actor);
                        itile.OnProximityExit(actor);
                    }
                }
            }
        }

        

        public void UpdateEffectsOnly(GameTime gameTime)
        {
            foreach (var effect in this.GetEffects())
            {
                effect.Update(gameTime);
                if (!effect.IsActive)
                    effects.Remove(effect);
            }
        }
        public void UpdateActorFastOnly(GameTime gameTime)
        {
            foreach (var actor in this.GetActors())
            {
                actor.UpdateFast(gameTime);
            }
        }
        public void UpdateMotionOnly(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            foreach (var actor in this.GetActors())
            {

                if (actor is DynamicActor)
                {
                    var dactor = (DynamicActor)actor;

                    var apoly = actor.GetBoundingPoly();
                    var proximity = actor.GetProximityOverlaps();

                    var potCollides = from tile in proximity
                                      from otherActor in tile.ProximityActors.CloneToList()
                                      where otherActor != actor
                                      select otherActor;

                    var collideSet = new HashSet<Actor>(potCollides);

                    foreach (var otherActor in collideSet)
                        if (CollisionTester.TestCollision(apoly, otherActor.GetBoundingPoly()))
                        {
                            actor.OnCollided(otherActor);

                            if (!(otherActor is DynamicActor) || actor.CollisionClass == CollisionClass.IsolatedNoPersist)
                                otherActor.OnCollided(actor); //required only for IsolatedNoPersist and static actors, because the otherActor will not detect the collision. (since actor isn't persisted to tile [isolatednopersist] or doesn't check for collisions [static])
                        }



                    if (actor.CollisionClass == CollisionClass.Normal)
                    {
                        var actualVel = getActualVelocity(dactor, dactor.DesiredVelocity);
                        dactor.ActualVelocity = actualVel;

                        var newPos = dactor.Position + actualVel * dt;
                        var newRot = dactor.DesiredRotation;

                        if (actualVel.LengthSquared() > 0)
                        {
                            MoveActor(dactor, newPos, newRot);
                        }
                    }
                    else if (actor.CollisionClass == CollisionClass.IsolatedNoPersist)
                    {
                        dactor.ActualVelocity = dactor.DesiredVelocity;
                        dactor.Position += dactor.ActualVelocity * dt;
                        dactor.Rotation = dactor.DesiredRotation;
                    }

                }
            }
        }


        public void UpdateAll(GameTime gameTime) //phase this out -- replace with separate functions
        {
            UpdateEffectsOnly(gameTime);

            UpdateActorFastOnly(gameTime);
            UpdateMotionOnly(gameTime);
        }



        /// <summary>
        /// checks if an actor is colliding with any other FatherBuilding (used for buildings)
        /// </summary>
        /// <param name="actor"></param>
        /// <returns></returns>
        public bool IsCollidingConBuilding(Actor actor)
        {
            actor.stage = this;

            var actorBounds = actor.GetBoundingPoly();
            var proximity = actor.GetProximityOverlaps();

            var potCollides = from tile in proximity
                              from otherActor in tile.ProximityActors
                              where otherActor != actor
                              select otherActor;

            var collideSet = new HashSet<Actor>(potCollides);

            foreach (Actor tileActor in collideSet.Where(act => act is FatherBuilding))
                if (CollisionTester.TestCollision(actorBounds, tileActor.GetBoundingPoly()))
                    return true;

            return false;
        }

        /// <summary>
        /// checks if an actor is colliding with any other actors (checks the tiles in the actor's tile overlaps)
        /// </summary>
        /// <param name="actor"></param>
        /// <returns></returns>
        public bool IsColliding(Actor actor)
        {
            actor.stage = this;

            var actorBounds = actor.GetBoundingPoly();
            var proximity = actor.GetProximityOverlaps();

            var potCollides = from tile in proximity
                              from otherActor in tile.ProximityActors
                              where otherActor != actor
                              select otherActor;

            var collideSet = new HashSet<Actor>(potCollides);

            foreach (Actor tileActor in collideSet)
                if (CollisionTester.TestCollision(actorBounds, tileActor.GetBoundingPoly()))
                    return true;
       
            return false;
        }

        Vector2 getActualVelocity(Actor actor, Vector2 desiredVel)
        {
            //TODO: change to account for terrain, etc.
            return desiredVel;
        }

        /// <summary>
        /// returns all actors contained within region. warning: does not detect IsolatedNoPersist types
        /// </summary>
        /// <param name="region"></param>
        /// <returns></returns>
        public IEnumerable<Actor> Query(IPolygon region)
        {
            var disk = TileGrid.GetDiskCells(TileGrid.ScreenToUV(region.Center), TileGrid.ScreenToUVRad(region.MaxRadius) + 1); //+1 to account for regions crossing over hexes

            var potCollides = from tile in disk
                              from actor in tile.ProximityActors.CloneToList()
                              select actor;

            var collideSet = new HashSet<Actor>(potCollides);

            foreach (var actor in collideSet)
                if (CollisionTester.TestCollision(actor.GetBoundingPoly(), region))
                    yield return actor;
        }
        public IEnumerable<Actor> QueryPoint(Vector2 point)
        {
            RectPoly pRect = new RectPoly(point, 0, 0, 0);

            var tile = TileGrid[TileGrid.ScreenToUV(point)];

            var potCollides = tile.ProximityActors.CloneToList();
            var collideSet = new HashSet<Actor>(potCollides);
       
            foreach (var actor in collideSet)
                if (CollisionTester.TestPointInside(point, actor.GetBoundingPoly()))
                    yield return actor;
        }

        public HashSet<Actor> QueryVisibleActors(FogOfWarGrid fow)
        {
            HashSet<Actor> visibleActors = new HashSet<Actor>();

            //old method = use overlaps
            /*for (int u = 0; u < this.TileGrid.U_length; u++)
                for (int v = 0; v < this.TileGrid.V_length; v++)
                {
                    var tile = this.TileGrid[u, v];
                    var pactors = tile.ProximityActors;

                    var tState = fow.GetTileState(u, v);

                    lock (pactors)
                    {
                        if (tState == TileState.Active)
                        {
                            foreach (var actor in pactors)
                                visibleActors.Add(actor);
                        }
                        else if (tState == TileState.Inactive)
                        {
                            foreach (var actor in pactors)
                            {
                                if (!(actor is FatherUnit))
                                    visibleActors.Add(actor);
                            }
                        }
                    }
                }
            foreach (Actor actor in this.GetActors())
            {
                if (actor.CollisionClass == CollisionClass.IsolatedNoPersist)
                {
                    var posUV = actor.GetPosUV();
                    if (fow.GetTileState(posUV.X, posUV.Y) == TileState.Active)
                        visibleActors.Add(actor);
                }
                //add to visible actors here, from GetPosUV<->fox.GetTileState, instead of above with tile prox actors? would be faster, but could look bad for large actors.
            }*/


            //new method: only check if centers are over active tiles
            foreach (var actor in this.GetActors())
            {
                var posUV = actor.GetPosUV();
                var tState = fow.GetTileState(posUV);

                if (tState == TileState.Active || (tState == TileState.Inactive && actor is FatherBuilding))
                    visibleActors.Add(actor);
            }

            return visibleActors;
        }

        /// <summary>
        /// calls collideCallback on the first colliding actor to pass the filter
        /// </summary>
        /// <param name="source"></param>
        /// <param name="targ"></param>
        /// <param name="filter"></param>
        /// <param name="collideCallback"></param>
        /// <returns></returns>
        public bool TestLOS(Vector2 source, Vector2 targ, Func<Actor, bool> filter, Action<Actor> collideCallback)
        {
            float incr = 0.10f;


            Point prevUV = TileGrid.ScreenToUV(source);
            IEnumerable<Actor> potCollides = TileGrid[prevUV].ProximityActors.CloneToList();  

            float len = (targ - source).Length();
            Vector2 dir = Vector2.Normalize(targ - source);   
            for (float dist = 0; dist < len; dist += incr)
            {
                Vector2 testPt = source + dist * dir;

                var tUV = TileGrid.ScreenToUV(testPt);
                if (TileGrid.IsValidUV(tUV))
                {
                    if (tUV != prevUV)
                    {
                        potCollides = TileGrid[tUV].ProximityActors.CloneToList();
                        prevUV = tUV;         
                    }

                    var col = potCollides.FirstOrDefault(a => filter(a) && CollisionTester.TestPointInside(testPt, a.GetBoundingPoly()));
                    if (col != null)
                    {
                        collideCallback(col);
                        return false;
                    }
                }
            }

            

            return true;
        }
        public bool TestLOS(Vector2 source, Vector2 targ, Func<Actor, bool> filter)
        {
            return TestLOS(source, targ, filter, a => { });
        }
        public bool TestLOS(Actor sourceActor, Actor targActor)
        {
            return TestLOS(sourceActor.Position, targActor.Position, a => a != sourceActor && a != targActor && a.Faction != sourceActor.Faction);
        }

        public bool TestLOS(Point sUV, Point tUV, Func<Actor, bool> filter, Action<Actor> colCallback)
        {
            //TODO: re-do this with Bresenham's line algorithm?

            return TestLOS(TileGrid.UVToScreen(sUV), TileGrid.UVToScreen(tUV), filter, colCallback);
        }

        public void InflictAttack(ArealAttack atk)
        {
            var targets = Query(atk.AreaOfAffect);
            foreach (var target in targets)
            {
                if (target.Faction.GetRelationship(atk.Attacker.Faction) != FactionRelationship.Allied)
                    target.OnAttacked(atk);
            }
        }
    }
}
