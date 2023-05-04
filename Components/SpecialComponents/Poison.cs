using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Bson;
using SadRogue.Primitives;

namespace Servants_of_Arcana
{
    [Serializable]
    public class Poison : Component
    {
        public int timeRemaining { get; set; }
        public int strength { get; set; }
        public override void SetDelegates()
        {
            entity.GetComponent<TurnComponent>().onTurnEnd += Damage;
        }
        public void Damage(Vector vector)
        {
            if (entity != null)
            {
                timeRemaining--;

                if (timeRemaining <= 0)
                {
                    entity.GetComponent<TurnComponent>().onTurnEnd -= Damage;

                    if (entity.GetComponent<PlayerController>() != null)
                    {
                        AttributeManager.UpdateAttributes(entity);
                    }
                    entity.RemoveComponent(this);
                }
                else
                {
                    entity.GetComponent<Harmable>().RecieveDamage(strength);
                    Particle particle = ParticleManager.CreateParticle(false, vector, 5, 5, "Attached", new Draw(Color.Green, Color.Black, (char)3), vector, true, false, null, false, true);
                    ParticleManager.CreateParticle(true, vector, 5, 5, "Attached", new Draw(Color.Green, Color.Black, (char)3), vector, true, false, new List<Particle>() { particle }, false, true);
                }
            }
        }
        public string ReturnStatusName() { return "Poisoned, "; }
        public Poison(int timeRemaining, int strength)
        {
            this.timeRemaining = timeRemaining;
            this.strength = strength;
        }
        public Poison() { }
    }
    [Serializable]
    public class ApplyPoison : Component
    {
        public int timeRemaining { get; set; }
        public int strength { get; set; }
        public int radius { get; set; }
        public string type { get; set; }
        public override void SetDelegates()
        {
            if (type == "Sphere")
            {
                entity.GetComponent<Usable>().areaOfEffect += AreaOfEffectModels.ReturnSphere;
                entity.GetComponent<Usable>().onUse += Poison;

                strength = entity.GetComponent<Usable>().strength;
            }
            else if (type == "Line")
            {
                entity.GetComponent<Usable>().areaOfEffect += AreaOfEffectModels.ReturnLine;
                entity.GetComponent<Usable>().onUse += Poison;

                strength = entity.GetComponent<Usable>().strength;
            }
            else if (type == "Hit")
            {
                entity.GetComponent<WeaponComponent>().onHit += Poison;
            }
        }
        public void Poison(Entity attacker, Entity receiver)
        {
            SpecialEffects.ApplyPoison(receiver, strength, timeRemaining);
        }
        public void Poison(Entity entity, Vector origin)
        {
            SpecialEffects.Poison(entity, origin, type, strength, radius, timeRemaining);
        }
        public ApplyPoison(int timeRemaining, int strength, int radius, string type)
        {
            this.timeRemaining = timeRemaining;
            this.strength = strength;
            this.radius = radius;
            this.type = type;
        }
        public ApplyPoison() { }
    }
}
