using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TwoDECS.Engine.World;

namespace TwoDECS.Engine.Algorithms
{
    public class Cell
    {
        public int UpperShadowCount { get; set; }
        public int UpperShadowMax { get; set; }
        public int LowerShadowCount { get; set; }
        public int LowerShadowMax { get; set; }
        public bool Visible { get; set; }
        public bool Lit { get; set; }
        public bool LitDelay { get; set; }

        public Cell()
        {
            this.UpperShadowCount = this.UpperShadowMax = this.LowerShadowCount = this.LowerShadowMax = 0;
            this.Visible = true;
            this.Lit = true;
            LitDelay = false;
        }
    }

    class LOSShadowCasting
    {
        List<Cell> Cells { get; set; }
        bool VISIBLE_CORNER;
        bool BlOCKER;
        int UP_INCH;
        int LOW_INC;
        Tile SOUTH;

        public void CalculateLOSOctant(TileMap tileMap, Vector2 currentPosition, float direction)
        {
            Cells = new List<Cell>();
            Cells.Add(new Cell());

            for (int y = 0; y < tileMap.ColumnCount; y++)
            {
                for (int x = 0; x < tileMap.RowCount; x++)
                {
                    if (tileMap.Map[x, y].Type == TileType.Solid)
                    {
                        BlOCKER = true;
                    }
                    UP_INCH = 1;
                    LOW_INC = 1;
                    if (y < tileMap.ColumnCount && y > 0)
                    {
                        SOUTH = tileMap.Map[x, y - 1];
                    }

                    if (x < tileMap.RowCount)
                    {

                    }

                }
            }
        }

    }

    public struct Circle
    {
        public Circle(int x, int y, int radius)
            : this()
        {
            X = x;
            Y = y;
            Radius = radius;
        }

        public Circle(Vector2 position, float radius)
            : this()
        {
            X = (int)position.X;
            Y = (int)position.Y;
            Radius = (int)radius;
        }

        public int Radius { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }

        public bool Intersects(Rectangle rectangle)
        {
            int circleDistanceX = Math.Abs(this.X - rectangle.X);
            int circleDistanceY = Math.Abs(this.Y - rectangle.Y);

            if (circleDistanceX > (rectangle.Width / 2 + this.Radius))
            {
                return false;
            }

            if (circleDistanceY > (rectangle.Height / 2 + this.Radius))
            {
                return false;
            }

            if (circleDistanceX <= (rectangle.Width / 2))
            {
                return true;
            }

            if (circleDistanceY <= (rectangle.Height / 2))
            {
                return true;
            }

            var cornerDistance_sq = Math.Pow((circleDistanceX - rectangle.Width / 2), 2) + Math.Pow((circleDistanceY - rectangle.Height / 2), 2);
            return (cornerDistance_sq <= (this.Radius * this.Radius));
        }

        public bool Intersects(Circle circle)
        {
            // put simply, if the distance between the circle centre's is less than
            // their combined radius
            var centre0 = new Vector2(circle.X, circle.Y);
            var centre1 = new Vector2(X, Y);
            return Vector2.Distance(centre0, centre1) < Radius + circle.Radius;
        }

        public bool ContainsPoint(Point point)
        {
            var vector2 = new Vector2(point.X - X, point.Y - Y);
            return vector2.Length() <= Radius;
        }
    }

    public static class LineOfSiteRayCast
    {
        public static bool CalculateLineOfSight(Vector2 position, Vector2 positionToCheck, float lineifsight, float direction, PlayingState playingState)
        {
            ////draw circle at our position with our radius
            //Circle circle = new Circle(position, lineifsight * 16);

            //Rectangle positon2Rectangle = new Rectangle((int)positionToCheck.X, (int)positionToCheck.Y, 16, 16);

            //if (circle.Intersects(positon2Rectangle))
            //{
            //    Console.WriteLine("Player is in sight radius");
            //    return true;
            //}
            //else
            //{
            //    return false;
            //}

            var degree = direction * (180 / Math.PI);
            var lowerDegree = degree - 45;
            var higherDegree = degree + 45;

            //var lowerRadian = lowerDegree * (Math.PI / 180);
            //Vector2 lowerPoint = RotateVector2(new Vector2(position.X + lineifsight, position.Y), (float)lowerRadian, position);

            var higherRadian = higherDegree * (Math.PI / 180);
            Vector2 higherPoint = RotateVector2(new Vector2(position.X + lineifsight, position.Y), (float)higherRadian, position);

            var lowerTracker = lowerDegree;

            Rectangle positionToCheckRect = new Rectangle((int)positionToCheck.X, (int)positionToCheck.Y, 16, 16);

            while (lowerTracker != higherDegree)
            {
                //get if there is anything between our origina and lower tracker
                var lowerRadian = lowerDegree * (Math.PI / 180);
                Vector2 lowerPoint = RotateVector2(new Vector2(position.X + lineifsight, position.Y), (float)lowerRadian, position);
                var collision = CheckIfPointOnLineIntersectsRect(position.X, position.Y, lowerPoint.X, lowerPoint.Y, positionToCheckRect);
                if (collision)
                {
                    return true;
                }
                lowerTracker++;
            }

            //now that we have our lower and upper bounds, check 6 lines ahead of each and see if they intersect the player

            return false;

        }

        public static Vector2 RotateVector2(Vector2 point, float radians, Vector2 pivot)
        {
            float cosRadians = (float)Math.Cos(radians);
            float sinRadians = (float)Math.Sin(radians);

            Vector2 translatedPoint = new Vector2();
            translatedPoint.X = point.X - pivot.X;
            translatedPoint.Y = point.Y - pivot.Y;

            Vector2 rotatedPoint = new Vector2();
            rotatedPoint.X = translatedPoint.X * cosRadians - translatedPoint.Y * sinRadians + pivot.X;
            rotatedPoint.Y = translatedPoint.X * sinRadians + translatedPoint.Y * cosRadians + pivot.Y;

            return rotatedPoint;
        }

        // a1 is line1 start, a2 is line1 end, b1 is line2 start, b2 is line2 end
        static bool intersects(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
        {
            Vector2 intersection = Vector2.Zero;

            Vector2 b = Vector2.Subtract(a2, a1);
            Vector2 d = Vector2.Subtract(b2, b1);
            float bDotDPerp = b.X * d.Y - b.Y * d.X;

            // if b dot d == 0, it means the lines are parallel so have infinite intersection points
            if (bDotDPerp == 0)
                return false;

            Vector2 c = Vector2.Subtract(b1, a1);
            float t = (c.X * d.Y - c.Y * d.X) / bDotDPerp;
            if (t < 0 || t > 1)
                return false;

            float u = (c.X * b.Y - c.Y * b.X) / bDotDPerp;
            if (u < 0 || u > 1)
                return false;

            intersection = Vector2.Add(a1, Vector2.Multiply(b, t));

            return true;
        }

        static void Swap<T>(ref T a, ref T b)
        {
            T temp = a;
            a = b;
            b = temp;
        }

        static bool CheckIfPointOnLineIntersectsRect(float x1, float y1, float x2, float y2, Rectangle rectangle)
        {
            List<Vector2> IntersecetionPointsToCheck = new List<Vector2>();

            // Bresenham's line algorithm
            bool steep = (Math.Abs(y2 - y1) > Math.Abs(x2 - x1));
            if (steep)
            {
                Swap(ref x1, ref y1);
                Swap(ref x2, ref y2);
            }

            if (x1 > x2)
            {
                Swap(ref x1, ref x2);
                Swap(ref y1, ref y2);
            }

            float dx = x2 - x1;
            float dy = Math.Abs(y2 - y1);

            float error = dx / 2.0f;
            int ystep = (y1 < y2) ? 1 : -1;
            int y = (int)y1;

            int maxX = (int)x2;

            for (int x = (int)x1; x < maxX; x++)
            {
                if (steep)
                {
                    //check if we intersect
                    //SetPixel(y,x, color);
                    if(rectangle.Contains(y,x)){
                        return true;
                    }
                }
                else
                {
                    //check if we intersect
                    //SetPixel(x,y, color);
                    if(rectangle.Contains(x,y)){
                        return true;
                    }
                }

                error -= dy;
                if (error < 0)
                {
                    y += ystep;
                    error += dx;
                }
            }

            return false;
        }


    }
}
