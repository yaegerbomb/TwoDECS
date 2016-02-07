using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using TwoDECS.Engine;
using TwoDECS.Engine.Cameras;
using TwoDECS.Engine.World;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using TwoDECS.Engine.Systems;
using TwoDECS.Engine.Components;

namespace TwoDECS
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        FollowCamera followCamera;
        Texture2D spriteSheet;
        SpriteFont spriteFont;

        TileMap tileMap { get; set; }

        LevelRenderer levelRenderer;

        PlayingState playingState;

        KeyboardState keyboardState;
        GamePadState gamepadState;
        MouseState mouseState;

        LevelCollisionDetection levelCollisionDetection;

        private const int tileSize = 16;
        private const int numberOfRows = 100;
        private const int numberOfColumns = 100;
        private const int mapWidth = tileSize * numberOfRows;
        private const int mapHeight = tileSize * numberOfColumns;
        private const int screenWidth = 800;
        private const int screenHeight = 600;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);

            this.graphics.PreferredBackBufferWidth = screenWidth;
            this.graphics.PreferredBackBufferHeight = screenHeight;

            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            this.IsMouseVisible = true;
            this.followCamera = new FollowCamera(GraphicsDevice.Viewport);
            this.playingState = new PlayingState();
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            spriteSheet = Content.Load<Texture2D>("Graphics\\UIpackSheet_transparent");
            spriteFont = Content.Load<SpriteFont>("Fonts\\kooten");


            levelRenderer = new LevelRenderer();
            levelRenderer.ImportTMX(Content, "Engine\\World\\Levels\\testmap.tmx", tileSize, spriteFont, playingState);

            levelCollisionDetection = new LevelCollisionDetection(levelRenderer.TileMap, tileSize);


        }

        //public void CreateWeapon(Vector2 position, float direction, float fireingRate, int ammo, Guid owner)
        //{
        //    Guid id = playingState.CreateEntity();
        //    playingState.Entities.Where(x => x.ID == id).First().ComponentFlags = ComponentMasks.Weapon;


        //    playingState.DirectionComponents[id] = new DirectionComponent() { Direction = direction };
        //    playingState.DisplayComponents[id] = new DisplayComponent() { Source = new Rectangle(488, 434, tileSize, tileSize) };
        //    playingState.PositionComponents[id] = new PositionComponent() { Position = position, Destination = new Rectangle((int)position.X, (int)position.Y, tileSize, tileSize) };
        //    playingState.TimerComponents[id] = new TimerComponent() { CountDown = 1, TimerReset = .25f };

        //}

        //public void CreateProjectile(Vector2 position, float direction, float acceleration, int tileSize, Guid owner)
        //{
        //    Guid id = playingState.CreateEntity();
        //    playingState.Entities.Where(x => x.ID == id).First().ComponentFlags = ComponentMasks.Projectile;


        //    playingState.DirectionComponents[id] = new DirectionComponent() { Direction = direction };
        //    playingState.DisplayComponents[id] = new DisplayComponent() { Source = new Rectangle(21, 504, tileSize, tileSize) };
        //    playingState.VelocityComponents[id] = new VelocityComponent() { xVelocity = 0, yVelocity = 0 };
        //    playingState.PositionComponents[id] = new PositionComponent() { Position = position, Destination = new Rectangle((int)position.X, (int)position.Y, tileSize, tileSize) };
        //    playingState.AccelerationComponents[id] = new AccelerationComponent() { xAcceleration = acceleration, yAcceleration = acceleration };

        //}

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
            Content.Unload();
            Content.Dispose();
            
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            keyboardState = Keyboard.GetState();
            gamepadState = GamePad.GetState(PlayerIndex.One);
            mouseState = Mouse.GetState();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            
            //remove unused entities
            if (playingState.EntitiesToDelete.Count > 0)
            {
                foreach (Guid entity in playingState.EntitiesToDelete)
                {
                    playingState.DestroyEntity(entity);
                }
            }

            //update camera
            Entity player = playingState.Entities.Where(x => (x.ComponentFlags & ComponentMasks.Player) == ComponentMasks.Player).FirstOrDefault();
            if (player != null)
            {
                Rectangle playerDestination = playingState.AABBComponents[player.ID].BoundedBox;
                followCamera.Update(gameTime, playerDestination, mapWidth, mapHeight);
            }

            //update player aabb collisions
            //AABBDetectionSystem.UpdateAABBPlayerCollision(playingState);

            //update movement
            PlayerInputSystem.HandlePlayerMovement(playingState, graphics, gameTime, keyboardState, mouseState, gamepadState, followCamera, levelRenderer.TileMap, levelCollisionDetection);


            //update player ai
            AISystem.UpdateEnemeyAI(playingState, gameTime, levelCollisionDetection, levelRenderer.TileMap.TileSize);

            //update projectiles
            ProjectileSystem.UpdateProjectiles(playingState);

            //handle projectile collision
            //AABBDetectionSystem.DetectAABBProjectileCollision(playingState);


            //empty unused entities
            playingState.EntitiesToDelete.Clear();

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: followCamera.transform);

            // TODO: Add your drawing code here
            levelRenderer.Draw(spriteBatch, spriteSheet, followCamera);

            //DisplaySystem.DrawEnemyNeighbors(playingState, spriteBatch, spriteSheet, graphics.GraphicsDevice);
            DisplaySystem.DrawPlayingStateDisplayEntities(playingState, followCamera, spriteBatch, spriteSheet, graphics);


            DisplaySystem.DrawEnemyLabelComponents(playingState, spriteBatch, spriteFont);

            //DisplaySystem.DrawEnemyActivePath(playingState, spriteBatch, spriteSheet, graphics.GraphicsDevice);
            //DisplaySystem.DrawDebugTiles(playingState, spriteBatch, spriteSheet, graphics.GraphicsDevice);
            //DisplaySystem.DrawAABBComponents(playingState, spriteBatch, graphics.GraphicsDevice);


            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
