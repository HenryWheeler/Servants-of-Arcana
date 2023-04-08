using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servants_of_Arcana
{
    /// <summary>
    /// This class allows you to pass an entity and fill its hated factions list full of all relevant factions it will hate on default.
    /// Some factions hate each other and will attack on sight.
    /// </summary>
    public class FactionManager
    {
        public static void AddHatedFactions(AIController AI)
        {
            if (AI == null) { return; }
            string faction = AI.entity.GetComponent<Faction>().faction;

            switch (faction) 
            {
                case "Undead":
                    {
                        AI.hatedFactions.Add("Beast");
                        AI.hatedFactions.Add("Drake");
                        AI.hatedFactions.Add("Evil-Humanoid");
                        AI.hatedFactions.Add("Player");
                        break;
                    }
                case "Oozeling":
                    {
                        AI.hatedFactions.Add("Beast");
                        AI.hatedFactions.Add("Drake");
                        AI.hatedFactions.Add("Evil-Humanoid");
                        AI.hatedFactions.Add("Player");
                        AI.hatedFactions.Add("Plant");
                        break;
                    }
                case "Construct":
                    {
                        AI.hatedFactions.Add("Undead");
                        AI.hatedFactions.Add("Elemental");
                        AI.hatedFactions.Add("Beast");
                        AI.hatedFactions.Add("Drake");
                        AI.hatedFactions.Add("Evil-Humanoid");
                        AI.hatedFactions.Add("Player");
                        break;
                    }
                case "Beast":
                    {
                        AI.hatedFactions.Add("Undead");
                        AI.hatedFactions.Add("Oozeling");
                        AI.hatedFactions.Add("Evil-Humanoid");
                        AI.hatedFactions.Add("Drake");
                        AI.hatedFactions.Add("Player");
                        break;
                    }
                case "Drake":
                    {
                        AI.hatedFactions.Add("Undead");
                        AI.hatedFactions.Add("Oozeling");
                        AI.hatedFactions.Add("Evil-Humanoid");
                        AI.hatedFactions.Add("Beast");
                        AI.hatedFactions.Add("Player");
                        break;
                    }
                case "Evil-Humanoid":
                    {
                        AI.hatedFactions.Add("Undead");
                        AI.hatedFactions.Add("Oozeling");
                        AI.hatedFactions.Add("Drake");
                        AI.hatedFactions.Add("Beast");
                        AI.hatedFactions.Add("Player");
                        break;
                    }
                case "Elemental":
                    {
                        AI.hatedFactions.Add("Undead");
                        AI.hatedFactions.Add("Oozeling");
                        AI.hatedFactions.Add("Construct");
                        AI.hatedFactions.Add("Beast");
                        AI.hatedFactions.Add("Drake");
                        AI.hatedFactions.Add("Evil-Humanoid");
                        AI.hatedFactions.Add("Player");
                        break;
                    }
                case "Plant":
                    {
                        AI.hatedFactions.Add("Oozeling");
                        AI.hatedFactions.Add("Beast");
                        AI.hatedFactions.Add("Drake");
                        AI.hatedFactions.Add("Evil-Humanoid");
                        AI.hatedFactions.Add("Player");
                        break;
                    }
            }
        }
    }
}
