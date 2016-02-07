using FluentBehaviourTree;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TwoDECS.Engine.Algorithms;
using TwoDECS.Engine.Components;
using TwoDECS.Engine.World;

namespace TwoDECS.Engine.Systems
{
    public static class AISystem
    {
        static float timer = 1;
        const float TIMER = .2f;
        static bool UpdateTimedEvents;

        public static void CheckForEnemy(Guid enemyId, PlayingState playingState, LevelCollisionDetection levelCollisionDetection)
        {
            AIComponent enemyAIComponent = playingState.AIComponents[enemyId];

            //determine if the player is in line of site
            IEnumerable<Guid> playerEntities = playingState.Entities.Where(x => (x.ComponentFlags & ComponentMasks.Player) == ComponentMasks.Player).Select(x => x.ID);
            foreach (Guid playerid in playerEntities)
            {
                //get the enemy direction
                DirectionComponent enemyDirectionComponent = playingState.DirectionComponents[enemyId];
                float direction = enemyDirectionComponent.Direction;
                if (enemyAIComponent.ActiveState.Peek() != AIState.ATTACK)
                {
                    bool playerSeen = LineOfSiteRayCast.CalculateLineOfSight(playingState.PositionComponents[enemyId].Position, playingState.PositionComponents[playerid].Position, enemyAIComponent.LineOfSight, playingState.DirectionComponents[enemyId].Direction, playingState);
                    if (playerSeen)
                    {
                        //set our path goal
                        enemyAIComponent.EntityToAttack = playerid;
                        enemyAIComponent.ActiveState.Push(AIState.ATTACK);

                        playingState.AIComponents[enemyId] = enemyAIComponent;

                        UpdateEnemyPath(enemyId, playingState);
                        AttackEnemy(enemyId, playingState, levelCollisionDetection);
                    }
                }
            }
        }

        public static void AttackEnemy(Guid enemyId, PlayingState playingState, LevelCollisionDetection levelCollisionDetection)
        {
            //update enemy ai pathing
            UpdateEnemyPath(enemyId, playingState);
            UpdateActivePath(enemyId, playingState, levelCollisionDetection);
        }


        public static void ProceedWithNextPath(Guid enemyId, PlayingState playingState, LevelCollisionDetection levelCollisionDetection)
        {
            UpdateEnemyPath(enemyId, playingState);
            UpdateActivePath(enemyId, playingState, levelCollisionDetection);
        }

        public static void UpdateEnemyPath(Guid enemyId, PlayingState playingState)
        {
            AIComponent enemyAIComponent = playingState.AIComponents[enemyId];

            //hrmm our current path goal is empty, lets get back to patrolling
            if (enemyAIComponent.PathGoal == Vector2.Zero || enemyAIComponent.ActivePath.Count <= 0 || enemyAIComponent.ActiveState.Peek() == AIState.ATTACK)
            {
                if (enemyAIComponent.ActiveState.Peek() == AIState.PATROL)
                {
                    //set our current path point to our first patrol path point
                    if (enemyAIComponent.PathGoal == Vector2.Zero)
                    {
                        enemyAIComponent.PathGoal = enemyAIComponent.PatrolPath[0];
                    }

                    if (enemyAIComponent.ActivePath.Count <= 0)
                    {
                        //we have reached our current path goal, update it to the next
                        //take the current path and move it to the bottom of the list
                        var currentPathGoal = enemyAIComponent.PatrolPath.First();
                        enemyAIComponent.PatrolPath.RemoveAt(0);
                        enemyAIComponent.PatrolPath.Add(currentPathGoal);

                        enemyAIComponent.PathGoal = enemyAIComponent.PatrolPath[0];

                    }
                    playingState.AIComponents[enemyId] = enemyAIComponent;

                    //set an active path too
                    SetActivePath(enemyId, playingState);
                }
                else if (enemyAIComponent.ActiveState.Peek() == AIState.ATTACK)
                {
                    PositionComponent attackPosition = playingState.PositionComponents[enemyAIComponent.EntityToAttack];
                    Vector2 attackPathGoal = attackPosition.Position;

                    //we can optimize this method by checking if the attack position has changed at all and ignoring the rest
                    if (enemyAIComponent.PathGoal == Vector2.Zero || !Vector2.Equals(enemyAIComponent.PathGoal, attackPosition))
                    {
                        enemyAIComponent.PathGoal = attackPathGoal;
                    }

                    if (enemyAIComponent.ActiveState.Count <= 0)
                    {
                        enemyAIComponent.PathGoal = attackPathGoal;
                    }

                    if (UpdateTimedEvents)
                    {
                        playingState.AIComponents[enemyId] = enemyAIComponent;

                        //set an active path too
                        SetActivePath(enemyId, playingState);
                    }
                }



            }
        }

        public static void SetActivePath(Guid enemyId, PlayingState playingState)
        {
            AIComponent enemyAIComponent = playingState.AIComponents[enemyId];

            var CurrentAABBComponent = playingState.AABBComponents[enemyId];
            var BoundedBox = playingState.AABBComponents[enemyId].BoundedBox;

            int width = BoundedBox.Width;
            int height = BoundedBox.Height;

            Point startingPoint = new Point((int)(CurrentAABBComponent.BoundedBox.X / width), (int)(CurrentAABBComponent.BoundedBox.Y / height));
            Point endingPoint = new Point((int)(enemyAIComponent.PathGoal.X / width), (int)(enemyAIComponent.PathGoal.Y / height));
            enemyAIComponent.ActivePath = OptimizePathByTheta(enemyAIComponent.Astar.Search(startingPoint, endingPoint, null), playingState, enemyId);

            playingState.AIComponents[enemyId] = enemyAIComponent;
        }

        public static float CalculateMagnitude(Vector2 x, Vector2 y)
        {
            return (float)Math.Sqrt(Math.Pow(x.X + y.X, 2) + Math.Pow(x.Y + y.Y, 2));
        }

        public static void UpdateActivePath(Guid enemyId, PlayingState playingState, LevelCollisionDetection levelCollisionDetection)
        {
            AIComponent enemyAIComponent = playingState.AIComponents[enemyId];

            if (enemyAIComponent.ActivePath != null && enemyAIComponent.ActivePath.Count > 0)
            {
                var CurrentPosition = playingState.PositionComponents[enemyId];
                var AABBComponent = playingState.AABBComponents[enemyId];
                var Acceleration = playingState.AccelerationComponents[enemyId];
                var AdjustedActive = new Vector2(enemyAIComponent.ActivePath.First.Value.TilePosition.X, enemyAIComponent.ActivePath.First.Value.TilePosition.Y);

                //create rectangle for our path goal
                var AdjustRect = new Rectangle((int)AdjustedActive.X, (int)AdjustedActive.Y, AABBComponent.BoundedBox.Width, AABBComponent.BoundedBox.Height);


                //use magnitiude
                if (Vector2.Distance(CurrentPosition.Position, AdjustedActive) <= Acceleration.xAcceleration)
                {
                    //remove the first element in our active path
                    enemyAIComponent.ActivePath.RemoveFirst();
                    if (enemyAIComponent.ActivePath.Count > 0)
                    {
                        AdjustedActive = new Vector2(enemyAIComponent.ActivePath.First.Value.TilePosition.X, enemyAIComponent.ActivePath.First.Value.TilePosition.Y);

                        //update our direction
                        var direction = AdjustedActive - new Vector2(AABBComponent.BoundedBox.X, AABBComponent.BoundedBox.Y);
                        direction.Normalize();
                        DirectionComponent enemyDirection = playingState.DirectionComponents[enemyId];
                        enemyDirection.Direction = (float)Math.Atan2((double)direction.Y, (double)direction.X);
                        playingState.DirectionComponents[enemyId] = enemyDirection;
                    }

                }
                else
                {
                    UpdateEnemyPosition(enemyId, enemyAIComponent, playingState, levelCollisionDetection);
                }
            }
        }

        public static void UpdateEnemyPosition(Guid enemyId, AIComponent enemyAIComponent, PlayingState playingState, LevelCollisionDetection levelCollisionDetection)
        {
            #region set our variables
            var CurrentPosition = playingState.PositionComponents[enemyId];
            var AABBComponent = playingState.AABBComponents[enemyId];
            var Acceleration = playingState.AccelerationComponents[enemyId];
            var AdjustedActive = new Vector2(enemyAIComponent.ActivePath.First.Value.TilePosition.X, enemyAIComponent.ActivePath.First.Value.TilePosition.Y);
            #endregion

            //update enemy position
            #region move right
            if (CurrentPosition.Position.X < AdjustedActive.X)
            {
                if ((CurrentPosition.Position.X + (int)Acceleration.xAcceleration) > AdjustedActive.X)
                {
                    CurrentPosition.Position.X = (int)AdjustedActive.X;

                    //update our bounded box to new position
                    AABBComponent.BoundedBox.X = (int)CurrentPosition.Position.X - (AABBComponent.BoundedBox.Width / 2);
                    AABBComponent.BoundedBox = levelCollisionDetection.CheckWallCollision(AABBComponent.BoundedBox, Direction.Right);
                }
                else
                {
                    CurrentPosition.Position.X += (int)Acceleration.xAcceleration;

                    //update our bounded box to new position
                    AABBComponent.BoundedBox.X = (int)CurrentPosition.Position.X - (AABBComponent.BoundedBox.Width / 2);
                    AABBComponent.BoundedBox = levelCollisionDetection.CheckWallCollision(AABBComponent.BoundedBox, Direction.Right);
                }
                //this.Position = mapCollisionDetection.CheckWallCollision(this.Position, this.Width, this.Height, Direction.Right);
            }
            #endregion

            #region move left
            else if (CurrentPosition.Position.X > AdjustedActive.X)
            {
                if ((CurrentPosition.Position.X - Acceleration.xAcceleration) < AdjustedActive.X)
                {
                    CurrentPosition.Position.X = (int)AdjustedActive.X;

                    //update our bounded box to new position
                    AABBComponent.BoundedBox.X = (int)CurrentPosition.Position.X - (AABBComponent.BoundedBox.Width / 2);
                    AABBComponent.BoundedBox = levelCollisionDetection.CheckWallCollision(AABBComponent.BoundedBox, Direction.Left);
                }
                else
                {
                    CurrentPosition.Position.X -= (int)Acceleration.xAcceleration;

                    //update our bounded box to new position
                    AABBComponent.BoundedBox.X = (int)CurrentPosition.Position.X - (AABBComponent.BoundedBox.Width / 2);
                    AABBComponent.BoundedBox = levelCollisionDetection.CheckWallCollision(AABBComponent.BoundedBox, Direction.Left);
                }
                //this.Position = mapCollisionDetection.CheckWallCollision(this.Position, this.Width, this.Height, Direction.Left);
            }
            #endregion

            #region move up
            if (CurrentPosition.Position.Y > AdjustedActive.Y)
            {
                if (CurrentPosition.Position.Y - (int)Acceleration.yAcceleration < AdjustedActive.Y)
                {
                    CurrentPosition.Position.Y = (int)AdjustedActive.Y;

                    //update our bounded box to new position
                    AABBComponent.BoundedBox.Y = (int)CurrentPosition.Position.Y - (AABBComponent.BoundedBox.Height / 2);
                    AABBComponent.BoundedBox = levelCollisionDetection.CheckWallCollision(AABBComponent.BoundedBox, Direction.Up);
                }
                else
                {
                    CurrentPosition.Position.Y -= (int)Acceleration.yAcceleration;

                    //update our bounded box to new position
                    AABBComponent.BoundedBox.Y = (int)CurrentPosition.Position.Y - (AABBComponent.BoundedBox.Height / 2);
                    AABBComponent.BoundedBox = levelCollisionDetection.CheckWallCollision(AABBComponent.BoundedBox, Direction.Up);
                }
                //this.Position = mapCollisionDetection.CheckWallCollision(this.Position, this.Width, this.Height, Direction.Up);
            }
            #endregion

            #region move down
            else if (CurrentPosition.Position.Y < AdjustedActive.Y)
            {
                if (CurrentPosition.Position.Y + (int)Acceleration.yAcceleration > AdjustedActive.Y)
                {
                    CurrentPosition.Position.Y = (int)AdjustedActive.Y;

                    //update our bounded box to new position
                    AABBComponent.BoundedBox.Y = (int)CurrentPosition.Position.Y - (AABBComponent.BoundedBox.Height / 2);
                    AABBComponent.BoundedBox = levelCollisionDetection.CheckWallCollision(AABBComponent.BoundedBox, Direction.Down);
                }
                else
                {
                    CurrentPosition.Position.Y += (int)Acceleration.yAcceleration;

                    //update our bounded box to new position
                    AABBComponent.BoundedBox.Y = (int)CurrentPosition.Position.Y - (AABBComponent.BoundedBox.Height / 2);
                    AABBComponent.BoundedBox = levelCollisionDetection.CheckWallCollision(AABBComponent.BoundedBox, Direction.Down);
                }
                //this.Position = mapCollisionDetection.CheckWallCollision(this.Position, this.Width, this.Height, Direction.Down);
            }
            #endregion

            //update our position based on our new bounded box after collision
            #region save component changes
            CurrentPosition.Position = new Vector2(AABBComponent.BoundedBox.X + (AABBComponent.BoundedBox.Width / 2), AABBComponent.BoundedBox.Y + (AABBComponent.BoundedBox.Height / 2));

            playingState.PositionComponents[enemyId] = CurrentPosition;

            playingState.AIComponents[enemyId] = enemyAIComponent;
            playingState.AABBComponents[enemyId] = AABBComponent;
            #endregion
        }

        public static void UpdateEnemeyAI(PlayingState playingState, GameTime gameTime, LevelCollisionDetection levelCollisionDetection, int tileSize)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            timer -= elapsed;
            if (timer < 0)
            {
                //Timer expired, execute action
                UpdateTimedEvents = true;
                timer = TIMER;   //Reset Timer
            }
            else
            {
                UpdateTimedEvents = false;
            }

            IEnumerable<Guid> enemyEntites = playingState.Entities.Where(x => (x.ComponentFlags & ComponentMasks.Enemy) == ComponentMasks.Enemy).Select(x => x.ID);

            foreach (Guid enemyid in enemyEntites)
            {
                //get current enemey ai state
                AIComponent enemyAIComponent = playingState.AIComponents[enemyid];
                enemyAIComponent.AITree.Tick(new TimeData((float)gameTime.ElapsedGameTime.TotalSeconds));
                //if (enemyAI.ActiveState.Peek() == AIState.ATTACK)
                //{
                //    AttackEnemy(enemyid, playingState, levelCollisionDetection);
                //}
                //else
                //{

                //    //handle pathing
                //    if (enemyAI.PatrolPath.Count() > 0)
                //    {
                //        //if we have a patrol path and we are not attacking, lets get back to patrolling
                //        if (enemyAI.ActiveState.Peek() != AIState.PATROL && enemyAI.ActiveState.Peek() != AIState.ATTACK)
                //        {
                //            enemyAI.ActiveState.Push(AIState.PATROL);
                //        }

                //        if (enemyAI.ActiveState.Peek() == AIState.PATROL)
                //        {
                //            //lets do our patrol pathing
                //            ProceedWithNextPath(enemyid, playingState, levelCollisionDetection);

                //        }
                //    }

                //    //check to see if we see the enemy, if so put ourselves in attack mode
                //    if (enemyAI.ActiveState.Peek() != AIState.ATTACK)
                //    {
                //        if (UpdateTimedEvents)
                //        {
                //            CheckForEnemy(enemyid, playingState, levelCollisionDetection);
                //        }
                //    }
                //}
            }


        }


        public static LinkedList<Tile> OptimizePathByTheta(LinkedList<Tile> pathUnoptimized, PlayingState playingState, Guid  enemyId)
        {
            if(pathUnoptimized.Count > 2){
                //center all our points - perhaps downt he road we can change the point based on direction
                LinkedList<Tile> pathCentered = new LinkedList<Tile>();
                foreach (Tile tile in pathUnoptimized)
                {
                    pathCentered.AddLast(new Tile()
                    {
                        TilePosition = new Vector2(tile.TilePosition.X + (tile.Destination.Width / 2), tile.TilePosition.Y + (tile.Destination.Height / 2))
                    });
                }
                
                LinkedList<Tile> pathOptimized = new LinkedList<Tile>();
                pathOptimized.AddLast(pathCentered.First());
                pathCentered.RemoveFirst();

                Tile checkPoint = pathCentered.First();
                bool straightLine = true;
                bool skip = false;
                while (pathCentered.Count > 0)
                {
                    if (LineOfSiteRayCast.LineOfSight(pathOptimized.First().TilePosition, pathCentered.First().TilePosition, playingState))
                    {
                        if (pathOptimized.Count > 2 && !skip)
                        {
                            pathOptimized.RemoveLast();
                        }

                        if (!skip)
                        {
                            skip = false;
                        }
                    }
                    else
                    {
                        skip = true;
                        straightLine = false;
                    }
                    pathOptimized.AddLast(pathCentered.First());
                    pathCentered.RemoveFirst();
                    
                }
                if (straightLine)
                {
                    pathOptimized.RemoveFirst();
                }
                return pathOptimized;
            }
            return pathUnoptimized;
        }

        public static LinkedList<Tile> OptimizePath(LinkedList<Tile> pathUnoptimized)
        {
            //if there are only 2 tiles then there is not optimization to take place
            if (pathUnoptimized.Count > 2)
            {
                LinkedList<Tile> optimizedPath = new LinkedList<Tile>();
                //add the first piont to the path
                optimizedPath.AddFirst(pathUnoptimized.First());

                //remove our first tile from the unoptimized list
                pathUnoptimized.RemoveFirst();

                //start with the first tile and work our way through tiles to 
                Vector2 lastComparitiveSlope = pathUnoptimized.First.Value.TilePosition - optimizedPath.Last().TilePosition;
                Tile lastPath = optimizedPath.Last();
                foreach (Tile path in pathUnoptimized)
                {
                    Vector2 comparitiveSlope = path.TilePosition - lastPath.TilePosition;

                    if (!Vector2.Equals(comparitiveSlope, lastComparitiveSlope))
                    {
                        //put our current path on the optimized path
                        optimizedPath.AddLast(lastPath);
                        optimizedPath.AddLast(path);
                        lastPath = path;
                    }
                    else if (Vector2.Equals(comparitiveSlope, lastComparitiveSlope) && path == pathUnoptimized.Last())
                    {
                        optimizedPath.AddLast(path);
                    }
                    else
                    {
                        lastPath = path;
                    }
                }

                return optimizedPath;
            }
            else
            {
                return pathUnoptimized;
            }
        }
    }
}
