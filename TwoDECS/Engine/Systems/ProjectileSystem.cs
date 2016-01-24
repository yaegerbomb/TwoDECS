using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TwoDECS.Engine.Components;

namespace TwoDECS.Engine.Systems
{
    public static class ProjectileSystem
    {
        public static void UpdateProjectiles(PlayingState playingState)
        {
            //IEnumerable<Guid> projectileEntities = playingState.Entities.Where(x => (x.ComponentFlags & ComponentMasks.Projectile) == ComponentMasks.Projectile).Select(x => x.ID).ToList();
            //foreach (Guid projectileId in projectileEntities)
            //{
            //    PositionComponent positionComponent = playingState.PositionComponents[projectileId];
            //    DirectionComponent directionComponent = playingState.DirectionComponents[projectileId];
            //    VelocityComponent speedComponent = playingState.VelocityComponents[projectileId];

            //    double vX = speedComponent.xVelocity * Math.Cos(directionComponent.Direction);
            //    double vY = speedComponent.yVelocity *Math.Sin(directionComponent.Direction);

            //    Vector2 newPosition = new Vector2(positionComponent.Position.X + (float)vX, positionComponent.Position.Y + (float)vY);
            //    Rectangle destination = new Rectangle((int)newPosition.X, (int)newPosition.Y, positionComponent.Destination.Width, positionComponent.Destination.Height);

            //    positionComponent.Position = newPosition;
            //    positionComponent.Destination = destination;

            //    playingState.PositionComponents[projectileId] = positionComponent;
            //}
        }
    }
}
