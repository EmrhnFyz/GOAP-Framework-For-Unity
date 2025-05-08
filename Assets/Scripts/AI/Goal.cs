using System.Collections.Generic;
using AI.GOAP.Enums;

namespace AI.GOAP
{
    public class Goal : IPrioritizable
    {
        public GoalName Name { get; }
        public float Priority { get; set; } = 1f;
        public HashSet<Belief> DesiredEffects { get; } = new();

        private Goal(GoalName name)
        {
            Name = name;
        }

        public class Builder
        {
            private readonly Goal _goal;

            public Builder(GoalName name)
            {
                _goal = new Goal(name);
            }

            public Builder WithPriority(float priority)
            {
                _goal.Priority = priority;
                return this;
            }

            public Builder WithDesiredEffect(Belief belief)
            {
                _goal.DesiredEffects.Add(belief);
                return this;
            }

            public Goal Build() => _goal;
        }
    }
}
