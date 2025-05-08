using System.Collections.Generic;
using AI.GOAP.Enums;

namespace AI.GOAP
{
    public class AgentAction
    {
        public ActionName Name { get; }

        public float Cost { get; private set; }

        public HashSet<Belief> Preconditions { get; } = new();
        public HashSet<Belief> Effects { get; } = new();

        private IActionStrategy _strategy;

        public bool Complete => _strategy.Complete;
        public bool CanPerform => _strategy.CanPerform;
        private AgentAction(ActionName name)
        {
            Name = name;
        }

        public void Start() => _strategy.Start();

        public void Update(float deltaTime)
        {
            // check if the action can be performed and update the strategy
            if (_strategy.CanPerform)
            {
                _strategy.Update(deltaTime);
            }

            // Bail out  if the strategy is still executing
            if (!_strategy.Complete)
            {
                return;
            }

            // Evaluate the effects

            foreach (var effect in Effects)
            {
                effect.Evaluate();
            }
        }

        public void Stop() => _strategy.Stop();

        public class Builder
        {
            private readonly AgentAction _action;

            public Builder(ActionName name)
            {
                _action = new AgentAction(name)
                {
                    Cost = 1
                };
            }

            public Builder WithCost(float cost)
            {
                _action.Cost = cost;
                return this;
            }

            public Builder WithStrategy(IActionStrategy strategy)
            {
                _action._strategy = strategy;
                return this;
            }

            public Builder AddPrecondition(Belief precondition)
            {
                _action.Preconditions.Add(precondition);
                return this;
            }

            public Builder AddEffect(Belief effect)
            {
                _action.Effects.Add(effect);
                return this;
            }

            public AgentAction Build() => _action;
        }
    }
}
