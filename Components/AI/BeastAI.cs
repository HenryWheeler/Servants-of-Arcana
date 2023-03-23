using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servants_of_Arcana
{
    public class BeastAI : AIController
    {
        public override void PerformAction()
        {
            switch (currentState)
            {
                case State.Asleep:
                    {
                        AIActions.ActionSleep(this);
                        break;
                    }
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
                { new StateMachine(State.Asleep, Input.Noise), State.Bored },
                { new StateMachine(State.Asleep, Input.Hurt), State.Angry },
                { new StateMachine(State.Asleep, Input.Hatred), State.Angry },
                { new StateMachine(State.Angry, Input.Tired), State.Asleep },
                { new StateMachine(State.Angry, Input.Bored), State.Bored },
                { new StateMachine(State.Bored, Input.Hatred), State.Angry },
                { new StateMachine(State.Bored, Input.Hurt), State.Angry },
                { new StateMachine(State.Bored, Input.Tired), State.Asleep },
            };
            currentInput = Input.None;
        }
        public BeastAI(int baseInterest, int minDistance, int maxDistance, int abilityChance, int hate, int fear, int greed)
            : base(baseInterest, minDistance, maxDistance, abilityChance, hate, fear, greed)
        {
            hatedFactions.Add("Player");
            SetTransitions();
            currentState = State.Bored;
        }
        public BeastAI() { }
    }
}
