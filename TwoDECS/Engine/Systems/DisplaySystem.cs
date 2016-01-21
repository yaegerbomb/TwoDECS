using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TwoDECS.Engine.Cameras;

namespace TwoDECS.Engine.Systems
{
    public static class DisplaySystem
    {
        public static void DrawPlayingStateDisplayEntities(PlayingState playingState, FollowCamera followCamera, SpriteBatch spriteBatch, Texture2D spriteSheet, GraphicsDeviceManager graphicsDevice)
        {
            IEnumerable<Guid> drawableEntities = playingState.Entities.Where(x => (x.ComponentFlags & ComponentMasks.Drawable) == ComponentMasks.Drawable).Select(x => x.ID);
            foreach (Guid id in drawableEntities)
            {
                Rectangle source = playingState.DisplayComponents[id].Source;
                Vector2 origin = new Vector2(source.Width / 2, source.Height / 2);
                spriteBatch.Draw(spriteSheet, playingState.PositionComponents[id].Destination, source, Color.White, playingState.DirectionComponents[id].Direction, origin, SpriteEffects.None, 0);


                Circle circle = new Circle(playingState.PositionComponents[id].Position.X, playingState.PositionComponents[id].Position.X, 6 * 16, Color.Red, graphicsDevice);
                circle.Draw();
            }
        }


        public static void DrawLevelObjects(PlayingState playingState, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            Texture2D rect = new Texture2D(graphicsDevice, 80, 30);

            Color[] data = new Color[80 * 30];
            for (int i = 0; i < data.Length; ++i) data[i] = Color.Chocolate;
            rect.SetData(data);

            IEnumerable<Guid> drawableEntities = playingState.Entities.Where(x => (x.ComponentFlags & ComponentMasks.LevelObjects) == ComponentMasks.LevelObjects).Select(x => x.ID);
            foreach (Guid id in drawableEntities)
            {
                Rectangle destination = playingState.PositionComponents[id].Destination;

                spriteBatch.Draw(rect, destination, Color.White);

            }
        }

        public class Circle
        {
            public Circle(float x, float y, int radius,
                GraphicsDeviceManager graphics)
                : this(x, y, radius, Color.White, graphics) { }

            public Circle(float x, float y, int radius,
                Color color, GraphicsDeviceManager graphics)
            {
                this.x = x;
                this.y = y;
                this.radius = radius;
                this.color = color;
                this.graphics = graphics;

                Initialize();
            }

            public void Draw()
            {
                effect.CurrentTechnique.Passes[0].Apply();
                graphics.GraphicsDevice.DrawUserPrimitives
                    (PrimitiveType.LineStrip, vertices, 0, vertices.Length - 1);
            }

            private void Initialize()
            {
                InitializeBasicEffect();
                InitializeVertices();
            }

            private void InitializeBasicEffect()
            {
                effect = new BasicEffect(graphics.GraphicsDevice);
                effect.VertexColorEnabled = true;
                effect.Projection = Matrix.CreateOrthographicOffCenter
                    (0, graphics.GraphicsDevice.Viewport.Width,
                     graphics.GraphicsDevice.Viewport.Height, 0,
                     0, 1);
            }

            private void InitializeVertices()
            {
                vertices = new VertexPositionColor[CalculatePointCount()];
                var pointTheta = ((float)Math.PI * 2) / (vertices.Length - 1);
                for (int i = 0; i < vertices.Length; i++)
                {
                    var theta = pointTheta * i;
                    var x = X + ((float)Math.Sin(theta) * Radius);
                    var y = Y + ((float)Math.Cos(theta) * Radius);
                    vertices[i].Position = new Vector3(x, y, 0);
                    vertices[i].Color = Color;
                }
                vertices[vertices.Length - 1] = vertices[0];
            }

            private int CalculatePointCount()
            {
                return (int)Math.Ceiling(Radius * Math.PI);
            }

            private GraphicsDeviceManager graphics;
            private VertexPositionColor[] vertices;
            private BasicEffect effect;

            private float x;
            public float X
            {
                get { return x; }
                set { x = value; InitializeVertices(); }
            }
            private float y;
            public float Y
            {
                get { return y; }
                set { y = value; InitializeVertices(); }
            }
            private float radius;
            public float Radius
            {
                get { return radius; }
                set { radius = (value < 1) ? 1 : value; InitializeVertices(); }
            }
            private Color color;
            public Color Color
            {
                get { return color; }
                set { color = value; InitializeVertices(); }
            }
            public int Points
            {
                get { return CalculatePointCount(); }
            }
        }
    }
}
