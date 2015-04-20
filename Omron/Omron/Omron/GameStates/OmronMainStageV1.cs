using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Omron.Framework;
using Omron.Framework.Networking;
using Omron.Actors;

namespace Omron.GameStates
{
    partial class OmronMainStageV1 : GameState
    {
        public OmronMainStageV1() 
        {
        }

        GameLoopTimer fastT, slowT;
        public override void Init()
        {
            MathHelper.ResetIDs();
            ResourceManager.BaseGame.IsMouseVisible = false;

            fastT = new GameLoopTimer(1000d / 60d);
            fastT.Update += new Action<GameTime>(fastT_Update);

            slowT = new GameLoopTimer(1000d / 2d);
            slowT.Update += new Action<GameTime>(slowT_Update);


            spriteBatch = new SpriteBatch(this.GraphicsDevice);


            world = new World(100, 100, ResourceManager.Resources["Settings"]["Grid Size"]);
            var cent = 0.5f * world.TileGrid.UVToScreen(new Point(world.TileGrid.U_length - 1, world.TileGrid.V_length - 1));

            playerF = new Faction(world);
            playerF.Resources.Metal = 300;
            playerF.Resources.Crystal = 200;
            wumpusF = new Faction(world);
            wumpusF.Resources.Crystal = 30000;

            Factions = new List<Faction>();
            Factions.Add(playerF);
            Factions.Add(wumpusF);

            playerF.Register(wumpusF, FactionRelationship.Hostile);
            //playerF.BuildingsUnlocked.Add("DefenseDept_h");
            //playerF.BuildingsUnlocked.Add("DefenseDept_w");
            //playerF.BuildingsUnlocked.Add("Baraks");
            //playerF.BuildingsUnlocked.Add("Outpost");
            //playerF.BuildingsUnlocked.Add("Spawning Pit");
            //playerF.BuildingsUnlocked.Add("Slime Pit");
            //playerF.BuildingsUnlocked.Add("Killer Growth");
            //playerF.BuildingsUnlocked.Add("Baraks");
            //playerF.BuildingsUnlocked.Add("Fence");


            MapGenerator.renewSeed();
            MapGenerator.GeneratePerlinMap(world);

            setupHumanBase(cent - Vector2.UnitX * 5f, playerF);
            setupWumpusBase(cent + Vector2.UnitX * 5f, wumpusF);

            startAllTimers();

            playerController = new GameUI(this, world, playerF);
            playerController.GodMode = true;

            playerF.SetVictory(ResourceManager.Resources["TroyRegicide"]);
            wumpusF.SetVictory(ResourceManager.Resources["Conquest"]);
        }

        void slowT_Update(GameTime gameTime)
        {
            int deadF = 0;
            Faction lastUndeadF = null;
            foreach (Faction f in Factions)
            {
                f.SlowUpdate(gameTime);
                if (f.FactionWon != null)
                {
                    if (f.FactionWon.Value)
                    {
                        //display victory of winning faction
                        finalMessage = "Faction of color " + f.GetColor().ToString() + " wins!";
                    }
                    else
                        deadF++;
                }
                else
                    lastUndeadF = f;
            }
            if (deadF + 1 >= Factions.Count)
            {
                //display victory of lastUndeadF
                finalMessage = "Faction of color " + lastUndeadF.GetColor().ToString() + " wins!";
            }
        }

        void startAllTimers()
        {
            fastT.Start();
            slowT.Start();
        }


        void fastT_Update(GameTime obj)
        {
            world.UpdateActorFastOnly(obj);
            world.UpdateMotionOnly(obj);

            world.UpdateEffectsOnly(obj);
        }

        void setupHumanBase(Vector2 basePos, Faction faction)
        {
            float fClear = 5f;

            var factSurround = new RectPoly(basePos, fClear, fClear, 0f);
            var acts = world.Query(factSurround);
            foreach (var actor in acts)
            {
                if ((actor is Wall || actor is Resource) && (actor.Type != "Monolith"))
                    actor.Die();
            }
            
            world.AddActor(UnitConverter.CreateActor("Fortress", basePos, faction, true));
            world.AddActor(UnitConverter.CreateActor("Troy", basePos - 1.5f * Vector2.One, faction, true));

            /*for (int i = 0; i < 3; i++)
            {
                var pos = basePos + Vector2.UnitX * ((float)MathHelper.Rand.NextDouble() * 2 - 1f) * (fClear / 2) + Vector2.UnitY * ((float)MathHelper.Rand.NextDouble() * 2 - 1f) * (fClear / 2);
                world.AddActor(UnitConverter.CreateActor("Miner", pos, faction));
            }
            for (int i = 0; i < 10; i++)
            {
                var pos = basePos + Vector2.UnitX * ((float)MathHelper.Rand.NextDouble() * 2 - 1f) * (fClear / 2) + Vector2.UnitY * ((float)MathHelper.Rand.NextDouble() * 2 - 1f) * (fClear / 2);
                world.AddActor(UnitConverter.CreateActor("Ranger", pos, faction));
            } 
            for (int i = 0; i < 10; i++)
            {
                var pos = basePos + Vector2.UnitX * ((float)MathHelper.Rand.NextDouble() * 2 - 1f) * (fClear / 2) + Vector2.UnitY * ((float)MathHelper.Rand.NextDouble() * 2 - 1f) * (fClear / 2);
                world.AddActor(UnitConverter.CreateActor("Troy", pos, faction));
            }*/
        }

        void setupWumpusBase(Vector2 basePos, Faction faction)
        {//put wumpus targes here
            var factory = (FatherBuilding)UnitConverter.CreateActor("Spawning Pit", basePos, faction);
            factory.OnWorkedOn(factory.WorkNeeded);
            world.AddActor(factory);

            float fClear = 10f;

            var factSurround = new RectPoly(basePos, fClear, fClear, 0f);
            var acts = world.Query(factSurround);
            foreach (var actor in acts)
            {
                if ((actor is Wall || actor is Resource) && (actor.Type != "Monolith"))
                    actor.Die();
            }

            for (int i = 0; i < 2; i++)
            {
                var pos = basePos + Vector2.UnitX * ((float)MathHelper.Rand.NextDouble() * 2 - 1f) * (fClear / 2) + Vector2.UnitY * ((float)MathHelper.Rand.NextDouble() * 2 - 1f) * (fClear / 2);
                world.AddActor(UnitConverter.CreateActor("Dark Magus", pos, faction));
            }
            for (int i = 0; i < 10; i++)
            {
                var pos = basePos + Vector2.UnitX * ((float)MathHelper.Rand.NextDouble() * 2 - 1f) * (fClear / 2) + Vector2.UnitY * ((float)MathHelper.Rand.NextDouble() * 2 - 1f) * (fClear / 2);
                world.AddActor(UnitConverter.CreateActor("Tentaclez", pos, faction));
            }
            for (int i = 0; i < 20; i++)
            {
                var pos = basePos + Vector2.UnitX * ((float)MathHelper.Rand.NextDouble() * 2 - 1f) * (fClear / 2) + Vector2.UnitY * ((float)MathHelper.Rand.NextDouble() * 2 - 1f) * (fClear / 2);
                world.AddActor(UnitConverter.CreateActor("Mite", pos, faction));
            }
        }
        
        GameUI playerController;

        SpriteBatch spriteBatch;

        World world;

        Faction playerF, wumpusF;

        List<Faction> Factions;
        string finalMessage;

        public override void Update(GameTime gameTime)
        {
            playerController.Update(gameTime);
        }

        
        public override void Draw(GameTime gameTime)
        {
            if (finalMessage == null)
                playerController.Draw(spriteBatch);
            else
            {
                spriteBatch.Begin();
                spriteBatch.DrawString(ResourceManager.Resources["font1"], finalMessage, new Vector2(300), Color.Red);
                spriteBatch.End();
            }
        }
    }
}
