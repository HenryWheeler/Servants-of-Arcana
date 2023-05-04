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
        public event Action<Entity, Vector> onDeath;
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
            
            if (Math.ReturnAIController(entity) != null && attacker.GetComponent<Faction>() != null)
            {
                AIController ai = Math.ReturnAIController(entity);
                if (!ai.hatedEntities.Contains(attacker))
                {
                    ai.hatedEntities.Add(attacker);
                }
                ai.currentInput = AIController.Input.Hurt;
                ai.target = attacker;
            }

            if (attributes.health <= 0)
            {
                Die(attacker);
            }
            else
            {
                Vector vector = entity.GetComponent<Vector>();
                Particle particle = ParticleManager.CreateParticle(false, vector, 5, 5, "Attached", new Draw(Color.Red, Color.Black, (char)3), vector, true, false, null, false, true);
                ParticleManager.CreateParticle(true, vector, 5, 5, "Attached", new Draw(Color.Red, Color.Black, (char)3), vector, true, false, new List<Particle>() { particle }, false, true);

                if (damage >= attributes.maxHealth / 4) { ParticleManager.CreateBloodStain(vector); }
            }
        }
        /// <summary>
        /// Use this version when applying damage from status effects
        /// </summary>
        /// <param name="damage"></param>
        public void RecieveDamage(int damage)
        {
            Attributes attributes = entity.GetComponent<Attributes>();

            attributes.health -= damage;

            if (entity.GetComponent<PlayerController> != null)
            {
                AttributeManager.UpdateAttributes(Program.player);
            }

            if (attributes.health <= 0)
            {
                Die(entity);
            }
            else
            {
                Vector vector = entity.GetComponent<Vector>();
                Particle particle = ParticleManager.CreateParticle(false, vector, 5, 5, "Attached", new Draw(Color.Red, Color.Black, (char)3), vector, true, false, null, false, true);
                ParticleManager.CreateParticle(true, vector, 5, 5, "Attached", new Draw(Color.Red, Color.Black, (char)3), vector, true, false, new List<Particle>() { particle }, false, true);
            }
        }
        public void Die(Entity killer)
        {
            TurnComponent component = entity.GetComponent<TurnComponent>();
            component.isAlive = false;
            component.isTurnActive = false;

            Vector position = entity.GetComponent<Vector>();

            ParticleManager.CreateBloodStain(position);
            Program.tiles[position.x, position.y].actor = null;

            Vector vector = entity.GetComponent<Vector>();
            Particle particle = ParticleManager.CreateParticle(false, vector, 5, 5, "Attached", new Draw(Color.Red, Color.Black, 'X'), vector, true, false, null, false, true);
            ParticleManager.CreateParticle(true, vector, 5, 5, "Attached", new Draw(Color.Red, Color.Black, 'X'), vector, true, false, new List<Particle>() { particle }, false, true);

            if (entity.GetComponent<PlayerController>() != null)
            {
                Log.Add($"{Program.playerName} dies.");
            }
            else
            {
                Log.Add($"The {entity.GetComponent<Description>().name} dies.");
            }

            onDeath?.Invoke(killer, position);

            TurnManager.RemoveActor(component);
            TurnManager.ProgressTurnOrder();
        }
    }
}
