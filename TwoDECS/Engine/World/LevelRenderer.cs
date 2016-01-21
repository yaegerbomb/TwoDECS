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

namespace TwoDECS.Engine.World
{
    class LevelRenderer
    {
        public TileMap TileMap { get; set; }
        private SpriteFont Font;

        public LevelRenderer()
        {

        }


        //import csv file
        public void ImportMap(ContentManager content, string filepath, int tileSize, SpriteFont font)
        {
            var reader = new StreamReader(File.OpenRead(filepath));

            List<List<Tile>> Tiles = new List<List<Tile>>();

            Point playerSpawnPoint = new Point();
            List<Point> enemySpawnPoints = new List<Point>();
            List<Rectangle> BoundingBoxes = new List<Rectangle>();
            int x = 0;
            int y = 0;
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');
                List<Tile> row = new List<Tile>();
                foreach (string value in values)
                {
                    if (Int32.Parse(value) == 907)
                    {
                        row.Add(new Tile("", x, y, tileSize, tileSize, new Rectangle(126, 540, tileSize, tileSize), TileType.Ground));
                    }
                    else if (Int32.Parse(value) == 904)
                    {
                        row.Add(new Tile("", x, y, tileSize, tileSize, new Rectangle(72, 540, tileSize, tileSize), TileType.Solid));
                    }
                    else if (Int32.Parse(value) == 805)
                    {
                        playerSpawnPoint = new Point(x, y);
                        row.Add(new Tile("", x, y, tileSize, tileSize, new Rectangle(126, 540, tileSize, tileSize), TileType.Ground));
                    }
                    else if (Int32.Parse(value) == 799)
                    {
                        enemySpawnPoints.Add(new Point(x, y));
                        row.Add(new Tile("", x, y, tileSize, tileSize, new Rectangle(126, 540, tileSize, tileSize), TileType.Ground));
                    }

                    x++;
                }
                Tiles.Add(row);
                x = 0;
                y++;
            }

            Tile[,] tiles = new Tile[Tiles.Count(), Tiles.First().Count()];
            x = 0;
            y = 0;
            foreach (List<Tile> t in Tiles)
            {
                foreach (Tile row in t)
                {
                    tiles[x, y] = row;
                    x++;
                }
                x = 0;
                y++;
            }

            this.Font = font;
            this.TileMap = new TileMap(tiles, tiles.GetLength(0), tiles.GetLength(1), tileSize, playerSpawnPoint, enemySpawnPoints);
        }

        public void CreateLevelObjects(PlayingState playingState, string filepath)
        {
            var reader = new StreamReader(File.OpenRead(filepath));
            List<Rectangle> rectangles = new List<Rectangle>();
            int x = 0;
            int y = 0;
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');
                rectangles.Add(new Rectangle(Int32.Parse(values[0]), Int32.Parse(values[1]), Int32.Parse(values[2]), Int32.Parse(values[3])));
            }


            foreach (Rectangle rectangle in rectangles)
            {
                Guid id = playingState.CreateEntity();
                playingState.Entities.Where(l => l.ID == id).First().ComponentFlags = ComponentMasks.LevelObjects;

                Vector2 position = new Vector2(rectangle.X * TileMap.TileSize, rectangle.Y * TileMap.TileSize);
                Rectangle destination = new Rectangle(rectangle.X * TileMap.TileSize, rectangle.Y * TileMap.TileSize, (rectangle.Width - rectangle.X + 1) * TileMap.TileSize, (rectangle.Height - rectangle.Y + 1) * TileMap.TileSize);

                playingState.DirectionComponents[id] = new DirectionComponent() { Direction = 0f};
                playingState.DisplayComponents[id] = new DisplayComponent() { Source = new Rectangle(342, 108, TileMap.TileSize, TileMap.TileSize) };
                playingState.PositionComponents[id] = new PositionComponent() { Position = position, Destination = destination };
            }
        }

        public void Initialize(Tile[,] tileMap, int tileSize, SpriteFont font)
        {
            this.TileMap = new TileMap(tileMap, tileMap.GetLength(0), tileMap.GetLength(1), tileSize);
            this.Font = font;
            for (int i = 0; i < TileMap.RowCount; i++)
            {
                for (int j = 0; j < TileMap.ColumnCount; j++)
                {
                    int width = TileMap.Map[i, j].Destination.Width;
                    int height = TileMap.Map[i, j].Destination.Height;
                    float posX = i * width;
                    float posY = j * height;
                    TileMap.Map[i, j].TilePosition = new Vector2(posX, posY);
                }
            }
        }

        public void Update()
        {

        }

        //public void Draw(SpriteBatch spriteBatch, Texture2D spriteSheet, FreeRangeCamera camera)
        //{
        //    //clip our x and y bounds to camera view
        //    //Point minPoints = camera.GetRenderRangeMin(TileMap.TileSize, TileMap.RowCount);
        //    //Point maxPoints = camera.GetRenderRangeMax(TileMap.TileSize, TileMap.RowCount, minPoints);
        //    for (int i = 0; i < TileMap.RowCount; i++)
        //    {
        //        for (int j = 0; j < TileMap.ColumnCount; j++)
        //        {
        //            camera.IsInView(TileMap.Map[i, j].TilePosition, 48);
        //            spriteBatch.Draw(spriteSheet, TileMap.Map[i, j].TilePosition, TileMap.Map[i, j].Source, Color.White);
        //            //spriteBatch.DrawString(Font, "(" + i + "," + j + ")", TileMap.Map[i, j].TilePosition, Color.White);
        //            //spriteBatch.DrawString(Font, "(" + TileMap.Map[i, j].TilePosition.X + "," + TileMap.Map[i, j].TilePosition.Y + ")", new Vector2(TileMap.Map[i, j].TilePosition.X, TileMap.Map[i, j].TilePosition.Y + 16), Color.White);
        //        }
        //    }
        //}

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
