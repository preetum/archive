using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Omron.Framework;
using Omron.Actors;
using Microsoft.Xna.Framework.Input;

namespace Omron.GameStates
{
    public class NetworkUI : GameUI
    {
        public NetworkUI(GameState gState, World w, Faction pF)
            : base(gState, w, pF) { }

        public void SetSendPeace(sendActorDelegate del)
        {
            goTroops = del;
        }

        public void SetSendAggressive(sendActorDelegate del)
        {
            goAgressive = del;
        }

        public void SetConstruct(constructDelegate del)
        {
            construct = del;
        }

        public void SetSendEngage(actorActorDelegate del)
        {
            sendEngage = del;
        }

        public void SetSendBuild(actorActorDelegate del)
        {
            sendBuild = del;
        }

        public void SetItemPressed(itemPressedDelegate del)
        {
            itemPressed = del;
        }

        public delegate void actorActorDelegate(ushort[] actIds, ushort target);
        public delegate void constructDelegate(string type, Vector2 loc, float rot);
        public delegate void sendActorDelegate(ushort[] actIds, Vector2 target);
        public delegate void itemPressedDelegate(ushort actID, int item);

        actorActorDelegate sendEngage, sendBuild;
        constructDelegate construct;
        sendActorDelegate goTroops, goAgressive;
        itemPressedDelegate itemPressed; 

        protected override void Construct(Actor act)
        {
            construct(act.Type, act.Position, act.Rotation);
        }

        protected override void SendActor(IEnumerable<Actor> acts, Vector2 target, bool agressive)
        {
            if (agressive)
                goAgressive(acts.Select(act => act.ActorID).ToArray(), target);
            else
                goTroops(acts.Select(act => act.ActorID).ToArray(), target);
        }

        protected override void SendBuild(IEnumerable<Actor> acts, Actor target)
        {
            sendBuild(acts.Select(act => act.ActorID).ToArray(), target.ActorID);
        }

        protected override void SendEngage(IEnumerable<Actor> acts, Actor target)
        {
            sendEngage(acts.Select(act => act.ActorID).ToArray(), target.ActorID);
        }

        protected override void UnitMenuPressed(Actor act, int item)
        {
            itemPressed(act.ActorID, item);
        }
    }

    public class GameUI
    {
        public bool GodMode = false;

        GameState gameState;
        public GameUI(GameState gState, World w, Faction pF)
        {
            gameState = gState;
            graphicsDevice = gState.GraphicsDevice;
            world = w;
            playerF = pF;

            UIMan = new UIManager();

            initDraw();
            initActorMenu();
            initFactionMenu();
            initMultiUnitDisp();
            initInput();
            initUI();

            world.NotifyActorRemoved += new ActorRemoved(worldActorRemoved);
            activeUnits = new List<Actor>();
            updateMenus();
        }

        #region local variables
        GraphicsDevice graphicsDevice;
        World world;
        Faction playerF;
        UIManager UIMan;
        List<Actor> activeUnits;
        Camera activeCam;

        Rectangle miniMapRect;
        Vector2 topLeft = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 botomRight = new Vector2(float.MinValue, float.MinValue);
        Camera miniMapCam;
        Text resourceText;
        #endregion

        public void SetTarget(Vector2 loc)
        {
            activeCam.Target = loc;
        }

        static class UISettings
        {
            public static Vector2
                ActorMenuBar = new Vector2(380, -100),
                ActorMenuText = new Vector2(490, -90);

            public static float
                MiniMapScreenRectLineWidth = 2;

            public static int
                ActorButtonHeight = 50,
                ActorButtonWidth = 100,
                ActorMenuX = 10,
                ActorMenuYFromBottom = 80,
                ActorMenuBarWidth = 50,
                ActorMenuBarHeight = 10,
                FactionMenuX = 10,
                FactionMenuY = -60,
                FactionItemWidth = 100,
                FactionItemHeight = 40,
                MultiUnitX = 30,
                MultiUnitY = -100,
                MultiUnitItemW = 30,
                MultiUnitItemH = 30,
                MultiUnitDistFromMiniMap = 30;

            public static Vector2
                ResourceInfoOffset = new Vector2(-140, -265);
        }

        void worldActorRemoved(Actor act)
        {
            if (activeUnits.Contains(act))
            {
                act.UpdateMenu -= act_UpdateMenu;
                activeUnits.Remove(act);
                updateMenus();
            }
        }

        void updateMenus()
        {
            switch (activeUnits.Count)
            {
                case 0:
                    clearActorMenu();
                    showFactionMenu();
                    hideMultiUnitDisp();
                    break;
                case 1:
                    createActorMenu(activeUnits[0]);
                    hideFactionMenu();
                    hideMultiUnitDisp();
                    break;
                default:
                    clearActorMenu();
                    hideFactionMenu();
                    showMultiUnitDisp(activeUnits.ToArray());
                    break;
            }
        }

        void clearActSelections()
        {
            foreach (Actor act in activeUnits)
            {
                act.IsSelected = false;
                act.UpdateMenu -= act_UpdateMenu;
            }
            activeUnits.Clear();
        }

        #region actor menu
        void initActorMenu()
        {
            actMenuButs = new ListBox(UISettings.ActorButtonWidth, UISettings.ActorButtonHeight, UIMan,
                UISettings.ActorMenuX, graphicsDevice.PresentationParameters.BackBufferHeight - UISettings.ActorMenuYFromBottom);
            actMenuBar = new Bar(Vector2.UnitY * graphicsDevice.PresentationParameters.BackBufferHeight + UISettings.ActorMenuBar,
                UISettings.ActorMenuBarWidth, UISettings.ActorMenuBarHeight);
            UIMan.AddControl(actMenuBar);
            actMenuBar.Active = false;
            actMenuText = new Text(Vector2.UnitY * graphicsDevice.PresentationParameters.BackBufferHeight + UISettings.ActorMenuText,
                "", ResourceManager.Resources["UnitMenuFont"]);
            actMenuText.Color = Color.Red;
            UIMan.AddControl(actMenuText);
            actMenuText.Active = false;

            actMenuButs.ItemPressed += new MenuItemPressed(actMenuButs_ItemPressed);
        }

        void actMenuButs_ItemPressed(int itemKey)
        {
            UnitMenuPressed(activeUnits[0], itemKey);
        }

        void createActorMenu(Actor act)
        {
            if (act.Menu != null)
            {
                actMenuButs.SetItems(act.Menu.Commands);
                actMenuButs.Show();

                actMenuText.Active = true;
                actMenuBar.Active = act.Menu.DisplayBar;
                act.UpdateMenu += act_UpdateMenu;
            }
        }

        void act_UpdateMenu(Actor sender)
        {
            if (activeUnits.Contains(sender))
                createActorMenu(sender);
        }

        void clearActorMenu()
        {
            actMenuBar.Active = false;
            actMenuText.Active = false;
            actMenuButs.Clear();
        }

        void updateActMenu(Actor act)
        {
            if (act.Menu != null)
            {
                actMenuText.TextMsg = act.Menu.Info;
                actMenuBar.Value = act.Menu.BarValue;
                actMenuBar.Active = act.Menu.DisplayBar;
            }
        }

        ListBox actMenuButs;
        Bar actMenuBar;
        Text actMenuText;
        #endregion

        #region faction menu
        void initFactionMenu()
        {
            factionBuldings = new ListBox(UISettings.FactionItemWidth, UISettings.FactionItemHeight, UIMan,
                UISettings.FactionMenuX, UISettings.FactionMenuY + graphicsDevice.PresentationParameters.BackBufferHeight);
            recontructFactionMenu();
            factionBuldings.ItemPressed += new MenuItemPressed(factionBuldings_ItemPressed);

            playerF.UpdateFactionMenu += new Action(newBuilding);
        }

        void factionBuldings_ItemPressed(int itemKey)
        {
            ResourceData data = UnitConverter.CreateResourceData(ResourceManager.Resources[
                playerF.BuildingsUnlocked[itemKey]].Cost);
            if (playerF.Resources > data)
            {
                playerF.Resources -= data;
                Vector2 mPos = Vector2.Transform(UIMan.GetMousePos(), activeCam.GetUntransform());
                building = (FatherBuilding)UnitConverter.CreateActor(playerF.BuildingsUnlocked[itemKey], mPos, playerF);
            }
        }

        bool buildingCollides;
        FatherBuilding building;
        Vector2? wallStart;

        void recontructFactionMenu()
        {
            List<Texture2D> buildImgs = new List<Texture2D>();
            foreach (string build in playerF.BuildingsUnlocked)
            {
                UnitsAndBuilduings.BuilduingTypeInfo info = ResourceManager.Resources[build];
                Texture2D image = ResourceManager.Resources[info.IdleAnimation.Animation][0];
                buildImgs.Add(image);
            }
            factionBuldings.SetItems(playerF.BuildingsUnlocked.ToArray(), buildImgs.ToArray());
        }

        void showFactionMenu()
        {
            factionBuldings.Show();
        }

        void hideFactionMenu()
        {
            factionBuldings.Hide();
        }

        void newBuilding()
        {
            recontructFactionMenu();
            updateMenus();
        }

        ListBox factionBuldings;
        #endregion

        #region multi unit display
        void initMultiUnitDisp()
        {
            multiActors = new ListBox(UISettings.MultiUnitItemW, UISettings.MultiUnitItemH, UIMan, UISettings.MultiUnitX, UISettings.MultiUnitY +
            graphicsDevice.PresentationParameters.BackBufferHeight, miniMapRect.Left  - UISettings.MultiUnitDistFromMiniMap - UISettings.MultiUnitItemW);
            multiActors.ItemPressed += new MenuItemPressed(multiActors_ItemPressed);
        }

        void multiActors_ItemPressed(int itemKey)
        {
            clearActSelections();
            if (UIMan.CtrlDown || UIMan.ShiftDown)
            {
                activeUnits.RemoveAt(itemKey);
            }
            else if (UIMan.AltDown)
            {
                activeUnits = activeUnits.Where(act => act.Type == activeUnits[itemKey].Type).ToList();
            }
            else
            {
                Actor act = activeUnits[itemKey];
                clearActSelections();
                activeUnits.Add(act);
            }
            foreach (Actor act in activeUnits)
            {
                act.IsSelected = true;
            }
            updateMenus();
        }

        void showMultiUnitDisp(Actor[] acts)
        {
            multiActors.SetItems(acts);
            multiActors.Show();
        }

        void hideMultiUnitDisp()
        {
            multiActors.Hide();
        }

        ListBox multiActors;
        #endregion

        #region unit drawring
        void initDraw()
        {
            var cent = 0.5f * world.TileGrid.UVToScreen(new Point(world.TileGrid.U_length - 1, world.TileGrid.V_length - 1));

            activeCam = new Camera(graphicsDevice.Viewport);
            //ActiveCam.Target = Vector2.Zero;
            activeCam.Target = cent; //target camera on center of tile grid
            activeCam.Zoom = 85f;

            Vector2 bounds = new Vector2(graphicsDevice.PresentationParameters.BackBufferWidth, graphicsDevice.PresentationParameters.BackBufferHeight);

            miniMapRect = ResourceManager.Resources["Settings"]["Minimap"];
            miniMapRect = new Rectangle(graphicsDevice.PresentationParameters.BackBufferWidth - miniMapRect.Width -
                miniMapRect.X,
                graphicsDevice.PresentationParameters.BackBufferHeight - miniMapRect.Height - miniMapRect.Y, 
                miniMapRect.Width,
                miniMapRect.Height);

            miniMapCam = new Camera(new Viewport(miniMapRect));

            UISettings.ResourceInfoOffset += bounds;

            SolidButton miniMapButton = new SolidButton(new Vector2(miniMapRect.X, miniMapRect.Y), miniMapRect.Width, miniMapRect.Height);
            miniMapButton.ActiveColor = Color.Transparent;
            miniMapButton.InactiveColor = Color.Transparent;
            miniMapButton.MouseDrag += new MouseOverEventHandler(miniMapButton_MouseDrag);
            UIMan.AddControl(miniMapButton);
        }

        void miniMapButton_MouseDrag(Vector2 mPos)
        {
            Vector2 gamePos = Vector2.Transform(mPos, miniMapCam.GetUntransform());
            activeCam.Target = gamePos;

            clampCam();
        }

        void drawScreen(SpriteBatch spriteBatch, HashSet<Actor> visibleActors)
        {
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, null, null, null, null, activeCam.GetTransform());

            for (int u = 0; u < world.TileGrid.U_length; u++)
            {
                for (int v = 0; v < world.TileGrid.V_length; v++)
                {
                    var tile = world.TileGrid[u, v];

                    //coloration
                    var tState = playerF.FOW.GetTileState(u, v);
                    if (tState != TileState.Undiscovered)
                    {
                        Color c = Color.Red;
                        if (tState == TileState.Inactive)
                            c = Color.DarkGray;
                        if (tState == TileState.Active)
                            c = Color.LightGray;
                        GraphicsHelper.DrawHex(tile.Position, world.TileGrid.HexSideLen, c, spriteBatch);
                    }
                }
            }

            foreach (Actor actor in visibleActors)
            {
                actor.Draw(spriteBatch);
            }

            foreach (var effect in world.GetEffects())
            {
                var posUV = world.TileGrid.ScreenToUV(effect.MainPos);
                if (playerF.FOW.GetTileState(posUV.X, posUV.Y) == TileState.Active) //only draw effects positioned above active tiles
                    effect.Draw(spriteBatch);
            }

            drawSelection(spriteBatch, Color.LightBlue * 0.5f);

            drawBuilding(spriteBatch);

            spriteBatch.End();
        }

        void drawMiniMap(SpriteBatch spriteBatch, HashSet<Actor> visibleActors)
        {
            spriteBatch.Begin();
            GraphicsHelper.DrawRectangle(spriteBatch, miniMapRect.Left, miniMapRect.Right, miniMapRect.Top, miniMapRect.Bottom, Color.Black);
            spriteBatch.End();


            float maxX = botomRight.X - topLeft.X + 2;
            float maxY = botomRight.Y - topLeft.Y + 2;
            float zoomX = miniMapRect.Width / maxX;
            float zoomY = miniMapRect.Height / maxY;

            miniMapCam.Zoom = Math.Min(zoomX, zoomY);
            miniMapCam.Target = (topLeft + botomRight) / 2;

            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, null, null, null, null, miniMapCam.GetTransform());
            for (int u = 0; u < world.TileGrid.U_length; u++)
            {
                for (int v = 0; v < world.TileGrid.V_length; v++)
                {
                    var tile = world.TileGrid[u, v];


                    //coloration
                    var tState = playerF.FOW.GetTileState(u, v);
                    if (tState != TileState.Undiscovered)
                    {
                        topLeft = Vector2.Min(topLeft, tile.Position);
                        botomRight = Vector2.Max(botomRight, tile.Position);

                        Color c = Color.Red;
                        if (tState == TileState.Inactive)
                            c = Color.DarkGray;
                        if (tState == TileState.Active)
                            c = Color.LightGray;
                        GraphicsHelper.DrawHex(tile.Position, world.TileGrid.HexSideLen, c, spriteBatch);
                    }
                }
            }
            foreach (Actor act in visibleActors)
            {
                if (act.CollisionClass != CollisionClass.IsolatedNoPersist)
                {
                    if (act.Faction == world.Faction)
                        act.Draw(spriteBatch);
                    else
                    {
                        Color col = act.Faction.GetColor();
                        if (act.IsSelected)
                        {
                            Color invCol = new Color(Vector3.One - col.ToVector3());
                            col = invCol; // Color.Lerp(col, invCol, .8f);
                        }
                        GraphicsHelper.DrawRectangleInv(spriteBatch, act.Position, act.GetBoundingPoly().MaxRadius * 2,
                            act.GetBoundingPoly().MaxRadius * 2, 0, col);
                    }
                }
            }
            drawSelection(spriteBatch, Color.DarkBlue * 0.8f);

            float rectLeft = activeCam.Target.X - activeCam.GetWidth() / 2;
            rectLeft = Math.Max(rectLeft, miniMapCam.Target.X - miniMapCam.GetWidth() / 2);
            float rectTop = activeCam.Target.Y - activeCam.GetHeight() / 2;
            rectTop = Math.Max(rectTop, miniMapCam.Target.Y - miniMapCam.GetHeight() / 2);
            float rectRight = activeCam.Target.X + activeCam.GetWidth() / 2;
            rectRight = Math.Min(rectRight, miniMapCam.Target.X + miniMapCam.GetWidth() / 2);
            float rectBot = activeCam.Target.Y + activeCam.GetHeight() / 2;
            rectBot = Math.Min(rectBot, miniMapCam.Target.Y + miniMapCam.GetHeight() / 2);
            GraphicsHelper.DrawRectOutlineInv(spriteBatch, rectLeft, rectTop, rectRight - rectLeft, 
                rectBot - rectTop, UISettings.MiniMapScreenRectLineWidth / miniMapCam.Zoom, Color.Yellow);

            spriteBatch.End();
        }
        #endregion

        #region UI drawring
        void initUI()
        {//initialize UI here
            resourceText = new Text(UISettings.ResourceInfoOffset, "", ResourceManager.Resources["SmallInfoFont"]);
            resourceText.Color = Color.Red;
            UIMan.AddControl(resourceText);
        }

        void drawUI(SpriteBatch spBatch)
        {
            Texture2D tex = ResourceManager.Resources["MinimapSkin"];
            Vector2 loc = new Vector2(graphicsDevice.PresentationParameters.BackBufferWidth,
                graphicsDevice.PresentationParameters.BackBufferHeight);
            loc -= new Vector2(tex.Width, tex.Height);
            spBatch.Begin();
            spBatch.Draw(tex, loc, Color.White);
            //spBatch.Draw(ResourceManager.Resources["MetalAnim"][0], new Rectangle(10, 10, 20, 20), Color.White);
            //spBatch.Draw(ResourceManager.Resources["CrystalAnim"][0], new Rectangle(10, 40, 20, 20), Color.White);
            spBatch.End();
        }
        void drawCursor(SpriteBatch spBatch)
        {
            spBatch.Begin();
            spBatch.Draw(getCursorAnim(), UIMan.GetMousePos(), Color.White);
            spBatch.End();
        }

        Texture2D getCursorAnim()
        {
            /*if (activeUnits.Count > 0)looks lame
            {
                List<Actor> acts = (List<Actor>)world.QueryPoint(Vector2.Transform(UIMan.GetMousePos(), activeCam.GetUntransform())).ToList();
                if (acts.Count() > 0)
                {
                    if (acts[0].Faction == playerF)
                    {
                        if ((acts[0] is FatherBuilding) && !(acts[0] as FatherBuilding).IsComplete)
                            return ResourceManager.Resources["BuildCursor"];
                    }
                    else if (acts[0].Faction == world.Faction)
                    {
                        if (acts[0] is Resource)
                        {
                            if (activeUnits.Any(act => (act.Type == "Miner")))
                                return ResourceManager.Resources["MineCursor"];
                        }
                    }
                    else
                    {
                        return ResourceManager.Resources["AttackCursor"];
                    }
                }
            }*/
            return ResourceManager.Resources["ArrowCursorSmall"];
        }
        #endregion

        public void Update(GameTime gameTime)
        {
            if (activeUnits.Count == 1)
                updateActMenu(activeUnits[0]);

            resourceText.TextMsg = "" + playerF.Resources.Crystal + "     " + playerF.Resources.Metal;

            UIMan.Update();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            HashSet<Actor> visibleActors = world.QueryVisibleActors(playerF.FOW);
            drawScreen(spriteBatch, visibleActors);
            drawMiniMap(spriteBatch, visibleActors);       
            drawUI(spriteBatch);

            spriteBatch.Begin();
            UIMan.Draw(spriteBatch);
            spriteBatch.End();

            drawCursor(spriteBatch);
        }

        #region input
        void initInput()
        {
            UIMan.KeyHeld += new KeyPressEventHandler(keyHeld);
            UIMan.KeyUp += new KeyPressEventHandler(keyUp);
            UIMan.KeyUp += new KeyPressEventHandler(keyDown);
            UIMan.MouseLeftDown += new MouseClickEventHandler(startSelection);
            UIMan.MouseLeftUp += new MouseClickEventHandler(doSelection);
            UIMan.MouseOver += new MouseOverEventHandler(moveCamera);
            UIMan.MouseRightDown += new MouseClickEventHandler(sendUnits);
            UIMan.MouseLeftDown += new MouseClickEventHandler(buildBuilding);
            UIMan.MouseLeftUp += new MouseClickEventHandler(dropWall);
            UIMan.MouseRightDown += new MouseClickEventHandler(cancelBuilding);
            selectionActive = false;
            //vCom = new VoiceCommand(world, playerF);
            //vCom.NewUnitsSelected += new VoiceCommand.SelectDelegate(vCom_NewUnitsSelected);
        }

        void cancelBuilding(Vector2 mPos)
        {
            building = null;
            wallStart = null;
        }

        void vCom_NewUnitsSelected(List<Actor> units)
        {
            clearActSelections();
            activeUnits = units;
            foreach (Actor act in activeUnits)
            {
                act.IsSelected = true;
            }
            updateMenus();
        }

        VoiceCommand vCom;

        #region building
        void drawBuilding(SpriteBatch spriteBatch)
        {
            if (building != null)
            {//draw the building where the cursor is
                Vector2 gamePt = Vector2.Transform(UIMan.GetMousePos(), activeCam.GetUntransform());
                if (wallStart == null)
                {
                    building.Position = gamePt;
                    buildingCollides = world.IsCollidingConBuilding(building);
                    Color col = Color.White;
                    if (buildingCollides)
                        col = Color.Red;
                    building.IdleAnimation.GetCurrentFrame().Draw(spriteBatch, building.Position,
                        building.Rotation, col);
                }
                else
                {
                    List<Actor> walls = genWalls(wallStart.Value, gamePt);
                    foreach (Actor wall in walls)
                    {
                        (wall as FatherBuilding).IdleAnimation.GetCurrentFrame().Draw(spriteBatch, wall.Position,
                        wall.Rotation, Color.White);
                    }
                }
            }
        }

        void dropWall(Vector2 mPos)
        {
            if (building is Wall)
            {
                genWalls(wallStart.Value, Vector2.Transform(mPos, activeCam.GetUntransform())).ForEach(wall => Construct(wall));
                wallStart = null;
                building = null;
            }
        }

        List<Actor> genWalls(Vector2 pt1, Vector2 pt2)
        {
            List<Actor> walls = new List<Actor>();
            Vector2 deltaVec = (pt2 - pt1) / ((pt2 - pt1).Length() / building.GetBoundingPoly().Width);
            for (Vector2 nextLoc = pt1; (nextLoc - pt1).Length() <= (pt2 - pt1).Length(); nextLoc += deltaVec)
            {
                Actor nextWall = UnitConverter.CreateActor(building.Type, nextLoc, MathHelper.GetAngle(deltaVec), playerF);
                if (!world.IsCollidingConBuilding(nextWall))
                    walls.Add(nextWall);
            }
            return walls;
        }

        void buildBuilding(Vector2 mPos)
        {
            if (building != null)
            {
                if (building is Wall)
                {
                    //build walls
                    wallStart = Vector2.Transform(mPos, activeCam.GetUntransform());
                }
                else
                {
                    if (!buildingCollides)
                    {
                        Construct(building);
                    }
                    building = null;
                }
            }
        }
        #endregion

        void sendUnits(Vector2 mPos)
        {
            Vector2 gamePt = Vector2.Transform(mPos, activeCam.GetUntransform());

            Actor coverActor = world.Query(new RectPoly(gamePt, gamePt)).FirstOrDefault();

            if (activeUnits.Any(act => act is FatherUnit))
            {
                IEnumerable<Actor> units = activeUnits.Where(act => act is FatherUnit);
                if (coverActor != null)
                {
                    if (coverActor.Faction.GetRelationship(playerF) != FactionRelationship.Allied)
                        SendEngage(units, coverActor);
                    else if (coverActor is FatherBuilding && !(coverActor as FatherBuilding).IsComplete)
                        SendBuild(units, coverActor);
                }
                else
                {
                    if (UIMan.CtrlDown)
                        SendActor(units, gamePt, true);
                    else
                        SendActor(units, gamePt, false);
                }
            }
            activeUnits.Where(act => act is ActionCenter).ToList().ForEach(act => (act as ActionCenter).SetRally(gamePt));
        }

        void moveCamera(Vector2 mPos)
        {
            //for moving screen
            Vector2 bounds = new Vector2(graphicsDevice.PresentationParameters.BackBufferWidth, graphicsDevice.PresentationParameters.BackBufferHeight);
            Vector2 screenCenter = 0.5f * bounds;
            Vector2 diffVect = UIMan.GetMousePos() - screenCenter;
            diffVect.X /= screenCenter.X;
            diffVect.Y /= -screenCenter.Y;
            if ((Math.Abs(diffVect.X) > ResourceManager.Resources["Settings"]["X Scroll"]) || (Math.Abs(diffVect.Y) > ResourceManager.Resources["Settings"]["Y Scroll"]))//move only if cursur is on the outer strip circle
            {
                activeCam.Target += diffVect * ResourceManager.Resources["Settings"]["Camera Speed"] / activeCam.Zoom;
                clampCam();
            }

            
        }

        void clampCam()
        {
            if (topLeft != botomRight)
                activeCam.Target = Vector2.Clamp(activeCam.Target, topLeft, botomRight);
        }

        #region Selection
        void drawSelection(SpriteBatch spriteBatch, Color c)
        {//***call from already initialized spriteBatch from Draw()
            if (selectionActive)
            {
                Vector2 otherSelPoint = Vector2.Transform(UIMan.GetMousePos(), activeCam.GetUntransform());
                GraphicsHelper.DrawRectangleInv(spriteBatch, (selectionPoint + otherSelPoint) / 2, Math.Abs(selectionPoint.X - otherSelPoint.X),
                    Math.Abs(selectionPoint.Y - otherSelPoint.Y), 0, c);
            }
        }

        void doSelection(Vector2 mPos)
        {
            if (selectionActive)
            {
                clearActSelections();

                selectionActive = false;
                Vector2 otherSelPoint = Vector2.Transform(UIMan.GetMousePos(), activeCam.GetUntransform());
                IEnumerable<Actor> actors = getUnitsInSelection(selectionPoint, otherSelPoint);
                actors = filterSelection(actors);
                if (UIMan.CtrlDown)
                {
                    activeUnits.RemoveAll(act => actors.Contains(act));
                }
                else if (UIMan.AltDown)
                {
                    /*if (actors.Count() > 0)
                    {
                        activeUnits = activeUnits.Where(act => actors.Any(act2 => act2.Type == act.Type)).ToList();
                    }*/
                    if (actors.Count() > 0)
                    {
                        Actor first = actors.First();
                        activeUnits = playerF.Actors.Where(act => act.Type == first.Type).ToList();
                    }
                }
                else if (UIMan.ShiftDown)
                {
                    activeUnits.AddRange(actors);
                }
                else
                {
                    activeUnits = actors.ToList();
                }
                foreach (Actor act in activeUnits)
                {
                    act.IsSelected = true;
                }
                updateMenus();

                Console.WriteLine("selected actorsIDs:");
                activeUnits.ForEach(u => Console.Write(u.ActorID.ToString() + ", "));
                Console.WriteLine("");
            }
        }

        bool selectionActive;
        Vector2 selectionPoint;

        IEnumerable<Actor> filterSelection(IEnumerable<Actor> acts)
        {
            return from act in acts
                   where act.Faction == playerF || (act.Faction == world.Faction && act is Resource) || (GodMode && !(act is Wall))
                   select act;
        }


        IEnumerable<Actor> getUnitsInSelection(Vector2 pos1, Vector2 pos2)
        {
            var min = Vector2.Min(pos1, pos2);
            var max = Vector2.Max(pos1, pos2);

            RectPoly testPoly = new RectPoly(min, max);

            return world.Query(testPoly);
        }

        void startSelection(Vector2 mPos)
        {
            if (building == null)
            {
                selectionActive = true;
                selectionPoint = Vector2.Transform(UIMan.GetMousePos(), activeCam.GetUntransform());
            }
        }
        #endregion

        void keyUp(Microsoft.Xna.Framework.Input.Keys key)
        {
            switch (key)
            {
                case Keys.OemTilde:
                    playerF.RevealAll = true;
                    playerF.FOW.ActivateAll();
                    break;
                case Keys.Escape:
                    if (building != null || wallStart != null)
                    {
                        building = null;
                        wallStart = null;
                    }
                    else
                        gameState.GameEngine.PopState();
                    break;
                case Keys.OemPlus:
                    activeCam.Zoom *= 2f;
                    break;
                case Keys.OemMinus:
                    activeCam.Zoom /= 2f;
                    break;
                case Keys.OemPipe: if (UIMan.AltDown && UIMan.CtrlDown) { playerF.Resources.Metal += 1000; playerF.Resources.Crystal += 1000; } break;
                case Keys.A:
                    foreach (Actor actor in activeUnits)
                    {
                        if (actor is FatherUnit)
                        {
                            (actor as FatherUnit).IssueRangedAttack(Vector2.Transform(UIMan.GetMousePos(), activeCam.GetUntransform()));
                        }
                    }
                    break;
                case Keys.W:
                    var gPos = Vector2.Transform(UIMan.GetMousePos(), activeCam.GetUntransform());
                    var uvPos = world.TileGrid.ScreenToUV(gPos);

                    var centPos = world.TileGrid.UVToScreen(uvPos);
                    var wall = (FatherBuilding)UnitConverter.CreateActor("Wall", centPos, world.Faction);
                    wall.OnWorkedOn(wall.WorkNeeded);
                    world.AddActor(wall);

                    break;
                case Keys.Home:
                    ResourceManager.GraphicsDeviceManager.ToggleFullScreen();
                    ResourceManager.GraphicsDeviceManager.ApplyChanges();
                    break;
                case Keys.Y:
                    vCom.Enabled = false;
                    break;
                default:
                    break;
            }
        }

        void keyHeld(Microsoft.Xna.Framework.Input.Keys key)
        {
            switch (key)
            {
                case Keys.Up:
                    activeCam.Target += Vector2.UnitY * ResourceManager.Resources["Settings"]["Camera Speed"] / activeCam.Zoom;
                    break;
                case Keys.Down:
                    activeCam.Target -= Vector2.UnitY * ResourceManager.Resources["Settings"]["Camera Speed"] / activeCam.Zoom;
                    break;
                case Keys.Left:
                    activeCam.Target -= Vector2.UnitX * ResourceManager.Resources["Settings"]["Camera Speed"] / activeCam.Zoom;
                    break;
                case Keys.Right:
                    activeCam.Target += Vector2.UnitX * ResourceManager.Resources["Settings"]["Camera Speed"] / activeCam.Zoom;
                    break;
            }
        }

        void keyDown(Keys key)
        {
            switch (key)
            {
                case Keys.Y:
                    vCom.Enabled = true;
                    break;
            }
        }
        #endregion

        #region UI Actions
        protected virtual void SendEngage(IEnumerable<Actor> acts, Actor target)
        {
            acts.ToList().ForEach(act => (act as FatherUnit).AI.Engage(target));
        }

        protected virtual void SendBuild(IEnumerable<Actor> acts, Actor target)
        {
            acts.ToList().ForEach(act => (act as FatherUnit).AI.Build(target as FatherBuilding));
        }

        protected virtual void SendActor(IEnumerable<Actor> acts, Vector2 target, bool agressive)
        {
            if (agressive)
                acts.ToList().ForEach(act => (act as FatherUnit).MayhamTrack(target));
            else
                acts.ToList().ForEach(act => (act as FatherUnit).Track(target));
        }

        protected virtual void Construct(Actor act)
        {
            world.AddActor(act);
        }

        protected virtual void UnitMenuPressed(Actor act, int item)
        {
            activeUnits[0].Menu.OnCommandInvoked(item);
        }
        #endregion
    }
}
