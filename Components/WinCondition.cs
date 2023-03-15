using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servants_of_Arcana
{
    [Serializable]
    public class WinCondition : Component
    {
        public override void SetDelegates()
        {
            entity.GetComponent<Usable>().onUse += WinGame;
        }
        public void WinGame(Entity user, Vector origin)
        {
            if (user.GetComponent<PlayerController>() != null)
            {
                foreach (TurnComponent component in TurnManager.entities)
                {
                    component.isAlive = false;
                    component.isTurnActive = false;
                }
                TurnManager.entities.Clear();
                Program.isGameActive = false;

                Program.depth = 0;
                ShadowcastFOV.RevealAll();
                ParticleEffects.FloorFadeAway();

                InteractionManager.CreatePopup(new List<string>() { "Your Winner!", "New Game: N", "Quit Game: Q" });
            }
        }
        public WinCondition() { }
    }
}
