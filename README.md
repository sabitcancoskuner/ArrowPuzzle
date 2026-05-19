
# Arrow Puzzle Prototype 🧩

A 2D grid-based logic puzzle game developed in Unity. The objective of the game is to resolve interlocking directional paths on a grid. 

This project was built with a strong focus on **clean architecture**, **mobile-ready rendering optimizations**, and **custom developer tooling**.

## 🔄 Project Status & Updates
This game is under active development. I believe in iterative design, so this README serves as a living document. I will be updating this page continuously to reflect new features, architectural decisions, and visual polish as the project evolves.

### 1. Custom Level Editor
To rapidly design puzzles, I built a custom Unity Editor Window (`LevelBlueprintEditor`). 
* **Continuous Painting:** Intercepts mouse events (`Event.current`) to allow buttery-smooth click-and-drag path drawing directly on the grid.
* **Smart Pathfinding:** Automatically calculates the orientation of Arrow Heads, Bodies, and Elbow joints based on the user's mouse drag trajectory.
* **Safe Data Management:** Utilizes a "Working Buffer" architecture via deep-copying. Changes are visualized instantly but only serialized to `ScriptableObject` assets when explicitly saved, preventing accidental data loss during design.
<br>
<img width="1920" height="1080" alt="editor" src="https://github.com/user-attachments/assets/c53fbca5-87ef-4123-a31f-f1bfdacc61f4" />
