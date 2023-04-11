using System;
using System.Collections.Generic;

namespace Servants_of_Arcana
{
    [Serializable]
    public class Movement : Component
    {
        public event Action <Entity> onMovement;
        public List<int> moveTypes = new List<int>();
        public void Move(Vector newPosition)
        {
            Vector originalPosition = entity.GetComponent<Vector>();

            if (Math.CheckBounds(newPosition.x, newPosition.y))
            {
                Tile newTraversable = Program.tiles[newPosition.x, newPosition.y];
                if (moveTypes.Contains(newTraversable.terrainType))
                {
                    if (newTraversable.actor == null)
                    {
                        Program.tiles[originalPosition.x, originalPosition.y].actor = null;

                        entity.GetComponent<Vector>().x = newPosition.x;
                        entity.GetComponent<Vector>().y = newPosition.y;

                        newTraversable.actor = entity;

                        onMovement?.Invoke(entity);

                        newTraversable.GetComponent<Trap>()?.TriggerTrap(entity);
                    }
                    else if (entity.GetComponent<PlayerController>() != null && newTraversable.actor != entity)
                    {
                        if (Math.ReturnAIController(newTraversable.actor).hatedEntities.Contains(entity) || 
                            Math.ReturnAIController(newTraversable.actor).hatedFactions.Contains(entity.GetComponent<Faction>().faction))
                        {
                            CombatManager.AttackTarget(entity, newTraversable.actor);
                            return;
                        }
                        else
                        {
                            Vector currentPosition = entity.GetComponent<Vector>();
                            Tile oldTraversable = Program.tiles[currentPosition.x, currentPosition.y];

                            newTraversable.actor.GetComponent<Vector>().x = currentPosition.x;
                            newTraversable.actor.GetComponent<Vector>().y = currentPosition.y;

                            oldTraversable.actor = newTraversable.actor;

                            onMovement?.Invoke(entity);

                            newTraversable.GetComponent<Trap>()?.TriggerTrap(newTraversable.actor);

                                                                                                                                                                                                  
                            entity.GetComponent<Vector>().x = newPosition.x;
                            entity.GetComponent<Vector>().y = newPosition.y;

                            newTraversable.actor = entity;

                            onMovement?.Invoke(entity);

                            newTraversable.GetComponent<Trap>()?.TriggerTrap(entity);
                        }
                    }
                }
                else if (entity.GetComponent<PlayerController>() != null)
                {
                    Log.Add($"{Program.playerName} cannot move there.");
                    return;
                }
            }
            else if (entity.GetComponent<PlayerController>() != null)
            {
                Log.Add($"{Program.playerName} cannot move there.");
                return;
            }

            entity.GetComponent<TurnComponent>().EndTurn();
        }
        public override void SetDelegates() 
        {
            //onMovement += ParticleManager.CreateMovementParticle;
        }
        public Movement(List<int> _moveTypes)
        {
            moveTypes = _moveTypes;
        }
        public Movement() { }
    }
}
