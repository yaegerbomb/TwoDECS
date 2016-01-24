using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TwoDECS.Engine.World
{
    public enum Direction
    {
        Up,
        Right,
        Down,
        Left
    }

    public class LevelCollisionDetection
    {
        TileMap TileMap;
        int TileSize;


        public LevelCollisionDetection(TileMap tileMap, int tileSize)
        {
            this.TileMap = tileMap;
            this.TileSize = tileSize;
        }

        public Rectangle CheckWallCollision(Rectangle boundedBox, Direction direction)
        {
            //normalize position to account for new origin (center of sprite)

            bool Finished = false;
            for(int x = 0; x < TileMap.RowCount; x++){
                if (Finished)
                {
                    break;
                }
                for (int y = 0; y < TileMap.ColumnCount; y++)
                {
                    if (Finished)
                    {
                        break;
                    }
                    Tile tile = TileMap.Map[x,y];
                    if (tile.Type == TileType.Solid)
                    {
                        if (((boundedBox.X + TileSize) > tile.TilePosition.X) && (boundedBox.X < (tile.TilePosition.X + TileSize)) && (boundedBox.Y + TileSize) > tile.TilePosition.Y && boundedBox.Y < (tile.TilePosition.Y + TileSize))
                        {
                            switch (direction)
                            {
                                case Direction.Up:
                                    boundedBox.Y = ((int)tile.TilePosition.Y + TileSize);
                                    break;
                                case Direction.Right:
                                    boundedBox.X = ((int)tile.TilePosition.X - TileSize);
                                    break;
                                case Direction.Down:
                                    boundedBox.Y = ((int)tile.TilePosition.Y - TileSize);
                                    break;
                                case Direction.Left:
                                    boundedBox.X = ((int)tile.TilePosition.X + TileSize);
                                    break;
                            }
                            Finished = true;
                        }
                    }
                }
            }
            return boundedBox;
        }

        public bool CheckBulletToWallCollision(Vector2 position)
        {
            //normalize position to account for new origin (center of sprite)

            for (int x = 0; x < TileMap.RowCount; x++)
            {
                for (int y = 0; y < TileMap.ColumnCount; y++)
                {
                    Tile tile = TileMap.Map[x, y];
                    if (tile.Type == TileType.Solid)
                    {
                        if (((position.X + TileSize) > tile.TilePosition.X) && (position.X < (tile.TilePosition.X + TileSize)) && (position.Y + TileSize) > tile.TilePosition.Y && position.Y < (tile.TilePosition.Y + TileSize))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
