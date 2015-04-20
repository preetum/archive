using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using MainGame.GameWorld.GameActors;
using MainGame.GameEngine;
namespace MainGame.GameWorld.Levels
{
    class TitleLevel : Level
    {
        public override void LoadLevel()
        {
            Actors.Add(new PlayerActor(new Vector2(205, 400), 0));
            Actors.Add(new ExitActor(new Vector2(500, 400)));


            levelTex = ResourceManager.Resources["titleLogoLayoutMask"];
            levelFieldTex = ResourceManager.Resources["titleLogoCollisionMask"];




            base.LoadLevel();
        }

        float acc = 0f;

        public override void Update(GameTime time)
        {
            acc += (float)time.ElapsedGameTime.TotalSeconds;

            if (acc > 3)
            {
                Pulse p = new Pulse();
                p.Root = Vector2.Zero;
                p.Radius = 2000f;
                
                p.StartTime = DateTime.Now;
                p.PlayerOriginated = true;

                pulseMan.pulses.Add(p);

                acc = 0f;
            }

            base.Update(time);
        }
    }
}
