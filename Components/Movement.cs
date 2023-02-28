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
                        
                        if (onMovement != null) { onMovement.Invoke(entity); }

                        entity.GetComponent<TurnComponent>().EndTurn();
                    }
                    else if (entity.GetComponent<PlayerController>() != null)
                    {
                        //AttackManager.MeleeAllStrike(entity, newTraversable.actorLayer);
                    }
                    else
                    {
                        entity.GetComponent<TurnComponent>().EndTurn();
                    }
                }
                else if (entity.GetComponent<PlayerController>() != null)
                {
                    Log.Add($"You cannot move there.");
                }
            }
            else if (entity.GetComponent<PlayerController>() != null)
            {
                Log.Add("You cannot move there.");
            }
        }
        public Movement(List<int> _moveTypes)
        {
            moveTypes = _moveTypes;
        }
        public Movement() { }
    }
}
