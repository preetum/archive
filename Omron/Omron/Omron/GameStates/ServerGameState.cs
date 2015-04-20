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


namespace Omron.GameStates
{
    public class ServerGameState : GameState
    {
        public ServerGameState() { }

        World world;
        NetServer server;

        List<Faction> playerFactions;

        Vector2 mapBoundsXY;

        enum ServerState
        {
            Lobby,
            Finalized,
            Started,
            Ended
        }
        ServerState serverState = ServerState.Lobby;

        GameLoopTimer motionT, fastT, slowT, serverT;
        public override void Init()
        {
            NetPeerConfiguration config = new NetPeerConfiguration("omron");
            config.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);
            config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
            config.Port = 9001;
            /*config.SimulatedLoss = 0.01f;
            config.SimulatedMinimumLatency = 0.003f;
            config.SimulatedRandomLatency = 0.010f;*/
            server = new NetServer(config);


            world = new World(50, 50, 1.0f);
            world.server = server;
            world.NotifyUpdatePushed += new UpdatePushed(world_NotifyUpdatePushed);         
            world.NotifyEffectPushed += new EffectPushed(world_NotifyEffectPushed);
            


            motionT = new GameLoopTimer(1000d / 60d);
            motionT.Update += new Action<GameTime>(motionT_Update);

            fastT = new GameLoopTimer(1000d / 60d);
            fastT.Update += new Action<GameTime>(fastT_Update);

            slowT = new GameLoopTimer(1000d / 2d);
            slowT.Update += new Action<GameTime>(slowT_Update);



            MapGenerator.renewSeed();
            MapGenerator.GeneratePerlinMap(world);


            mapBoundsXY = world.TileGrid.UVToScreen(new Point(world.TileGrid.U_length - 1, world.TileGrid.V_length - 1));
            var cent = 0.5f * mapBoundsXY;


            playerFactions = new List<Faction>();

            //setupBase(cent, playerF);
            //setupBase(cent + Vector2.One * 10f, wumpusF);


            //networking
            

            server.Start();

            serverT = new GameLoopTimer(1000d / 60d);
            serverT.Update += new Action<GameTime>(serverT_Update);
            serverT.Start();

            startAllWorldTimers();
        }

        TimeSpan startDelay = new TimeSpan(0, 0, 0, 0, 10000); 
        DateTime gameFinalizedTime;
        //time to wait between game finalized and game start
        void serverT_Update(GameTime obj)
        {
            //handle messages client->server
            NetIncomingMessage msg;
            //try
            //{
            while ((msg = server.ReadMessage()) != null)
            {
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.ConnectionApproval:
                        if (serverState == ServerState.Lobby)
                            msg.SenderConnection.Approve();
                        else
                            msg.SenderConnection.Deny("sorry, this server has finalized");
                        break;
                    case NetIncomingMessageType.DiscoveryRequest:
                        //
                        // Server received a discovery request from a client; send a discovery response (with no extra data attached)
                        //
                        if (serverState == ServerState.Lobby)
                            server.SendDiscoveryResponse(null, msg.SenderEndpoint);
                        break;
                    case NetIncomingMessageType.VerboseDebugMessage:
                    case NetIncomingMessageType.DebugMessage:
                    case NetIncomingMessageType.WarningMessage:
                    case NetIncomingMessageType.ErrorMessage:
                        //
                        // Just print diagnostic messages to console
                        //
                        Console.WriteLine(msg.ReadString());
                        break;
                    case NetIncomingMessageType.StatusChanged:
                        NetConnectionStatus status = (NetConnectionStatus)msg.ReadByte();
                        if (status == NetConnectionStatus.Connected)
                        {
                            //
                            // A new player just connected!
                            //
                            Console.WriteLine(NetUtility.ToHexString(msg.SenderConnection.RemoteUniqueIdentifier) + " connected!");
                        }

                        break;
                    case NetIncomingMessageType.Data:
                        //
                        // The client sent input to the server
                        //

                        NetOutgoingMessage om;
                        ushort num;
                        ushort id;
                        Vector2 targ;

                        switch ((MessageDataType)msg.ReadByte())
                        {
                            case MessageDataType.RequestFaction:
                                PlayerType pType = (PlayerType)msg.ReadByte();

                                byte factionID = (byte)MathHelper.GetUniqueID();

                                Faction newFaction = new Faction(factionID, world);
                                newFaction.Resources.Metal = 0;
                                newFaction.Resources.Crystal = 0;
                                newFaction.SetVictory(ResourceManager.Resources["Conquest"]);
                                foreach (var fact in playerFactions)
                                    fact.Register(newFaction, FactionRelationship.Hostile);
                                playerFactions.Add(newFaction);

                                newFaction.NotifyBuildingUnlocked += new BuildingUnlocked(newFaction_NotifyBuildingUnlocked);
                                newFaction.NotifyResearchUnlocked += new ResearchUnlocked(newFaction_NotifyResearchUnlocked);

                                msg.SenderConnection.Tag = factionID; //store the factionID in the connection

                                var centUV = new Point(world.TileGrid.U_length / 2, world.TileGrid.V_length / 2);
                                //int rad = 10;
                                //var dispUV = new Point(MathHelper.Rand.Next(-rad, rad + 1), MathHelper.Rand.Next(-rad, rad + 1));
                                //var posUV = new Point(centUV.X + dispUV.X, centUV.Y + dispUV.Y);

                                var basePos = world.TileGrid.UVToScreen(new Point(world.TileGrid.U_length / 4 + MathHelper.Rand.Next(world.TileGrid.U_length / 2), world.TileGrid.V_length / 4 + MathHelper.Rand.Next(world.TileGrid.V_length / 2))); //try to position closer to center than edges
                                //var basePos = world.TileGrid.UVToScreen(posUV);
                                //var basePos = world.TileGrid.UVToScreen(new Point(MathHelper.Rand.Next(world.TileGrid.U_length), MathHelper.Rand.Next(world.TileGrid.V_length)));
                                setupBase(basePos, pType, newFaction); //set up a base

                                NetOutgoingMessage resp = server.CreateMessage();
                                resp.Write((byte)MessageDataType.FactionRegistered);
                                resp.Write((byte)pType);
                                resp.Write(factionID);
                                resp.Write(basePos);

                                server.SendMessage(resp, msg.SenderConnection, NetDeliveryMethod.ReliableUnordered); //send factionID
                                break;
                            case MessageDataType.RequestFinalize:

                                if (serverState == ServerState.Lobby)
                                {
                                    //do any last-minute things (maybe something with the map?)

                                    serverState = ServerState.Finalized;
                                    gameFinalizedTime = DateTime.Now;

                                    foreach (var player in server.Connections)
                                    {
                                        om = server.CreateMessage();
                                        om.Write((byte)MessageDataType.GameFinalized);

                                        //send final world infos
                                        om.Write((byte)world.TileGrid.U_length);
                                        om.Write((byte)world.TileGrid.V_length);
                                        om.Write(world.TileGrid.HexSideLen);

                                        om.Write(world.Faction.ID);

                                        Faction pfact = getFactionByID((byte)player.Tag);
                                        var hostiles = pfact.GetAllFactions(FactionRelationship.Hostile);
                                        var allies = pfact.GetAllFactions(FactionRelationship.Allied);

                                        //send all hostile factionIDs
                                        om.Write((byte)hostiles.Count());
                                        foreach (var f in hostiles)
                                            om.Write(f.ID);

                                        //send all allied factionIDs
                                        om.Write((byte)allies.Count());
                                        foreach (var f in allies)
                                            om.Write(f.ID);

                                        server.SendMessage(om, player, NetDeliveryMethod.ReliableUnordered);
                                    }

                                    world.NotifyActorRemoved += new ActorRemoved(world_NotifyActorRemoved); //only subscribe to actorremove handler once game has been finalized (all bases and map has been set up)
                                    world.NotifyActorAdded += new ActorRemoved(world_NotifyActorAdded);


                                    Console.WriteLine("GAME FINALIZED!");
                                    Console.WriteLine("will start in " + startDelay.TotalSeconds + " seconds");
                                }
                                break;


                            case MessageDataType.SendClientPregameData:

                                //send entire world to sender
                                sendEntireWorld(msg.SenderConnection);

                                break;
                            case MessageDataType.SendActors:
                                targ = msg.ReadVector2();
                                num = msg.ReadUInt16();

                                for (ushort i = 0; i < num; i++)
                                {
                                    id = msg.ReadUInt16();
                                    Actor actor = world.GetActorByID(id);
                                    if (actor != null)
                                        (actor as FatherUnit).Track(targ);
                                }

                                break;

                            case MessageDataType.SendActorsAggressive:
                                targ = msg.ReadVector2();
                                num = msg.ReadUInt16();

                                for (ushort i = 0; i < num; i++)
                                {
                                    id = msg.ReadUInt16();
                                    Actor actor = world.GetActorByID(id);
                                    if (actor != null)
                                        (actor as FatherUnit).MayhamTrack(targ);
                                }

                                break;

                            case MessageDataType.SendActorsToBuild:
                                FatherBuilding targActor = world.GetActorByID(msg.ReadUInt16()) as FatherBuilding;
                                num = msg.ReadUInt16();

                                for (ushort i = 0; i < num; i++)
                                {
                                    id = msg.ReadUInt16();
                                    Actor actor = world.GetActorByID(id);
                                    if (actor != null)
                                        (actor as FatherUnit).AI.Build(targActor);
                                }

                                break;

                            case MessageDataType.EngageActor:
                                ushort engageID = msg.ReadUInt16();
                                Actor engagement = world.GetActorByID(engageID);

                                if (engagement != null)
                                {
                                    num = msg.ReadUInt16();
                                    for (ushort i = 0; i < num; i++)
                                    {
                                        id = msg.ReadUInt16();
                                        Actor actor = world.GetActorByID(id);
                                        if (actor != null)
                                            (actor as FatherUnit).AI.Engage(engagement);
                                    }
                                }
                                break;

                            case MessageDataType.ActorMenuCommand:
                                id = msg.ReadUInt16();
                                int item = (int)msg.ReadByte();

                                world.GetActorByID(id).Menu.OnCommandInvoked(item);

                                break;

                            case MessageDataType.RequestBuildingConstruct:
                                string type = msg.ReadString();
                                Vector2 pos = msg.ReadVector2();
                                float rot = msg.ReadFloat();

                                Faction faction = getFactionByID((byte)msg.SenderConnection.Tag);
                                Actor newactor = UnitConverter.CreateActor(type, pos, faction);
                                newactor.Rotation = rot;
                                world.AddActor(newactor);
                                faction.Resources -= UnitConverter.CreateResourceData(ResourceManager.Resources[newactor.Type].Cost);

                                break;
                        }

                        break;
                }
                server.Recycle(msg);
            }
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("ERROR!! " + ex);
            //}


            if (serverState == ServerState.Finalized && (DateTime.Now - gameFinalizedTime) > startDelay)
            {
                serverState = ServerState.Started; //start the game after a delay
                Console.WriteLine("STARTING GAME!");

                NetOutgoingMessage om = server.CreateMessage();
                om.Write((byte)MessageDataType.GameStarting);
                server.SendToAll(om, NetDeliveryMethod.ReliableUnordered);
            }

            //broadcast updates, etc from server->client

            if (serverState == ServerState.Started)
            {
                foreach (var player in server.Connections)
                {
                    sendResourcesMsg(getFactionByID((byte)player.Tag), player);
                }

                foreach (var actor in world.GetActors())
                {
                    if ((actor is IChangeUpdate && (actor as IChangeUpdate).IsInvalid))
                    {
                        NetOutgoingMessage om = server.CreateMessage();
                        om.Write((byte)MessageDataType.ActorChangeUpdate);

                        writeUpdateActor(actor, om);

                        server.SendToAll(om, NetDeliveryMethod.ReliableUnordered);
                        (actor as IChangeUpdate).IsInvalid = false;
                    }
                    if (actor is IFullSynchronize)
                    {
                        NetOutgoingMessage om = server.CreateMessage();
                        om.Write((byte)MessageDataType.ActorUpdate);

                        writeUpdateActor(actor, om);

                        server.SendToAll(om, NetDeliveryMethod.Unreliable);
                    }
                }
            }
            if (serverState == ServerState.Ended)
            {
                Console.WriteLine("GAME OVER!");
                foreach (var player in server.Connections)
                {
                    sendGameOverMsg(getFactionByID((byte)player.Tag), player);
                }
                server.Shutdown("bye!");
                this.serverT.Stop();
            }
        }

        void startAllWorldTimers()
        {
            slowT.Start();
            fastT.Start();
            motionT.Start();
        }

        void slowT_Update(GameTime obj)
        {
            if (serverState != ServerState.Started) return;

            int deadF = 0;
            foreach (Faction f in playerFactions)
            {
                f.SlowUpdate(obj);
                if (f.FactionWon != null)
                {
                    if (f.FactionWon.Value)
                    {
                        //display victory of winning faction
                        //serverState = ServerState.Ended;
                    }
                    else
                        deadF++;
                }
            }
            if (deadF + 1 >= playerFactions.Count)
            {
                //serverState = ServerState.Ended;
            }
        }

        void fastT_Update(GameTime obj)
        {
            if (serverState != ServerState.Started) return;

            world.UpdateActorFastOnly(obj);
            world.UpdateEffectsOnly(obj);
        }

        void motionT_Update(GameTime obj)
        {
            if (serverState != ServerState.Started) return;

            world.UpdateMotionOnly(obj);
        }


        void setupBase(Vector2 basePos, PlayerType pType, Faction faction)
        {
            /*var factory = UnitConverter.CreateActor("Factory", basePos, faction);
            world.AddActor(factory);

            float fClear = 7f;

            var factSurround = new RectPoly(basePos, fClear, fClear, 0f);
            var acts = world.Query(factSurround);
            foreach (var actor in acts)
            {
                if (actor is Wall || actor is Resource)
                    actor.Die();
            }


            for (int i = 0; i < 10; i++)
            {
                var pos = basePos + Vector2.UnitX * ((float)MathHelper.Rand.NextDouble() * 2 - 1f) * (fClear / 2) + Vector2.UnitY * ((float)MathHelper.Rand.NextDouble() * 2 - 1f) * (fClear / 2);
                world.AddActor(UnitConverter.CreateActor("Troy", pos, faction));
            }*/
            float fClear = 7f;

            var factSurround = new RectPoly(basePos, fClear, fClear, 0f);
            var acts = world.Query(factSurround);
            foreach (var actor in acts)
            {
                if (actor is Wall || actor is Resource)
                    actor.Die();
            }
            if (pType == PlayerType.Human)
            {
                foreach (string act in ResourceManager.Resources["Settings"]["StartHumanUnits"])
                {
                    var pos = basePos + Vector2.UnitX * ((float)MathHelper.Rand.NextDouble() * 2 - 1f) * (fClear / 2) + Vector2.UnitY * ((float)MathHelper.Rand.NextDouble() * 2 - 1f) * (fClear / 2);
                    world.AddActor(UnitConverter.CreateActor(act, pos, faction));
                }
            }
            else
            {
                foreach (string act in ResourceManager.Resources["Settings"]["StartWumpusUnits"])
                {
                    var pos = basePos + Vector2.UnitX * ((float)MathHelper.Rand.NextDouble() * 2 - 1f) * (fClear / 2) + Vector2.UnitY * ((float)MathHelper.Rand.NextDouble() * 2 - 1f) * (fClear / 2);
                    world.AddActor(UnitConverter.CreateActor(act, pos, faction));
                }
            }
        }

        Faction getFactionByID(byte id)
        {
            return playerFactions.FirstOrDefault(f => f.ID == id);
        }
        NetConnection getPlayerByFaction(Faction fact)
        {
            return server.Connections.Find(c => (byte)c.Tag == fact.ID);
        }

        void sendEntireWorld(NetConnection player)
        {
            foreach (var actor in world.GetActors())
            {
                NetOutgoingMessage om = server.CreateMessage();
                om.Write((byte)MessageDataType.ActorCreated);

                writeCreateActor(actor, om);

                server.SendMessage(om, player, NetDeliveryMethod.ReliableUnordered);
            }
        }
        void sendEntireWorldToAll()
        {
            foreach (var actor in world.GetActors())
            {
                NetOutgoingMessage om = server.CreateMessage();
                om.Write((byte)MessageDataType.ActorCreated);

                writeCreateActor(actor, om);

                server.SendToAll(om, NetDeliveryMethod.ReliableUnordered);
            }
        }

        void newFaction_NotifyResearchUnlocked(Faction sender, string res)
        {
            NetOutgoingMessage om = server.CreateMessage();
            om.Write((byte)MessageDataType.ResearchUnlocked);
            om.Write(res);
            var player = getPlayerByFaction(sender);
            server.SendMessage(om, player, NetDeliveryMethod.ReliableUnordered);
        }

        void newFaction_NotifyBuildingUnlocked(Faction sender, string building)
        {
            NetOutgoingMessage om = server.CreateMessage();
            om.Write((byte)MessageDataType.BuildingUnlocked);
            om.Write(building);
            var player = getPlayerByFaction(sender);
            server.SendMessage(om, player, NetDeliveryMethod.ReliableUnordered);
        }

        void sendResourcesMsg(Faction faction, NetConnection player)
        {
            NetOutgoingMessage om = server.CreateMessage();
            om.Write((byte)MessageDataType.ResourceUpdate);
            om.Write(faction.Resources.Metal);
            om.Write(faction.Resources.Crystal);
            server.SendMessage(om, player, NetDeliveryMethod.ReliableUnordered);
        }

        void sendGameOverMsg(Faction fac, NetConnection player)
        {
            NetOutgoingMessage om = server.CreateMessage();
            om.Write((byte)MessageDataType.GameOver);
            server.SendMessage(om, player, NetDeliveryMethod.ReliableUnordered);
        }

        void world_NotifyUpdatePushed(Actor actor, NetOutgoingMessage update)
        {
            NetOutgoingMessage om = server.CreateMessage();
            om.Write((byte)MessageDataType.ActorPushUpdate);
            om.Write(actor.ActorID);
            om.Write(update);
            server.SendToAll(om, NetDeliveryMethod.ReliableUnordered);
        }
        void world_NotifyEffectPushed(Effects.GraphicsEffect effect)
        {
            NetOutgoingMessage om = server.CreateMessage();
            om.Write((byte)MessageDataType.PushEffect);
            om.Write(effect.Type);
            effect.WriteOutInitialData(om);
            server.SendToAll(om, NetDeliveryMethod.ReliableUnordered);
        }

        void world_NotifyActorAdded(Actor actor)
        {
            NetOutgoingMessage om = server.CreateMessage();
            om.Write((byte)MessageDataType.ActorCreated);
            writeCreateActor(actor, om);
            server.SendToAll(om, NetDeliveryMethod.ReliableUnordered);
        }

        void world_NotifyActorRemoved(Actor actor)
        {
            NetOutgoingMessage om = server.CreateMessage();
            om.Write((byte)MessageDataType.ActorDestroyed);
            om.Write(actor.ActorID);
            server.SendToAll(om, NetDeliveryMethod.ReliableUnordered);
        }

        void writeCreateActor(Actor actor, NetOutgoingMessage om)
        {
            //standard send required to create actors
            om.Write(actor.ActorID);
            om.Write(actor.Type);
            om.Write(actor.Faction.ID);

            //specific to actors, once created
            actor.WriteOutInitialData(om);
        }
        void writeUpdateActor(Actor actor, NetOutgoingMessage om)
        {
            //required to identify actor
            om.Write(actor.ActorID);

            //specific to actors, once created
            actor.WriteOutUpdateData(om);
        }
        void writeChangeUpdateActor(Actor actor, NetOutgoingMessage om)
        {
            //required to identify actor
            om.Write(actor.ActorID);

            //specific to actors, once created
            (actor as IChangeUpdate).WriteOutChangeUpdateData(om);
        }

        public override void Update(GameTime gameTime)
        {
        }
        public override void Draw(GameTime gameTime)
        {
        }
    }
}
