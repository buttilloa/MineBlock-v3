using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MineBlock.Blocks;
using MineBlock.Commands;
using MineBlock.Items;
using MineBlock.Managers;
using System;
using System.Collections.Generic;



namespace MineBlock
{

    public class Game1 : Microsoft.Xna.Framework.Game
    {

        GraphicsDeviceManager graphics; // Graphics
        SpriteBatch spriteBatch; // SpriteBatch
        public static MobManager mobManager; // Manages Mobs
        public static Chunk[,] chunks = new Chunk[10,10];
        //public static Block[,] Chunk = new Block[200, 200];
        public static Random randy = new Random(System.Environment.TickCount); // Random?
        public static PlayerManager player; // Manages Player
        public static Weather weather; // Manages Weather
        public static int selectedSave = 0; // Save Slot
        public static MenuRef menu;  // Manages Menus
        public static SaveManager saves = new SaveManager(); // Manages Saves
        public static ConsoleManager console; // Manages the ingame Console
        public float zoom = 0.0f;
        public static Color cursorColor = Color.White;
        public static Game1 Instance;
        public static Color lasercolor = Color.Red;
        public static Color breakanimcolor = Color.White;
        private SpriteFont pericles14;
        public static int RenderDistance = 11;
        public static int renderXStart, renderYStart, renderXEnd, renderYEnd;
        private static Texture2D t;

#if XBOX
        bool GameSaveRequested = false;  // Xbox specific saving Variables
         IAsyncResult result;
#endif

        public static int currentChunkNumber = 0; // Current Chunk

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);

            Content.RootDirectory = "Content";
            graphics.PreferMultiSampling = true;
            //graphics.PreferredBackBufferHeight = 1080;
             //graphics.PreferredBackBufferWidth = 1920;
            // graphics.IsFullScreen = true;
            this.Window.Title = "Colonization";
            // this.graphics.SynchronizeWithVerticalRetrace = false;
            // this.IsFixedTimeStep = false;
            Instance = this;
            Components.Add(new FrameRateCounter(this));

#if XBOX
        Components.Add(new GamerServicesComponent(this)); // Xbox Specific Player Manager
#endif

        }
        //Initialize Game
        protected override void Initialize()
        {

            base.Initialize();

        }
        //Load Content
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            //Textures
            Tm.loadContent(Content, GraphicsDevice);
            //Sounds
            SoundEffects.LoadSounds(Content);

            //Register Player
            menu = new MenuRef();
            mobManager = new MobManager();
            console = new ConsoleManager();
            weather = new Weather();
            player = new PlayerManager(Tm.getTexture(Tm.Texture.playerSheet), Tm.getTexture(Tm.Texture.hotbarsheet), Tm.getTexture(Tm.Texture.hotbarselector));
            pericles14 = Tm.getFontFromString("f14");
            t = Tm.getTextureFromString("t");

#if WINDOWS
            saves.GetDevice(); // Get Save Device
#endif
#if XBOX
if ((!Guide.IsVisible) && (GameSaveRequested == false)) // Request Xbox Storage Device
            {
                GameSaveRequested = true;
                result = StorageDevice.BeginShowSelector(
                        PlayerIndex.One, null, null);
            }
#endif
            menu.Init();

        }
        //Unload Content
        protected override void UnloadContent()
        {


        }

        //Update the Games Logic
        protected override void Update(GameTime gameTime)
        {
            //try
           // {

                if (console.isShown)
                    console.getKeyStrokes();
                else if (HandleInputs.isKeyDown("OemTilde")) console.isShown = true;
                if (HandleInputs.isKeyDown("Escape") && MenuRef.state != MenuRef.GameStates.Paused) // Opens pause menu
                {
                    MenuRef.state = MenuRef.GameStates.Paused;
                    MenuRef.SetMenu(new Menus.Paused());
                    //saves.SaveAll(selectedSave, player, mobManager);
                }
                if (MenuRef.state == MenuRef.GameStates.Playing && !console.isShown)
                {
#if XBOX

if ((GameSaveRequested) && (result.IsCompleted))
            {
                StorageDevice device = StorageDevice.EndShowSelector(result);
                if (device != null && device.IsConnected)
                {
                    saves.setDevice(device);
                    saves.GetContainer("MineBlock" +selectedSave);
                    if (saves.hasSaved())
                    {
                        currentChunk = saves.LoadChunk(currentChunkNumber);
                        chunks.Add(currentChunk);
                        loadTerrainCollum();
                        saves.LoadPlayer(player);
                        saves.LoadMobs(mobManager);
                    }
                    else
                    {
                        GenerateSpawnTerrain();
                        genTerrainCollum();
                   }
                }
                // Reset the request flag
                GameSaveRequested = false;
            }
#endif
                    if (HandleInputs.isKeyDown("Up")) zoom += .01f;
                    if (HandleInputs.isKeyDown("Down")) zoom -= .01f;
                    player.update(gameTime,chunks);
                    mobManager.update(gameTime);
                    Vector2 playerLoc = player.Player.Location;
                    renderXStart = (int)MathHelper.Clamp((playerLoc.X / 40) - RenderDistance + 1, 0f, 199);
                    renderYStart = (int)MathHelper.Clamp((playerLoc.Y / 40) - RenderDistance + 1, 0f, 129);
                    if (renderXStart == 0) renderXEnd = (int)MathHelper.Clamp((playerLoc.X / 40) + RenderDistance + 12, 0, 199);
                    else renderXEnd = (int)MathHelper.Clamp((playerLoc.X / 40) + RenderDistance + 4, 0, 199);
                    renderYEnd = (int)MathHelper.Clamp((playerLoc.Y / 40) + RenderDistance + 1, 0, 129);

                    for (int i = renderXStart; i <= renderXEnd; i++)
                        for (int j = renderYStart; j <= renderYEnd; j++)
                            Chunk.UpdateBlock(chunks, i, j);
                    
                    //chunks[i % 20][j % 20].getBlocks()[i, j].update(chunks[i][j], chunks[i][j - 1], chunks[i - 1][j], chunks[i + 1][j], chunks[i][j + 1]);
                            //chunks[i][j].updateBlocks(chunks[i][j],chunks[i][j-1],chunks[i-1][j],chunks[i+1][j],chunks[i][j+1]);

                    if (!weather.isPercipitationing() && randy.Next(0, 2000) == 4)
                        toggleDownfall();
                    weather.update(gameTime.ElapsedGameTime.TotalSeconds);

                    checkClicks(gameTime);
                }
                else menu.update();
                base.Update(gameTime);
            /*}
            catch (Exception e)
            {
                MenuRef.SetErrorMenu(e.Message, e.StackTrace);
                MenuRef.state = MenuRef.GameStates.Error;
            }
             */
        }
        public static void toggleDownfall()
        {
            if (!weather.isPercipitationing())
            {
                weather.Rain();
                Console.WriteLine("WEATHEREDING!");
            }
        }
        public static void DrawLine(SpriteBatch sb, Vector2 start, Vector2 end)
        {
            Vector2 edge = end - start;
            // calculate angle to rotate line
            float angle =
                (float)Math.Atan2(edge.Y, edge.X);


            sb.Draw(t,
                new Rectangle(// rectangle defines shape of line and position of start of line
                    (int)start.X,
                    (int)start.Y,
                    (int)edge.Length(), //sb will strech the texture to fill this rectangle
                    1), //width of line, change this to make thicker line
                null,
                lasercolor, //colour of line
                angle,     //angle of line (calulated above)
                new Vector2(0, 0), // point in line about which to rotate
                SpriteEffects.None,
                0);

        }

        float minetimer = -1;
        void checkClicks(GameTime gametime)
        {
            if (!player.playerinv.isdisplayed)
            {
                if (HandleInputs.RightTrigger())
                {
                    if (player.hotbar[player.selected].Blockindex != -1)
                    {
                        //if (Chunk.CalculateChunk(chunks,(int)player.highlighted.X, (int)player.highlighted.Y).index == 26)
                        //    player.useChest((Chest)chunk[(int)player.highlighted.X, (int)player.highlighted.Y]);
                        if (player.hotbar[player.selected].Count > 0 && Chunk.CalculateChunk(chunks,(int)player.highlighted.X, (int)player.highlighted.Y).index == 0 || player.hotbar[player.selected].Count > 0 && Chunk.CalculateChunk(chunks,(int)player.highlighted.X, (int)player.highlighted.Y).index == 14)
                        {

                            Chunk.PlaceBlock(chunks,(int)player.highlighted.X, (int)player.highlighted.Y, player.hotbar[player.selected].ReturnBlock().Place((int)player.highlighted.X, (int)player.highlighted.Y));

                            player.hotbar[player.selected].Count--;
                            if (player.hotbar[player.selected].Count == 0)
                                player.hotbar[player.selected] = new Air((player.selected * 40) + 16, 16).ItemBlock();
                            //player.updateBlocks(chunk);
                        }

                    }
                }

                if (HandleInputs.LeftTrigger())
                {

                    if (Chunk.CalculateChunk(chunks,(int)player.highlighted.X, (int)player.highlighted.Y).index != 0 && Chunk.CalculateChunk(chunks,(int)player.highlighted.X, (int)player.highlighted.Y).canMine)
                    {

                        Tool currentTool = null;
                        if (player.hotbar[player.selected] is Tool)
                            currentTool = (Tool)player.hotbar[player.selected];
                        float minetime = Chunk.CalculateChunk(chunks,(int)player.highlighted.X, (int)player.highlighted.Y).MineTime;
                        if (minetimer == -1)
                            minetimer = 0;
                        if (minetimer > -1 && minetimer < minetime)
                        {
                            float extradmg = 0;
                            if (player.hotbar[player.selected].Blockindex < 0 && Chunk.CalculateChunk(chunks,(int)player.highlighted.X, (int)player.highlighted.Y).preferedTool != null) if (Chunk.CalculateChunk(chunks,(int)player.highlighted.X, (int)player.highlighted.Y).preferedTool.index == player.hotbar[player.selected].index) extradmg = 1f * (currentTool.upgrade + 5f);
                            minetimer += 1f + extradmg;
                           Chunk.CalculateChunk(chunks,(int)player.highlighted.X, (int)player.highlighted.Y).damage += 1f + extradmg;
                        }
                        if (minetimer >= minetime)
                        {
                            minetimer = -1;
                            player.addToInv(Chunk.CalculateChunk(chunks,(int)player.highlighted.X, (int)player.highlighted.Y).Mine((int)player.highlighted.X, (int)player.highlighted.Y), 1);
                            Chunk.PlaceBlock(chunks,(int)player.highlighted.X, (int)player.highlighted.Y, new Air((int)player.highlighted.X, (int)player.highlighted.Y));
                            if (player.hotbar[player.selected] is Tool) currentTool.damage--;
                        }
                    }
                }
            }
        }
        //Render on Screen
        Matrix cameraTransform;
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            Vector2 Target = new Vector2((Convert.ToInt32(player.Player.Location.X) - 350), (Convert.ToInt32(player.Player.Location.Y) - 130));
            Vector2 Target1 = player.highlighted;
            Target += Target1;

            cameraTransform = Matrix.CreateTranslation((int)-Target.X, (int)-Target.Y, 0f);
            cameraTransform.M41 = MathHelper.Clamp(cameraTransform.M41, -7200, 0);
            cameraTransform.Translation.Normalize();

            spriteBatch.Begin(SpriteSortMode.Immediate,
                  BlendState.AlphaBlend, SamplerState.AnisotropicClamp, null, null, null,
                  cameraTransform); // moveable objects
            if (MenuRef.state == MenuRef.GameStates.Playing)
            {
                for (int i = renderXStart; i <= renderXEnd; i++)
                    for (int j = renderYStart; j <= renderYEnd; j++)
                        Chunk.CalculateChunk(chunks,i,j).Draw(spriteBatch);
                player.Draw(spriteBatch);//Draw Player
                mobManager.Draw(spriteBatch); // Draw Mobs
                weather.Draw(spriteBatch);// Draw Weather 
            }

            console.Draw(spriteBatch);
            spriteBatch.End();

            spriteBatch.Begin(); //static objects
            if (MenuRef.state == MenuRef.GameStates.Playing)
            {
                player.Drawstatic(spriteBatch);
                spriteBatch.DrawString(pericles14, "X: " + (((int)player.Player.Location.X / 40) + 1), new Vector2(this.Window.ClientBounds.Width - 110, 10), Color.White);// Draw Current Chunk int
                spriteBatch.DrawString(pericles14, "Y: " + (((int)player.Player.Location.Y / 40) + 1), new Vector2(this.Window.ClientBounds.Width - 110, 24), Color.White); // Draw Current Biome
            }

            else menu.Draw(spriteBatch);  // Draw Menus
            console.Drawstatic(spriteBatch);
            spriteBatch.End();
            base.Draw(gameTime);
        }

    }
}
