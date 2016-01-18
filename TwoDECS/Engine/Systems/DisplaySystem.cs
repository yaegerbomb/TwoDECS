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
        public static void DrawPlayingStateDisplayEntities(PlayingState playingState, FollowCamera followCamera, SpriteBatch spriteBatch, Texture2D spriteSheet)
        {
            IEnumerable<Guid> drawableEntities = playingState.Entities.Where(x => (x.ComponentFlags & ComponentMasks.Drawable) == ComponentMasks.Drawable).Select(x => x.ID);
            foreach (Guid id in drawableEntities)
            {
                Rectangle source = playingState.DisplayComponents[id].Source;
                Vector2 origin = new Vector2(source.Width / 2, source.Height / 2);
                spriteBatch.Draw(spriteSheet, playingState.PositionComponents[id].Destination, source, Color.White, playingState.DirectionComponents[id].Direction, origin, SpriteEffects.None, 0);

            }
        }
    }
}
