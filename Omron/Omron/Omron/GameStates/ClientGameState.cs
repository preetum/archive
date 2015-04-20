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

using Omron.Helpers;

using System.Threading;

using Lidgren.Network;
using Lidgren.Network.Xna;
using Omron.Effects;

namespace Omron.GameStates
{
    public partial class ClientGameState : GameState
    {
        enum ClientState
        {
            Waiting,
            Started
        }

        ClientState clientState = ClientState.Waiting;

        GameLoopTimer clientT;
        public ClientGameState(NetClient client, byte usize, byte vsize, float sideLen, Faction playerF, Faction worldF, IEnumerable<Faction> otherFactions, Vector2 basePos)
        {
            //ResourceManager.GraphicsDeviceManager.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            //ResourceManager.GraphicsDeviceManager.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            //ResourceManager.GraphicsDeviceManager.IsFullScreen = true;
            //ResourceManager.GraphicsDeviceManager.ApplyChanges();
            
            this.client = client;

            this.usize = usize;
            this.vsize = vsize;

            PlayerF = playerF;
            WorldF = worldF;
            facRegister = otherFactions.ToDictionary<Faction, byte>(fac => fac.ID);
            facRegister.Add(PlayerF.ID, playerF);
            facRegister.Add(WorldF.ID, worldF);

            if (playerF.FactionType == PlayerType.Human)
            {
                PlayerF.BuildingsUnlocked.AddRange(ResourceManager.Resources["Settings"]["StartHumanBuildings"]);
            }
            else
            {
                PlayerF.BuildingsUnlocked.AddRange(ResourceManager.Resources["Settings"]["StartWumpusBuildings"]);
            }

            cWorld = new World(usize, vsize, sideLen);

            clientT = new GameLoopTimer(1000d / 60d);
            clientT.Update += new Action<GameTime>(clientT_Update);
            clientT.Start();
        }

        NetClient client;

        int usize, vsize;
        NetworkUI gameUI;

        World cWorld;

        HashSet<ushort> bannedIDs = new HashSet<ushort>();

        Faction PlayerF, WorldF;
        Dictionary<byte, Faction> facRegister;

        SpriteBatch spriteBatch;

        bool gameOver = false;
        
        public override void Init()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //get entire cWorld -- run only once
            NetOutgoingMessage om = client.CreateMessage();
            om.Write((byte)MessageDataType.SendClientPregameData);
            client.SendMessage(om, NetDeliveryMethod.ReliableUnordered);

            gameUI = new NetworkUI(this, cWorld, PlayerF);
            gameUI.SetSendPeace(new NetworkUI.sendActorDelegate(sendActorsMsg));
            gameUI.SetSendEngage(new NetworkUI.actorActorDelegate(engageActorMsg));
            gameUI.SetSendBuild(new NetworkUI.actorActorDelegate(sendActorsToBuild));
            gameUI.SetSendAggressive(new NetworkUI.sendActorDelegate(sendActorsAggressiveMsg));
            gameUI.SetConstruct(new NetworkUI.constructDelegate(requestConstructMsg));
            gameUI.SetItemPressed(new NetworkUI.itemPressedDelegate(sendMenuItem));
        }

        void clientT_Update(GameTime obj)
        {
            NetIncomingMessage msg;
            //try
            //{
            while ((msg = client.ReadMessage()) != null)
            {
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.DiscoveryResponse:
                        //connect to first discovered server
                        client.Connect(msg.SenderEndpoint);
                        break;
                    case NetIncomingMessageType.Data:

                        ushort id;
                        string type;
                        byte fid;
                        Actor actor;

                        switch ((MessageDataType)msg.ReadByte())
                        {
                            case MessageDataType.GameStarting:
                                clientState = ClientState.Started;
                                break;

                            case MessageDataType.ActorCreated:
                                id = msg.ReadUInt16();
                                type = msg.ReadString();
                                fid = msg.ReadByte();

                                var f = getFactionFromID(fid);

                                if (cWorld.GetActorByID(id) == null && !bannedIDs.Contains(id) && f != null)
                                {
                                    actor = UnitConverter.CreateActor(type, Vector2.Zero, f);
                                    actor.ReadInInitialData(msg);

                                    cWorld.AddActor(actor, id);
                                }

                                break;

                            case MessageDataType.ActorUpdate:
                                id = msg.ReadUInt16();

                                if ((actor = cWorld.GetActorByID(id)) != null)
                                {
                                    actor.ReadInUpdateData(msg);
                                }
                                break;
                            case MessageDataType.ActorChangeUpdate:
                                id = msg.ReadUInt16();

                                if ((actor = cWorld.GetActorByID(id)) != null)
                                {
                                    (actor as IChangeUpdate).ReadInChangeUpdateData(msg);
                                }
                                break;
                            case MessageDataType.ActorPushUpdate:
                                id = msg.ReadUInt16();

                                if ((actor = cWorld.GetActorByID(id)) != null)
                                {
                                    (actor as IPushUpdate).ReadInPushUpdateData(msg);
                                }
                                break;

                            case MessageDataType.ActorDestroyed:
                                id = msg.ReadUInt16();
                                if ((actor = cWorld.GetActorByID(id)) != null)
                                {
                                    cWorld.RemoveActor(actor);
                                }
                                else
                                {
                                    bannedIDs.Add(id); //prevent actor from being created if creation msg arrives in the future
                                }
                                break;
                            case MessageDataType.ResourceUpdate:
                                PlayerF.Resources.Metal = msg.ReadFloat();
                                PlayerF.Resources.Crystal = msg.ReadFloat();
                                break;

                            case MessageDataType.PushEffect:
                                type = msg.ReadString();
                                GraphicsEffect effect = EffectFactory.CreateEffect(type);
                                effect.world = cWorld;
                                effect.ReadInInitialData(msg);
                                cWorld.PushEffect(effect);
                                break;

                            case MessageDataType.ResearchUnlocked:
                                var res = msg.ReadString();
                                PlayerF.UnlockResearch(res);
                                break;

                            case MessageDataType.BuildingUnlocked:
                                var build = msg.ReadString();
                                PlayerF.UnlockBuilding(build);
                                break;
                            case MessageDataType.GameOver:
                                gameOver = true;
                                this.clientT.Stop();
                                Console.WriteLine("Game over broham");
                                break;
                        }

                        break;
                }

            }
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("ERROR!! " + ex);
            //}
        }

        Faction getFactionFromID(byte fid)
        {
            if (facRegister.ContainsKey(fid))
                return facRegister[fid];
            else
                return null;
        }

        public override void Update(GameTime gameTime)
        {
            cWorld.UpdateEffectsOnly(gameTime);
            gameUI.Update(gameTime);
        }

        void sendMenuItem(ushort actId, int itemKey)
        {
            NetOutgoingMessage om = client.CreateMessage();
            om.Write((byte)MessageDataType.ActorMenuCommand);
            om.Write(actId);
            om.Write((byte)itemKey);
            client.SendMessage(om, NetDeliveryMethod.ReliableUnordered);
        }

        void sendActorsMsg(ushort[] actorIDs, Vector2 targ)
        {
            NetOutgoingMessage om = client.CreateMessage();
            om.Write((byte)MessageDataType.SendActors);

            om.Write(targ);
            om.Write((ushort)actorIDs.Length);
            foreach (ushort id in actorIDs)
                om.Write(id);

            client.SendMessage(om, NetDeliveryMethod.ReliableUnordered);
        }

        void sendActorsAggressiveMsg(ushort[] actorIDs, Vector2 targ)
        {
            NetOutgoingMessage om = client.CreateMessage();
            om.Write((byte)MessageDataType.SendActorsAggressive);

            om.Write(targ);
            om.Write((ushort)actorIDs.Length);
            foreach (ushort id in actorIDs)
                om.Write(id);

            client.SendMessage(om, NetDeliveryMethod.ReliableUnordered);
        }

        void sendActorsToBuild(ushort[] actorIDs, ushort buildingID)
        {
            NetOutgoingMessage om = client.CreateMessage();
            om.Write((byte)MessageDataType.SendActorsToBuild);

            om.Write(buildingID);
            om.Write((ushort)actorIDs.Length);
            foreach (ushort id in actorIDs)
                om.Write(id);

            client.SendMessage(om, NetDeliveryMethod.ReliableUnordered);
        }

        void engageActorMsg(ushort[] actorIDs, ushort engagement)
        {
            NetOutgoingMessage om = client.CreateMessage();
            om.Write((byte)MessageDataType.EngageActor);

            om.Write(engagement);
            om.Write((ushort)actorIDs.Length);
            foreach (ushort id in actorIDs)
                om.Write(id);

            client.SendMessage(om, NetDeliveryMethod.ReliableUnordered);
        }

        void requestConstructMsg(string type, Vector2 pos, float rot)
        {
            NetOutgoingMessage om = client.CreateMessage();
            om.Write((byte)MessageDataType.RequestBuildingConstruct);
            om.Write(type);
            om.Write(pos);
            om.Write(rot);
            client.SendMessage(om, NetDeliveryMethod.ReliableUnordered);
        }

        public override void Draw(GameTime gameTime)
        {
            if (!gameOver)
                gameUI.Draw(spriteBatch);
            else
            {
                spriteBatch.Begin();
                string mes = "Your faction " + (PlayerF.FactionWon.HasValue && !PlayerF.FactionWon.Value ? "lost!" : "won!");
                spriteBatch.DrawString(ResourceManager.Resources["font1"], mes, new Vector2(300), Color.Red);
                spriteBatch.End();
            }
        }
    }
}
