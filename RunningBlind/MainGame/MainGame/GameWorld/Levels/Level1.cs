using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using MainGame.GameWorld.GameActors;
using MainGame.GameEngine;
namespace MainGame.GameWorld.Levels
{
    class Level1 : Level
    {
        public override void LoadLevel()
        {
            Actors.Add(new PlayerActor(new Vector2(205,400), 0));
            Actors.Add(new ExitActor(new Vector2(1070, 400)));



            //Waypoint root = 
            //    new Waypoint(new Vector2(230, 560),
            //            new Waypoint(new Vector2(388, 560),
            //                new Waypoint(new Vector2(292, 620))));

            //var enemy = new Enemy1(new Vector2(0, 0), root);

            //Actors.Add(enemy);



            levelTex = ResourceManager.Resources["simpleLevelLayoutMap"];
            levelFieldTex = ResourceManager.Resources["SimpleLevelCollisionMap"];

            

            base.LoadLevel();
        }
    }
}
