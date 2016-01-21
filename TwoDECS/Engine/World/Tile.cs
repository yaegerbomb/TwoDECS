using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TwoDECS.Engine.Algorithms;

namespace TwoDECS.Engine.World
{
    public enum TileType
    {
        Solid,
        Ground
    }

    public class Tile : IPathNode<Object>
    {
        public string Name { get; set; }

        private bool isWalkable { get; set; }
        public bool IsWalkable(Object unused)
        {
            return isWalkable;
        }

        public Rectangle Destination { get; set; }
        public Rectangle Source { get; set; }

        public Vector2 TilePosition { get; set; }

        public TileType Type { get; set; }

        public Tile()
        {

        }

        public Tile(string name, int x, int y, int width, int height, Rectangle source, TileType type)
        {
            this.Name = name;
            this.Destination = new Rectangle(x, y, width, height);
            this.Type = type;
            this.Source = source;
            this.Destination = new Rectangle(0, 0, width, height);
            this.TilePosition = new Vector2(x * width, y * height);
            this.isWalkable = type == TileType.Ground;
        }

        
    }

    public class AStarSolver<Tile, TUserContext> : SpatialAStar<Tile, TUserContext> where Tile : IPathNode<TUserContext>
    {
        protected override Double Heuristic(PathNode inStart, PathNode inEnd)
        {
            return Math.Abs(inStart.X - inEnd.X) + Math.Abs(inStart.Y - inEnd.Y);
        }

        protected override Double NeighborDistance(PathNode inStart, PathNode inEnd)
        {
            return Heuristic(inStart, inEnd);
        }

        public AStarSolver(Tile[,] inGrid)
            : base(inGrid)
        {
        }
    }
}
