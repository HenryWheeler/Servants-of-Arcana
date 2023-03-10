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

            if (actor == healed)
            {
                if (actor.GetComponent<PlayerController>() != null)
                {
                    Log.Add("You heal yourself!");
                }
                else
                {
                    Log.Add($"The {actor.GetComponent<Description>().name} heals itself!");
                }
            }
            else
            {
                if (actor.GetComponent<PlayerController>() != null)
                {
                    Log.Add($"You heal the {healed.GetComponent<Description>().name}!");
                }
                else
                {
                    Log.Add($"The {actor.GetComponent<Description>().name} heals the {healed.GetComponent<Description>().name}!");
                }
            }

            Program.CreateSFX(target, new Draw[] { new Draw(Color.Red, Color.Black, (char)3) }, 30, "Attached", 2, target);
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
        public int strength { get; set; }
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
                        if (healed.GetComponent<PlayerController>() != null)
                        {
                            Log.Add("You feel mightier!");
                        }
                        else
                        {
                            Log.Add($"The {healed.GetComponent<Description>().name} feels mightier!");
                        }

                        attributes.strength += strength;

                        Program.CreateSFX(target, new Draw[] { new Draw(Color.Orange, Color.Black, (char)19) }, 30, "Attached", 2, target);
                        break;
                    }
                case "Intelligence":
                    {
                        if (healed.GetComponent<PlayerController>() != null)
                        {
                            Log.Add("You feel smarter!");
                        }
                        else
                        {
                            Log.Add($"The {healed.GetComponent<Description>().name} feels smarter!");
                        }

                        attributes.intelligence += strength;

                        Program.CreateSFX(target, new Draw[] { new Draw(Color.Blue, Color.Black, (char)19) }, 30, "Attached", 2, target);
                        break;
                    }
                case "Speed":
                    {
                        if (healed.GetComponent<PlayerController>() != null)
                        {
                            Log.Add("You feel faster!");
                        }
                        else
                        {
                            Log.Add($"The {healed.GetComponent<Description>().name} feels faster!");
                        }

                        attributes.maxEnergy += strength;

                        Program.CreateSFX(target, new Draw[] { new Draw(Color.Yellow, Color.Black, (char)19) }, 30, "Attached", 2, target);
                        break;
                    }
            }

            if (healed.GetComponent<PlayerController>() != null)
            {
                AttributeManager.UpdateAttributes(Program.player);
            }
        }
        public IncreaseAttribute(int strength, string attribute)
        {
            this.strength = strength;
            this.attribute = attribute;
        }
        public IncreaseAttribute() { }
    }
}
