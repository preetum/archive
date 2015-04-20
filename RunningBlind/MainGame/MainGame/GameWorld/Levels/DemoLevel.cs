using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using MainGame.GameWorld.GameActors;
using MainGame.GameEngine;
namespace MainGame.GameWorld.Levels
{
    class DemoLevel : Level
    {
        public override void LoadLevel()
        {
            Actors.Add(new PlayerActor(new Vector2(250,200), 0));
            Actors.Add(new ExitActor(new Vector2(500, 500)));



            Waypoint root = 
                new Waypoint(new Vector2(230, 560),
                        new Waypoint(new Vector2(388, 560),
                            new Waypoint(new Vector2(292, 620))));

            var enemy = new Enemy1(new Vector2(0, 0), root);

            Actors.Add(enemy);



            levelTex = ResourceManager.Resources["complexLevelLayoutMap"];
            levelFieldTex = ResourceManager.Resources["complexLevelCollisionMap"];

            

            base.LoadLevel();
        }
    }
}
