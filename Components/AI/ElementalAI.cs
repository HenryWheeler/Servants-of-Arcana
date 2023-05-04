using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                case State.Recall:
                    {
                        AIActions.ActionRecall(this);
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

                { new StateMachine(State.Asleep, Input.Recall), State.Recall },
                { new StateMachine(State.Angry, Input.Recall), State.Recall },
                { new StateMachine(State.Bored, Input.Recall), State.Recall },

                { new StateMachine(State.Recall, Input.Hurt), State.Angry },
                { new StateMachine(State.Recall, Input.Hatred), State.Angry },
                { new StateMachine(State.Recall, Input.Bored), State.Bored },
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
