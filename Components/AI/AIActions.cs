using SadConsole.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Servants_of_Arcana
{
    public class AIActions
    {
        public static void ActionSleep(AIController AI)
        {

        }
        public static void ActionPatrol(AIController AI)
        {
            if (AI.entity.GetComponent<PatrolComponent>() == null) 
            {
                AI.entity.AddComponent(new PatrolComponent());
            }

            PatrolComponent patrolComponent = AI.entity.GetComponent<PatrolComponent>();

            AI.interest--;

            Vector vector = DijkstraMap.PathFromMap(AI.entity, $"Patrol-{patrolComponent.currentRoute}");
            AI.entity.GetComponent<Movement>().Move(vector);

            if (patrolComponent.lastPosition == vector)
            {
                AI.interest = AI.baseInterest;
                patrolComponent.currentRoute = Program.random.Next(1, DungeonGenerator.patrolRouteCount + 1);
            }
            patrolComponent.lastPosition = vector;
        }
        public static void ActionWander(AIController AI)
        {

        }
        public static void ActionEngage(AIController AI)
        {
            AI.interest--;

            AI.entity.GetComponent<Description>().description = "Red*Angry.";

            if (AI.target != null)
            {
                Vector position = AI.entity.GetComponent<Vector>();
                Vector targetPosition = AI.target.GetComponent<Vector>();
                if (Math.Distance(position.x, position.y, targetPosition.x, targetPosition.y) > AI.maxDistance)
                {
                    Vector nextPosition = AStar.ReturnPath(AI.entity.GetComponent<Vector>(), AI.target.GetComponent<Vector>())[1].position;
                    if (nextPosition != null)
                    {
                        AI.entity.GetComponent<Movement>().Move(nextPosition);
                        return;
                    }
                }
                else if ((int)Math.Distance(position.x, position.y, targetPosition.x, targetPosition.y) <= 1)
                {
                    CombatManager.AttackTarget(AI.entity, AI.target);
                    return;
                }
            }

            AI.entity.GetComponent<TurnComponent>().EndTurn();
        }
    }
}
