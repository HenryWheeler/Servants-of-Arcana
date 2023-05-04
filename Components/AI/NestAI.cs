using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servants_of_Arcana
{
    public class NestAI : AIController
    {
        public int maxMinions { get; set; }
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
                        entity.GetComponent<TurnComponent>().EndTurn();
                        break;
                    }
                case State.Angry:
                    {
                        foreach (Entity minion in entity.GetComponent<SpawnMinions>().minions)
                        {
                            if (Math.ReturnAIController(minion).target != target) 
                            {
                                Math.ReturnAIController(minion).currentInput = Input.Recall;
                            }
                        }

                        entity.GetComponent<TurnComponent>().EndTurn();
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
                { new StateMachine(State.Asleep, Input.Noise), State.Bored },
                { new StateMachine(State.Asleep, Input.Hurt), State.Angry },
                { new StateMachine(State.Asleep, Input.Hatred), State.Angry },
                { new StateMachine(State.Angry, Input.Tired), State.Asleep },
                { new StateMachine(State.Angry, Input.Bored), State.Bored },
                { new StateMachine(State.Bored, Input.Hatred), State.Angry },
                { new StateMachine(State.Bored, Input.Hurt), State.Angry },
                { new StateMachine(State.Bored, Input.Tired), State.Asleep },

                { new StateMachine(State.Asleep, Input.Recall), State.Recall },
                { new StateMachine(State.Angry, Input.Recall), State.Recall },
                { new StateMachine(State.Bored, Input.Recall), State.Recall },

                { new StateMachine(State.Recall, Input.Hurt), State.Angry },
                { new StateMachine(State.Recall, Input.Hatred), State.Angry },
                { new StateMachine(State.Recall, Input.Tired), State.Asleep },
                { new StateMachine(State.Recall, Input.Bored), State.Bored },
            };
            currentInput = Input.None;
        }
        public NestAI(int baseInterest, int minDistance, int maxDistance, int abilityChance, int hate, int fear, int greed, int maxMinions)
            : base(baseInterest, minDistance, maxDistance, abilityChance, hate, fear, greed)
        {
            SetTransitions();
            currentState = State.Bored;
            this.maxMinions = maxMinions;
        }
        public NestAI() { }
    }
}
