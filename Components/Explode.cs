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
        public string message { get; set; }
        public bool onUse { get; set; }
        public override void SetDelegates()
        {
            if (onUse)
            {
                entity.GetComponent<Usable>().onUse += Detonate;
                entity.GetComponent<Usable>().areaOfEffect += AreaOfEffectModels.ReturnSphere;
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
                if (user.GetComponent<PlayerController>() != null && origin == null)
                {

                }
                else
                {
                    Log.Add(message);
                    SpecialEffects.Explosion(user, origin, strength, "Explosion");
                }
            }
            else
            {
                Log.Add(message);
                SpecialEffects.Explosion(user, origin, strength, "Explosion");
            }
        }
        public Explode(int strength, bool onUse, string message)
        {
            this.strength = strength;
            this.onUse = onUse;
            this.message = message;
        }
    }
}
