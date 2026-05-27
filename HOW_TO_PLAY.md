# How to Play (Dev Build)

> Updated after Step A. Pull latest → open Unity → press **Play**. No clicking around the Inspector required.

## What works right now (v0.3 — D/E/F/G + bug fixes)

### Sir Spoonacci is now UPRIGHT
Handle = body. Bowl = head. Two black eyes on the bowl front. Procedural waddle while walking, hop-squash on landing, idle sway.

### Scene 1 — Island beach (`Assets/Scenes/SampleScene.unity`)
- Beach + ocean + ring of palm trees + coconuts
- **Salon Cucchiaio** — huge pink building with sign, front counter, gold trim, salon attendant NPC, 2 flag poles. Walk into the trigger zone → tooltip auto-pops → press **E** to cycle 12 skins
- **Pool deck** with cyan water + pink flamingo float
- **Tiki Bar** with bottles + bartender NPC
- **Deck chairs** with 4 sunbathers under umbrellas
- **6 walker NPCs** wandering + 2 security guards
- **VIOLATOR SYSTEM** — every ~7s a random NPC is flagged as a violator (red pulsing arrow above head + crime label e.g. "PEEING IN POOL")
  - When a violator is within 18m → bottom HUD shows ⚠ VIOLATOR DETECTED — Press F to BONK!
  - Press **F** → Spoonacci LUNGES at them, slams head, slow-mo punch lands, victim flies back with a **persistent red Spoon Mark** on their forehead
  - Chain bonks within 3s build a **COMBO** counter (top-left)

### Scene 2 — Cutlery Chamber (`Assets/Scenes/CutleryChamber.unity`)
- Dark cell + corridor extension beyond the bars (abyss bug fixed)
- The bars are now sealed (no escape gap) — corridor ends at a **locked door** with floating sign
- Cell Crew NPCs unchanged — walk close, press **E** for dialogue

### Scene 3 — Shark Fusion (`Assets/Scenes/SharkFusion.unity`)
- Big beach, deep ocean, 5 palms, beach shack
- Walk into the water → triggers fusion cutscene
- Confused Shark approaches, CHOMP, Spoonacci lodges in snout like a unicorn horn
- Control transfers to the **SPOON-SHARK** — WASD swim · Space dash · cruise the ocean

## Controls

| Key | On foot | Salon | As Spoon-Shark |
|---|---|---|---|
| `W A S D` | Walk | — | Swim |
| `Space` | Hop | — | Dash boost |
| `Right-mouse drag` | Orbit camera | — | Orbit camera |
| `E` | Interact | Cycle skin | — |
| `F` | **BONK** lock-on violator | — | — |

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
