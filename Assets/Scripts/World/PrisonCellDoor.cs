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
            // door visual: 5 vertical bars
            var doorGO = new GameObject("Door");
            doorGO.transform.SetParent(transform, false);
            doorVisual = doorGO.transform;

            var mat = MakeMat(barColor, 0.85f, 0.4f);
            for (int i = -2; i <= 2; i++)
            {
                var bar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                bar.transform.SetParent(doorGO.transform, false);
                bar.transform.localPosition = new Vector3(i * 0.45f, 1.4f, 0f);
                bar.transform.localScale = new Vector3(0.1f, 1.4f, 0.1f);
                bar.GetComponent<Renderer>().sharedMaterial = mat;
            }
            // top/bottom crossbeams
            for (int y = 0; y < 2; y++)
            {
                var cross = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cross.transform.SetParent(doorGO.transform, false);
                cross.transform.localPosition = new Vector3(0f, y == 0 ? 0.05f : 2.75f, 0f);
                cross.transform.localScale = new Vector3(2.4f, 0.12f, 0.14f);
                cross.GetComponent<Renderer>().sharedMaterial = mat;
            }

            // lock indicator (red sphere)
            var lockGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            lockGO.name = "Lock";
            lockGO.transform.SetParent(doorGO.transform, false);
            lockGO.transform.localPosition = new Vector3(0f, 1.5f, 0.15f);
            lockGO.transform.localScale = Vector3.one * 0.25f;
            Destroy(lockGO.GetComponent<Collider>());
            var lockMat = new Material(ShaderCache.Lit) { color = new Color(1f, 0.2f, 0.2f) };
            lockMat.SetFloat("_Metallic", 0.3f); lockMat.SetFloat("_Smoothness", 0.6f);
            lockMat.EnableKeyword("_EMISSION");
            lockMat.SetColor("_EmissionColor", new Color(0.8f, 0f, 0f));
            lockGO.GetComponent<Renderer>().sharedMaterial = lockMat;
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
            SoundFx.Instance.Chime();
            SparkleBurst.Spawn(transform.position + Vector3.up * 1.5f, new Color(0.4f, 1f, 0.5f));

            // swing door open via rotation
            if (doorVisual != null) doorVisual.localRotation = Quaternion.Euler(0f, -85f, 0f);

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
