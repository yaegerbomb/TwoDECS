using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TwoDECS.Engine.Components;
using TwoDECS.Engine.World;

namespace TwoDECS.Engine.Algorithms
{

    public struct Line
    {
        public int XMin;
        public int XMax;

        public int YMin;
        public int YMax;

        public Line(Point a, Point b) {
            XMin = Math.Min(a.X, b.X);
            XMax = Math.Max(a.X, b.X);
            YMin = Math.Min(a.Y, b.Y);
            YMax = Math.Max(a.Y, b.Y);
        }

        public float CalculateYForX(int x) 
        {  
            //y = mx + b
            if (this.XMax - this.XMin == 0)
            {
                return 0;
            }
            var m = (this.YMax - this.YMax) / (this.XMax - this.XMin);
            return m * x;
        }
    }

    public static class LineOfSiteRayCast
    {
        //return true if we can get from one point to the other in a straight line without running into anything
        public static bool LineOfSight(Vector2 startPosition, Vector2 endPosition, PlayingState playingState)
        {
            IEnumerable<Guid> levelObjectEntities = playingState.Entities.Where(x => (x.ComponentFlags & ComponentMasks.LevelObjects) == ComponentMasks.LevelObjects).Select(x => x.ID);

            foreach (Guid loid in levelObjectEntities)
            {
                if (LineIntersectsRect(startPosition.ToPoint(), endPosition.ToPoint(), playingState.AABBComponents[loid].BoundedBox))
                {
                    return false;
                }

            }

            return true;

        }



        public static bool LineIntersectsRect(Point p1, Point p2, Rectangle r)
        {
            return LineIntersectsLine(p1, p2, new Point(r.X, r.Y), new Point(r.X + r.Width, r.Y)) ||
                   LineIntersectsLine(p1, p2, new Point(r.X + r.Width, r.Y), new Point(r.X + r.Width, r.Y + r.Height)) ||
                   LineIntersectsLine(p1, p2, new Point(r.X + r.Width, r.Y + r.Height), new Point(r.X, r.Y + r.Height)) ||
                   LineIntersectsLine(p1, p2, new Point(r.X, r.Y + r.Height), new Point(r.X, r.Y)) ||
                   (r.Contains(p1) && r.Contains(p2));
        }

        private static bool LineIntersectsLine(Point l1p1, Point l1p2, Point l2p1, Point l2p2)
        {
            float q = (l1p1.Y - l2p1.Y) * (l2p2.X - l2p1.X) - (l1p1.X - l2p1.X) * (l2p2.Y - l2p1.Y);
            float d = (l1p2.X - l1p1.X) * (l2p2.Y - l2p1.Y) - (l1p2.Y - l1p1.Y) * (l2p2.X - l2p1.X);

            if (d == 0)
            {
                return false;
            }

            float r = q / d;

            q = (l1p1.Y - l2p1.Y) * (l1p2.X - l1p1.X) - (l1p1.X - l2p1.X) * (l1p2.Y - l1p1.Y);
            float s = q / d;

            if (r < 0 || r > 1 || s < 0 || s > 1)
            {
                return false;
            }

            return true;
        }


        public static bool CalculateLineOfSight(Vector2 position, Vector2 positionToCheck, float lineOfSite, float direction, PlayingState playingState)
        {            
            var degree = direction * (180 / Math.PI);
            var lowerDegree = degree - 45;
            var higherDegree = degree + 45;

            if (lowerDegree > higherDegree)
            {
                Swap(ref lowerDegree, ref higherDegree);
            }

            var higherRadian = higherDegree * (Math.PI / 180);
            Vector2 higherPoint = RotateVector2(new Vector2(position.X + lineOfSite, position.Y), (float)higherRadian, position);


            Rectangle positionToCheckRect = new Rectangle((int)positionToCheck.X, (int)positionToCheck.Y, 16, 16);

            //get neighboring squares by bounded box
            Rectangle neighborBoundedBox = new Rectangle((int)(position.X - (lineOfSite)), (int)(position.Y - (lineOfSite)), (int)(lineOfSite * 2), (int)(lineOfSite * 2));

            IEnumerable<Guid> levelObjectEntities = playingState.Entities.Where(x => (x.ComponentFlags & ComponentMasks.LevelObjects) == ComponentMasks.LevelObjects).Select(x => x.ID);
            List<Rectangle> levelAABBBoundedRects = playingState.AABBComponents.Where(a => levelObjectEntities.Contains(a.Key) && a.Value.BoundedBox.Intersects(neighborBoundedBox)).Select(x => x.Value).Select(s => s.BoundedBox).ToList();


            var lowerTracker = lowerDegree;
            while (lowerTracker <= higherDegree)
            {
                //get if there is anything between our origina and lower tracker
                var lowerRadian = lowerTracker * (Math.PI / 180);
                Vector2 lowerPoint = RotateVector2(new Vector2(position.X + lineOfSite, position.Y), (float)lowerRadian, position);

                var collision = CheckIfPointOnLineIntersectsRect(position.X, position.Y, lowerPoint.X, lowerPoint.Y, position, positionToCheckRect, playingState, levelAABBBoundedRects);
                if (collision)
                {
                    return true;
                }
                lowerTracker+=5;
            }

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

        public static void Swap<T>(ref T a, ref T b)
        {
            T temp = a;
            a = b;
            b = temp;
        }

        public static bool CheckIfPointOnLineIntersectsRect(float x1, float y1, float x2, float y2, Vector2 startingPosition, Rectangle rectangle, PlayingState playingState, List<Rectangle> levelAABBComponents)
        {
            List<Vector2> IntersecetionPointsToCheck = new List<Vector2>();

            // Bresenham's line algorithm - get all the point on the line between our 2 points
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
                    IntersecetionPointsToCheck.Add(new Vector2(y, x));
                }
                else
                {
                    IntersecetionPointsToCheck.Add(new Vector2(x, y));
                }

                error -= dy;
                if (error < 0)
                {
                    y += ystep;
                    error += dx;
                }
            }

            //get the points that intersect with our wall
            List<Vector2> pointThatCollidedWithWall = new List<Vector2>();
            List<Vector2> pointThatCollidedWithPlayer = new List<Vector2>();

            foreach (Vector2 point in IntersecetionPointsToCheck)
            {
                if (levelAABBComponents.Where(s => s.Contains(point)).Count() > 0)
                {
                    pointThatCollidedWithWall.Add(point);
                }

                if (rectangle.Contains(point))
                {
                    pointThatCollidedWithPlayer.Add(point);
                }
            }

            //if we potentially hit a wall and a player we need to see if the distance from the wall is less than the distance from the player
            //if we our wall wins, we dont see the player
            if (pointThatCollidedWithWall.Count > 0 && pointThatCollidedWithPlayer.Count > 0)
            {
                //sort arrays by shorted distance from enemy
                pointThatCollidedWithWall.Sort(delegate(Vector2 v1, Vector2 v2)
                {
                    if (Vector2.Distance(startingPosition, v1) < Vector2.Distance(startingPosition, v2))
                    {
                        return -1;
                    }
                    else
                    {
                        return 1;
                    }
                });

                pointThatCollidedWithPlayer.Sort(delegate(Vector2 v1, Vector2 v2)
                {
                    if (Vector2.Distance(startingPosition, v1) < Vector2.Distance(startingPosition, v2))
                    {
                        return -1;
                    }
                    else
                    {
                        return 1;
                    }
                });

                //because its easier....
                Vector2 lowestDistanceFromWall = pointThatCollidedWithWall.First();
                Vector2 LowestDistanceFromPlayer = pointThatCollidedWithPlayer.First();

                if (lowestDistanceFromWall != Vector2.Zero && LowestDistanceFromPlayer != Vector2.Zero)
                {
                    //is our smallest wall distance less than our smallest player distance????
                    if (Vector2.Distance(startingPosition, lowestDistanceFromWall) < Vector2.Distance(startingPosition, LowestDistanceFromPlayer))
                    {
                        //if our wall wins we dont see the player
                        return false;
                    }
                }
            }

            //if we saw the player and we didnt see any walls than we saw the player
            if (pointThatCollidedWithPlayer.Count > 0)
            {
                return true;
            }


            //we didnt see anything
            return false;
        }
    }    
}
