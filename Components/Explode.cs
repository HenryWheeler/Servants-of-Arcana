using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servants_of_Arcana.Components
{
    [Serializable]
    public class Explode : Component
    {
        public int strength { get; set; }
        public bool onUse { get; set; }
        public bool fireball { get; set; }
        public override void SetDelegates()
        {
            if (onUse)
            {
                entity.GetComponent<Usable>().onUse += Detonate;
                entity.GetComponent<Usable>().areaOfEffect += AreaOfEffectModels.ReturnSphere;
                strength = entity.GetComponent<Usable>().strength;
            }
            else
            {
                entity.GetComponent<Harmable>().onDeath += Detonate;
            }
        }
        public void Detonate(Entity user, Vector origin)
        {
            if (onUse)
            {
                SpecialEffects.Explosion(user, origin, strength, "Explosion", fireball);
            }
            else
            {
                SpecialEffects.Explosion(entity, origin, strength, "Explosion", fireball);
            }
        }
        public Explode(int strength, bool onUse, bool fireball)
        {
            this.strength = strength;
            this.onUse = onUse;
            this.fireball = fireball;
        }
    }
}
