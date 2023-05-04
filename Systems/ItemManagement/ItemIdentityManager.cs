using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servants_of_Arcana
{
    public class ItemIdentityManager
    {
        public static List<string> unidentifiedItems = new List<string>();
        public ItemIdentityManager() 
        {
            unidentifiedItems.Add("Scroll of Mapping");
            unidentifiedItems.Add("Wand of Digging");
            unidentifiedItems.Add("Wand of Fireball");
            unidentifiedItems.Add("Wand of Lightning");
            unidentifiedItems.Add("Wand of Transposition");
        }
        public static bool IsItemIdentified(string itemIdentity)
        {
            if (unidentifiedItems.Contains(itemIdentity)) return false;
            else return true;
        }
        public static void IdentifyItem(string itemIdentity)
        {
            if (unidentifiedItems.Contains(itemIdentity))
            {
                unidentifiedItems.Remove(itemIdentity);
            }
        }
    }
}
