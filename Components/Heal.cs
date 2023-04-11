using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SadRogue.Primitives;

namespace Servants_of_Arcana
{
    [Serializable]
    public class Heal : Component
    {
        public int strength { get; set; }
        public override void SetDelegates()
        {
            entity.GetComponent<Usable>().onUse += HealDrinker;
        }
        public void HealDrinker(Entity actor, Vector target)
        {
            Entity healed = Program.tiles[target.x, target.y].actor;

            Attributes attributes = healed.GetComponent<Attributes>();

            if (attributes.health + strength > attributes.maxHealth)
            {
                attributes.health = attributes.maxHealth;
            }
            else
            {
                attributes.health += strength;
            }
        }
        public Heal(int strength)
        {
            this.strength = strength;
        }
        public Heal() { }
    }
    [Serializable]
    public class IncreaseAttribute : Component
    {
        public float strength { get; set; }
        public string attribute { get; set; }
        public override void SetDelegates()
        {
            entity.GetComponent<Usable>().onUse += HealDrinker;
        }
        public void HealDrinker(Entity actor, Vector target)
        {
            Entity healed = Program.tiles[target.x, target.y].actor;
            Attributes attributes = healed.GetComponent<Attributes>();

            switch (attribute)
            {
                case "Strength":
                    {
                        attributes.strength += (int)strength;
                        break;
                    }
                case "Intelligence":
                    {
                        attributes.intelligence += (int)strength;
                        break;
                    }
                case "Speed":
                    {
                        attributes.maxEnergy += strength;
                        break;
                    }
            }

            if (healed.GetComponent<PlayerController>() != null)
            {
                AttributeManager.UpdateAttributes(Program.player);
            }
        }
        public IncreaseAttribute(float strength, string attribute)
        {
            this.strength = strength;
            this.attribute = attribute;
        }
        public IncreaseAttribute() { }
    }
}
