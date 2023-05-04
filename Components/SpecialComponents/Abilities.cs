using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servants_of_Arcana
{
    [Serializable]
    public class ActorAbilities : Component
    {
        public List<ActorAbilitySlot> abilities = new List<ActorAbilitySlot>();
        public override void SetDelegates() 
        {
            entity.GetComponent<TurnComponent>().onTurnEnd += ProgressCooldowns;
        }
        public bool ActivateAbility(Entity user, Vector target, string type)
        {
            List<ActorAbilitySlot> selection = new List<ActorAbilitySlot>();
            foreach (var ability in abilities)
            {
                if (ability.type == type)
                {
                    selection.Add(ability);
                }
            }

            if (selection.Count < 0) { return false; }

            ActorAbilitySlot finalSelection = selection[Program.random.Next(selection.Count)];
            finalSelection.isOnCooldown = true;
            finalSelection.currentCooldown = finalSelection.cooldownDuration;

            switch (finalSelection.ability)
            {
                case "Magic Map":
                    {
                        SpecialEffects.MagicMap(user, target);
                        break;
                    }
            }

            return true;
        }
        public void ProgressCooldowns(Vector vector)
        {
            foreach (var ability in abilities)
            {
                if (ability.isOnCooldown)
                {
                    ability.currentCooldown--;
                    if (ability.currentCooldown <= 0)
                    {
                        ability.isOnCooldown = false; ;
                    }
                }
            }
        }
        public ActorAbilities(List<ActorAbilitySlot> abilities)
        {
            this.abilities = abilities;
        }
        public ActorAbilities() { }
    }
    [Serializable]
    public class ItemAbilities : Component
    {
        public List<ItemAbilitySlot> abilities = new List<ItemAbilitySlot>();
        public override void SetDelegates()
        {
            entity.GetComponent<Usable>().onUse += ActivateAbilities;
        }
        public void ActivateAbilities(Entity user, Vector target)
        {
            foreach (var ability in abilities) 
            {
                switch (ability.ability)
                {
                    case "Magic Map":
                        {
                            SpecialEffects.MagicMap(user, target);
                            break;
                        }
                }
            }
        }
        public List<Vector> AreaOfEffect(Vector origin, Vector target, int range)
        {
            List<Vector> effectedTiles = new List<Vector>();
            foreach (var ability in abilities)
            {
                switch (ability.ability)
                {
                    case "Lightning":
                        {
                            effectedTiles.AddRange(AreaOfEffectModels.ReturnLine(origin, target, range));
                            break;
                        }
                }
            }

            return effectedTiles;
        }
        public ItemAbilities(List<ItemAbilitySlot> abilities)
        {
            this.abilities = abilities;
        }
        public ItemAbilities() { }
    }
    public class ActorAbilitySlot
    {
        public string ability { get; set; }
        public string type { get; set; }
        public int range { get; set; }
        public int strength { get; set; }
        public int cooldownDuration { get; set; }
        public int currentCooldown { get; set; } = 0;
        public bool isOnCooldown { get; set; } = false;
        public ActorAbilitySlot(string ability, string type, int range, int strength, int cooldownDuration)
        {
            this.ability = ability;
            this.type = type;
            this.range = range;
            this.strength = strength;
            this.cooldownDuration = cooldownDuration;
        }
    }
    public class ItemAbilitySlot
    {
        public string ability { get; set; }
        public int strength { get; set; }
        public ItemAbilitySlot(string ability, int strength)
        {
            this.ability = ability;
            this.strength = strength;
        }
        public ItemAbilitySlot() { }
    }
}
