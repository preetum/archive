using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using MainGame.GameWorld.GameActors;
using MainGame.GameEngine;
namespace MainGame.GameWorld.Levels
{
    class Level3 : Level
    {
        public override void LoadLevel()
        {
            Actors.Add(new PlayerActor(new Vector2(1072,400), 0));
            Actors.Add(new ExitActor(new Vector2(688, 176)));



            Waypoint punel1 = new Waypoint(new Vector2(430, 144));

            Waypoint root1 =
                new Waypoint(new Vector2(192, 64),
                        new Waypoint(new Vector2(432, 64),
                            punel1));

            Waypoint end1 = new Waypoint(new Vector2(190, 144), root1, punel1);
            punel1.NextWaypoint = end1;




            Waypoint punel2 = new Waypoint(new Vector2(1128, 170));

            Waypoint root2 =
                new Waypoint(new Vector2(880, 80),
                        new Waypoint(new Vector2(1128, 80),
                            punel2));

            Waypoint end2 = new Waypoint(new Vector2(888, 180), root2, punel2);
            punel2.NextWaypoint = end2;



            Waypoint punel3 = new Waypoint(new Vector2(635, 695));

            Waypoint root3 =
                new Waypoint(new Vector2(158, 553),
                        new Waypoint(new Vector2(635, 553),
                            punel3));

            Waypoint end3 = new Waypoint(new Vector2(158, 695), root3, punel3);
            punel3.NextWaypoint = end3;




            var enemy = new Enemy1(new Vector2(0, 0), root1);
            var enemy2 = new Enemy1(new Vector2(0, 0), root2);
            var enemy3 = new Enemy1(new Vector2(0, 0), root3);

            Actors.Add(enemy);
            //Actors.Add(enemy2);
            Actors.Add(enemy3);

            levelTex = ResourceManager.Resources["complexLevelLayoutMap"];
            levelFieldTex = ResourceManager.Resources["complexLevelCollisionMap"];

            

            base.LoadLevel();
        }
    }
}
