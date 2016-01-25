using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TwoDECS.Engine.Cameras;
using TwoDECS.Engine.Components;

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
                spriteBatch.Draw(spriteSheet, playingState.PositionComponents[id].Position, source, Color.White, playingState.DirectionComponents[id].Direction, origin, 1f, SpriteEffects.None, 0);

            }
        }

        public static void DrawEnemyNeighbors(PlayingState playingState, SpriteBatch spriteBatch, Texture2D spriteSheet, GraphicsDevice graphicsDevice)
        {
            Texture2D rect = new Texture2D(graphicsDevice, 80, 30);

            Color[] data = new Color[80 * 30];
            for (int i = 0; i < data.Length; ++i) data[i] = Color.Violet;
            rect.SetData(data);

            IEnumerable<Guid> enemies = playingState.Entities.Where(x => (x.ComponentFlags & ComponentMasks.Enemy) == ComponentMasks.Enemy).Select(x => x.ID);
            foreach (Guid enemyid in enemies)
            {
                var position = playingState.PositionComponents[enemyid].Position;
                var lineOfSite = playingState.AIComponents[enemyid].LineOfSite;
                Rectangle neighborBoundedBox = new Rectangle((int)(position.X - (lineOfSite)), (int)(position.Y - (lineOfSite)), lineOfSite * 2, lineOfSite * 2);
                spriteBatch.Draw(rect, neighborBoundedBox, Color.White);
            }
        }

        public static void DrawAABBComponents(PlayingState playingState, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            Texture2D rect = new Texture2D(graphicsDevice, 80, 30);

            Color[] data = new Color[80 * 30];
            for (int i = 0; i < data.Length; ++i) data[i] = Color.Chocolate;
            rect.SetData(data);

            var aabbComponents = playingState.AABBComponents;
            foreach (var aabb in aabbComponents)
            {
                Rectangle destination = aabb.Value.BoundedBox;

                spriteBatch.Draw(rect, destination, Color.White);

            }
        }

        public static void DrawEnemyLabelComponents(PlayingState playingState, SpriteBatch spriteBatch, SpriteFont spriteFont)
        {            
            IEnumerable<Guid> enemyEntities = playingState.Entities.Where(x => (x.ComponentFlags & ComponentMasks.Enemy) == ComponentMasks.Enemy).Select(x => x.ID);

            foreach (Guid enemyid in enemyEntities)
            {
                spriteBatch.DrawString(spriteFont, playingState.LabelComponents[enemyid].Label, playingState.PositionComponents[enemyid].Position, Color.Black);
            }

        }


        public static void DrawLine(SpriteBatch batch, float width, Color color, Vector2 point1, Vector2 point2, GraphicsDevice graphics)
        {
            Texture2D blank = new Texture2D(graphics, 1, 1, false, SurfaceFormat.Color);
            blank.SetData(new[] { Color.White });
            float angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
            float length = Vector2.Distance(point1, point2);

            batch.Draw(blank, point1, null, color, angle, Vector2.Zero, new Vector2(length, width), SpriteEffects.None, 0);
        }
    }
}
