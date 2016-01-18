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

        AStarSolver<Tile, Object> aStar;

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
            levelRenderer.ImportMap(Content, "Engine\\World\\Levels\\TestMap.csv", tileSize, spriteFont);

            levelCollisionDetection = new LevelCollisionDetection(levelRenderer.TileMap, tileSize);


            aStar = new AStarSolver<Tile, Object>(levelRenderer.TileMap.Map);

            Vector2 playerSpawn = levelRenderer.TileMap.PlayerSpawn.ToVector2();
            playerSpawn.X *= tileSize;
            playerSpawn.Y *= tileSize;

            CreatePlayer(playerSpawn, tileSize);

            foreach (Point enemy in levelRenderer.TileMap.EnemySpawns)
            {
                Vector2 enemySpawn = enemy.ToVector2();
                enemySpawn.X *= tileSize;
                enemySpawn.Y *= tileSize;
                CreateEnemy(enemySpawn, tileSize);
            }

        }

        public void CreatePlayer(Vector2 position, int tileSize)
        {
            Guid id = playingState.CreateEntity();
            playingState.Entities.Where(x => x.ID == id).First().ComponentFlags = ComponentMasks.Player;

            playingState.DirectionComponents[id] = new DirectionComponent() { Direction = 0f };
            playingState.DisplayComponents[id] = new DisplayComponent() { Source = new Rectangle(451, 470, tileSize, tileSize) };
            playingState.HealthComponents[id] = new HealthComponent() { Health = 100 };
            playingState.PositionComponents[id] = new PositionComponent() { Position = position, Destination = new Rectangle((int)position.X, (int)position.Y, tileSize, tileSize) };
            playingState.SpeedComponents[id] = new SpeedComponent() { Speed = 8f };
            CreateWeapon(position, 0f, .25f, 30, id);
        }

        public void CreateEnemy(Vector2 position, int tileSize)
        {
            Guid id = playingState.CreateEntity();
            playingState.Entities.Where(x => x.ID == id).First().ComponentFlags = ComponentMasks.Enemy;

            playingState.DirectionComponents[id] = new DirectionComponent() { Direction = 0f };
            playingState.DisplayComponents[id] = new DisplayComponent() { Source = new Rectangle(343, 470, tileSize, tileSize) };
            playingState.HealthComponents[id] = new HealthComponent() { Health = 15 };
            playingState.PositionComponents[id] = new PositionComponent() { Position = position, Destination = new Rectangle((int)position.X, (int)position.Y, tileSize, tileSize) };
            playingState.SpeedComponents[id] = new SpeedComponent() { Speed = 6f };
        }

        public void CreateWeapon(Vector2 position, float direction, float fireingRate, int ammo, Guid owner)
        {
            Guid id = playingState.CreateEntity();
            playingState.Entities.Where(x => x.ID == id).First().ComponentFlags = ComponentMasks.Weapon;


            playingState.DirectionComponents[id] = new DirectionComponent() { Direction = direction };
            playingState.DisplayComponents[id] = new DisplayComponent() { Source = new Rectangle(488, 434, tileSize, tileSize) };
            playingState.PositionComponents[id] = new PositionComponent() { Position = position, Destination = new Rectangle((int)position.X, (int)position.Y, tileSize, tileSize) };
            playingState.TimerComponents[id] = new TimerComponent() { CountDown = 1, TimerReset = .25f };
            playingState.OwnerComponents[id] = new OwnerComponent() { OwnerID = owner };

        }

        public void CreateProjectile(Vector2 position, float direction, float speed, int tileSize, Guid owner)
        {
            Guid id = playingState.CreateEntity();
            playingState.Entities.Where(x => x.ID == id).First().ComponentFlags = ComponentMasks.Projectile;


            playingState.DirectionComponents[id] = new DirectionComponent() { Direction = direction };
            playingState.DisplayComponents[id] = new DisplayComponent() { Source = new Rectangle(21, 504, tileSize, tileSize) };
            playingState.SpeedComponents[id] = new SpeedComponent() { Speed = speed };
            playingState.PositionComponents[id] = new PositionComponent() { Position = position, Destination = new Rectangle((int)position.X, (int)position.Y, tileSize, tileSize) };
            playingState.OwnerComponents[id] = new OwnerComponent() { OwnerID = owner };

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
            
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
                Rectangle playerDestination = playingState.PositionComponents[player.ID].Destination;
                followCamera.Update(gameTime, playerDestination, mapWidth, mapHeight);
            }

            //update movement
            PlayerInputSystem.HandlePlayerMovement(playingState, graphics, gameTime, keyboardState, mouseState, gamepadState, followCamera, levelRenderer.TileMap, levelCollisionDetection);

            //update projectiles
            ProjectileSystem.UpdateProjectiles(playingState);

            //handle projectile collision
            AABBDetectionSystem.DetectAABBProjectileCollision(playingState);

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
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: followCamera.transform); 

            // TODO: Add your drawing code here
            levelRenderer.Draw(spriteBatch, spriteSheet, followCamera);
            
            DisplaySystem.DrawPlayingStateDisplayEntities(playingState, followCamera, spriteBatch, spriteSheet);


            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
