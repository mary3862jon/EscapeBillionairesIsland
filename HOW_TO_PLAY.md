# How to Play (Dev Build)

> Updated after Step A. Pull latest → open Unity → press **Play**. No clicking around the Inspector required.

## What works right now (Step A — v0.1)

- **Open island demo** loads when you Play `Assets/Scenes/SampleScene.unity`
- You spawn as **Sir Spoonacci** (procedural silver spoon) on a sandy beach
- **Salon Cucchiaio** (pink kiosk) is to your right — walk into it and press **E** to cycle through 12 skins

## Controls

| Key | Action |
|---|---|
| `W A S D` / arrows | Walk (camera-relative) |
| `Space` | Hop |
| `Right-mouse drag` | Rotate camera |
| `E` | Interact (Salon, NPCs later) |

## What to expect visually

Ugly. By design. Everything is procedurally built from cubes / spheres / cylinders — no art assets yet. Greybox phase. We replace primitives with real models once gameplay is fun.

## If nothing happens when you press Play

1. Console shows red errors? Screenshot it and send.
2. Spoon falls forever? Likely the ground didn't spawn — send Console output.
3. Nothing on screen at all? Make sure the loaded scene is `Assets/Scenes/SampleScene.unity` (double-click it in the Project panel).
