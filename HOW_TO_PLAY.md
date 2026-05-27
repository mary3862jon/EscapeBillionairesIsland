# How to Play (Dev Build)

> v0.5 — eye-level camera, save system, police siren loop, NPC tumble-on-bonk, multi-stage cell dig with pickaxe, Salon "pay or threaten" negotiation, MissionManager-driven objectives chaining cell → island → salon → shark.

## Pull → re-import → Play

1. GitHub Desktop → **Fetch** → **Pull origin**
2. Switch to Unity → wait for re-import bar
3. Open a scene from `Assets/Scenes/` and hit **Play**:
   - `CutleryChamber.unity` — **start here** for the proper story (cell → escape → island)
   - `SampleScene.unity` — direct island sandbox (skip the cell)
   - `SharkFusion.unity` — direct shark mode (skip everything)

## Controls

| Key | Action |
|---|---|
| `W A S D` / arrows | Walk |
| `Space` | Hop |
| `F` | **Auto-Bonk** — lock onto nearest violator, lunge, slow-mo bonk |
| `Q` | **Manual Bonk** — swing in front, no lock-on |
| `E` | Interact (NPCs, Pickaxe, Loose Stone, Salon) |
| `P` | Toggle **Police Mode** — uniform + badge + flashing siren lights + **looped police siren audio** |
| `F5` | Manual save (auto-saves every 30s anyway) |
| `Right-mouse drag` | Orbit camera |
| `Mouse wheel` | Zoom in/out |

## Camera (FIXED — no more drone view)

Eye-level walking-sim feel — ~1.8m above ground, ~5.5m behind, slight downward pitch. You can still mouse-orbit. Wheel zooms 3-14m.

## Save System

- Auto-saves every **30 seconds** + on **scene transitions**
- Press **F5** anytime for manual save
- "Saved ✔" toast pops top-center for 2s
- Save file: `<persistentDataPath>/save.json` (Windows: `%appdata%\..\LocalLow\...\save.json`)
- All progress preserved: Troll Tokens, bonks, talked NPCs, mission completions, mode flags

## Story chain

### Act 2 — Cutlery Chamber

1. Dark cell with 5 spoons + hay pile + glowing **Loose Stone** (dim until ready)
2. **Talk to all 5 Cell Crew** (walk close + E for each)
3. **Find the Pickaxe** — sitting on the hay pile (E to grab; attaches to your side)
4. Once both done, the **Loose Stone glows orange and pulses**
5. Walk close + press **E** to dig (3 stages, ~1.2s each — bonk sound per stage)
6. Tunnel breaks → **auto-save** → loads `SampleScene` with you wearing **POLICE UNIFORM** (looped siren)

### Act 4 (slight reorder) — Island arrival in uniform

1. You spawn at the Salon door in the blue uniform with flashing siren
2. **NEW MISSION:** "Ditch the police uniform at Salon Cucchiaio"
3. Walk in → menu pops:
   - **[E] PAY 30 Troll Tokens** for a civilian skin (Coiffeur smiles)
   - **[Q] THREATEN** the Coiffeur (free skin, but Daze Birds + ouch sound — she fled)
4. Uniform shed → random fresh skin → siren stops → free roam unlocked
5. Remaining missions chain in: **Bonk 5 violators → 100 tokens → Bonk Magnus Tusk → Bust dealers in Police Mode → Find the Ocean**

### Act 3 — Ocean (final)

1. Walk into the ocean (south of beach) — triggers Shark Fusion cutscene
2. Spoon-Shark formed → roam ocean (WASD swim · Space dash)

## NPC reactions (new)

When you bonk anyone:
- 🔊 **Ouch sound** (procedural yelp)
- 🦅 **3 Daze Birds** circle their head for 4s (cartoon flap)
- 💥 **Knockback + tumble** — they fall over, rotate, then stand up after ~2s
- 🥄 **Permanent red Spoon Mark** on forehead

## Active missions (MissionManager — chained)

| ID | Title | Prereqs |
|---|---|---|
| cell.talk | Talk to all 5 Cell Crew | — |
| cell.pickaxe | Find a pickaxe (try the hay pile) | — |
| cell.dig | Dig the loose stone (E) | cell.talk + cell.pickaxe |
| island.salon | Ditch the police uniform at Salon | cell.dig |
| isle.bonk5 | Bonk 5 violators | — |
| isle.tokens100 | Earn 100 Troll Tokens | — |
| isle.bust | Police Mode + 10 bonks | — |
| isle.tusk | Bonk Magnus Tusk | — |
| shark.fuse | Fuse with the Shark | — |

Top-right panel shows only missions whose prerequisites are met.

## Known FX

- Slow-mo on every bonk landing (0.32s × 0.35 timescale)
- Combo counter top-left
- Salon skin change: sparkle SFX + tiny squash anim on spoon
- Police badge: gold disc on head + red/blue point lights flashing 14Hz
- Looped 2-tone siren (~2s loop) plays while Police Mode on

## If errors

Console screenshot → I patch in minutes.
