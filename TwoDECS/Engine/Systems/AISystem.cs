using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TwoDECS.Engine.Algorithms;
using TwoDECS.Engine.Components;

namespace TwoDECS.Engine.Systems
{
    public static class AISystem
    {
        public static void AttackPlayer(Guid EnemyID, AIComponent enemeyAIComponent, PlayingState playingState)
        {
            //determine if the player is in line of site
            IEnumerable<Guid> playerEntities = playingState.Entities.Where(x => (x.ComponentFlags & ComponentMasks.Player) == ComponentMasks.Player).Select(x => x.ID);
            foreach(Guid playerid in playerEntities){
                //get the enemy direction
                DirectionComponent enemenyDirectionComponent = playingState.DirectionComponents[EnemyID];
                float direction = enemenyDirectionComponent.Direction;
                //if (LineOfSiteRayCast.CalculateLineOfSight(playingState.PositionComponents[EnemyID].Position, playingState.PositionComponents[playerid].Position, enemeyAIComponent.LineOfSite, playingState.DirectionComponents[EnemyID].Direction, playingState))
                //{
                //    //do something
                //}
                bool playerSeen = LineOfSiteRayCast.CalculateLineOfSight(playingState.PositionComponents[EnemyID].Position, playingState.PositionComponents[playerid].Position, enemeyAIComponent.LineOfSite, playingState.DirectionComponents[EnemyID].Direction, playingState);
                if(playerSeen)
                    Console.WriteLine(playerSeen);
                //enemeyComponent.Astar.Search()
            }
            
        }

       

        public static void UpdateEnemeyAI(PlayingState playingState)
        {
            IEnumerable<Guid> enemyEntites = playingState.Entities.Where(x => (x.ComponentFlags & ComponentMasks.Enemy) == ComponentMasks.Enemy).Select(x => x.ID);

            foreach (Guid enemyid in enemyEntites)
            {
                //get current enemey ai state
                AIComponent enemyAI = playingState.AIComponents[enemyid];
                if (enemyAI.PatrolPath.Count() > 0)
                {
                    if (enemyAI.ActiveState.First() != AIState.PATROL)
                    {
                        enemyAI.ActiveState.Insert(0, AIState.PATROL);
                    }
                }


                AttackPlayer(enemyid, enemyAI, playingState);
            }


        }
    }
}
