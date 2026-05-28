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
            // realistic cell door — 6 thick vertical bars + 3 horizontal crossbeams + frame + hinges + barrel lock
            var doorGO = new GameObject("Door");
            doorGO.transform.SetParent(transform, false);
            doorVisual = doorGO.transform;

            var barMat   = MakeMat(barColor, 0.85f, 0.35f);
            var frameMat = MakeMat(new Color(0.22f, 0.22f, 0.25f), 0.6f, 0.3f);
            var ironMat  = MakeMat(new Color(0.15f, 0.15f, 0.18f), 0.85f, 0.25f);

            // outer frame (4 sides)
            var frameTop    = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frameTop.transform.SetParent(doorGO.transform, false);
            frameTop.transform.localPosition = new Vector3(0f, 2.85f, 0f);
            frameTop.transform.localScale = new Vector3(2.8f, 0.22f, 0.28f);
            frameTop.GetComponent<Renderer>().sharedMaterial = frameMat;

            var frameBot    = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frameBot.transform.SetParent(doorGO.transform, false);
            frameBot.transform.localPosition = new Vector3(0f, 0.05f, 0f);
            frameBot.transform.localScale = new Vector3(2.8f, 0.22f, 0.28f);
            frameBot.GetComponent<Renderer>().sharedMaterial = frameMat;

            for (int side = -1; side <= 1; side += 2)
            {
                var sideF = GameObject.CreatePrimitive(PrimitiveType.Cube);
                sideF.transform.SetParent(doorGO.transform, false);
                sideF.transform.localPosition = new Vector3(side * 1.35f, 1.45f, 0f);
                sideF.transform.localScale = new Vector3(0.22f, 2.8f, 0.28f);
                sideF.GetComponent<Renderer>().sharedMaterial = frameMat;
            }

            // 6 thick vertical bars
            for (int i = 0; i < 6; i++)
            {
                float x = -1.05f + i * 0.42f;
                var bar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                bar.transform.SetParent(doorGO.transform, false);
                bar.transform.localPosition = new Vector3(x, 1.45f, 0f);
                bar.transform.localScale = new Vector3(0.13f, 1.32f, 0.13f);
                bar.GetComponent<Renderer>().sharedMaterial = barMat;
            }
            // 3 horizontal crossbeams
            for (int y = 0; y < 3; y++)
            {
                var cross = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cross.transform.SetParent(doorGO.transform, false);
                cross.transform.localPosition = new Vector3(0f, 0.4f + y * 1f, 0f);
                cross.transform.localScale = new Vector3(2.5f, 0.1f, 0.12f);
                cross.GetComponent<Renderer>().sharedMaterial = barMat;
            }

            // big iron lock box on the right
            var lockBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lockBox.transform.SetParent(doorGO.transform, false);
            lockBox.transform.localPosition = new Vector3(1.0f, 1.45f, 0.2f);
            lockBox.transform.localScale = new Vector3(0.38f, 0.5f, 0.22f);
            lockBox.GetComponent<Renderer>().sharedMaterial = ironMat;

            // glowing red lock indicator
            var lockGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            lockGO.name = "Lock";
            lockGO.transform.SetParent(doorGO.transform, false);
            lockGO.transform.localPosition = new Vector3(1.0f, 1.45f, 0.34f);
            lockGO.transform.localScale = Vector3.one * 0.18f;
            Destroy(lockGO.GetComponent<Collider>());
            var lockMat = new Material(ShaderCache.Lit) { color = new Color(1f, 0.2f, 0.2f) };
            lockMat.SetFloat("_Metallic", 0.3f); lockMat.SetFloat("_Smoothness", 0.6f);
            lockMat.EnableKeyword("_EMISSION");
            lockMat.SetColor("_EmissionColor", new Color(1.5f, 0f, 0f));
            lockGO.GetComponent<Renderer>().sharedMaterial = lockMat;

            // hinges (left side)
            for (int h = 0; h < 2; h++)
            {
                var hinge = GameObject.CreatePrimitive(PrimitiveType.Cube);
                hinge.transform.SetParent(doorGO.transform, false);
                hinge.transform.localPosition = new Vector3(-1.35f, 0.4f + h * 2f, 0.15f);
                hinge.transform.localScale = new Vector3(0.18f, 0.25f, 0.2f);
                hinge.GetComponent<Renderer>().sharedMaterial = ironMat;
            }
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
