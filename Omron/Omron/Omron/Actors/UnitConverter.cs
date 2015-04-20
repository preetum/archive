using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnitsAndBuilduings;

using Omron.Framework;
using Omron.Actors;
using Omron.AI;

namespace Omron.Actors
{
    static class UnitConverter
    {
        public static Actor CreateActor(string stInfo, Microsoft.Xna.Framework.Vector2 location, float rotation, Faction f)
        {
            Actor act = CreateActor(stInfo, location, f);
            act.Rotation = rotation;
            return act;
        }

        public static Actor CreateActor(string stInfo, Microsoft.Xna.Framework.Vector2 location, Faction f)
        {//use this to get an instance of an actor
            dynamic info = ResourceManager.Resources[stInfo];
            
            Actor act;
            switch ((string)info.Type)
            {

                case "Wall":
                    act = new Wall(info, stInfo, location);
                    break;
                case "Infantry":
                    act = new Infantry(info, stInfo, location);
                    break;
                case "Bullet":
                    act = new Bullet(info, stInfo, location);
                    break;
                case "Resource":
                    act = new Resource(info, stInfo, location);
                    break;
                case "Tower":
                    act = new Tower(info, stInfo, location);
                    break;
                case "ActionCenter":
                    act = new ActionCenter(info, stInfo, location, f);
                    break;
                default:
                    throw new Exception("Building type not found.");
            }
            act.Faction = f;
            return act;
        }
        public static Actor CreateActor(string stInfo, Microsoft.Xna.Framework.Vector2 location, Faction f, bool autoComplete)
        {
            var actor = CreateActor(stInfo, location, f);
            if (autoComplete && actor is FatherBuilding)
                (actor as FatherBuilding).AutoComplete();
            return actor;
        }
        public static IPolygon CreatePolygon(PolygonInfo info, Microsoft.Xna.Framework.Vector2 loc)
        {
            switch (info.Type)
            {
                case "Rectangle":
                    return new RectPoly(loc, info.Width, info.Height, 0);
                case "Hexagon":
                    return new HexPoly(loc, info.Width, 0);
                default:
                    throw new Exception("Polygon info not handled");
            }
        }

        public static IUnitAI CreateAI(string ai, FatherUnit act, float mSpeed)
        {
            switch (ai)
            {
                case "troop":
                    return new TroopAI(act, mSpeed);
                case "bullet":
                    return new BulletAI(act, mSpeed);
                //case "test":
                //    return new BetterTroopAI(act, mSpeed);
                case "none":
                    return null;
                default:
                    throw new Exception("AI " + ai + " not found!");
            }
        }

        public static CollisionClass CreateCollisionType(string col)
        {
            switch (col)
            {
                case "normal":
                    return CollisionClass.Normal;
                case "isolated":
                    return CollisionClass.IsolatedNoPersist;
                default:
                    throw new Exception("Collsion type not found and stuff");
            }
        }

        public static ResourceData CreateResourceData(ResourceCostInfo[] infos)
        {
            ResourceData data = new ResourceData();
            foreach (ResourceCostInfo inf in infos)
            {
                switch (inf.Resource)
                {
                    case "Metal":
                        data.Metal = inf.Cost;
                        break;
                    case "Crystal":
                        data.Crystal = inf.Cost;
                        break;
                    default:
                        throw new Exception("Resource not found in CreateResourceData");
                }
            }
            return data;
        }

        public static Animation CreateAnimation(IPolygon poly, AnimationTypeInfo data)
        {
            AnimationData dat = new AnimationData();
            var datFrames = ResourceManager.Resources[data.Animation];
            dat.SclWidth = (float)datFrames[0].Width / poly.Width;
            dat.SclHeight = (float)datFrames[0].Height / poly.Height;
            dat.Offset = new OffsetData(data.DrawArea.Width, data.DrawArea.Height, new Microsoft.Xna.Framework.Vector2(data.DrawArea.X, data.DrawArea.Y));
            dat.Loop = false;
            dat.Reverse = data.Reverse;
            dat.ImageName = data.Animation;
            dat.FPS = data.FPS;
            return new Animation(dat);
        }

        public static string CreateResourceDescription(ResourceData data)
        {
            string str = "";
            if (data.Metal != 0)
            {
                str += "Metal, ";
            }
            if (data.Crystal != 0)
            {
                str += "Crystal, ";
            }
            return str.TrimEnd(new char[]{' ',','});
        }

        public static ActionType[] CreateActionTypes(int[] types)
        {
            ActionType[] acTypes = new ActionType[types.Length];
            for (int i = 0; i < types.Length; i++)
            {
                acTypes[i] = (ActionType)types[i];
            }
            return acTypes;
        }

        public static Effects.EffectData CreateEffectData(EffectTypeInfo info)
        {
            Effects.EffectData data = new Effects.EffectData();
            data.Image = info.Image;
            data.Type = info.Type;
            return data;
        }
    }
}
