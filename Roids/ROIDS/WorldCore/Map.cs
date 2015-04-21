using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorldCore
{
    public class Map
    {
        public IPlayer Player { get; private set; }
        public List<WorldObject> WorldObjects { get; private set; }
        public List<IDynamicObject> DynamicObjects { get; private set; }



        public Map(IList<WorldObject> objects)
        {
            WorldObjects = new List<WorldObject>();
            DynamicObjects = new List<IDynamicObject>();
            _removalList = new List<WorldObject>();
            foreach (WorldObject obj in objects)
            {
                if (obj != null)
                {
                    WorldObjects.Add(obj);
                    if (obj is IDynamicObject)
                        DynamicObjects.Add((IDynamicObject)obj);
                    if (obj is IPlayer)
                        Player = (IPlayer)obj;
                }
            }
            //if (Player == null)
            //    throw new Exception("Map has no player");
        }

        public void AddObject(WorldObject obj)
        {
            WorldObjects.Add(obj);
            if (obj is IPlayer)
                Player = (IPlayer)obj;
            if (obj is IDynamicObject)
                DynamicObjects.Add((IDynamicObject)obj);

        }

        public void UpdateDynamicObjects(Microsoft.Xna.Framework.GameTime time)
        {
            _removalList.ForEach(
                obj =>
                {
                    if (obj is IPlayer)
                        throw new Exception("Cannot Destroy Player");
                    if (obj is IDynamicObject)
                        DynamicObjects.Remove((IDynamicObject)obj);
                    WorldObjects.Remove(obj);
                });
            _removalList.Clear();
            DynamicObjects.ForEach(obj => obj.Update(time));
        }

        List<WorldObject> _removalList;
        /// <summary>
        /// Safely Destroy Even in ForEach
        /// </summary>
        /// <param name="obj"></param>
        public void SafelyRemoveObject(WorldObject obj)
        {
            _removalList.Add(obj);
        }
    }
}
