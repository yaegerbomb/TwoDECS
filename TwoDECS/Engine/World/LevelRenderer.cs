using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TwoDECS.Engine.Cameras;
using TwoDECS.Engine.Components;
using TiledSharp;
using TwoDECS.Engine.Systems.AI;

namespace TwoDECS.Engine.World
{
    struct SpriteHolder{
        public Vector2 origin;
    }

    class LevelRenderer
    {
        public TileMap TileMap { get; set; }
        public Dictionary<int, SpriteHolder> SpritePositions { get; set; }
        private SpriteFont Font;
        private Random random;

        public LevelRenderer()
        {
            random = new Random();
        }

        private void GetSpriteCoordinates(int ID)
        {
            int row = (int)Math.Floor((double)(ID / TileMap.ColumnCount));
            int column = ID % TileMap.ColumnCount;
            int xCoord = column * 16;
            int yCoord = row * 16;
        }

        public void CreateTileMap(TmxTileset tmxTileSet)
        {
            TileMap.TileSize = tmxTileSet.TileWidth;
            
            int columnCount = tmxTileSet.Columns;
            int? NumOfIds = tmxTileSet.TileCount;

            //create our sprite positions
            SpritePositions = new Dictionary<int, SpriteHolder>();
            for (int i = 0; i < NumOfIds; i++)
            {
                int row = (int)Math.Floor((double)(i / columnCount));
                int column = i % columnCount;
                int x = column * TileMap.TileSize;
                int y = row * TileMap.TileSize;

                SpritePositions[i] = new SpriteHolder()
                {
                    origin = new Vector2(x + (2 * column), y + (2 * row))
                };
            }
        }

        public void CreateTileArray(TmxLayer levelLayer)
        {
            Tile[,] tiles = new Tile[TileMap.RowCount, TileMap.ColumnCount];
            for (var i = 0; i < levelLayer.Tiles.Count; i++)
            {
                //our value
                int gid = levelLayer.Tiles[i].Gid;
                if (gid != 0)
                {
                    gid--;
                    SpriteHolder spriteHolder = SpritePositions[gid];
                    //we are a ground type else solid
                    if (gid == 434)
                    {
                        tiles[levelLayer.Tiles[i].X, levelLayer.Tiles[i].Y] = new Tile("", levelLayer.Tiles[i].X, levelLayer.Tiles[i].Y, TileMap.TileSize, TileMap.TileSize, new Rectangle(spriteHolder.origin.ToPoint().X, spriteHolder.origin.ToPoint().Y, TileMap.TileSize, TileMap.TileSize), TileType.Ground);
     
                    }
                    else
                    {
                        tiles[levelLayer.Tiles[i].X, levelLayer.Tiles[i].Y] = new Tile("", levelLayer.Tiles[i].X, levelLayer.Tiles[i].Y, TileMap.TileSize, TileMap.TileSize, new Rectangle(spriteHolder.origin.ToPoint().X, spriteHolder.origin.ToPoint().Y, TileMap.TileSize, TileMap.TileSize), TileType.Solid);
                    }                    
                }
                else
                {
                    SpriteHolder spriteHolder = SpritePositions[301];
                    int size = random.Next(0, 5);
                    tiles[levelLayer.Tiles[i].X, levelLayer.Tiles[i].Y] = new Tile("", levelLayer.Tiles[i].X, levelLayer.Tiles[i].Y, TileMap.TileSize, TileMap.TileSize, new Rectangle(spriteHolder.origin.ToPoint().X, spriteHolder.origin.ToPoint().Y, size, size), TileType.Solid);
                }
            }

            TileMap.Map = tiles;
            TileMap.aStar = new AStarSolver<Tile, Object>(TileMap.Map);
        }

        public void CreateBoundingRects(TmxList<TmxObject> rectangles, PlayingState playingState)
        {
            foreach (TmxObject rectangle in rectangles)
            {
                Guid id = playingState.CreateEntity();
                playingState.Entities.Where(l => l.ID == id).First().ComponentFlags = ComponentMasks.LevelObjects;
                Vector2 position = new Vector2((float)rectangle.X, (float)rectangle.Y);
                Rectangle boundedBox = new Rectangle((int)rectangle.X, (int)rectangle.Y, (int)rectangle.Width, (int)rectangle.Height);

                playingState.DirectionComponents[id] = new DirectionComponent() { Direction = 0f };
                playingState.DisplayComponents[id] = new DisplayComponent() { Source = new Rectangle(342, 108, TileMap.TileSize, TileMap.TileSize) };
                playingState.PositionComponents[id] = new PositionComponent() { Position = position };
                playingState.AABBComponents[id] = new AABBComponent() { BoundedBox = boundedBox };
                playingState.MapObjectComponents[id] = new MapObjectComponent() { Type = MapObjectType.Wall };
            }
        }

        public void CreatePlayerSpawn(TmxList<TmxObject> playerSpawns, PlayingState playingState)
        {
            foreach (TmxObject playerspawn in playerSpawns)
            {
                Guid id = playingState.CreateEntity();
                playingState.Entities.Where(x => x.ID == id).First().ComponentFlags = ComponentMasks.Player;

                playingState.DirectionComponents[id] = new DirectionComponent() { Direction = 0f };
                playingState.DisplayComponents[id] = new DisplayComponent() { Source = new Rectangle(451, 470, TileMap.TileSize, TileMap.TileSize) };
                playingState.HealthComponents[id] = new HealthComponent() { Health = 100 };
                playingState.PositionComponents[id] = new PositionComponent() { Position = new Vector2((float)playerspawn.X, (float)playerspawn.Y) };
                playingState.VelocityComponents[id] = new VelocityComponent() { xVelocity = 0f, yVelocity = 0f, xTerminalVelocity = 6f, yTerminalVelocity = 6f };
                playingState.AccelerationComponents[id] = new AccelerationComponent() { xAcceleration = 6f, yAcceleration = 6f };

                //our origin is going to offset our bounded box
                playingState.AABBComponents[id] = new AABBComponent(){ BoundedBox = new Rectangle((int)playerspawn.X - (TileMap.TileSize / 2), (int)playerspawn.Y - (TileMap.TileSize / 2), TileMap.TileSize, TileMap.TileSize)};
            }
        }

        public void CreateEnemySpawns(TmxList<TmxObject> enemySpawns, PlayingState playingState)
        {
            LevelCollisionDetection levelCollisionDetection = new LevelCollisionDetection(TileMap, TileMap.TileSize);
            foreach (TmxObject enemypawn in enemySpawns)
            {
                Guid id = playingState.CreateEntity();
                playingState.Entities.Where(x => x.ID == id).First().ComponentFlags = ComponentMasks.Enemy;                

                playingState.DirectionComponents[id] = new DirectionComponent() { Direction = (float)(enemypawn.Rotation * (Math.PI / 180)) };
                playingState.DisplayComponents[id] = new DisplayComponent() { Source = new Rectangle(343, 470, TileMap.TileSize, TileMap.TileSize) };
                playingState.HealthComponents[id] = new HealthComponent() { Health = 15 };
                playingState.PositionComponents[id] = new PositionComponent() { Position = new Vector2((float)enemypawn.X + (TileMap.TileSize / 2), (float)enemypawn.Y + (TileMap.TileSize / 2)) };
                playingState.VelocityComponents[id] = new VelocityComponent() { xVelocity = 0f, yVelocity = 0f, xTerminalVelocity = 3f, yTerminalVelocity = 3f };
                playingState.AccelerationComponents[id] = new AccelerationComponent() { xAcceleration = 3f, yAcceleration = 3f };
                playingState.DamageComponents[id] = new DamageComponent()
                {
                    AttackRange = 1 * TileMap.TileSize,
                    Damage = 5
                };

                //our origin is going to offset our bounded box
                playingState.AABBComponents[id] = new AABBComponent() { BoundedBox = new Rectangle((int)enemypawn.X, (int)enemypawn.Y, TileMap.TileSize, TileMap.TileSize) };

                //parse patrol paths
                string patrolPaths = enemypawn.Properties["PatrolPath"];
                List<Vector2> PatrolVectors = new List<Vector2>();
                if (!string.IsNullOrWhiteSpace(patrolPaths))
                {
                    string[] newPaths = patrolPaths.Split('|'); 
                    
                    for (int i = 0; i < newPaths.Length; i++)
                    {
                        newPaths[i] = newPaths[i].Replace("(", "");
                        newPaths[i] = newPaths[i].Replace(")", "");
                        int x = Int32.Parse(newPaths[i].Split(',')[0]);
                        int y = Int32.Parse(newPaths[i].Split(',')[1]);

                        //center our points too
                        x = (x * TileMap.TileSize) + (TileMap.TileSize / 2);
                        y = (y * TileMap.TileSize) + (TileMap.TileSize / 2);

                        PatrolVectors.Add(new Vector2(x, y));
                    }
                }

                playingState.AIComponents[id] = new AIComponent()
                {
                    ActiveState = new Stack<AIState>(),
                    LineOfSight = 6 * TileMap.TileSize,
                    PatrolPath = PatrolVectors,
                    ActivePath = new LinkedList<Tile>(),
                    Astar = TileMap.aStar,
                    AITree = AIPawnSystem.CreatePawnTree(id, playingState, levelCollisionDetection, TileMap.TileSize, TileMap.TileSize)
                };
                playingState.AIComponents[id].ActiveState.Push(AIState.STILL);
                playingState.LabelComponents[id] = new LabelComponent() { Label = "", Position = new Vector2((float)enemypawn.X, (float)(enemypawn.Y - 5)) };
            }
        }

        public void ImportTMX(ContentManager content, string filepath, int tilesize, SpriteFont font, PlayingState playingState)
        {
            TileMap = new TileMap();

            var map = new TmxMap(filepath);
            TileMap.RowCount = map.Width;
            TileMap.ColumnCount = map.Height;


            //get and set tile map details
            var tileMap = map.Tilesets["Basic"];
            CreateTileMap(tileMap);

            //get and set level layer
            var levelLayer = map.Layers["Level"];
            CreateTileArray(levelLayer);

            //get and set level bounding rects
            var boudingRects = map.ObjectGroups["Level Rects"].Objects;
            CreateBoundingRects(boudingRects, playingState);

            //get and set player spawn
            var playersSpawn = map.ObjectGroups["Player Spawn"].Objects;
            CreatePlayerSpawn(playersSpawn, playingState);

            //get and set enemy spawns
            var enemySpawns = map.ObjectGroups["Enemy Spawns"].Objects;
            CreateEnemySpawns(enemySpawns, playingState);

            this.Font = font;
        }      

        public void Update()
        {

        }
        
        public void Draw(SpriteBatch spriteBatch, Texture2D spriteSheet, FollowCamera camera)
        {
            for (int i = 0; i < TileMap.RowCount; i++)
            {
                for (int j = 0; j < TileMap.ColumnCount; j++)
                {
                    spriteBatch.Draw(spriteSheet, TileMap.Map[i, j].TilePosition, TileMap.Map[i, j].Source, Color.White);
                    //spriteBatch.DrawString(Font, "(" + i + "," + j + ")", TileMap.Map[i, j].TilePosition, Color.White);
                }
            }
        }
    }
}
