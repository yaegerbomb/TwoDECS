using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TwoDECS.Engine.World
{
    public class TileMap
    {
        public Tile[,] Map { get; set; }
        public int RowCount { get; set; }
        public int ColumnCount { get; set; }
        public int TileSize { get; set; }
        public Point PlayerSpawn { get; set; }
        public List<Point> EnemySpawns { get; set; }
        public AStarSolver<Tile, Object> aStar { get; set; }

        public TileMap()
        {

        }

        public TileMap(Tile[,] tileMap, int rowCount, int columnCount, int tileSize)
        {
            this.Map = tileMap;
            this.RowCount = rowCount;
            this.ColumnCount = columnCount;
            this.TileSize = tileSize;
        }

        public TileMap(Tile[,] tileMap, int rowCount, int columnCount, int tileSize, Point playerSpawn, List<Point> enemySpawns)
        {
            this.Map = tileMap;
            this.RowCount = rowCount;
            this.ColumnCount = columnCount;
            this.TileSize = tileSize;
            this.PlayerSpawn = playerSpawn;
            this.EnemySpawns = enemySpawns;
        }
    }
}
