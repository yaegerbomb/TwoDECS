using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TwoDECS.Engine.Components;

namespace TwoDECS.Engine.Systems
{

    public struct Manifold
    {
        public Object a;
        public Object b;
        public float penetration;
        public Vector2 normal;
    }

    public static class AABBDetectionSystem
    {
        public const float mass = 100;
        public const float invmass = .01f;
        public const float restitution = 0;

        public static bool RectCollision(Rectangle rect1, Rectangle rect2)
        {
            return rect1.Intersects(rect2);
        }

        public static void DetectAABBProjectileCollision(PlayingState playingState)
        {
            IEnumerable<Guid> projectileEntites = playingState.Entities.Where(x => (x.ComponentFlags & ComponentMasks.Projectile) == ComponentMasks.Projectile).Select(x => x.ID);
            IEnumerable<Guid> playerEntities = playingState.Entities.Where(x => (x.ComponentFlags & ComponentMasks.Player) == ComponentMasks.Player).Select(x => x.ID);
            IEnumerable<Guid> enemyEntities = playingState.Entities.Where(x => (x.ComponentFlags & ComponentMasks.Enemy) == ComponentMasks.Enemy).Select(x => x.ID);

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

                foreach (Guid enemyid in enemyEntities)
                {
                    PositionComponent enemyPosition = playingState.PositionComponents[enemyid];
                    AABBComponent aABBComponent = playingState.AABBComponents[enemyid];
                    if (RectCollision(aABBComponent.BoundedBox, aABBComponent.BoundedBox))
                    {
                        HealthComponent enemyHealthComponent = playingState.HealthComponents[enemyid];
                        DamageComponent projectileDamageComponent = playingState.DamageComponents[projectileid];

                        enemyHealthComponent.Health -= projectileDamageComponent.Damage;

                        Console.WriteLine("Enemy Health: " + enemyHealthComponent.Health);

                        playingState.HealthComponents[enemyid] = enemyHealthComponent;
                        EntitiesToRemove.Add(projectileid);
                    }
                }
            }

            foreach (Guid projectileid in EntitiesToRemove)
            {
                playingState.DestroyEntity(projectileid);
            }

        }

        public static void UpdateAABBPlayerCollision(PlayingState playingState)
        {
            IEnumerable<Guid> playerEntities = playingState.Entities.Where(x => (x.ComponentFlags & ComponentMasks.Player) == ComponentMasks.Player).Select(x => x.ID);
            IEnumerable<Guid> levelObjectEntities = playingState.Entities.Where(x => (x.ComponentFlags & ComponentMasks.LevelObjects) == ComponentMasks.LevelObjects).Select(x => x.ID);

            //create aa non velocity component for our level objects
            VelocityComponent levelObjectVelocityComponent = new VelocityComponent();
            levelObjectVelocityComponent.xVelocity = 0f;
            levelObjectVelocityComponent.yVelocity = 0f;

            foreach (Guid playerid in playerEntities)
            {
                AABBComponent playerAABBComponent = playingState.AABBComponents[playerid];
                VelocityComponent playerVelocityComponent = playingState.VelocityComponents[playerid];

                foreach (Guid lobjectid in levelObjectEntities)
                {
                    AABBComponent levelAABBComponent = playingState.AABBComponents[lobjectid];
                    Manifold manifold = overlapAABB(playerAABBComponent.BoundedBox, playerVelocityComponent, levelAABBComponent.BoundedBox, levelObjectVelocityComponent);
                    if (manifold.a != null)
                    {
                        resolveCollision(playerAABBComponent.BoundedBox, playerVelocityComponent, levelAABBComponent.BoundedBox, levelObjectVelocityComponent, manifold, playingState, playerid);
                    }

                }
            }

        }

        public static bool AABBvsAABB(Rectangle a, Rectangle b)
        {
            if (a.Width < b.X || a.X > b.Width)
            {
                return false;
            }

            if (a.Height < b.Y || a.Y > b.Height)
            {
                return false;
            }

            return true;
        }

        public static Manifold overlapAABB(Rectangle a, VelocityComponent velocityA, Rectangle b, VelocityComponent velocityB)
        {
            Manifold manifold = new Manifold();
            manifold.a = a;
            manifold.b = b;

            // vectrom from a to b
            Vector2 normal = new Vector2(b.X - a.X, b.Y - a.Y);

            //calculate half extents along x axis for each object
            float a_extent = (a.Width - a.X) / 2;
            float b_extent = (b.Width - b.X) / 2;

            //calculate overlap on x axis
            var x_overlap = a_extent + b_extent - Math.Abs(normal.X);

            // SAT test on x axis
            if (x_overlap > 0)
            {
                a_extent = (a.Height - a.X) / 2;
                b_extent = (b.Height - b.X) / 2;

                //calculate overlap on y axes
                var y_overlap = a_extent + b_extent - Math.Abs(normal.Y);

                // SAT test on y axis
                if (y_overlap > 0)
                {
                    // Find out which axis is axis of least penetration
                    if (x_overlap < y_overlap)
                    {
                        // point towards b knowing that dist points from a to b
                        if (normal.X < 0)
                        {
                            manifold.normal = new Vector2(-1, 0);
                        }
                        else
                        {
                            manifold.normal = new Vector2(1, 0);
                        }
                        manifold.penetration = x_overlap;
                        return manifold;
                    }
                    else
                    {
                        // Point toward B knowing that dist points from A to B
                        if (normal.Y < 0)
                        {
                            manifold.normal = new Vector2(0, -1);
                        }
                        else
                        {
                            manifold.normal = new Vector2(0, 1);
                        }
                        manifold.penetration = y_overlap;
                        return manifold;
                    }
                }
            }
            return new Manifold();
        }

        public static float DotProduct(Vector2 vrelative, Vector2 normal)
        {
            return (vrelative.X * normal.X + vrelative.Y * normal.Y);
        }

        public static void resolveCollision(Rectangle a, VelocityComponent velocityA, Rectangle b, VelocityComponent velocityB, Manifold m, PlayingState playingState, Guid aid, Guid? bid = null)
        {
            // calculate relative velocity
            Vector2 rv = new Vector2(velocityB.xVelocity - velocityA.xVelocity, velocityB.yVelocity - velocityA.yVelocity);

            //calculate relative velocity in terms of normal direction
            var velAlongNormal = DotProduct(rv, m.normal);

            //do not resolve if velocities are seperating
            if (velAlongNormal > 0)
            {
                return;
            }

            //calculate restitution
            var e = Math.Min(restitution, restitution);

            //calculate impulse scaler
            var j = -(1 + e) * velAlongNormal;
            j /= invmass + invmass;

            // Apply impulse
            Vector2 _impulse = new Vector2(m.normal.X * j, m.normal.Y * j);

            velocityA.xVelocity -= (invmass * _impulse.X);
            velocityA.yVelocity -= (invmass * _impulse.Y);

            velocityB.xVelocity += (invmass * _impulse.X);
            velocityB.yVelocity += (invmass * _impulse.Y);

            playingState.VelocityComponents[aid] = velocityA;

            if (bid != null)
            {
                playingState.VelocityComponents[(Guid)bid] = velocityB;
            }

            //float percent = .8f;
            //float slop = .01f;
            //var c = Math.Max(m.penetration - slop, 0) / (invmass + invmass) * percent * m.normal;
            //there is like some position shit that should go here but i dont know what
        }
    }
}
