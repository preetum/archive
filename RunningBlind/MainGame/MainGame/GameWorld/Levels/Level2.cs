using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using MainGame.GameWorld.GameActors;
using MainGame.GameEngine;
namespace MainGame.GameWorld.Levels
{
    class Level2 : Level
    {
        public override void LoadLevel()
        {
            Actors.Add(new PlayerActor(new Vector2(1070, 400), 0));
            Actors.Add(new ExitActor(new Vector2(574, 400)));



            Waypoint root =
                new Waypoint(new Vector2(320, 192),
                        new Waypoint(new Vector2(640, 192)));

            var enemy = new Enemy1(new Vector2(0, 0), root);

            Actors.Add(enemy);



            levelTex = ResourceManager.Resources["intermediateLevelLayoutMap"];
            levelFieldTex = ResourceManager.Resources["intermediateLevelCollisionMap"];

            

            base.LoadLevel();
        }
    }
}
