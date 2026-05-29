# REQUESTS.md — Boba's standing requests & running checklist

> Single source of truth for everything Boba has asked for. I (Claude) update this
> EVERY session: add new requests the moment they're made, tick them only after the
> change is in code AND verified to compile. Boba can read this anytime to see what's
> done and what's outstanding. Nothing gets "remembered" in my head — it lives here.

## ✅ Done & verified
- [x] HUD redesign — rounded translucent gold-bordered cards, soft shadow, text shadows (no more flat black boxes with loud yellow text). `UiTheme.cs`, `QuestBanner`, `ObjectiveTracker`, `WorldLabel`.
- [x] Fix HUD freeze — panels drawn via `GUI.Box`, not `GUIStyle.Draw` (was flooding the console every frame).
- [x] Can't walk off the map — invisible soft wall on the player in EVERY playable scene: island (square), prison/Cutlery chamber (rectangular 50x30), shark scene (sides only, ocean open). `FallGuard` boundsXZ/boundsX/boundsZ. First pass only covered the island — prison was still escapable.
- [x] SEXY BIKINI WOMEN everywhere a beach/pool/spa crowd belongs — real feminine figures (hourglass, bikini, hair), not recoloured clothed men. `BikiniWoman.cs` + shared `BikiniWoman.Spawn()` factory, wired into: pool party dancers, beach sunbathers & volleyball, central hub deck chairs, pool swimmers/loungers, main infinity-pool loungers, poolside cocktail bar, tiki-bar guests, salon receptionist. (Bartenders/guards/yacht-staff stay clothed.)
- [x] Two HUD layout bugs from the audit: token-strip shadow smear + quest-banner overlap on narrow windows.

- [x] Scroll-to-zoom fixed — `ThirdPersonCamera` required scroll magnitude >5, but the new Input System reports ±1 per notch on many systems, so it never fired. Now triggers on any scroll (sign only).
- [x] Title/main menu redesigned — themed gold-bordered buttons with hover (no more default grey boxes); Continue dims when there's no save.
- [x] Music MUTE BUTTON added — on-screen toggle on the title screen (top-right) AND in-game (top-left corner). `[M]` key still works too. `MusicPlayer.ToggleMute()`.

## 🔜 Outstanding / next — BOSS FIGHTS (4, one-by-one, each its own commit)
Spec: each billionaire = a 3-phase "clout-bar" humiliation fight (Minions → signature Gimmick → Humiliation finisher) in a detailed arena. Rules locked with Boba:
- **Asymmetric placement** — scatter the bosses, NOT the 4 tidy ±80 corners.
- **AAA detail bar** — multi-part procedural meshes + real textures/metallic/emission (new `BossTextures`). NO single-cube "3rd-grader" props. Drones = rotors+gimbal+LEDs; rockets = stages+bell+plume.
- **Police-mode rule** — in police mode the bosses go passive EXCEPT **Magnus Tusk = God-Boss** who fights even in police mode. Normal mode = all bosses fight.
- [x] **Boss 1 — Magnus Tusk (God-Boss, rocket pad)** — DONE & compile-verified. Reusable `BossFight` framework (3-phase clout fight), detailed `BossDrone` (rotors/gimbal/LEDs), `BossCloutBar` HUD, new `BossTextures` (brushed-metal/carbon/concrete/rust/hazard/tarmac), `MagnusArenaBuilder` (gantry/tanks/floodlights/tarmac), `MagnusTuskBoss` (launches multi-stage rockets, fights even in police mode). Anchor moved to asymmetric (50,30); quest `isle.boss.tusk` added.
- [x] Boss 2 — Beff Jezos (mega-yacht + champagne-tower topple; butler drones). Anchor → (-54,-38). compile-verified.
- [x] Boss 3 — Mark Zuckersnort (AI datacenter; selfie-drone swarm + holo shockwaves). Anchor → (-66,22). compile-verified.
- [x] Boss 4 — Crypto Chad (neon vault/casino; pump-and-dump giant coins + crash wave). Anchor → (20,-62). compile-verified.
- All 3 reuse the BossFight framework, isGodBoss=false (passive in police mode), each completes its isle.* + isle.boss.* quest on defeat.
- [ ] (add new requests here the instant Boba makes them)

## 📌 Standing rules for the game
- 100% code-driven — Claude writes ALL C#/scene/prefab/settings. Boba only pulls via GitHub Desktop + hits Play. Never tell him to click/drag in the Unity Inspector.
- Verify before claiming done — never push a build that hasn't been compile-reviewed.
- Bikini women, not clothed men, anywhere a beach/pool/party crowd is implied.
