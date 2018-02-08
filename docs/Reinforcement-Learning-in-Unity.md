# Reinforcement Learning in Unity

Reinforcement learning is an artificial intelligence technique that trains _agents_ to perform tasks by rewarding desirable behavior. During reinforcement learning, an agent explores its environment, observes the state of things, and, based on those observations, takes an action. If the action leads to a better state, the agent receives a positive reward. If it leads to a less desirable state, then the agent receives no reward or a negative reward (punishment). As the agent learns during training, it optimizes its decision making so that it receives the maximum reward over time.

ML Agents uses a reinforcement learning technique called [Proximal Policy Optimization (PPO)](https://blog.openai.com/openai-baselines-ppo/). PPO uses a neural network to approximate the ideal function that maps an agent's observations to the best action an agent can take in a given state. The ML Agents PPO algorithm is implemented in TensorFlow and runs in a separate Python process (communicating with the running Unity application over a socket). 

**Note:** if you aren't studying machine and reinforcement learning as a subject and just want to train agents to accomplish tasks, you can treat PPO training as a _black box_. There are a few training-related parameters to adjust inside Unity as well as on the Python training side, but you do not need in-depth knowledge of the algorithm itself to successfully create and train agents. Step-by-step procedures for running the training process are provided in the [Training section](1-Training-ML-Agents.md). 

## The Simulation and Training Process

Training and simulation proceed in steps orchestrated by the ML Agents [Academy](link) class. The Academy works with Agent and Brain objects in the scene to step through the simulation. When either the Academy has reached its maximum number of steps or all agents in the scene are _done_, one training episode is finished. 

During training, the external Python PPO process communicates with the Academy to run a series of episodes while it collects data and optimizes its neural network model. The type of Brain assigned to an agent determines whether it participates in training or not. The **External** brain commmunicates with the external process to train the TensorFlow model. When training is completed successfully, you can add the trained model file to your Unity project for use with an **Internal** brain.

The ML Agents [Academy](link) class orchestrates the agent simulation loop as follows:

1. Calls your Academy subclass's `AcademyReset()` function.
2. Calls the `AgentReset()` function for each agent in the scene.
3. Calls the  `CollectState()` function for each agent in the scene.
4. Uses each agent's Brain class to decide on the agent's next action. 
5. Calls your subclass's `AcademyStep()` function.
6. Calls the  `AgentStep()` function for each agent in the scene, passing in the action chosen by the agent's brain. (This function is not called if the agent is done.)
7. Calls the agent's `AgentOnDone()` function if the agent has reached its `Max Step` count or has otherwise marked itself as `done`. Optionally, you can set an agent to restart if it finishes before the end of an episode. In this case, the Academy calls the `AgentReset()` function.
8. When the Academy reaches its own `Max Step` count, it starts the next episode again by calling your Academy subclass's `AcademyReset()` function.

To create a training environment, extend the Academy and Agent classes to implement the above methods. The [Agent.CollectState()](link) and [Agent.AgentStep()](link) functions are required; the other methods are optional — whether you need to implement them or not depends on your specific scenario.
  
**Note:** The API used by the Python PPO training process to communicate with and control the Academy during training can be used for other purposes as well. For example, you could use the API to use Unity as the simulation engine for your own machine learning algorithms. See [External ML API](link) for more information.

## Organizing the Unity Scene

To train and use ML Agents in a Unity scene, the scene must contain a single [Academy](link) subclass along with as many [Brain](link) objects and [Agent](link) subclasses as you need. Any Brain instances in the scene must be attached to GameObjects that are children of the Academy in the Unity Scene Hierarchy. Agent instances should be attached to the GameObject representing that agent.

[Screenshot of scene hierarchy]

You must assign a brain to every agent, but you can share brains between multiple agents. Each agent will make its own observations and act independently, but will use the same decision-making logic and, for **Internal** brains, the same trained TensorFlow model. 

**Academy**

The [Academy](link) object orchestrates agents and their decision making processes. Only place a single Academy object in a scene. 

You must create a subclass of the Academy class (since the base class is abstract). When you create your Academy subclass, implement the following methods:

* `AcademyReset()` — Prepare the environment and agents for the next training episode. Use this function to place and initialize entities in the scene as necessary.
* `AcademyStep()` — Prepare the environment for the next simulation step. The base Academy class calls this function before calling any `AgentStep()` methods for the current step. You can use this function to update other objects in the scene before the agents take their actions. Note that the agents have already collected their observations and chosen an action before the Academy invokes this method.

  The base Academy classes also defines several important properties that you can set in the Unity Editor Inspector. For training, the most important of these properties is `Max Steps`, which determines how long each training episode lasts. Once the Academy's step counter reaches this value, it calls the `AcademyReset()` function to start the next episode. 
  
  See [Academy Topic](link) for a complete list of the Academy properties and their uses.  

**Brain** 
The Brain encapsulates the decision making process. Brain objects must be children of the Academy in the Unity scene hierarchy. Every Agent must be assigned a Brain, but you can use the same Brain with more than one Agent. 

Use the Brain class directly, rather than a subclass. Brain behavior is determined by the brain type. During training, set your agent's brain type to **External**. To use the trained model, import the model file into the Unity project and change the brain type to **Internal**. See [Brain topic](link) for details on using the different types of brains. You can extend the [CoreBrain class](link) to create different brain types if the four built-in types don't do what you need.

The Brain class has several important properties that you can set using the Inspector window. These properties must be appropriate for the agents using the brain. For example, the `State Size` property must match the length of the feature vector created by an agent exactly. See [Agent topic]() for information about creating agents and setting up a Brain instance correctly.

See [Brains](link) for a complete list of the Brain properties.

**Agent**
The Agent class represents an actor in the scene that collects observations and carries out actions. The Agent class is typically attached to the GameObject in the scene that otherwise represents the actor — for example, to a player object in a football game or a car object in a vehicle simulation. Every Agent must be assigned a Brain.  

To create an agent, extend the Agent class and implement the essential `CollectState()` and `AgentStep()` methods:

* `CollectState()` — Collects the agent's observation of its environment.
* `AgentStep()` — Carries out the action chosen by the agent's brain and assigns a reward to the current state.

Your implementations of these functions determine how the properties of the Brain assigned to this agent must be set.
 
You must also determine how an Agent finishes its task or times out. You can manually set an agent to done in your `AgentStep()` function when the agent has finished (or irrevocably failed) its task. You can also set the agent's `Max Steps` property to a positive value and the agent will consider itself done after it has taken that many steps. The Academy waits until all agents are done, or the Academy reaches its own `Max Steps` count before starting the next episode. 

See [Agents](link) for detailed information on creating agents.

## Environments

An _environment_ in ML Agents can be any scene built in Unity. The Unity scene provides the environment in which agents observe, act, and learn. How you set up the Unity scene to serve as a learning environment really depends on your goal. You may be trying to solve a specific reinforcement learning problem of limited scope, in which case you can use the same scene for both training and for testing trained agents. Or, you may be training agents to operate in a complex game or simulation. In this case, it might be more efficient and practical to create a purpose-built training scene.

Both training and testing (or normal game) scenes must contain an Academy object to control the agent decision making process. The Academy defines several properties that can be set differently for a training scene versus a regular scene. The Academy's **Configuration** properties control rendering and time scale. You can set the **Training Configuration** to minimize the time Unity spends rendering graphics in order to speed up training. You may need to adjust the other functional, Academy settings as well. For example, `Max Steps` should be as short as possible for training — just long enough for the agent to accomplish its task, with some extra time for "wandering" while it learns. In regular scenes, you often do not want the Academy to reset the scene at all; if so, `Max Steps` should be set to zero. 

When you create a training environment in Unity, you must set up the scene so that it can be controlled by the external training process. Considerations include:

* The training scene must start automatically when your Unity application is launched by the training process.
* The scene must include at least one **External** brain.
* The Academy must reset the scene to a valid starting point for each episode of training.
* A training episode must have a definite end — either using `Max Steps` or by each agent setting itslef to `done`.

## Types of Training Scenarios

With the Unity ML Agents SDK, you can create a variety of different kinds of training scenarios, depending on how you connect agents, brains, and rewards. For those new to training intelligent agents, here are a few examples that can serve as inspiration:

* **Single-Agent** — A single agent linked to a single brain. This is the traditional way of training an agent. An example is any single-player game, such as Chicken. [Video Link](https://www.youtube.com/watch?v=fiQsmdwEGT8&feature=youtu.be).

* **Simultaneous Single-Agent** — Multiple independent agents with independent reward functions linked to a single brain. This parallelized version of the traditional training scenario can speed-up and stabilize the training process. An example of this scenario is training a dozen robot-arms to each open a door simultaneously. [Video Link](https://www.youtube.com/watch?v=fq0JBaiCYNA).

* **Adversarial Self-Play** — Two interacting agents with inverse reward functions linked to a single brain. In two-player games, adversarial self-play can allow an agent to become increasingly more skilled, while always having the perfectly matched opponent: itself. This was the strategy employed when training AlphaGo, and more recently used by OpenAI to train a human-beating 1v1 Dota 2 agent.

* **Cooperative Multi-Agent** — Multiple interacting agents with a shared reward function linked to either a single or multiple different brains. In this scenario, all agents must work together to accomplish a task than couldn’t be done alone. Examples include environments where each agent only has access to partial information, which needs to be shared in order to accomplish the task or  collaboratively solve a puzzle. 

* **Competitive Multi-Agent** — Multiple interacting agents with inverse reward functions linked either to a single brain or to multiple, different brains. In this scenario, agents must compete with one another to either win a competition, or obtain some limited set of resources. All team sports fall into this scenario. 

* **Ecosystem** — Multiple interacting agents with independent reward functions linked to the same or different brains. This scenario can be thought of as creating a small world in which animals with different goals all interact, such as a savanna in which there might be zebras, elephants, and giraffes, or an autonomous driving simulation within an urban environment.


## Alternate Learning schemes

ML Agents supports the following alternate learning schemes:

* **Curriculum Learning** — It is often difficult for agents to learn a complex task at the beginning of the training process. Curriculum learning is the process of gradually increasing the difficulty of a task to allow more efficient learning. ML Agents supports setting custom environment parameters every time the environment is reset. This allows elements of the environment related to difficulty or complexity to be dynamically adjusted based on training progress. See [Curriculum Learning](link) for more information.

* **Imitation Learning** — It is often more intuitive to simply demonstrate the behavior you want an agent to perform, rather than attempting to have it learn via trial-and-error methods. Imitation learning provides the ability to record all state/action/reward information for use in supervised learning scenarios. You can provide demonstrations of how an agent should behave in an environment and then utilize those demonstrations to train an agent in either a standalone fashion, or as a first-step in a reinforcement learning process. Imitation Learning will be available in a future version of ML Agents.
