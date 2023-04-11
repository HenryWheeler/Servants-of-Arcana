using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servants_of_Arcana
{
    [Serializable]
    public class SpawnMinions : Component
    {
        public string message { get; set; }
        public List<string> minionNames { get; set; } 
        public int amountToSummon { get; set; }
        public List<Entity> minions = new List<Entity>();
        public string delegateType { get; set; }
        public override void SetDelegates()
        {
            switch (delegateType)
            {
                case "Use":
                    {
                        entity.GetComponent<Usable>().onUse += Summon;
                        break;
                    }
                case "Death":
                    {
                        entity.GetComponent<Harmable>().onDeath += Summon;
                        break;
                    }
                case "Spawn":
                    {
                        entity.GetComponent<SpawnDetails>().onSpawn += Summon;
                        break;
                    }
            }
        }
        public void Summon(Entity user, Vector origin)
        {
            Log.Add(message);
            minions = SpecialEffects.SummonMinions(user, origin, minionNames, amountToSummon);
        }
        public void Summon(Room room)
        {
            minions = SpecialEffects.SummonMinions(entity, room, minionNames, amountToSummon);
        }
        public SpawnMinions(List<string> minionNames, int amountToSummon, string delegateType)
        {
            this.minionNames = minionNames;
            this.amountToSummon = amountToSummon;
            this.delegateType = delegateType;
        }
    }
    /// <summary>
    /// A component for items that summon minions when the item is equipped, and despawns the minions when the item is unequipped.
    /// </summary>
    [Serializable]
    public class BoundMinion : SpawnMinions
    {
        public override void SetDelegates()
        {
            entity.GetComponent<Equipable>().onEquip += Summon;
        }
        public void Summon(Entity user, bool equipped)
        {
            if (equipped)
            {
                if (minionNames.Count == 0)
                {
                    Log.Add("Nothing happens.");
                }
                else
                {
                    Log.Add(message);

                    List<Vector> chosenTiles = new List<Vector>();
                    Vector startingLocation = user.GetComponent<Vector>();

                    for (int x = startingLocation.x - 3; x <= startingLocation.x + 3; x++)
                    {
                        for (int y = startingLocation.y - 3; y <= startingLocation.y + 3; y++)
                        {
                            if (Math.CheckBounds(x, y) && Program.tiles[x, y].terrainType != 0 && new Vector(x, y) != startingLocation && Program.tiles[x, y].actor == null && !chosenTiles.Contains(new Vector(x, y)))
                            {
                                chosenTiles.Add(new Vector(x, y));
                            }
                        }
                    }

                    Vector chosen = null;

                    if (chosenTiles.Count > 0)
                    {
                        chosen = chosenTiles[Program.random.Next(chosenTiles.Count)];
                    }

                    if (chosen != null)
                    {
                        minions = SpecialEffects.SummonMinions(user, chosen, minionNames, amountToSummon);
                        minionNames.Clear();
                    }
                    else
                    {
                        Log.Add("There is not enough room to summon minions.");
                    }
                }
            }
            else
            {
                foreach (Entity minion in minions)
                {
                    TurnComponent component = minion.GetComponent<TurnComponent>();
                    if (component.isAlive)
                    {
                        Vector vector = minion.GetComponent<Vector>();

                        component.isAlive = false;
                        component.isTurnActive = false;
                        TurnManager.RemoveActor(component);
                        Program.tiles[vector.x, vector.y].actor = null;

                        minionNames.Add(minion.GetComponent<Description>().name);
                    }
                }
            }
        }
        public BoundMinion(List<string> minionNames, int amountToSummon) : base(minionNames, amountToSummon, "Use") { }
    }
}
