using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System.Threading;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;

using Omron.Framework;
using Omron.Framework.Networking;
using Omron.Actors;

using Omron.Helpers;

using Lidgren.Network;
using Lidgren.Network.Xna;

namespace Omron.GameStates
{
    public class LobbyGameState : GameState
    {
        public LobbyGameState() { }

        UIManager UIMan;
        SpriteBatch spriteBatch;

        NetClient client;

        Vector2 hlistStart;

        byte factionID;
        PlayerType factionType;

        Vector2 basePos;

        Text text;

        public override void Init()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            UIMan = new UIManager();
            UIMan.KeyDown += new KeyPressEventHandler(UIMan_KeyDown);

            text = new Text(new Vector2(50, GraphicsDevice.PresentationParameters.BackBufferHeight - 50), "find and select a host");
            text.Color = Color.Red;
            UIMan.AddControl(text);

            SolidButton hostsBut = new SolidButton(Vector2.One * 50, 200, 75);
            hostsBut.Text = "find hosts";
            hostsBut.MouseLeftDown += new MouseClickEventHandler(hostsBut_MouseLeftDown);

            UIMan.AddControl(hostsBut);
            hlistStart = hostsBut.Position + Vector2.UnitY * (hostsBut.Height + 100);

            SolidButton jHuman = new SolidButton(new Vector2(500, 50), 200, 100);
            jHuman.Text = "reg as Human";
            jHuman.MouseLeftDown += new MouseClickEventHandler(jHuman_MouseLeftDown);
            UIMan.AddControl(jHuman);

            SolidButton jWumpus = new SolidButton(new Vector2(750, 50), 200, 100);
            jWumpus.Text = "reg as Wumpus";
            jWumpus.MouseLeftDown += new MouseClickEventHandler(jWumpus_MouseLeftDown);
            UIMan.AddControl(jWumpus);

            SolidButton fBut = new SolidButton(new Vector2(750, 250), 200, 100);
            fBut.Text = "start!";
            fBut.MouseLeftDown += new MouseClickEventHandler(fBut_MouseLeftDown);
            UIMan.AddControl(fBut);

            NetPeerConfiguration config = new NetPeerConfiguration("omron");
            config.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
            client = new NetClient(config);
            client.Start();
        }

        void fBut_MouseLeftDown(Vector2 mPos)
        {
            NetOutgoingMessage om = client.CreateMessage();
            om.Write((byte)MessageDataType.RequestFinalize);
            client.SendMessage(om, NetDeliveryMethod.ReliableUnordered);
        }

        void jWumpus_MouseLeftDown(Vector2 mPos)
        {
            joinAs(PlayerType.Wumpus);
        }

        void jHuman_MouseLeftDown(Vector2 mPos)
        {
            joinAs(PlayerType.Human);
        }
        void joinAs(PlayerType type)
        {
            NetOutgoingMessage om = client.CreateMessage();
            om.Write((byte)MessageDataType.RequestFaction);
            om.Write((byte)type);
            client.SendMessage(om, NetDeliveryMethod.ReliableUnordered);
        }
        void UIMan_KeyDown(Keys key)
        {
            switch (key)
            {
                case Keys.Escape:
                    GameEngine.PopState();
                    break;
            }
        }

        void hostsBut_MouseLeftDown(Vector2 mPos)
        {
            client.DiscoverLocalPeers(9001);
        }
        public override void Update(GameTime gameTime)
        {

            NetIncomingMessage msg;
            while ((msg = client.ReadMessage()) != null)
            {
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.DiscoveryResponse:
                        addHost(msg.SenderEndpoint);
                        break;

                    case NetIncomingMessageType.Data:
                        switch ((MessageDataType)msg.ReadByte())
                        {
                            case MessageDataType.FactionRegistered:
                                factionType = (PlayerType)msg.ReadByte(); 
                                factionID = msg.ReadByte();
                                basePos = msg.ReadVector2();

                                Console.WriteLine("faction registered. factionID: " + factionID);
                                text.TextMsg = "factionID: " + factionID + "     READY TO START!";
                                break;
                            case MessageDataType.GameFinalized:

                                byte usize = msg.ReadByte();
                                byte vsize = msg.ReadByte();
                                float slen = msg.ReadFloat();

                                Faction worldF = new Faction(msg.ReadByte());

                                Faction playerF = new Faction(factionID, usize, vsize);
                                playerF.FactionType = factionType;
                                List<Faction> otherFactions = new List<Faction>();

                                byte hnum = msg.ReadByte();
                                for (byte i = 0; i < hnum; i++)
                                {
                                    Faction otherFact = new Faction(msg.ReadByte());
                                    playerF.Register(otherFact, FactionRelationship.Hostile);
                                    otherFactions.Add(otherFact);
                                }

                                byte anum = msg.ReadByte();
                                for (byte i = 0; i < anum; i++)
                                {
                                    Faction otherFact = new Faction(msg.ReadByte());
                                    playerF.Register(otherFact, FactionRelationship.Allied);
                                    otherFactions.Add(otherFact);
                                }

                                GameEngine.PopState(); //lobby is dirty, will probably be invalid
                                GameEngine.PushState(new ClientGameState(client, usize, vsize, slen, playerF, worldF, otherFactions, basePos));

                                break;
                        }
                        break;
                    default:
                        break;
                }
            }


            UIMan.Update();
        }
        void addHost(IPEndPoint server)
        {
            SolidButton newHostBut = new SolidButton(hlistStart, 400, 50);
            newHostBut.Text = server.ToString();
            newHostBut.MouseLeftDown += new MouseClickEventHandler(mPos => tryConnect(server));
            UIMan.AddControl(newHostBut);

            hlistStart += Vector2.UnitY * (newHostBut.Height + 0);
        }
        void tryConnect(IPEndPoint server)
        {
            try
            {
                client.Connect(server);
                Console.WriteLine("CONNECTED TO " + server);

                text.TextMsg = "register your faction";
            }
            catch(Exception ex)
            {
                Console.WriteLine("CONNECTION TO " + server + " FAILED");
                Console.WriteLine(ex.Message);
            }
        }
        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();
            UIMan.Draw(spriteBatch);
            spriteBatch.Draw(ResourceManager.Resources["Cursor"], UIMan.GetMousePos(), Color.White);
            spriteBatch.End();
        }
    }
}
