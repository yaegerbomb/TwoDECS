using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TwoDECS.Engine.Cameras
{
    public class FollowCamera
    {
        public Matrix transform;
        public Viewport View;
        public Vector2 Center;

        public FollowCamera(Viewport view)
        {
            this.View = view;
        }

        public void Update(GameTime gameTime, Rectangle destination, int MapWidth, int MapHeight)
        {
            //bound camera to map constraints
            float xBounds = MathHelper.Clamp(destination.X + (destination.Width / 2) - (this.View.Width / 2), 0, (MapWidth / 2));
            float yBounds = MathHelper.Clamp(destination.Y + (destination.Height / 2) - (this.View.Height / 2), 0, MapHeight - View.Height);

            this.Center = new Vector2(xBounds, yBounds);
            transform = Matrix.CreateScale(new Vector3(1, 1, 0)) * Matrix.CreateTranslation(new Vector3(-Center.X, -Center.Y, 0));
        }
    }
}
