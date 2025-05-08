# GOAP Framework for Unity
A robust, optimized Goal-Oriented Action Planning (GOAP) framework for Unity, designed to create intelligent AI behaviors with minimal performance overhead.

## Overview
GOAP is an AI architecture that allows agents to generate plans to satisfy goals based on the current world state. This framework provides:

- **Flexible AI Planning**: Agents dynamically create plans to achieve goals based on changing world conditions
- **Memory-Optimized Design**: Extensive object pooling system to minimize garbage collection
- **Modular Architecture**: Easy to extend with new actions, sensors, and beliefs
- **Performance-Focused**: Built with game performance in mind
## Features
- 🧠 **Complete GOAP Implementation** - All core GOAP components (agents, actions, goals, beliefs)
- 🔄 **Object Pool System** - Optimized memory usage with pooled collections and objects
- 🎯 **Priority-Based Goal Selection** - Agents choose the most relevant goals
- 📊 **Pluggable Sensor System** - Flexible perception system for agents
- 🔍 **A * Planning Algorithm** - Efficient pathfinding through action space
- 🏗️ **Builder Pattern Support** - Clean API for creating goals and actions

## Architecture
The framework consists of several core components:

- **GoapAgent**: Base class for all GOAP-powered entities
- **GoapPlanner**: Plans the optimal sequence of actions to achieve goals
- **AgentAction**: Actions that agents can perform
- **Goal**: Desired world states agents try to achieve
- **Belief**: Agent's understanding of the world state
- **Sensor**: Components that gather information from the environment

## Getting Started
1. Clone the repository
2. Open the project in Unity
3. Explore the examples in the AI folder

## Usage Example

```csharp
// Creating a new GOAP agent
public class ZombieAgent : GoapAgent
{
    protected override void SetupBeliefs()
    {
        base.SetupBeliefs();
        
        // Add beliefs about the world
        BeliefFactory.Create(BeliefName.IsHungry, true);
        BeliefFactory.Create(BeliefName.TargetVisible, false);
    }
    
    protected override void SetupActions()
    {
        base.SetupActions();
        
        // Add available actions
        Actions.Add(new SearchForFood(this));
        Actions.Add(new AttackTarget(this));
        Actions.Add(new Wander(this));
    }
    
    protected override void SetupGoals()
    {
        base.SetupGoals();
        
        // Add goals with priorities
        Goals.Add(new Goal.Builder(GoalName.FindFood)
            .WithPriority(5)
            .WithDesiredEffect(Beliefs[BeliefName.HasEaten])
            .Build());
            
        Goals.Add(new Goal.Builder(GoalName.Attack)
            .WithPriority(8)
            .WithDesiredEffect(Beliefs[BeliefName.KilledTarget])
            .Build());
    }
}
```

## Performance Considerations
This framework is built with performance in mind:

- Object pooling for all collections and temporary objects
- Efficient A* implementation for planning
- Optimized memory usage patterns
- Reuse of data structures when possible

## Extension Points
The framework is designed to be extended:

- Create custom agent types by inheriting from GoapAgent
- Implement new actions by extending AgentAction
- Add custom sensors to perceive the environment
- Define domain-specific beliefs and goals
