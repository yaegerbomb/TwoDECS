using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TwoDECS.Engine.Components;

namespace TwoDECS.Engine.Systems
{
    public static class AABBDetectionSystem
    {
        public static bool RectCollision(Rectangle rect1, Rectangle rect2)
        {
            return rect1.Intersects(rect2);
        }
        
        public static void DetectAABBProjectileCollision(PlayingState playingState)
        {
            List<Guid> projectileEntites = playingState.Entities.Where(x => (x.ComponentFlags & ComponentMasks.Projectile) == ComponentMasks.Projectile).Select(x => x.ID).ToList() ;
            List<Guid> playerEntities = playingState.Entities.Where(x => (x.ComponentFlags & ComponentMasks.Player) == ComponentMasks.Player).Select(x => x.ID).ToList();
            List<Guid> enemyEntities = playingState.Entities.Where(x => (x.ComponentFlags & ComponentMasks.Enemy) == ComponentMasks.Enemy).Select(x => x.ID).ToList();

            List<Guid> EntitiesToRemove = new List<Guid>();
            foreach (Guid projectileid in projectileEntites)
            {
                PositionComponent projectilePosition = playingState.PositionComponents[projectileid];
                //foreach (Guid playerid in playerEntities)
                //{
                //    PositionComponent playerPosition = playingState.PositionComponents[projectileid];
                //    if (RectCollision(projectilePosition.Destination, playerPosition.Destination))
                //    {
                //        HealthComponent playerHealthComponent = playingState.HealthComponents[playerid];
                //        DamageComponent projectileDamageComponent = playingState.DamageComponents[projectileid];

                //        playerHealthComponent.Health -= projectileDamageComponent.Damage;

                //        Console.WriteLine("Player Health: " + playerHealthComponent.Health);

                //        playingState.HealthComponents[playerid] = playerHealthComponent;
                //        playingState.DestroyEntity(projectileid);
                //    }
                //}

                foreach (Guid enemeyid in enemyEntities)
                {
                    PositionComponent enemyPosition = playingState.PositionComponents[enemeyid];
                    if (RectCollision(projectilePosition.Destination, enemyPosition.Destination))
                    {
                        HealthComponent enemyHealthComponent = playingState.HealthComponents[enemeyid];
                        DamageComponent projectileDamageComponent = playingState.DamageComponents[projectileid];

                        enemyHealthComponent.Health -= projectileDamageComponent.Damage;

                        Console.WriteLine("Enemy Health: " + enemyHealthComponent.Health);

                        playingState.HealthComponents[enemeyid] = enemyHealthComponent;
                        playingState.DestroyEntity(projectileid);
                    }
                }
            }

        }
    }
}
