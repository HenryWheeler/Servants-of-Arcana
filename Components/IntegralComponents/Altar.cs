using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servants_of_Arcana
{
    public class Altar : Component
    {
        public List<string> options = new List<string>();
        public bool active = true;
        public override void SetDelegates()
        {
            options.Add(PrayerManager.terminalOptions[Program.dungeonGenerator.seed.Next(PrayerManager.terminalOptions.Count)]);
            bool trying = true;
            while (trying)
            {
                string option = PrayerManager.terminalOptions[Program.dungeonGenerator.seed.Next(PrayerManager.terminalOptions.Count)];
                if (options.Contains(option)) { continue; }
                else { options.Add(option); trying = false; }
            }
        }
        public Altar() { }
    }
}
