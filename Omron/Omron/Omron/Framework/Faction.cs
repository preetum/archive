using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Omron.Framework.Networking;

using Omron.Helpers;
using Omron.Actors;

namespace Omron.Framework
{

    public struct ResourceData
    {
        public float Metal;
        public float Crystal;

        public void AddResource(ResourceData d1, float factor, float max)
        {
            this.Metal += Math.Min(factor * d1.Metal, max);
            this.Crystal += Math.Min(factor * d1.Crystal, max);
        }
        public static ResourceData operator +(ResourceData d1, ResourceData d2)
        {
            d1.Metal += d2.Metal;
            d1.Crystal += d2.Crystal;
            return d1;
        }

        public static ResourceData operator -(ResourceData d1, ResourceData d2)
        {
            d1.Metal -= d2.Metal;
            d1.Crystal -= d2.Crystal;
            return d1;
        }

        public static  bool operator >(ResourceData d1, ResourceData d2)
        {
            return (d1.Metal > d2.Metal) && (d1.Crystal > d2.Metal);
        }

        public static bool operator <(ResourceData d1, ResourceData d2)
        {
            return (d1.Metal < d2.Metal) && (d1.Crystal < d2.Metal);
        }

        public static bool operator >=(ResourceData d1, ResourceData d2)
        {
            return (d1.Metal >= d2.Metal) && (d1.Crystal >= d2.Crystal);
        }

        public static bool operator <=(ResourceData d1, ResourceData d2)
        {
            return (d1.Metal <= d2.Metal) && (d1.Crystal <= d2.Crystal);
        }
    }
    public enum FactionRelationship
    {
        Allied,
        Neutral,
        Hostile
    }

    public delegate void FactionTileDiscovered(int u, int v, Faction sender);
    public delegate void ResearchUnlocked(Faction sender, string res);
    public delegate void BuildingUnlocked(Faction sender, string building);

    public class Faction
    {
        public PlayerType FactionType;

        public ResourceData Resources;
        public List<string> BuildingsUnlocked;
        public List<string> ResearchUnlocked;
        public byte ID;

        World world;
        public FogOfWarGrid FOW;
        public event FactionTileDiscovered NotifyTileDiscovered;
        public event ResearchUnlocked NotifyResearchUnlocked;
        public event BuildingUnlocked NotifyBuildingUnlocked;
        
        VictoryChecker vicChecker;

        public static Color[] FactionColors = new Color[6] { Color.Gray, Color.Blue, Color.Black, Color.Violet, Color.Red,
            Color.Red};

        public List<Actor> Actors;

        public Color GetColor()
        {
            return FactionColors[ID];
        }

        public event Action UpdateFactionMenu;

        public void UnlockResearch(string res)
        {
            if (!ResearchUnlocked.Contains(res))
            {
                ResearchUnlocked.Add(res);
                if (NotifyResearchUnlocked != null)
                    NotifyResearchUnlocked(this, res);
            }

            updateBuildingMenus();
        }

        public void UnlockBuilding(string building)
        {
            if (!BuildingsUnlocked.Contains(building))
            {
                BuildingsUnlocked.Add(building);
                if (NotifyBuildingUnlocked != null)
                    NotifyBuildingUnlocked(this, building);
            }

            if (UpdateFactionMenu != null)
                UpdateFactionMenu();
            updateBuildingMenus();
        }

        void updateBuildingMenus()
        {
            Actors.ForEach(act => act.MenuNeedsUpdate());
        }

        public bool FOWEnabled
        {
            get { return FOW != null; }
        }

        private Faction()
        {
            BuildingsUnlocked = new List<string>();
            ResearchUnlocked = new List<string>();
            Actors = new List<Actor>();
        }

        public Faction(byte id, int usize, int vsize)
            : this()
        {
            FOW = new FogOfWarGrid(usize, vsize);
            FOW.NotifyTileDiscovered += new TileDiscovered(FOW_NotifyTileDiscovered);

            ID = id;
            register = new Dictionary<Faction, FactionRelationship>();
            this.Resources = new ResourceData();
        }
        public Faction(byte id, World baseWorld)
            : this(id, baseWorld.TileGrid.U_length, baseWorld.TileGrid.V_length)
        {
            world = baseWorld;
        }
        public Faction(World baseWorld)
            : this((byte)MathHelper.GetUniqueID(), baseWorld)
        {
            world = baseWorld;
        }

        public void SetVictory(VictoryChecker victry)
        {
            vicChecker = victry;
            switch (vicChecker.CheckTime)
            {
                case CheckTime.Timer:
                    vicTimer = new Timer(vicChecker.TimerTime);
                    vicTimer.Start();
                    vicTimer.Triggered += new TimerEventHandler(vicTimer_Triggered);
                    break;
                case CheckTime.Creation:
                    world.NotifyActorAdded += new ActorRemoved(ActorAddedVictoryCheck);
                    break;
                case CheckTime.Death:
                    world.NotifyActorRemoved += new ActorRemoved(ActorRemovedVictoryCheck);
                    break;
            }
        }

        void checkVic()
        {
            if (vicChecker.FactionWon(world, this))
                facWon = true;
            if (vicChecker.FactionLost(world, this))
                facWon = false;
        }

        bool? facWon;

        public bool? FactionWon
        {
            get { return facWon; }
        }

        //bool viccheckerfactionlost(world world, faction faction)
        //{
        //    return !(faction.actors.any(act => (act.type == "troy")));
        //}

        void vicTimer_Triggered(GameTime gameTime)
        {
            checkVic();
        }

        void ActorRemovedVictoryCheck(Actor actor)
        {
            checkVic();
        }

        void ActorAddedVictoryCheck(Actor actor)
        {
            checkVic();
        }
        Timer vicTimer;

        public Faction(byte id)
            : this()
        {
            ID = id;
            register = new Dictionary<Faction, FactionRelationship>();
            this.Resources = new ResourceData();
        }

        void FOW_NotifyTileDiscovered(int u, int v)
        {
            if (NotifyTileDiscovered != null)
                NotifyTileDiscovered(u, v, this);
        }

        Dictionary<Faction, FactionRelationship> register;
        public IEnumerable<Faction> GetAllFactions(FactionRelationship filter)
        {
            return register.Keys.Where(k => register[k] == filter);
        }
        public void Register(Faction otherF, FactionRelationship fRel)
        {
            register[otherF] = fRel;
            otherF.register[this] = fRel;
        }
        public FactionRelationship GetRelationship(Faction otherF)
        {
            if (otherF == this)
                return FactionRelationship.Allied;

            if (register.ContainsKey(otherF))
                return register[otherF];
            else
                return FactionRelationship.Neutral;
        }

        #region Update Methods

        public void SlowUpdate(GameTime gameTime)
        {
            if (vicTimer != null)
            {
                vicTimer.Update(gameTime);
            }

            //TODO: put game AI here
            updateActorsSlow(gameTime);

            computeFOW();

            if (RevealAll)
                this.FOW.ActivateAll();
            
        }

        void updateActorsSlow(GameTime gameTime)
        {
            foreach (var actor in this.Actors.CloneToList())
            {
                actor.UpdateSlow(gameTime);
            }
        }

        public bool RevealAll = false;
        void computeFOW()
        {
            var newFOW = new FogOfWarGrid(this.FOW.usize, this.FOW.vsize);

            for (int u = 0; u < newFOW.usize; u++)
                for (int v = 0; v < newFOW.vsize; v++)
                {
                    if (this.FOW.GetTileState(u, v) != TileState.Undiscovered)
                        newFOW.Discover(u, v);
                }

            foreach (var actor in this.Actors.CloneToList())
            {
                if(!(actor is Bullet))
                    foreach (var uv in actor.GetCurrentSightUV())
                    {
                        newFOW.IncrementVis(uv.X, uv.Y);
                    }
            }

            this.FOW = newFOW;
        }

        #endregion
    }

}
