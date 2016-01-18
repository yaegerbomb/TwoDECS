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

        public Vector2 CheckWallCollision(Vector2 position, Direction direction)
        {
            //normalize position to account for new origin (center of sprite)

            bool Finished = false;
            for(int x = 0; x < TileMap.RowCount; x++){
                if (Finished)
                {
                    break;
                }
                for(int y = 0; y < TileMap.ColumnCount; y++){
                    Tile tile = TileMap.Map[x,y];
                    if (tile.Type == TileType.Solid)
                    {
                        if (((position.X + TileSize) > tile.TilePosition.X) && (position.X < (tile.TilePosition.X + TileSize)) && (position.Y + TileSize) > tile.TilePosition.Y && position.Y < (tile.TilePosition.Y + TileSize))
                        {
                            switch (direction)
                            {
                                case Direction.Up:
                                    position.Y = (tile.TilePosition.Y + TileSize);
                                    break;
                                case Direction.Right:
                                    position.X = (tile.TilePosition.X - TileSize);
                                    break;
                                case Direction.Down:
                                    position.Y = (tile.TilePosition.Y - TileSize);
                                    break;
                                case Direction.Left:
                                    position.X = (tile.TilePosition.X + TileSize);
                                    break;
                            }
                            Finished = true;
                        }
                    }
                }
            }
            return position;
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
