using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servants_of_Arcana
{
    [Serializable]
    public class Explode : Component
    {
        public int strength { get; set; }
        public string type { get; set; }
        public bool fireball { get; set; }
        public override void SetDelegates()
        {
            switch (type)
            {
                case "Use":
                    {
                        entity.GetComponent<Usable>().onUse += Detonate;
                        entity.GetComponent<Usable>().areaOfEffect += AreaOfEffectModels.ReturnSphere;
                        strength = entity.GetComponent<Usable>().strength;
                        break;
                    }
                case "Death":
                    {
                        entity.GetComponent<Harmable>().onDeath += Detonate;
                        break;
                    }
                case "Step":
                    {
                        entity.GetComponent<Trap>().onStep += Detonate;
                        break;
                    }
            }
        }
        public void Detonate(Entity user, Vector origin)
        {
            switch (type)
            {
                case "Use":
                    {
                        SpecialEffects.Explosion(user, origin, strength, "Explosion", fireball);
                        break;
                    }
                case "Death":
                    {
                        SpecialEffects.Explosion(entity, origin, strength, "Explosion", fireball);
                        break;
                    }
            }
        }
        public void Detonate(Entity user)
        {
            SpecialEffects.Explosion(entity, entity.GetComponent<Vector>(), strength, "Explosion", fireball);
        }
        public Explode(int strength, string type, bool fireball)
        {
            this.strength = strength;
            this.type = type;
            this.fireball = fireball;
        }
    }
}
