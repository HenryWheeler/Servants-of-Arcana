using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servants_of_Arcana
{
    public class CombatManager
    {
        public static void AttackTarget(Entity attacker, Entity reciever)
        {
            Entity weapon = attacker.GetComponent<InventoryComponent>().ReturnSlot("Weapon").item;
            if (weapon != null) 
            {
                WeaponComponent weaponComponent = weapon.GetComponent<WeaponComponent>();
                AttackTarget(attacker, reciever, weaponComponent.toHitBonus, weaponComponent.damageBonus, 
                    weaponComponent.damageDie1, weaponComponent.damageDie2, weapon.GetComponent<Description>().name);
            }
            else
            {
                AttackTarget(attacker, reciever, 0, 0, 1, 1, "Fists");
            }
        }
        public static void AttackTarget(Entity attacker, Entity reciever, int toHitBonus, int damageBonus, int damageDie1, int damageDie2, string weaponName)
        {
            Harmable harmable = reciever.GetComponent<Harmable>();
            if (harmable != null) 
            {
                Attributes attackerAttributes = attacker.GetComponent<Attributes>();
                Attributes recieverAttributes = reciever.GetComponent<Attributes>();

                int dmg = damageBonus;

                for (int d = 0; d <= damageDie1; d++)
                {
                    dmg += Program.random.Next(1, damageDie2 + 1);
                }

                if (Program.random.Next(1, 21) + attackerAttributes.strength + toHitBonus >= recieverAttributes.armorValue && dmg > 0)
                {
                    harmable.RecieveDamage(dmg, attacker);

                    if (attacker.GetComponent<PlayerController>() != null && attacker == reciever)
                    {
                        Log.Add($"You hit yourself for {dmg} damage with your {weaponName}!");
                    }
                    else if (attacker.GetComponent<PlayerController>() != null)
                    {
                        Log.Add($"You hit the {reciever.GetComponent<Description>().name} for {dmg} damage with your {weaponName}!");
                    }
                    else if (reciever.GetComponent<PlayerController>() != null)
                    {
                        Log.Add($"{attacker.GetComponent<Description>().name} hit you for {dmg} damage with its {weaponName}!");
                    }
                    else if (attacker == reciever)
                    {
                        Log.Add($"{attacker.GetComponent<Description>().name} hit the itself for {dmg} damage with its {weaponName}!");
                    }
                    else
                    {
                        Log.Add($"{attacker.GetComponent<Description>().name} hit the {reciever.GetComponent<Description>().name} for {dmg} damage with its {weaponName}!");
                    }
                }
                else if (attacker.GetComponent<PlayerController>() != null && attacker == reciever)
                {
                    Log.Add($"You missed yourself.");
                }
                else if (attacker.GetComponent<PlayerController>() != null)
                {
                    Log.Add($"You missed the {reciever.GetComponent<Description>().name}.");
                }
                else if (reciever.GetComponent<PlayerController>() != null)
                {
                    Log.Add($"{attacker.GetComponent<Description>().name} missed you.");
                }
                else if (attacker == reciever)
                {
                    Log.Add($"{attacker.GetComponent<Description>().name} missed itself.");
                }
                else
                {
                    Log.Add($"{attacker.GetComponent<Description>().name} missed the {reciever.GetComponent<Description>().name}.");
                }
            }

            attacker.GetComponent<TurnComponent>().EndTurn();
        }
        public static void SpecialAttack(Entity attacker, Entity reciever, int toHitBonus, int damageBonus, int damageDie1, int damageDie2, string weaponName)
        {
            Harmable harmable = reciever.GetComponent<Harmable>();
            if (harmable != null)
            {
                Attributes attackerAttributes = attacker.GetComponent<Attributes>();
                Attributes recieverAttributes = reciever.GetComponent<Attributes>();

                int dmg = damageBonus;

                for (int d = 0; d <= damageDie1; d++)
                {
                    dmg += Program.random.Next(1, damageDie2 + 1);
                }

                if (Program.random.Next(1, 21) + attackerAttributes.intelligence + toHitBonus >= recieverAttributes.armorValue && dmg > 0)
                {
                    harmable.RecieveDamage(dmg, attacker);

                    if (reciever.GetComponent<PlayerController>() != null)
                    {
                        Log.Add($"The {weaponName} hit you for {dmg} damage!");
                    }
                    else
                    {
                        Log.Add($"The {weaponName} hit the {reciever.GetComponent<Description>().name} for {dmg} damage!");
                    }
                }
                if (reciever.GetComponent<PlayerController>() != null)
                {
                    Log.Add($"The {weaponName} missed you.");
                }
                else
                {
                    Log.Add($"The {weaponName} missed the {reciever.GetComponent<Description>().name}.");
                }
            }

            attacker.GetComponent<TurnComponent>().EndTurn();
        }
    }
}
