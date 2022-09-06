# DDA Boss Fights

### **Introduction**

This is a MSc project of Computer Games in Queen Mary, University of London. The topic is **Improving Boss Fight Experience in 2D Action  Games by Applying Dynamic Difficulty Adjustment *(DDA) to Behaviour Tree (BT)**. This project is based on Unity Editor 2022.1.11f1.
WebGL Build: https://nofacegoose.github.io/DDA-Boss-Fight/Build/index.html



### System Design

Based on the design of BT, the DDA system targets at **narrowing the difference** between the player and the boss **health**. It focuses on the **action choice** (or **behaviour pattern**) of the boss. The selection is based on the **fitness** of each action with three action selection strategies are involved: choosing the **fittest** action, **Roulette** Selection and **Rank** Selection. The system is **lightweight** and can **quickly start** to adjust as it needs each action to be executed only **once**.

Additionally, a small **DDA waiting** system is designed to dynamically adjust the intervals between actions according to the current **health difference** between the player and the boss.

The system's core code is in **Boss.cs** and **ActionData.cs**.



### Boss Options

There are four bosses designed for comparison in the demo.

The main menu includes:

1. **Challenging Order**: update only when the player beats or is defeated by the boss.

2. **Win/Loss Ratio**: update only when the player beats or is defeated by the boss.

3. **Bosses**: the name of the boss, update only when the player encounter the boss fight.

4. **Average HP Difference**: the average Hit Point(HP) difference during the boss fight throughout all attempts. Update only when update only when the player beats or is defeated by the boss.

   

### Instruction

1. Click any boss's name to get started.

2. Pass the tutorial.

3. Fight against the boss.

4. Can skip the tutorial after challenging one boss.

5. Choose another boss fight to play.

6. Pause at any time by access the menu (press M) and quit the current attempt.

   

### Third Party Libraries and Assets

1. **NPBehave**: Event driven Behavior Tree Library. [meniku/NPBehave: Event Driven Behavior Trees for Unity 3D (github.com)](https://github.com/meniku/NPBehave)

2. **Sprites**:  All sprites used in this project.

3. **Animations**: Animations used in the boss, player and effects. Except Fire, Throw Potion and Summon animation of boss.

4. Function **LookAtPlayer** in **Boss.cs**: Turning the boss to face the player.

5. **Player.cs** and **PlayerMovement.cs**: Logic of player controls, including moving,  flipping and jumping.

   2-5 from [Brackeys/Boss-Battle: Project files for our tutorial on how to create a boss battle using state machines in Unity. (github.com)](https://github.com/Brackeys/Boss-Battle)

6. **Tiles**: Ground and wall tiles. 

7. **RuleTile.cs** and **RuleTileEditor.cs**: The tile rules and editor.

   6-7 from **[Strata Easy 2D Level Generator For Unity by mattmirrorfish (itch.io)](https://mattmirrorfish.itch.io/strata-easy-2d-level-generator-for-unity)** (The old free version)

	8. Background image is from [NoFaceGoose/Tales-of-Syrinx (github.com)](https://github.com/NoFaceGoose/Tales-of-Syrinx)
	8. Audios are all from free assets in Unity Asset Store
