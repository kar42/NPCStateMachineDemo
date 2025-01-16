# NPC State Machine Demo
| ![spearthrow](https://github.com/kar42/NPCStateMachineDemo/blob/main/DemoSamples/spearthrow.gif "spearthrow") | ![combat](https://github.com/kar42/NPCStateMachineDemo/blob/main/DemoSamples/combat.gif "combat") |
|---------------------------------------------------------------------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------|


## Overview
This repository contains a wave-based game system which demonstrates the capabilities of an adaptive and dynamic state machine configured to allow NPC agents to interact with the game scene and smoothly transition between animations and abilities to create an immersive game experience.

## Motivation
The integration of seemingly sophisticated Artificial Intelligence systems has become a cornerstone for enhancing player experience and enriching gameplay dynamics. This repo introduces a 2D platformer game with dynamic AI-driven enemy units (agents), weaving inspirations from Towerfall's intense, fast-paced wave-based gameplay, loaded with varying enemies alongside the unique weapon-throw-retrieval mechanic found in God of War. 

To achieve a challenging wave-based implementation, the game required diverse enemies with the ability to respond to player behavior meaningfully. In the research paper, **"Applying Goal-Driven Autonomy to a Team Shooter Game"**, it was concluded that, in a dynamic team shooter environment, agent performance and plan execution could be improved by allowing agents to self-select goals in realtime. The challenge then became to design a system which could define, organize and prioritize an expandable list of goals and animations desired for the autonomous enemy units. Furthermore, it was desirable for the system to be both generic and highly customizable to allow for a range of character behaviors and configurations to be possible with minimal code repetition. 

## Key Benefits
- **Modularity**: This system is designed so each character’s behavior is plugged in like LEGO pieces—swap or modify Goals, Tasks, and States without reinventing the AI or copy and pasting functionality.
- **Flexibility**: New moves, new animations, or new behaviors? Just create a new Task class and it instantly slots into the system.
- **Scalability**: Whether you have 5 enemies or 50, the architecture handles complex priority logic gracefully without code repetition.
- **Maintainability**: Separated levels of functionality mean less code conflict and easier debugging - our GoalManager focuses on high-level AI choice, while each Task and State handles the specifics.

## Key Features
- **Goal Driven Autonomy**: The implementation of a Goal, Task and State managed framework, assigned to every enemy agent in the game, is a simplistic system controlled by a GoalManager class. This class will determine agents' available goals and tasks, resulting in certain states of animation which can smoothly be interrupted or strung together.
- **Priority Based Task Selection**: For task selection within a goal, the framework will determine which task has the highest priority score. This implementation was done with the idea that different approaches to prioritizing tasks can be explored on a goal-by-goal basis, so by calculating this during goal evaluation allows characters to dynamically select the correct goal, task and state based on their immediate surroundings and status. This allows an agent to prioritize healing during a combat goal if their health has reached a dangerous level.
- **Perception System**: To allow for real time responses, the agents needed to be configured with a perception system to gather inputs about the game scene. This allows agents to effectively perform behavior such as patrolling and attacking, or to enable redefining targets for goal behavior based on aspects such as proximity.
- **Demeanor System**: To allow for varying agent interactions, a system was designed to allow each agent to be defined as Aggressive, Neutral, or Docile. This system allows characters to change their demeanor dynamically, which in turn affects the goals, tasks and states available. This also allowed for more complex mechanics to be integrated smoothly, like NPCs switching from neutral to aggressive stances when they are attacked, or switching a character who would normally flee on sight to instead be "mind controlled" and attack others (including allies).
- **Overriding Behavior Support**: A fast-paced game with engaging mechanics and responsive NPC agents often requires the agents to respond to external stimulation that would interrupt normal behavior. By building an overriding goal into the goal driven development, it allows characters to have a defined list of separately managed tasks which will always take priority, such as freezing mobility or death.
- **Interactable Unity Components**: By defining the configuration of the goal manager to be a default component on NPC game objects, aspects of the system are configurable from the IDE, highly debuggable, as well as capable of enabling or disabling additional components on the fly.
- **Resource Management**: By instantiating enemy agents with a pool of available tasks that can be activated and managed by the GoalManager, there is no delay when accessing or swapping between dependent game components. This being said, the system does also allow for dynamic addition or removal of goals and tasks to characters during runtime.

## Architecture Overview
Every agent in the game scene is instantiated with an instance of a `GoalManager` class. They will call on this manager during every frame update of the game scene, which will determine a specific agent's available Goals and Tasks, resulting in certain States of animation to be executed. The goal manager object is instantiated with an ordered list of goals, prioritized in last-in-first-out order, which the owning agent can perform if the goal is determined as available. The goal manager object also creates and manages a single pool of available tasks, which any of the agent's goals can call on to perform during their respective execution. 

The `Goal` class defines the core framework of the behavioral driven functionality. It is designed to be inherited and customized into specific goals with varying behavior, and it manages groups of tasks that work in conjunction to allow the NPC to carry out complicated behaviors. Goals contain high level information, such as where the target vector position of that goal exists in the 2D game scene, or which tasks it can carry out to accomplish said goal.

The `Task` class represents an individual instance of a task that is available to the goals - depending on their individually calculated priority scores. Tasks can be considered as smaller units of related behavior which can be triggered singly or on repeat to accomplish a goal, such as attacking, fleeing, dying or simply staying in a standing position. 

The `State` class represents the smallest unit in the framework, aligning to a specific animation which can be triggered for the enemy agent from their owning task. 

For example, certain goals may require agents to carry out the Move task, which utilizes an A* implementation to determine a path to a goal defined object, and then uses velocity in conjunction with triggered animations to move the characters across the screen or interact with a given goal position. This goal can be interrupted or replaced, causing the agent to stop moving or adjust their behavior. Most tasks are either standalone within their owning goal or calculate states to change animations based on a variety of environmental factors, such as their own ray-cast driven perception system or external factors outside the agent’s control, like gravity or player triggered effects.

## TLDR
In practice, the game AI flows in a nested or hierarchical manner:
- GoalManager picks which Goal is “active” (selected by availability and priority).
- That Goal picks which Task is “active” (based on calculated priority scores).
- That Task picks which internal animation State is active (based on customised logic configured for each instance of a task).

`GoalManager` keeps a linked list of Goal objects (myGoals) in order of increasing priority which is defined generically for characters in the game.

Each frame, `GoalManager.ProcessGoals()` will:
- Iterate from the end of the list to the front (highest to lowest priority).
- Call `IsGoalAvailable()` on each provided instance of a `Goal`.
- Pick the first one that’s “available” as `activeGoal`.
- Call `ProcessTasks()` on the selected goal. 
- Allow Tasks to continue their selection and sub-processing of states which result in triggering animations and abilities.

This effectively ensures only one Goal, Task and State is active at any given time - whichever is of highest priority and determined to be available.

## Architecture Diagrams and Examples
```
Class Overview Key Features
    
    class GoalManager {
        - activeGoal : Goal
        - myGoals : LinkedList<Goal>
        + ProcessGoals() : void
        + GetBestAvailableGoal() : Goal
    }
    
    class Goal {
        - isGoalAvailable : bool
        - goalTaskDictionary : Dictionary<TaskName, Task>
        + ProcessTasks() : void
        + IsGoalAvailable() : bool
    }
    
    class Task {
        - _states : List<State>
        - _priorityScore : int
        + PerformTask(goalPosition : Vector2) : void
        + DetermineState() : State
        + HandleStates(goalPosition : Vector2) : void
    }
    
    class State {
        - _stateName : StateName
        - _animatorStateID : int
        + IsAnimationInProgress() : bool
        + TriggerState() : void
        + EnableState() : void
        + DisableState() : void
    }
    
    %% Relationships
    GoalManager --> "1..*" Goal : manages
    Goal --> "1..*" Task : references
    Task --> "1..*" State : owns
```

Sequence Diagram (Class Interactions at Runtime)

This **sequence diagram** shows the **method calls** between `GoalManager`, an available `Goal`, the `Task` with the highest priority, and that `Task`’s current `State`.

```
sequenceDiagram
    participant GM as GoalManager
    participant G as Goal
    participant T as Task
    participant S as State
    
    GM->>GM: ProcessGoals()
    GM->>G: GetBestAvailableGoal() 
    alt Goal is different from activeGoal
        G-->>G: DisableGoalBehavior() (on old activeGoal)
    end
    
    GM->>G: activeGoal.ProcessTasks()
    G->>G: CalculateTaskPriorities()
    G->>T: Select highest-priority Task
    
    T->>T: DetermineState() 
    T->>S: Trigger/EnableState (animation)
    T->>T: PerformTask(goalPosition)
    
    Note over GM,G,T,S: Each frame, the same pattern repeats
```

Goal-Task-State Transition Example

Below is an example of how **a single `Goal`**, ChaseTarget, might transition between multiple `Task` objects. 
Each task instance involved in carrying out patrol behavior may trigger a specific `State` corresponding to a particular animation and behavior.

```
ChaseTarget Goal
    [*] --> Idle Task : Idle State - default
    Idle Task --> Move Task : Move State - if movement triggered due to enemy detection
    Move Task --> Attack Task : Attack State - if in range to attack
    Attack Task --> Move Task : Move State - if target moves away but is still detected
    Move Task --> Idle Task : Idle State - if no input or no target detected
    Attack Task --> Idle Task : Idle State - if target is defeated
    Idle Task --> Block Task : Defend State - if threat recognized
    Block Task --> Idle Task : Idle State - if threat passes or block ability timer runs out
```

## Demo
| Mindfog Ability triggering combat in allied enemy units |
| :----------------------------------------: |
| ![mindfog](https://github.com/kar42/NPCStateMachineDemo/blob/main/DemoSamples/mindfog.gif "mindfog") |


| Combat and ability system in simple enemy unit |
| :----------------------------------------: |
| ![combat](https://github.com/kar42/NPCStateMachineDemo/blob/main/DemoSamples/combat.gif "combat") |


| Same Combat and ability system in advanced enemy unit |
| :----------------------------------------: |
| ![combat](https://github.com/kar42/NPCStateMachineDemo/blob/main/DemoSamples/combat.gif "combat") |


| Triggering enemy unit to switch personality from neutral to aggressive |
| :----------------------------------------: |
| ![neutralEnemy](https://github.com/kar42/NPCStateMachineDemo/blob/main/DemoSamples/neutralEnemy.gif "neutralEnemy") |


## Keywords
`State Machine`, `Goal Driven Autonomy`, `Target Selection`, `Perception System`, `Ability System`, `Combat System`, `Pathfinding`
