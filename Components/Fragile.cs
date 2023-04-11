using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servants_of_Arcana.Components
{
    [Serializable]
    public class Fragile : Component
    {
        public int breakChance { get; set; }
        public string breakText { get; set; }
        public override void SetDelegates()
        {
            if (entity.GetComponent<Usable>() != null)
            {
                entity.GetComponent<Usable>().onUse += AttemptBreak;
            }

            if (entity.GetComponent<Equipable>() != null)
            {
                //When implementing proper armor add an action for when the equipped creature is hit.
                //entity.GetComponent<Equipable>().onEquip += AttemptBreak;
            }

            if (entity.GetComponent<WeaponComponent>() != null)
            {
                entity.GetComponent<WeaponComponent>().onHit += AttemptBreak;
            }
        }
        public void AttemptBreak(Entity user, Vector location)
        {
            if (Program.random.Next(1, 101) < breakChance)
            {
                Log.Add($"{user.GetComponent<Description>().name}'s {entity.GetComponent<Description>().name} {breakText}");

                if (entity.GetComponent<Equipable>() != null && entity.GetComponent<Equipable>().equipped)
                {
                    InventoryManager.UnequipItem(user, entity, false);
                }
                user.GetComponent<InventoryComponent>().items.Remove(entity);
            }
        }
        public void AttemptBreak(Entity user, Entity reciever)
        {
            if (Program.random.Next(1, 101) < breakChance)
            {
                if (user.GetComponent<PlayerController>() != null)
                {
                    Log.Add($"Your {entity.GetComponent<Description>().name} {breakText}");
                }
                else
                {
                    Log.Add($"The {user.GetComponent<Description>().name}'s {entity.GetComponent<Description>().name} {breakText}");
                }

                if (entity.GetComponent<Equipable>() != null && entity.GetComponent<Equipable>().equipped)
                {
                    InventoryManager.UnequipItem(user, entity, false);
                }
                user.GetComponent<InventoryComponent>().items.Remove(entity);
            }
        }
        public Fragile(int breakChance, string breakText)
        {
            this.breakChance = breakChance;
            this.breakText = breakText;
        }
        public Fragile() { }
    }
}
