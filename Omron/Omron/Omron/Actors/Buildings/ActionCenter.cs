using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;

using UnitsAndBuilduings;
using Omron.Framework.Networking;

using Microsoft.Xna.Framework;
using Omron.Framework;

using Omron.Helpers;

namespace Omron.Actors
{
    public enum ActionType
    {
        unit,
        building,
        unlock
    }

    class ActionCenter: FatherBuilding, IFullSynchronize, IPushUpdate
    {//factory unit
        public ActionCenter(BuilduingTypeInfo info, string type, Vector2 loc, Faction f)
            : base(info, type, loc)
        {
            if ((info.SubTypeCosts.Length != info.SubTypes.Length) || (info.ActionTimes.Length != info.SubTypes.Length))
                throw new Exception("Unit Subtypes must have the same length as SubTypeCosts and as ActionTimes!!");
            this.Faction = f;

            elapsedTime = 0;
            
            queue = new Queue<byte>();
            createActions(info);
            createMenu();
            createDefaultRally();
        }

        #region Variables
        Vector2 rally;
        List<ActionType> actionTypes;
        List<float> actionTimes;
        List<string> actions;
        List<ResourceData> costs;
        List<string> unlocksNeeded;
        List<byte> menuConv;

        public float elapsedTime;
        ActorMenu menu;
        public Queue<byte> queue;
        #endregion

        #region create/update menu
        void createActions(BuilduingTypeInfo info)
        {
            actionTimes = new List<float>();
            actions = new List<string>();
            costs = new List<ResourceData>();
            actionTypes = new List<ActionType>();
            unlocksNeeded = new List<string>();

            for (int i = 0; i < info.SubTypes.Length; i++)
            {
                actionTimes.Add(info.ActionTimes[i]);
                actions.Add(info.SubTypes[i]);
                costs.Add(UnitConverter.CreateResourceData(info.SubTypeCosts[i]));
                actionTypes.Add((ActionType)info.ActionTypes[i]);
                unlocksNeeded.Add(info.UnlocksNeeded[i]);
            }
            //by the way, if there is an error here check your xml
        }

        void createMenu()
        {
            menuConv = new List<byte>();//action[menuConv[i]] is what the i'th button does

            for (byte i = 0; i < actions.Count; i++)
            {
                if ((unlocksNeeded[i] == "") || (Faction.ResearchUnlocked.Contains(unlocksNeeded[i])))
                {
                    if (actionTypes[i] == ActionType.unit)
                    {//unit && unit is unlocked
                        menuConv.Add(i);
                    }
                    else if (actionTypes[i] == ActionType.building)
                    {
                        if (!Faction.BuildingsUnlocked.Contains(actions[i]))
                        {
                            menuConv.Add(i);
                        }
                    }
                    else if (actionTypes[i] == ActionType.unlock)
                    {
                        if (!Faction.ResearchUnlocked.Contains(actions[i]))
                        {
                            menuConv.Add(i);
                        }
                    }
                }
            }

            string[] menuStrs = new string[menuConv.Count];
            for (byte i = 0; i < menuStrs.Length; i++)
            {
                int curInd = menuConv[i];
                menuStrs[i] = actions[curInd] + "\n" + createResourceDesc(costs[curInd]);
            }
            menu = new ActorMenu(this, menuStrs);
            menu.CommandInvoked += new MenuItemPressed(menu_ItemClicked);
            OnUpdateMenu(this);
        }

        string createResourceDesc(ResourceData data)
        {
            string str = "";
            if (data.Metal != 0)
                str += "Metal: " + data.Metal + "\n";
            if (data.Crystal != 0)
                str += "Crystal: " + data.Crystal;
            return str;
        }

        void updateMenu()
        {
            if (queue.Count > 0)
            {
                menu.BarValue = calcBarVal();
                menu.DisplayBar = true;

                menu.Info = "cue: ";
                int counter = 2;
                foreach (byte i in queue.CloneToList())
                {
                    counter--;
                    if (counter == 0)
                    {
                        counter = 3;
                        menu.Info += "\n";
                    }
                    menu.Info += actions[i] + ", ";
                }
                menu.Info.TrimEnd(new char[] { ' ', ',' });
            }
            else
            {
                menu.Info = "";
                menu.DisplayBar = false;
            }
        }

        float calcBarVal()
        {
            return (float)elapsedTime / actionTimes[queue.Peek()];
        }
        
        public override void MenuNeedsUpdate() 
        {
            base.MenuNeedsUpdate();
            createMenu();
        }
        #endregion

        public void SetRally(Vector2 loc)
        {
            if (CollisionTester.TestPointInside(loc, this.polygon))
            {
                createDefaultRally();
            }
            else
            {
                rally = loc;
            }
        }

        void createDefaultRally()
        {
            rally = this.Position + this.MaxRadius * Vector2.UnitX;
        }

        #region input revieved
        void menu_ItemClicked(int itemKey)
        {
            Build(menuConv[itemKey]);
        }

        public bool Build(int itemKey)
        {
            if (Faction.Resources >= costs[itemKey])
            {
                if (actionTypes[itemKey] != ActionType.unit)
                {
                    if (queue.Contains((byte)itemKey))
                    {
                        return false;
                    }
                }
                Faction.Resources -= costs[itemKey];
                queue.Enqueue((byte)itemKey);
                pushEnqueue((byte)itemKey);
                return true;
            }
            return false;
        }
        #endregion

        #region networking
        public override void WriteOutUpdateData(Lidgren.Network.NetOutgoingMessage om)
        {
            base.WriteOutUpdateData(om);


            om.Write(this.elapsedTime);
        }

        public override void ReadInUpdateData(Lidgren.Network.NetIncomingMessage im)
        {
            base.ReadInUpdateData(im);

            elapsedTime = im.ReadFloat();

            updateMenu();
        }

        enum NetworkActionType : byte
        {
            Enqueue,
            Dequeue
        }
        void pushEnqueue(byte data) 
        {
            NetOutgoingMessage update = stage.CreateServerOM();
            if (update != null)
            {
                update.Write((byte)NetworkActionType.Enqueue);
                update.Write(data);
                stage.PushUpdateActor(this, update);
            }
        }
        void pushDequeue()
        {
            NetOutgoingMessage update = stage.CreateServerOM();
            if (update != null)
            {
                update.Write((byte)NetworkActionType.Dequeue);
                stage.PushUpdateActor(this, update);
            }
        }
        public void ReadInPushUpdateData(NetIncomingMessage im)
        {
            switch ((NetworkActionType)im.ReadByte())
            {
                case NetworkActionType.Dequeue:
                    if (queue.Count > 0)
                        queue.Dequeue();
                    break;
                case NetworkActionType.Enqueue:
                    queue.Enqueue(im.ReadByte());
                    break;
            }
            updateMenu();
        }
        #endregion
        
        public override ActorMenu Menu
        {
            get 
            {
                if (IsComplete)
                    return menu;
                return base.Menu;
            }
        }

        public override void UpdateFast(GameTime gameTime)
        {
            base.UpdateFast(gameTime);

            if (queue.Count != 0)
            {
                SetBaseAnimation(WorkAnimation);

                elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                var top = queue.Peek();
                if (elapsedTime >= actionTimes[top])
                {
                    elapsedTime -= actionTimes[top];
                    queue.Dequeue();
                    doAction(top);
                    pushDequeue();
                }
            }
            else if (IsComplete)
            {
                SetBaseAnimation(IdleAnimation);
            }
            updateMenu();
        }

        void doAction(int action)
        {
            if (actionTypes[action] == ActionType.unit)
            {
                Actor act = UnitConverter.CreateActor(actions[action], Position, this.Faction);
                Vector2 actOffset = Vector2.Normalize(rally - Position) * this.polygon.MaxRadius;
                (act as FatherUnit).Track(rally);
                act.Position += actOffset;
                stage.AddActor(act);
            }
            else if (actionTypes[action] == ActionType.building)
            {
                Faction.UnlockBuilding(actions[action]);
                createMenu();
            }
            else
            {
                Faction.UnlockResearch(actions[action]);
                createMenu();
            }
        }
    }
}
