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
                        EntitiesToRemove.Add(projectileid);
                    }
                }
            }

            foreach (Guid projectileid in EntitiesToRemove)
            {
                playingState.DestroyEntity(projectileid);
            }

        }


        // describes an axis-aligned rectangle with a velocity
        public class Box
        {
            public Box(float _x, float _y, float _w, float _h, float _vx, float _vy)
            {
                x = _x;
                y = _y;
                w = _w;
                h = _h;
                vx = _vx;
                vy = _vy;
            }

            public Box(float _x, float _y, float _w, float _h)
            {
                x = _x;
                y = _y;
                w = _w;
                h = _h;
                vx = 0.0f;
                vy = 0.0f;
            }

            // position of top-left corner
            public float x, y;

            // dimensions
            public float w, h;

            // velocity
            public float vx, vy;
        }

        // returns true if the boxes are colliding (velocities are not used)
        public static bool AABBCheck(Box b1, Box b2)
        {
            return !(b1.x + b1.w < b2.x || b1.x > b2.x + b2.w || b1.y + b1.h < b2.y || b1.y > b2.y + b2.h);
        }

        // returns true if the boxes are colliding (velocities are not used)
        // moveX and moveY will return the movement the b1 must move to avoid the collision
        public static bool AABB(Box b1, Box b2, out float moveX, out float moveY)
        {
            moveX = moveY = 0.0f;

            float l = b2.x - (b1.x + b1.w);
            float r = (b2.x + b2.w) - b1.x;
            float t = b2.y - (b1.y + b1.h);
            float b = (b2.y + b2.h) - b1.y;

            // check that there was a collision
            if (l > 0 || r < 0 || t > 0 || b < 0)
                return false;

            // find the offset of both sides
            moveX = Math.Abs(l) < r ? l : r;
            moveY = Math.Abs(t) < b ? t : b;

            // only use whichever offset is the smallest
            if (Math.Abs(moveX) < Math.Abs(moveY))
                moveX = 0.0f;
            else
                moveY = 0.0f;

            return true;
        }

        // returns a box the spans both a current box and the destination box
        public static Box GetSweptBroadphaseBox(Box b)
        {
            Box broadphasebox = new Box(0.0f, 0.0f, 0.0f, 0.0f);

            broadphasebox.x = b.vx > 0 ? b.x : b.x + b.vx;
            broadphasebox.y = b.vy > 0 ? b.y : b.y + b.vy;
            broadphasebox.w = b.vx > 0 ? b.vx + b.w : b.w - b.vx;
            broadphasebox.h = b.vy > 0 ? b.vy + b.h : b.h - b.vy;

            return broadphasebox;
        }

        // performs collision detection on moving box b1 and static box b2
        // returns the time that the collision occured (where 0 is the start of the movement and 1 is the destination)
        // getting the new position can be retrieved by box.x = box.x + box.vx * collisiontime
        // normalx and normaly return the normal of the collided surface (this can be used to do a response)
        public static float SweptAABB(Box b1, Box b2, out float normalx, out float normaly)
        {
            float xInvEntry, yInvEntry;
            float xInvExit, yInvExit;

            // find the distance between the objects on the near and far sides for both x and y
            if (b1.vx > 0.0f)
            {
                xInvEntry = b2.x - (b1.x + b1.w);
                xInvExit = (b2.x + b2.w) - b1.x;
            }
            else
            {
                xInvEntry = (b2.x + b2.w) - b1.x;
                xInvExit = b2.x - (b1.x + b1.w);
            }

            if (b1.vy > 0.0f)
            {
                yInvEntry = b2.y - (b1.y + b1.h);
                yInvExit = (b2.y + b2.h) - b1.y;
            }
            else
            {
                yInvEntry = (b2.y + b2.h) - b1.y;
                yInvExit = b2.y - (b1.y + b1.h);
            }

            // find time of collision and time of leaving for each axis (if statement is to prevent divide by zero)
            float xEntry, yEntry;
            float xExit, yExit;

            if (b1.vx == 0.0f)
            {
                xEntry = -float.PositiveInfinity;
                xExit = float.PositiveInfinity;
            }
            else
            {
                xEntry = xInvEntry / b1.vx;
                xExit = xInvExit / b1.vx;
            }

            if (b1.vy == 0.0f)
            {
                yEntry = -float.PositiveInfinity;
                yExit = float.PositiveInfinity;
            }
            else
            {
                yEntry = yInvEntry / b1.vy;
                yExit = yInvExit / b1.vy;
            }

            if (yEntry > 1.0f) yEntry = -float.NegativeInfinity;
            if (xEntry > 1.0f) xEntry = -float.NegativeInfinity;

            // find the earliest/latest times of collision
            float entryTime = Math.Max(xEntry, yEntry);
            float exitTime = Math.Min(xExit, yExit);

            // if there was no collision
            //if (entryTime > exitTime || xEntry < 0.0f && yEntry < 0.0f || xEntry > 1.0f || yEntry > 1.0f)
            //{
            //    normalx = 0.0f;
            //    normaly = 0.0f;
            //    return 1.0f;
            //}


            if (entryTime > exitTime)
            {

                normalx = 0.0f;
                normaly = 0.0f;
                return 1.0f;
            }
            if (xEntry < 0.0f && yEntry < 0.0f){ 
                
                normalx = 0.0f;
                normaly = 0.0f;
                return 1.0f;
            }
            if (xEntry < 0.0f)
            {
                // Check that the bounding box started overlapped or not.
                if (b1.w < b2.x || b1.x > b2.w)
                {

                    normalx = 0.0f;
                    normaly = 0.0f;
                    return 1.0f;
                }
            }
            if (yEntry < 0.0f)
            {
                // Check that the bounding box started overlapped or not.
                if (b1.h < b2.y || b1.y > b2.h)
                {
                    normalx = 0.0f;
                    normaly = 0.0f;
                    return 1.0f;
                };
            }


            //else // if there was a collision
            //{
                // calculate normal of collided surface
                if (xEntry > yEntry)
                {
                    if (xInvEntry < 0.0f)
                    {
                        normalx = 1.0f;
                        normaly = 0.0f;
                    }
                    else
                    {
                        normalx = -1.0f;
                        normaly = 0.0f;
                    }
                }
                else
                {
                    if (yInvEntry < 0.0f)
                    {
                        normalx = 0.0f;
                        normaly = 1.0f;
                    }
                    else
                    {
                        normalx = 0.0f;
                        normaly = -1.0f;
                    }
                }

                // return the time of collision
                return entryTime;
            //}
        }

        public static void DetectAABBPlayerCollision(PlayingState playingState)
        {
            IEnumerable<Guid> playerEntities = playingState.Entities.Where(x => (x.ComponentFlags & ComponentMasks.Player) == ComponentMasks.Player).Select(x => x.ID);
            IEnumerable<Guid> levelObjects = playingState.Entities.Where(x => (x.ComponentFlags & ComponentMasks.LevelObjects) == ComponentMasks.LevelObjects).Select(x => x.ID);

            foreach (Guid playerid in playerEntities)
            {
                foreach (Guid levelobjectid in levelObjects)
                {
                    PositionComponent playerPositionComponent = playingState.PositionComponents[playerid];
                    VelocityComponent playerSpeedComponent = playingState.VelocityComponents[playerid];

                    PositionComponent levelObjectPostionComponent = playingState.PositionComponents[levelobjectid];


                    Box playerbox = new Box(playerPositionComponent.Destination.X, playerPositionComponent.Destination.Y, playerPositionComponent.Destination.Width, playerPositionComponent.Destination.Height, playerSpeedComponent.xVelocity, playerSpeedComponent.yVelocity);
                    Box levelbox = new Box(levelObjectPostionComponent.Destination.X, levelObjectPostionComponent.Destination.Y, levelObjectPostionComponent.Destination.Width, levelObjectPostionComponent.Destination.Height, 0f, 0f);

                    Box broadphasebox = GetSweptBroadphaseBox(playerbox);
                    if (AABBCheck(broadphasebox, levelbox))
                    {

                        float normalx, normaly;
                        float collisiontime = SweptAABB(playerbox, levelbox, out normalx, out normaly);
                        playerbox.x += playerbox.vx * collisiontime;
                        playerbox.y += playerbox.vy * collisiontime;

                        float remainingtime = 1.0f - collisiontime;

                        if (remainingtime < 1.0f)
                        {
                            playerSpeedComponent.xVelocity *= normalx;
                            playerSpeedComponent.yVelocity *= normaly;

                            Console.WriteLine(playerSpeedComponent.xVelocity);
                            Console.WriteLine(playerSpeedComponent.yVelocity);


                            playerPositionComponent.Destination = new Rectangle((int)playerbox.x, (int)playerbox.y, (int)playerbox.w, (int)playerbox.h);
                            playerPositionComponent.Position = new Vector2(playerbox.x, playerbox.y);
                            playingState.PositionComponents[playerid] = playerPositionComponent;
                            playingState.VelocityComponents[playerid] = playerSpeedComponent;
                        }
                    }

                    //    playerPositionComponent.Destination = playerBoundedBox;
                    //    playingState.PositionComponents[playerid] = playerPositionComponent;
                    //}
                }
            }
        }

        //public static void DetectAABBPlayerCollision(PlayingState playingState)
        //{
        //    IEnumerable<Guid> playerEntities = playingState.Entities.Where(x => (x.ComponentFlags & ComponentMasks.Player) == ComponentMasks.Player).Select(x => x.ID);
        //    IEnumerable<Guid> levelObjects = playingState.Entities.Where(x => (x.ComponentFlags & ComponentMasks.LevelObjects) == ComponentMasks.LevelObjects).Select(x => x.ID);

        //    foreach (Guid playerid in playerEntities)
        //    {
        //        foreach (Guid levelobjectid in levelObjects)
        //        {
        //            PositionComponent playerPositionComponent = playingState.PositionComponents[playerid];
        //            VelocityComponent playerSpeedComponent = playingState.VelocityComponents[playerid];

        //            PositionComponent levelObjectPostionComponent = playingState.PositionComponents[levelobjectid];
        //            if(playerPositionComponent.Destination.Intersects(levelObjectPostionComponent.Destination))
        //            {
        //                //determin which way we entered the rectangle and reset our position

        //                //we are going left
        //                if(playerSpeedComponent.xVelocity < 0){
        //                    playerPositionComponent.Position.X = levelObjectPostionComponent.Destination.Right;
        //                    playerPositionComponent.Destination.X = levelObjectPostionComponent.Destination.Right;
        //                }
        //                else if(playerSpeedComponent.xVelocity > 0) // we are going right
        //                {                            
        //                    playerPositionComponent.Position.X = levelObjectPostionComponent.Destination.Left - playerPositionComponent.Destination.Width;
        //                    playerPositionComponent.Destination.X = levelObjectPostionComponent.Destination.Left - playerPositionComponent.Destination.Width;
        //                }

        //                //going up
        //                if(playerSpeedComponent.yVelocity < 0)
        //                {
        //                    playerPositionComponent.Position.Y = levelObjectPostionComponent.Destination.Bottom;
        //                    playerPositionComponent.Destination.Y = levelObjectPostionComponent.Destination.Bottom;
        //                }
        //                else if(playerSpeedComponent.yVelocity > 0)
        //                {
        //                    playerPositionComponent.Position.Y = levelObjectPostionComponent.Destination.Top - playerPositionComponent.Destination.Height;
        //                    playerPositionComponent.Destination.Y = levelObjectPostionComponent.Destination.Top - playerPositionComponent.Destination.Height;
        //                }
        //            }

        //            playingState.PositionComponents[playerid] = playerPositionComponent;
        //        }
        //    }
        //}
    }
}
