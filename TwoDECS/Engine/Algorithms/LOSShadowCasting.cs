using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TwoDECS.Engine.World;

namespace TwoDECS.Engine.Algorithms
{
        
    public static class LineOfSiteRayCast
    {
        public static bool CalculateLineOfSight(Vector2 position, Vector2 positionToCheck, float lineOfSite, float direction, PlayingState playingState)
        {            
            //Texture2D blank = new Texture2D(graphics, 1, 1, false, SurfaceFormat.Color);
            //blank.SetData(new[] { Color.White });

            var degree = direction * (180 / Math.PI);
            var lowerDegree = degree - 45;
            var higherDegree = degree + 45;

            if (lowerDegree > higherDegree)
            {
                var temp = lowerDegree;
                lowerDegree = higherDegree;
                higherDegree = temp;
            }

            var higherRadian = higherDegree * (Math.PI / 180);
            Vector2 higherPoint = RotateVector2(new Vector2(position.X + lineOfSite, position.Y), (float)higherRadian, position);

            var lowerTracker = lowerDegree;

            Rectangle positionToCheckRect = new Rectangle((int)positionToCheck.X, (int)positionToCheck.Y, 16, 16);

            while (lowerTracker <= higherDegree)
            {
                //get if there is anything between our origina and lower tracker
                var lowerRadian = lowerTracker * (Math.PI / 180);
                Vector2 lowerPoint = RotateVector2(new Vector2(position.X + lineOfSite, position.Y), (float)lowerRadian, position);

                //DrawLine(spriteBatch, blank, 1f, Color.ForestGreen, position, lowerPoint); ;
                
                var collision = CheckIfPointOnLineIntersectsRect(position.X, position.Y, lowerPoint.X, lowerPoint.Y, positionToCheckRect);
                if (collision)
                {
                    return true;
                }
                lowerTracker++;
            }

            return false;

        }

        static void DrawLine(SpriteBatch batch, Texture2D blank, float width, Color color, Vector2 point1, Vector2 point2)
        {
            float angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
            float length = Vector2.Distance(point1, point2);

            batch.Draw(blank, point1, null, color, angle, Vector2.Zero, new Vector2(length, width), SpriteEffects.None, 0);
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
