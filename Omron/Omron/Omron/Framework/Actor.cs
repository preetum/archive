using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Omron;
using Omron.Actors;
using Omron.Framework;
using Omron.Framework.Networking;

using Lidgren.Network;
using Lidgren.Network.Xna;

namespace Omron
{
    public delegate void AttackedEventHandler(ArealAttack damage, Actor attacker);
    public delegate void CollisionEventHandler(Actor actor);

    public enum CollisionClass
    {
        /// <summary>
        /// default. persists proximity to tiles and has mutual collisions
        /// </summary>
        Normal,
        /// <summary>
        /// does not persist to tiles; cannot collide between each other. is UNAFFECTED by tile terrain.
        /// </summary>
        IsolatedNoPersist
    }

    public abstract class Actor : ISpriteDrawable
    {
        /// <summary>
        /// because all the world's a stage
        /// </summary>
        public World stage;
        public bool Handled;
        public Faction Faction;

        public Vector2 Position;
        public float Rotation = 0.0f;
        public string Type;

        public float Health { get; set; }
        public bool IsDead { get { return Health <= 0f; } }
        public Damage Defense { get; protected set; }

        public virtual ActorMenu Menu { get { return null; } }
        public event Action<Actor> UpdateMenu;
        protected void OnUpdateMenu(Actor sender)
        {
            if (UpdateMenu != null)        
                UpdateMenu(sender);
        }
        public abstract void MenuNeedsUpdate();
        public event Action Died;

        public float SightRange;
        public ushort ActorID;

        public abstract float HealthRatio { get; }

        #region actor_events
        public event CollisionEventHandler Collided;
        public void OnCollided(Actor actor)
        {
            if (Collided != null)
                Collided(actor);
        }
        #endregion actor_events


        public virtual void WriteOutInitialData(NetOutgoingMessage om)
        {
            om.Write(Position);
            om.Write(Rotation);
            om.Write(Health);
            om.Write((byte)GetActiveAnimationType());
            om.Write(GetActiveAnimation().FrameIndex);
        }
        public virtual void ReadInInitialData(NetIncomingMessage im)
        {
            Position = im.ReadVector2();
            Rotation = im.ReadFloat();
            Health = im.ReadFloat();

            Animation anim = GetAnim((AnimationType)im.ReadByte());
            if (anim != null) //if the byte is not corrupted
                SetBaseAnimation(anim);

            GetActiveAnimation().FrameIndex = im.ReadByte();
        }

        public virtual void WriteOutUpdateData(NetOutgoingMessage om)
        {
            om.Write(Health);
            om.Write((byte)GetActiveAnimationType());
            om.Write(GetActiveAnimation().FrameIndex);
        }

        public virtual void ReadInUpdateData(NetIncomingMessage im)
        {
            Health = im.ReadFloat();

            Animation anim = GetAnim((AnimationType)im.ReadByte());
            if (anim != null) //if the byte is not corrupted
                SetBaseAnimation(anim);

            GetActiveAnimation().FrameIndex = im.ReadByte();
        }

        public Animation GetActiveAnimation()
        {
            if (CurrentAnimation != null)
                return CurrentAnimation;
            else
                return BaseLoopAnimation;
        }


        public Actor(IPolygon boundingPoly)
        {
            polygon = boundingPoly;
        }

        protected IPolygon polygon;
        public IPolygon GetBoundingPoly()
        {
            polygon.Rotation = this.Rotation;
            polygon.Center = Position;
            return polygon;
        }
        public float MaxRadius
        {
            get { return polygon.MaxRadius; }
        }



        /// <summary>
        /// called by the world when Actor is first added
        /// </summary>
        public virtual void Init()
        {
        }

        protected bool isSelected = false;

        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                isSelected = value;
            }
        }

        public CollisionClass CollisionClass = CollisionClass.Normal;


        public abstract Animation GetAnim(AnimationType type);
        public abstract AnimationType GetAnimType(Animation anim);

        public AnimationType GetActiveAnimationType()
        {
            return GetAnimType(GetActiveAnimation());
        }

        public Animation BaseLoopAnimation;
        public Animation CurrentAnimation;

        public float DrawDepth = DrawPriority.WallDepth;
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            GetActiveAnimation().GetCurrentFrame().Draw(spriteBatch, Position, Rotation, isSelected ? Color.SteelBlue : Color.White, DrawDepth);
        }
        /// <summary>
        /// pushes an animation (such as attack) on top of the base loop. the animation will be removed when completed.
        /// </summary>
        /// <param name="anim"></param>
        public void PushAnimation(Animation anim)
        {
            BaseLoopAnimation.Stop();
            CurrentAnimation = anim;
            anim.Reset();
            anim.Start();
        }
        /// <summary>
        /// sets the base loop, played by default if no currentanimation
        /// </summary>
        /// <param name="anim"></param>
        public void SetBaseAnimation(Animation anim)
        {
            if (anim == BaseLoopAnimation)
            {
                BaseLoopAnimation.Start();
                return;
            }

            if (BaseLoopAnimation != null)
                BaseLoopAnimation.Stop();

            BaseLoopAnimation = anim;
            anim.Loop = true;
            anim.Reset();
            anim.Start();
        }

        public virtual void UpdateFast(GameTime gameTime)
        {
            BaseLoopAnimation.Update(gameTime);

            if (CurrentAnimation != null)
            {
                CurrentAnimation.Update(gameTime);
                if (CurrentAnimation.IsComplete)
                {
                    CurrentAnimation = null;
                    BaseLoopAnimation.Start();
                }
            }
        }


        public virtual void UpdateSlow(GameTime gameTime)
        {
            computeSight();
        }

        HashSet<Point> sightUVs = new HashSet<Point>();
        void computeSight()
        {
            var posUV = GetPosUV();

            sightUVs = new HashSet<Point>();

            if (SightRange == 0f) return;

            var sightRad = stage.TileGrid.GetDiskUV(posUV, stage.TileGrid.ScreenToUVRad(SightRange), false);
            foreach (var uv in sightRad)
            {
                if (stage.TestLOS(posUV, uv, a => (a is FatherBuilding) && a != this && a.Faction != this.Faction, col => sightUVs.Add(col.GetPosUV()))) //the callback adds the uvpos of the first actor to collide with the LOS (this is the expected behavior of LOS -- otherwise the blocking actor itself would not be seen)
                {
                    sightUVs.Add(uv);
                }
            }
        }

        public HashSet<Point> GetCurrentSightUV()
        {
            return sightUVs;
        }

        public Point GetPosUV()
        {
            return stage.TileGrid.ScreenToUV(this.Position);
        }

        /// <summary>
        /// returns the tiles overlapped by this actor. non-dynamic actors use this EXACT method.
        /// </summary>
        /// <returns></returns>
        public virtual HashSet<Tile> GetProximityOverlaps()
        {
            var region = this.GetBoundingPoly();
            HashSet<Tile> overlaps = new HashSet<Tile>();
            var disk = stage.TileGrid.GetDiskCells(stage.TileGrid.ScreenToUV(region.Center), stage.TileGrid.ScreenToUVRad(region.MaxRadius)  + 1); //+1 to account for regions crossing over hexes
            foreach (var tile in disk)
                if (CollisionTester.TestCollision(region, tile.BoundingHex))
                    overlaps.Add(tile);
            return overlaps;
        }


        public event AttackedEventHandler Attacked;
        
        public void OnAttacked(ArealAttack atk)
        {
            var damage = atk.Damage;
            this.Health -= damage.Slashing * Defense.Slashing;
            this.Health -= damage.Blunt * Defense.Blunt;
            this.Health -= damage.Pierce * Defense.Pierce;
            this.Health -= damage.Blight * Defense.Blight;
            this.Health -= damage.Mining * Defense.Mining;

            if (this.IsDead)
            {
                Die();
            }

            if (Attacked != null)
                Attacked(atk, atk.Attacker);
        }

        public void Die()
        {
            Health = -1;
            stage.RemoveActor(this);

            if (Died != null)
                Died();
        }
    }

    public abstract class DynamicActor : Actor, IFullSynchronize
    {
        public Vector2 DesiredVelocity { get; set; }
        public Vector2 ActualVelocity { get; set; }
        public float Inertia { get; protected set; }

        public float DesiredRotation { get; set; }

        public DynamicActor(IPolygon boundingPoly)
            : base(boundingPoly)
        {
        }

        public override void WriteOutUpdateData(NetOutgoingMessage om)
        {
            base.WriteOutUpdateData(om);

            om.Write(Position);
            om.Write(Rotation);
        }
        public override void ReadInUpdateData(NetIncomingMessage im)
        {
            base.ReadInUpdateData(im);

            var newPos = im.ReadVector2();
            var newRot = im.ReadFloat();

            if (stage != null && CollisionClass == CollisionClass.Normal)
            {
                if (stage.TileGrid.IsValidUV(stage.TileGrid.ScreenToUV(newPos))) //error checking
                    stage.MoveActor(this, newPos, newRot);
            }
            else
            {
                Position = newPos;
                Rotation = newRot;
            }
        }

        public override void Init()
        {
            base.Init();
            computeProximityOffsets();
        }

        IEnumerable<Point> proximityOffsets;
        protected void computeProximityOffsets()
        {
            var disk = stage.TileGrid.GetDiskUV(Point.Zero, stage.TileGrid.ScreenToUVRad(MaxRadius) + 1, true); //+1 to cover moving between tiles
            proximityOffsets = disk;
        }

        /// <summary>
        /// returns the tiles overlapped by this actor. dynamic actors use this ROUGH method (FASTER).
        /// </summary>
        /// <returns></returns>
        public override HashSet<Tile> GetProximityOverlaps()
        {
            HashSet<Tile> proximity = new HashSet<Tile>();

            foreach (var uvOffset in proximityOffsets)
            {
                Point pos = GetPosUV();
                Point uv = new Point(pos.X + uvOffset.X, pos.Y + uvOffset.Y);
                if (stage.TileGrid.IsValidUV(uv))
                    proximity.Add(stage.TileGrid[uv]);
            }
            return proximity;
        }
    }
}
