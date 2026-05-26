# How to Play (Dev Build)

> Updated after Step A. Pull latest → open Unity → press **Play**. No clicking around the Inspector required.

## What works right now (Step A + B — v0.2)

### Scene 1 — Island beach (`Assets/Scenes/SampleScene.unity`)
- Spawn as **Sir Spoonacci** (procedural silver spoon) on a sandy beach
- **Salon Cucchiaio** (pink kiosk to your right) — walk in, press **E** to cycle 12 skins
- Palm trees scattered around for vibes

### Scene 2 — Cutlery Chamber (`Assets/Scenes/CutleryChamber.unity`)
- Dark stone cell with prison bars, hay pile, single torch
- Talk to the **Cell Crew** — Big Bjørn, Plastic Pete, Goldie, Tasting Tina, The Ladle — walk close, press **E** to advance dialogue
- Each has a 3-line intro. Tone-setting only for now; the tunnel-dig gameplay comes next milestone.

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
