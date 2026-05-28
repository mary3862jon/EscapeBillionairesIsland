using UnityEngine;
using UnityEngine.InputSystem;

namespace Spoonacci
{
    // A barred cell door. Walk close + E to unlock — slides open, inmates inside flip to Unlocked.
    public class PrisonCellDoor : MonoBehaviour
    {
        public PrisonInmate[] inmatesInside;
        public Transform doorVisual; // we move/rotate this on unlock
        public string cellLabel = "CELL";
        public bool unlocked;
        public Color barColor = new Color(0.35f, 0.35f, 0.4f);

        bool playerInside;
        GUIStyle promptStyle;

        void Awake()
        {
            BuildBars();
            var trig = gameObject.AddComponent<SphereCollider>();
            trig.isTrigger = true;
            trig.radius = 2.5f;
        }

        void BuildBars()
        {
            // BIG realistic prison cell door — full 3.6m wide × 4.5m tall frame
            // 8 thick steel bars + 4 horizontal crossbeams + massive lock box + dangling padlock + 3 hinges
            var doorGO = new GameObject("Door");
            doorGO.transform.SetParent(transform, false);
            doorVisual = doorGO.transform;

            var barMat   = MakeMat(barColor, 0.95f, 0.25f);
            var frameMat = MakeMat(new Color(0.16f, 0.16f, 0.18f), 0.7f, 0.25f);
            var ironMat  = MakeMat(new Color(0.10f, 0.10f, 0.13f), 0.9f, 0.2f);
            var rustMat  = MakeMat(new Color(0.45f, 0.20f, 0.10f), 0.4f, 0.4f);

            // wall-to-wall (cell front is 8m wide in CellChamberBootstrapper)
            const float W = 7.6f, H = 4.5f;

            // HEAVY outer frame
            var frameTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frameTop.transform.SetParent(doorGO.transform, false);
            frameTop.transform.localPosition = new Vector3(0f, H - 0.2f, 0f);
            frameTop.transform.localScale = new Vector3(W, 0.4f, 0.45f);
            frameTop.GetComponent<Renderer>().sharedMaterial = frameMat;

            var frameBot = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frameBot.transform.SetParent(doorGO.transform, false);
            frameBot.transform.localPosition = new Vector3(0f, 0.1f, 0f);
            frameBot.transform.localScale = new Vector3(W, 0.2f, 0.45f);
            frameBot.GetComponent<Renderer>().sharedMaterial = frameMat;

            for (int side = -1; side <= 1; side += 2)
            {
                var sideF = GameObject.CreatePrimitive(PrimitiveType.Cube);
                sideF.transform.SetParent(doorGO.transform, false);
                sideF.transform.localPosition = new Vector3(side * (W * 0.5f - 0.2f), H * 0.5f, 0f);
                sideF.transform.localScale = new Vector3(0.4f, H, 0.45f);
                sideF.GetComponent<Renderer>().sharedMaterial = frameMat;
            }

            // 14 thick steel bars (spread across full width since gate is now wall-to-wall)
            for (int i = 0; i < 14; i++)
            {
                float x = -(W * 0.5f - 0.55f) + i * ((W - 1.1f) / 13f);
                var bar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                bar.transform.SetParent(doorGO.transform, false);
                bar.transform.localPosition = new Vector3(x, H * 0.5f, 0f);
                bar.transform.localScale = new Vector3(0.16f, (H - 0.4f) * 0.5f, 0.16f);
                bar.GetComponent<Renderer>().sharedMaterial = barMat;
            }
            // 4 horizontal crossbeams
            for (int y = 0; y < 4; y++)
            {
                var cross = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cross.transform.SetParent(doorGO.transform, false);
                cross.transform.localPosition = new Vector3(0f, 0.6f + y * ((H - 1.2f) / 3f), 0f);
                cross.transform.localScale = new Vector3(W - 0.6f, 0.14f, 0.18f);
                cross.GetComponent<Renderer>().sharedMaterial = barMat;
            }

            // BIG iron lock box on the right
            float lockX = W * 0.5f - 0.5f;
            var lockBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lockBox.name = "Lock Box";
            lockBox.transform.SetParent(doorGO.transform, false);
            lockBox.transform.localPosition = new Vector3(lockX, H * 0.5f, 0.3f);
            lockBox.transform.localScale = new Vector3(0.6f, 0.85f, 0.35f);
            lockBox.GetComponent<Renderer>().sharedMaterial = ironMat;

            // keyhole (small dark cube on lock box face)
            var keyhole = GameObject.CreatePrimitive(PrimitiveType.Cube);
            keyhole.transform.SetParent(doorGO.transform, false);
            keyhole.transform.localPosition = new Vector3(lockX, H * 0.5f, 0.48f);
            keyhole.transform.localScale = new Vector3(0.12f, 0.18f, 0.04f);
            Destroy(keyhole.GetComponent<Collider>());
            keyhole.GetComponent<Renderer>().sharedMaterial = MakeMat(Color.black, 0f, 0.1f);

            // DANGLING PADLOCK from a small chain on the lock box
            var padlock = GameObject.CreatePrimitive(PrimitiveType.Cube);
            padlock.transform.SetParent(doorGO.transform, false);
            padlock.transform.localPosition = new Vector3(lockX + 0.4f, H * 0.5f - 0.2f, 0.3f);
            padlock.transform.localScale = new Vector3(0.35f, 0.45f, 0.18f);
            padlock.GetComponent<Renderer>().sharedMaterial = rustMat;
            // padlock shackle (the U-shaped bit on top)
            var shackle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            shackle.transform.SetParent(doorGO.transform, false);
            shackle.transform.localPosition = new Vector3(lockX + 0.4f, H * 0.5f + 0.12f, 0.3f);
            shackle.transform.localScale = new Vector3(0.25f, 0.05f, 0.25f);
            shackle.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            shackle.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.4f, 0.4f, 0.45f), 0.9f, 0.4f);

            // glowing red lock indicator
            var lockGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            lockGO.name = "Lock LED";
            lockGO.transform.SetParent(doorGO.transform, false);
            lockGO.transform.localPosition = new Vector3(lockX, H * 0.5f + 0.55f, 0.5f);
            lockGO.transform.localScale = Vector3.one * 0.22f;
            Destroy(lockGO.GetComponent<Collider>());
            var lockMat = new Material(ShaderCache.Lit) { color = new Color(0.8f, 0.15f, 0.15f) };
            lockMat.SetFloat("_Metallic", 0.3f); lockMat.SetFloat("_Smoothness", 0.5f);
            // toned-down emission (was 2.0 = sun-bright through the bars)
            lockMat.EnableKeyword("_EMISSION");
            lockMat.SetColor("_EmissionColor", new Color(0.3f, 0f, 0f));
            lockGO.GetComponent<Renderer>().sharedMaterial = lockMat;

            // 3 hinges on the left side
            for (int h = 0; h < 3; h++)
            {
                var hinge = GameObject.CreatePrimitive(PrimitiveType.Cube);
                hinge.transform.SetParent(doorGO.transform, false);
                hinge.transform.localPosition = new Vector3(-(W * 0.5f - 0.05f), 0.5f + h * (H - 1f) * 0.5f, 0.22f);
                hinge.transform.localScale = new Vector3(0.25f, 0.35f, 0.28f);
                hinge.GetComponent<Renderer>().sharedMaterial = ironMat;

                var bolt = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                bolt.transform.SetParent(doorGO.transform, false);
                bolt.transform.localPosition = new Vector3(-(W * 0.5f - 0.05f), 0.5f + h * (H - 1f) * 0.5f, 0.36f);
                bolt.transform.localScale = Vector3.one * 0.12f;
                Destroy(bolt.GetComponent<Collider>());
                bolt.GetComponent<Renderer>().sharedMaterial = barMat;
            }

            // "CELL X" plaque above the door
            var plaque = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plaque.transform.SetParent(doorGO.transform, false);
            plaque.transform.localPosition = new Vector3(0f, H + 0.25f, 0.05f);
            plaque.transform.localScale = new Vector3(1.2f, 0.4f, 0.05f);
            plaque.GetComponent<Renderer>().sharedMaterial = rustMat;
        }

        void Update()
        {
            if (unlocked || !playerInside) return;
            var kb = Keyboard.current;
            if (kb != null && kb.eKey.wasPressedThisFrame) Unlock();
        }

        void Unlock()
        {
            unlocked = true;
            SoundFx.Instance.CellClank();
            SparkleBurst.Spawn(transform.position + Vector3.up * 1.5f, new Color(0.4f, 1f, 0.5f));

            // swing door open via rotation around the LEFT side (hinge axis)
            if (doorVisual != null)
            {
                // pivot the door visual around its left edge for realistic swing
                doorVisual.localPosition = new Vector3(-1.35f, 0f, 0.15f);
                foreach (Transform child in doorVisual)
                    child.localPosition += new Vector3(1.35f, 0f, -0.15f);
                doorVisual.localRotation = Quaternion.Euler(0f, -95f, 0f);
            }

            if (inmatesInside != null)
                foreach (var i in inmatesInside)
                    if (i != null) i.Unlock();

            PrisonState.OnCellUnlocked();
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<ProceduralSpoonBuilder>() != null) playerInside = true;
        }
        void OnTriggerExit(Collider other)
        {
            if (other.GetComponent<ProceduralSpoonBuilder>() != null) playerInside = false;
        }

        void OnGUI()
        {
            if (unlocked || !playerInside) return;
            if (promptStyle == null)
                promptStyle = new GUIStyle(GUI.skin.label) { fontSize = UiScale.Font(26), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(1f, 0.85f, 0.3f) } };
            string txt = Loc.T("prompt.cell_unlock") + " " + cellLabel;
            float w = 600f;
            var sh = new GUIStyle(promptStyle); sh.normal.textColor = Color.black;
            GUI.Label(new Rect((Screen.width - w) * 0.5f + 2, Screen.height * 0.55f + 2, w, 36f), txt, sh);
            GUI.Label(new Rect((Screen.width - w) * 0.5f, Screen.height * 0.55f, w, 36f), txt, promptStyle);
        }

        static Material MakeMat(Color c, float metallic, float smoothness)
        {
            var sh = ShaderCache.Lit;
            var m = new Material(sh) { color = c };
            m.SetFloat("_Metallic", metallic);
            m.SetFloat("_Smoothness", smoothness);
            return m;
        }
    }
}
