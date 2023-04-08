using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Servants_of_Arcana.Components;

namespace Servants_of_Arcana
{
    public class ElementalAI : AIController
    {
        public override void PerformAction()
        {
            switch (currentState)
            {
                case State.Bored:
                    {
                        //AIActions.ActionSleep(this);
                        AIActions.ActionWander(this);
                        break;
                    }
                case State.Angry:
                    {
                        AIActions.ActionEngage(this);
                        break;
                    }
            }
        }
        public override void SetTransitions()
        {
            transitions = new Dictionary<StateMachine, State>
            {
                { new StateMachine(State.Angry, Input.Bored), State.Bored },
                { new StateMachine(State.Bored, Input.Hatred), State.Angry },
                { new StateMachine(State.Bored, Input.Hurt), State.Angry },
            };
            currentInput = Input.None;
        }
        public ElementalAI(int baseInterest, int minDistance, int maxDistance, int abilityChance, int hate, int fear, int greed)
            : base(baseInterest, minDistance, maxDistance, abilityChance, hate, fear, greed)
        {
            SetTransitions();
            currentState = State.Bored;
        }
        public ElementalAI() { }
    }
}
