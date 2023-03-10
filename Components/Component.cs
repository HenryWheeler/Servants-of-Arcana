using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servants_of_Arcana
{
    [Serializable]
    public abstract class Component
    {
        public Entity entity { get; set; }
        public abstract void SetDelegates();
        public Component() { }
    }
    [Serializable]
    class Actor : Component
    {
        public override void SetDelegates() { }
        public Actor() { }
    }
    [Serializable]
    class Item : Component
    {
        public override void SetDelegates() { }
        public Item() { }
    }
}
