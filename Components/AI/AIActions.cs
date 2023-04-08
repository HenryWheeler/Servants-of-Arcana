using SadConsole.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Servants_of_Arcana.Components
{
    public class AIActions
    {
        public static void ActionSleep(AIController AI)
        {
            AI.interest--;
            AI.entity.GetComponent<TurnComponent>().EndTurn();
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
            List<Vector> chosenTiles = new List<Vector>();
            Vector startingLocation = AI.entity.GetComponent<Vector>();

            for (int x = startingLocation.x - 1; x <= startingLocation.x + 1; x++)
            {
                for (int y = startingLocation.y - 1; y <= startingLocation.y + 1; y++)
                {
                    if (Math.CheckBounds(x, y) && Program.tiles[x, y].terrainType != 0 && new Vector(x, y) != startingLocation && !chosenTiles.Contains(new Vector(x, y)))
                    {
                        chosenTiles.Add(new Vector(x, y));
                    }
                }
            }

            if (chosenTiles.Count > 0)
            {
                AI.entity.GetComponent<Movement>().Move(chosenTiles[Program.random.Next(chosenTiles.Count)]);
            }
            else
            {
                AI.entity.GetComponent<TurnComponent>().EndTurn();
            }
        }
        public static void ActionEngage(AIController AI)
        {
            AI.interest--;

            if (AI.target != null && !AI.target.GetComponent<TurnComponent>().isAlive)
            {
                AI.hatedEntities.Remove(AI.target);
                AI.target = null;

                AI.currentInput = AIController.Input.Bored;
                AI.Execute();
                return;
            }

            if (AI.target != null)
            {
                Vector position = AI.entity.GetComponent<Vector>();
                Vector targetPosition = AI.target.GetComponent<Vector>();
                double distance = Math.Distance(position.x, position.y, targetPosition.x, targetPosition.y);
                if (distance > AI.maxDistance + .5f)
                {
                    Vector nextPosition = AStar.ReturnPath(AI.entity.GetComponent<Vector>(), AI.target.GetComponent<Vector>())[1].position;
                    if (nextPosition != null)
                    {
                        AI.entity.GetComponent<Movement>().Move(nextPosition);
                        return;
                    }
                }
                else if (distance != 1 && distance != 1.5)
                {
                    if (distance > AI.minDistance + .5f && Program.random.Next(0, 101) > AI.abilityChance)
                    {
                        Vector nextPosition = AStar.ReturnPath(AI.entity.GetComponent<Vector>(), AI.target.GetComponent<Vector>())[1].position;
                        if (nextPosition != null)
                        {
                            AI.entity.GetComponent<Movement>().Move(nextPosition);
                            return;
                        }
                    }
                    else if (AI.entity.GetComponent<Usable>() != null && distance <= AI.entity.GetComponent<Usable>().range && !Math.CheckPath(position.x, position.y, targetPosition.x, targetPosition.y))
                    {
                        bool targetValid = true;

                        foreach (Vector vector in AI.entity.GetComponent<Usable>().areaOfEffect.Invoke(position, targetPosition, AI.entity.GetComponent<Usable>().range))
                        {
                            if (Program.tiles[vector.x, vector.y] != null && Program.tiles[vector.x, vector.y].actor == AI.entity)
                            {
                                targetValid = false;
                            }
                        }

                        if (targetValid)
                        {
                            AI.entity.GetComponent<Usable>().Use(AI.entity, targetPosition);
                            AI.entity.GetComponent<TurnComponent>().EndTurn();
                            return;
                        }
                    }
                }

                if (AI.target != null && Math.Distance(position.x, position.y, targetPosition.x, targetPosition.y) <= 1.5)
                {
                    CombatManager.AttackTarget(AI.entity, AI.target);
                    return;
                }

                AI.entity.GetComponent<TurnComponent>().EndTurn();
            }
        }
    }
}
