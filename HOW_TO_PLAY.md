# How to Play (Dev Build)

> v0.4 — major polish, bug fixes, mission tracker, Police Mode, Daze Birds, Pool, Shady Alley, Magnus Tusk.

## How to play (every time)

1. **GitHub Desktop → Fetch → Pull**.
2. Switch to **Unity** — wait for the bottom-right re-import bar to finish.
3. Open one of the 3 scenes from `Assets/Scenes/` (double-click in Project panel):
   - `SampleScene.unity` — open island sandbox (the meat)
   - `CutleryChamber.unity` — Act 2 prison-break
   - `SharkFusion.unity` — Act 3 shark fusion
4. Hit **Play** (▶ at top-center). Everything spawns automatically — no Inspector setup.

## Controls

| Key | What it does |
|---|---|
| `W A S D` / arrows | Walk (camera-relative) |
| `Space` | Hop |
| `F` | **Auto-Bonk** — lock onto nearest red-arrow violator, lunge & bonk |
| `Q` | **Manual Bonk** — swing at whoever's in front (no lock-on) |
| `E` | Interact (NPCs / Salon / Loose Stone) |
| `P` | Toggle **Police Mode** (cop badge + flashing lights, drug dealers become auto-targets) |
| `Right-mouse drag` | Orbit camera |
| `Mouse wheel` | Zoom in/out |

## Scene 1 — Island Sandbox (`SampleScene.unity`)

You spawn as **Sir Spoonacci** (upright, eyes + brows + monocle + mouth) on a 200×200 beach.

**Highlights:**
- **Salon Cucchiaio** with front counter, gold trim, attendant Coiffeur spoon (bow on head), human receptionist, big neon sign, flag posts. Walk into the trigger → instant tooltip → press **E** to cycle 12 skins.
- **Skin Kiosk** (next to Salon) — 12 mini-spoons on shelves so you can see what's available.
- **Infinity Pool** with deck + swimmers + poolside loungers.
- **Tiki Bar** with counter, bottles, bartender, drunk guests.
- **5 Deck Chairs** with sunbathers + tropical umbrellas.
- **Billionaire Mansion** patio with **Magnus Tusk** (suit, sunglasses, gold tie, perma-violator "BEING A BILLIONAIRE").
- **Shady Alley** with flickering lamp + 3 Shady Characters (drug dealers — invisible in normal mode, become auto-violators in Police Mode).
- **18 random walkers + 4 security guards** scattered.
- **Violation Spawner** — every 5s a random NPC gets a pulsing red arrow + crime label ("PEEING IN POOL", "INSIDER TRADING", "BAD TIPPING" etc).
- **Top-right objective tracker** with **Troll Tokens** counter.

**Objectives:**
1. Bonk 5 violators (10 Troll Tokens each)
2. Visit Salon Cucchiaio
3. Bonk Magnus Tusk
4. Enter Police Mode (P)
5. Earn 100 Troll Tokens

**When you bonk someone:** persistent **red Spoon Mark** on forehead + **3 cartoon birds** circle their head for 4s (Daze Birds VFX) + procedural bonk sound + slow-mo + combo counter.

## Scene 2 — Cutlery Chamber (`CutleryChamber.unity`)

Dark torch-lit cell. Talk to all 5 Cell Crew (Big Bjørn = hardhat, Plastic Pete = toothpick, Goldie = crown, Tasting Tina = bow, The Ladle = monocle) by walking close + pressing E.

When all 5 talked, the **Loose Stone** in the corner starts glowing orange. Press **E** on it → 3-second dig animation → automatically loads **SharkFusion** scene.

Objective tracker shows progress.

## Scene 3 — Shark Fusion (`SharkFusion.unity`)

Beach with palms + tiki shack + beachgoers. **Walk into the ocean** (south) → dramatic shark approach → **CHOMP** → Spoonacci lodges in shark snout like a unicorn horn → **control transfers to the Spoon-Shark**.

Shark now has: huge dorsal fin (above water), teeth, white eyes with pupils, gill stripes, big tail wagging, white belly, BIG visibly shark.

**WASD** = swim, **Space** = dash boost. Cruise the ocean.

## If something errors

Paste the **Console** errors. Common ones (and fixes I already applied):
- Procedural shark looked like a featureless submarine → now has dorsal fin, teeth, eyes, fins, tail
- Cell spoons lay flat → now upright like player
- Font too small → all HUD/dialogue now 26-32pt with shadows
- Default camera too low → now angled higher with wider FOV
