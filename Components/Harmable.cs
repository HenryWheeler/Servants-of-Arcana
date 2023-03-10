using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SadRogue.Primitives;

namespace Servants_of_Arcana
{
    [Serializable]
    public class Harmable : Component
    {
        public event Action<Entity> onDeath;
        public Harmable() { }
        public override void SetDelegates() { }
        public void RecieveDamage(int damage, Entity attacker)
        {
            Attributes attributes = entity.GetComponent<Attributes>();

            attributes.health -= damage;

            if (entity.GetComponent<PlayerController> != null)
            {
                AttributeManager.UpdateAttributes(Program.player);
            }

            if (attributes.health <= 0)
            {
                Die(attacker);
            }
            else
            {
                Vector vector = entity.GetComponent<Vector>();
                Program.CreateSFX(vector, new Draw[] { new Draw(Color.Red, Color.Black, (char)3) }, 30, "Attached", 1, vector);
            }
        }
        public void Die(Entity killer)
        {
            TurnComponent component = entity.GetComponent<TurnComponent>();
            component.isAlive = false;
            component.isTurnActive = false;

            Vector position = entity.GetComponent<Vector>();

            Program.tiles[position.x, position.y].actor = null;

            Vector vector = entity.GetComponent<Vector>();
            Program.CreateSFX(vector, new Draw[] { new Draw(Color.Red, Color.Black, 'X') }, 30, "Attached", 1, vector);

            Log.Add($"The {entity.GetComponent<Description>().name} dies.");

            onDeath?.Invoke(killer);

            TurnManager.RemoveActor(component);
            TurnManager.ProgressTurnOrder();
        }
    }
}
