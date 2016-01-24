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
        static float timer = 1;         //Initialize a 10 second timer
        const float TIMER = .2f;

        public static void AttackPlayer(Guid EnemyID, AIComponent enemyAIComponent, PlayingState playingState)
        {
            //determine if the player is in line of site
            IEnumerable<Guid> playerEntities = playingState.Entities.Where(x => (x.ComponentFlags & ComponentMasks.Player) == ComponentMasks.Player).Select(x => x.ID);
            foreach (Guid playerid in playerEntities)
            {
                //get the enemy direction
                DirectionComponent enemyDirectionComponent = playingState.DirectionComponents[EnemyID];
                float direction = enemyDirectionComponent.Direction;
                //if (LineOfSiteRayCast.CalculateLineOfSight(playingState.PositionComponents[EnemyID].Position, playingState.PositionComponents[playerid].Position, enemeyAIComponent.LineOfSite, playingState.DirectionComponents[EnemyID].Direction, playingState))
                //{
                //    //do something
                //}
                bool playerSeen = LineOfSiteRayCast.CalculateLineOfSight(playingState.PositionComponents[EnemyID].Position, playingState.PositionComponents[playerid].Position, enemyAIComponent.LineOfSite, playingState.DirectionComponents[EnemyID].Direction, playingState);
                if (playerSeen)
                {
                    //we see the player
                    LabelComponent labelComponent = playingState.LabelComponents[EnemyID];
                    labelComponent.Label = "!!!";
                    playingState.LabelComponents[EnemyID] = labelComponent;
                    Point startingPoint = playingState.PositionComponents[EnemyID].Position.ToPoint();
                    Point endingPoint = playingState.PositionComponents[playerid].Position.ToPoint();
                    int width = playingState.AABBComponents[EnemyID].BoundedBox.Width;
                    int height = playingState.AABBComponents[EnemyID].BoundedBox.Height;
                    enemyAIComponent.ActivePath = enemyAIComponent.Astar.Search(new Point(startingPoint.X / width, startingPoint.Y / height), new Point(endingPoint.X / width, endingPoint.Y / height), null);
                }
            }

        }

        public static void ProceedWithNextPath(Guid EnemyID, AIComponent enemyAIComponent, PlayingState playingState)
        {
            //hrmm our current path is empty, lets get back to patrolling
            if (enemyAIComponent.PathGoal == Vector2.Zero || enemyAIComponent.ActivePath.Count <= 0)
            {
                //set our current path point to our first patrol path point
                if (enemyAIComponent.PathGoal == Vector2.Zero)
                {
                    enemyAIComponent.PathGoal = enemyAIComponent.PatrolPath[0];
                }
                else if (enemyAIComponent.ActivePath.Count <= 0)
                {
                    //we have reached our current path goal, update it to the next
                    //take the current path and move it to the bottom of the list
                    var currentPathGoal = enemyAIComponent.PatrolPath.First();
                    enemyAIComponent.PatrolPath.RemoveAt(0);
                    enemyAIComponent.PatrolPath.Add(currentPathGoal);

                    enemyAIComponent.PathGoal = enemyAIComponent.PatrolPath[0];

                }

                //set an active path too
                var CurrentPosition = playingState.PositionComponents[EnemyID];
                var BoundedBox = playingState.AABBComponents[EnemyID].BoundedBox;
                int width = BoundedBox.Width;
                int height = BoundedBox.Height;
                Point startingPoint = new Point((int)(CurrentPosition.Position.X / width), (int)(CurrentPosition.Position.Y / height));
                Point endingPoint = new Point((int)(enemyAIComponent.PathGoal.X), (int)(enemyAIComponent.PathGoal.Y));
                enemyAIComponent.ActivePath = enemyAIComponent.Astar.Search(startingPoint, endingPoint, null);
                
                playingState.AIComponents[EnemyID] = enemyAIComponent;

            }
            else
            {
                var CurrentPosition = playingState.PositionComponents[EnemyID];
                var AABBComponent = playingState.AABBComponents[EnemyID];
                var Acceleration = playingState.AccelerationComponents[EnemyID];
                var AdjustedActive = new Vector2(enemyAIComponent.ActivePath.First.Value.TilePosition.X, enemyAIComponent.ActivePath.First.Value.TilePosition.Y);
                //if our distance is within 1 point, lets get a new path
                Console.WriteLine(Vector2.Distance(CurrentPosition.Position, AdjustedActive));
                if (Vector2.Distance(CurrentPosition.Position, AdjustedActive) <= 2.5)
                {
                    //remove the first element in our active path
                    enemyAIComponent.ActivePath.RemoveFirst();
                    if (enemyAIComponent.ActivePath.Count > 0)
                    {
                        AdjustedActive = new Vector2(enemyAIComponent.ActivePath.First.Value.TilePosition.X, enemyAIComponent.ActivePath.First.Value.TilePosition.Y);

                        //update our direction
                        var direction = AdjustedActive - CurrentPosition.Position;
                        direction.Normalize();
                        DirectionComponent enemyDirection = playingState.DirectionComponents[EnemyID];
                        enemyDirection.Direction = (float)Math.Atan2((double)direction.Y, (double)direction.X);
                        playingState.DirectionComponents[EnemyID] = enemyDirection;
                    }

                }
                else
                {

                    //update enemy position
                    if (CurrentPosition.Position.X < AdjustedActive.X)
                    {
                        CurrentPosition.Position.X += Acceleration.xAcceleration;
                        //this.Position = mapCollisionDetection.CheckWallCollision(this.Position, this.Width, this.Height, Direction.Right);
                    }
                    else if (CurrentPosition.Position.X > AdjustedActive.X)
                    {
                        CurrentPosition.Position.X -= Acceleration.xAcceleration;
                        //this.Position = mapCollisionDetection.CheckWallCollision(this.Position, this.Width, this.Height, Direction.Left);
                    }

                    if (CurrentPosition.Position.Y > AdjustedActive.Y)
                    {
                        CurrentPosition.Position.Y -= Acceleration.yAcceleration;
                        //this.Position = mapCollisionDetection.CheckWallCollision(this.Position, this.Width, this.Height, Direction.Up);
                    }
                    else if (CurrentPosition.Position.Y < AdjustedActive.Y)
                    {
                        CurrentPosition.Position.Y += Acceleration.yAcceleration;
                        //this.Position = mapCollisionDetection.CheckWallCollision(this.Position, this.Width, this.Height, Direction.Down);
                    }

                    AABBComponent.BoundedBox = new Rectangle((int)CurrentPosition.Position.X - (AABBComponent.BoundedBox.Width / 2), (int)CurrentPosition.Position.Y - (AABBComponent.BoundedBox.Height / 2), AABBComponent.BoundedBox.Width, AABBComponent.BoundedBox.Height);
                    playingState.PositionComponents[EnemyID] = CurrentPosition;

                    playingState.AIComponents[EnemyID] = enemyAIComponent;
                    playingState.AABBComponents[EnemyID] = AABBComponent;
                }

            }

            //get our path to our current path point
        }



        public static void UpdateEnemeyAI(PlayingState playingState, GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            bool CheckWherePlayerIs = false;
            timer -= elapsed;
            if (timer < 0)
            {
                //Timer expired, execute action
                CheckWherePlayerIs = true;
                timer = TIMER;   //Reset Timer
            }

            IEnumerable<Guid> enemyEntites = playingState.Entities.Where(x => (x.ComponentFlags & ComponentMasks.Enemy) == ComponentMasks.Enemy).Select(x => x.ID);

            foreach (Guid enemyid in enemyEntites)
            {
                //get current enemey ai state
                AIComponent enemyAI = playingState.AIComponents[enemyid];
                if (enemyAI.PatrolPath.Count() > 0)
                {
                    //if we have a patrol path and we are not attacking, lets get back to patrolling
                    if (enemyAI.ActiveState.Peek() != AIState.PATROL && enemyAI.ActiveState.Peek() != AIState.ATTACK)
                    {
                        enemyAI.ActiveState.Push(AIState.PATROL);
                    }

                    if (enemyAI.ActiveState.Peek() == AIState.PATROL)
                    {
                        //lets do our patrol pathing
                        ProceedWithNextPath(enemyid, enemyAI, playingState);

                    }
                }



                if (CheckWherePlayerIs)
                {
                    AttackPlayer(enemyid, enemyAI, playingState);
                }
            }


        }
    }
}
