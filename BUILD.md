# How to Build & Ship (Windows)

## Build steps (Unity)

1. **File → Build Profiles** (or `Ctrl+Shift+B`)
2. **Scenes In Build** — confirm all 4 are listed and checked, in this order:
   - `Assets/Scenes/TitleScreen.unity`   *(must be index 0)*
   - `Assets/Scenes/SampleScene.unity`
   - `Assets/Scenes/CutleryChamber.unity`
   - `Assets/Scenes/SharkFusion.unity`
   - If any missing → drag them in from the Project panel, then drag TitleScreen to the top.
3. **Platform**: Windows · **Target Platform**: Windows · **Architecture**: x86_64
4. Click **Build** → pick a fresh empty folder (e.g., `E:\Builds\Spoonacci_v0.6\`)
5. Wait 1-3 min. Unity outputs:
   - `EscapeBillionairesIsland.exe`
   - `EscapeBillionairesIsland_Data/`
   - `UnityPlayer.dll`
   - `MonoBleedingEdge/`

## Test the build LOCALLY first

Open the .exe before sending. The Title Screen should appear → **NEW GAME** should load Cutlery Chamber.

If the title is stuck (buttons do nothing):
- Open the **Player.log** at `%appdata%\..\LocalLow\<CompanyName>\<ProductName>\Player.log`
- Look for `[Title] StartNew failed:` lines — tells you exactly what's wrong
- Most common cause: scene not in Build Settings (re-do step 2)

## Ship to a friend

1. Right-click the build folder → **Compress to ZIP file**. Will be ~250-450 MB.
2. Upload to **WeTransfer** (https://wetransfer.com — free up to 2 GB) or **Google Drive**.
3. Send the download link.

## Friend's side

1. Download → unzip the whole folder somewhere
2. Double-click `EscapeBillionairesIsland.exe`
3. If Windows says **"Windows protected your PC"** (SmartScreen, unsigned exe):
   - Click **More info** → **Run anyway**
   - Normal — happens with any unsigned exe
4. Requires Windows 10/11 64-bit. Mac/Linux need separate builds.

## Music controls in-game (NEW)

Looping background music plays from title screen onward (4 procedural tracks).

| Key | Action |
|---|---|
| `K` | Next track |
| `J` | Previous track |
| `M` | Mute toggle |
| `=` / `+` | Volume up |
| `-` | Volume down |

Track list: **Tropical Lounge**, **Heist Synth**, **Spoonacci Theme**, **Night Drive**.
