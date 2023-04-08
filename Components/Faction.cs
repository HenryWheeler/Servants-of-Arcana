using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servants_of_Arcana
{
    [Serializable]
    public class Faction : Component
    {
        public string faction { get; set; }
        public override void SetDelegates() 
        {
            FactionManager.AddHatedFactions(Math.ReturnAIController(entity));
        }
        public Faction(string faction)
        {
            this.faction = faction;
        }
        public Faction() { }
    }
}
