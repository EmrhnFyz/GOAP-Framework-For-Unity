using System;
using System.Collections.Generic;
using AI.GOAP.Enums;
using AI.GOAP.Sensor;
using UnityEngine;

namespace AI.GOAP
{
    public class BeliefFactory
    {
        private readonly GoapAgent _agent;
        private readonly Dictionary<BeliefName, Belief> _beliefs = new();

        public BeliefFactory(GoapAgent agent, Dictionary<BeliefName, Belief> beliefs)
        {
            this._agent = agent;
            this._beliefs = beliefs;
        }

        public void AddBelief(BeliefName key, Func<bool> condition)
        {
            _beliefs.Add(key, new Belief.Builder(key).WithCondition(condition)
                                                    .Build());
        }

        public void AddHasTargetsSensorBelief(BeliefName key, SensorBase sensor)
        {
            _beliefs.Add(key, new Belief.Builder(key)
                .WithCondition(() => sensor.HasTargets)
                .Build());
        }

        public void AddLocationBelief(BeliefName key, float distance, Vector3 locationCondition)
        {
            _beliefs.Add(key, new Belief.Builder(key).WithCondition(() => InRangeOf(locationCondition, distance))
                                                    .WithLocation(() => locationCondition)
                                                    .Build());
        }

        private bool InRangeOf(Vector3 position, float range) => Vector3.Distance(_agent.transform.position, position) < range;
    }

    public class Belief
    {
        public BeliefName Name { get; }

        private Func<bool> _condition = () => false;
        private Func<Vector3> _observedLocation = () => Vector3.zero;

        public Vector3 Location => _observedLocation();

        private Belief(BeliefName name)
        {
            Name = name;
        }

        public bool Evaluate() => _condition();

        public class Builder
        {
            private readonly Belief _belief;

            public Builder(BeliefName name)
            {
                _belief = new Belief(name);
            }

            public Builder WithCondition(Func<bool> condition)
            {
                _belief._condition = condition;
                return this;
            }

            public Builder WithLocation(Func<Vector3> observedLocation)
            {
                _belief._observedLocation = observedLocation;
                return this;
            }

            public Belief Build() => _belief;
        }
    }
}
