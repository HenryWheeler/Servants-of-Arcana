using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servants_of_Arcana
{
    public class TurnManager
    {
        public static List<TurnComponent> entities = new List<TurnComponent>();
        private static int turn = 0;
        private static int entityTurn = 0;
        public static void ProgressTurnOrder()
        {
            if (entities.Count != 0 && Program.isGameActive)
            {
                turn++;
                if (entityTurn >= entities.Count - 1) entityTurn = 0;
                else entityTurn++;
                if (entities[entityTurn].currentEnergy <= 0) ProgressActorTurn(entities[entityTurn]);
                else entities[entityTurn].StartTurn();
            }
        }
        public static void ProgressActorTurn(TurnComponent entity)
        {
            if (Program.isGameActive)
            {
                if (entity.currentEnergy <= 0) { entity.currentEnergy += entity.entity.GetComponent<Attributes>().maxEnergy; ProgressTurnOrder(); }
                else
                {
                    entity.currentEnergy--;
                    if (entity.currentEnergy <= 0) { entity.currentEnergy += entity.entity.GetComponent<Attributes>().maxEnergy; ProgressTurnOrder(); }
                    else { entity.StartTurn(); }
                }
            }
        }
        public static void AddActor(TurnComponent entity)
        {
            if (entity != null && !entities.Contains(entity)) 
            {
                entities.Add(entity);
            }
        }
        public static void RemoveActor(TurnComponent entity) 
        { 
            entities.Remove(entity); 
        }
    }
}
