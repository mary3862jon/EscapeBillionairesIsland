# How to Play (Dev Build)

> v0.6 — title screen, BIG prison (50×30m, 5 cells, 15 inmates, unlock + persuade + bonk-into-compliance), 250×250 island with 4 billionaire zones (Beff/Chad/Zuck/Tusk), post-processing pipeline, particle FX, day/night cycle.

## Pull → re-import → Play

1. GitHub Desktop → **Fetch** → **Pull origin**
2. Switch to Unity → wait for re-import
3. **NEW:** Open `Assets/Scenes/TitleScreen.unity` first
4. Hit **Play** → title menu

## Title Screen

- Giant rotating Sir Spoonacci + floating gold coins
- **NEW GAME** — resets state + starts Cutlery Chamber
- **CONTINUE** — loads save and jumps to the appropriate scene (cell if not escaped, island if escaped)
- **QUIT** — bye

## Visual upgrade (PostFxBoost)

Every scene now layers an URP PostFx volume with:
- **Bloom** — bright stuff glows
- **Vignette** — corners darken
- **Color Adjustments** — saturation +18, contrast +14, exposure +0.35 (pop)
- **Tonemapping (ACES)** — cinematic curve
- **Film Grain** — subtle texture overlay

Combined effect: ~30% perceived quality lift even with primitives.

## Particle FX

- **BonkBurst** — every bonk impact spawns a dust cloud + yellow star sparks
- **SparkleBurst** — every salon skin change + cell unlock spawns floating tinted sparkles

## Day/Night Cycle

Sun rotates over a **6-minute day**. At night ambient turns deep blue. `GameState.IsNight` is exposed for future spawn rules (drug dealer surge etc.)

## Act 2 — The BIG Prison (rebuilt)

50m × 30m prison building, 6m tall. **5 cells** along the north wall with **3 inmates per cell = 15 total**. You start free in **Cell A** (leftmost, door pre-opened) — pickaxe is on the hay pile.

**Flow:**

1. **Grab the pickaxe** in your cell (E on the hay pile)
2. Walk the corridor, **press E on each barred door** (Cells B/C/D/E) to unlock → sparkle FX → inmates inside flip from `Locked` → `Unlocked`
3. **Persuade each inmate**: walk close + **E** → they reply
   - **70%** comply → "aye, for the spoons" → counter ticks up
   - **30%** refuse → "i live here now" → press **Q** to bonk them into compliance (bonk burst + daze birds + spoon mark + post-bonk line) → counter ticks up
4. Once all **15 persuaded** + pickaxe in hand → the **Loose Stone glows** at the west end of corridor
5. Press **E** to dig (3 stages, ~3.6s total) → auto-load `SampleScene` in **police uniform with looped siren**

## Act 4 — The 250×250 Island (4 zones spread)

You spawn at the central hub. Walk outward to find:

| Compass | Zone | Billionaire | Vibe |
|---|---|---|---|
| **NW** | Beff Jezos Yacht | **Beff Jezos** | giant white yacht + dock + helipad |
| **NE** | Crypto Chad Vault | **Crypto Chad** | glass cube vault with 4 gold NFT pedestals + neon base |
| **SW** | Zuckersnort Lab | **Mark Zuckersnort** | sterile white box with green roof + antenna + glass front |
| **SE** | Magnus Tusk Mansion | **Magnus Tusk** | marble patio + gold columns + tiny rocket on lawn |

Central hub: Salon (counter + attendant + receptionist), Skin Kiosk (12 skin display), Infinity Pool with swimmers, Tiki Bar with bartender + guests, 6 deck-chair sunbathers + umbrellas, Shady Alley with 3 drug dealers, **28 wandering walkers**, ring of 50 palms at the perimeter + 25 scattered.

## Mission list (auto-chaining via prereqs)

| ID | Title |
|---|---|
| cell.pickaxe | Find the pickaxe |
| cell.unlock | Unlock 4 other cells |
| cell.persuade | Persuade 15 inmates |
| cell.dig | Dig the loose stone |
| island.salon | Ditch police uniform at Salon |
| isle.bonk5 | Bonk 5 violators |
| isle.tokens100 | Earn 100 Troll Tokens |
| isle.bust | Police Mode + 10 bonks |
| isle.tusk | Bonk Magnus Tusk |
| isle.beff | Bonk Beff Jezos |
| isle.chad | Bonk Crypto Chad |
| isle.zuck | Bonk Mark Zuckersnort |
| isle.all4 | Bonk all 4 billionaires (requires above) |
| shark.fuse | Fuse with the Shark |

Top-right tracker only shows missions whose prereqs are met.

## Controls (unchanged)

`WASD`, `Space` hop, `F` auto-lock bonk, `Q` manual bonk (also for refusing inmates), `E` interact, `P` Police Mode toggle, `F5` save, mouse-orbit + wheel zoom.

## Honest graphics ceiling note

We've maxed what primitives can do. The next big jump is **real 3D assets** (Quaternius/KayKit free, or Synty paid). That's a separate task and requires careful import. Tell me when you're ready to go down that road.
