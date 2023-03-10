using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servants_of_Arcana
{
    public class TurnComponent : Component
    {
        public bool isAlive { get; set; } = true;
        public bool isTurnActive { get; set; } = false;
        public float currentEnergy { get; set; }
        public Controller controller { get; set; }
        public void StartTurn()
        {
            if (isAlive)
            {
                isTurnActive = true;
                if (controller == null)
                {
                    controller = Math.ReturnController(entity);
                }
                controller.Execute();
            }
        }
        public void EndTurn()
        {
            if (isAlive)
            {
                isTurnActive = false;
                if (entity.GetComponent<PlayerController>() != null)
                {
                    ShadowcastFOV.ClearSight();
                    ShadowcastFOV.Compute(entity.GetComponent<Vector>(), 10);
                }

                Program.DrawMap();

                TurnManager.ProgressActorTurn(this);
            }
            else
            {
                TurnManager.RemoveActor(this);
                TurnManager.ProgressTurnOrder();
            }
        }
        public override void SetDelegates() { }
        public TurnComponent(Controller controller)
        {
            this.controller = controller;
        }
        public TurnComponent() { }
    }
}
