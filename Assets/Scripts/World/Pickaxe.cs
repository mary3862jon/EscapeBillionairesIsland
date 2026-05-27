using UnityEngine;
using UnityEngine.InputSystem;

namespace Spoonacci
{
    // Pickaxe pickup — sits on the hay pile in the cell. Walk close + E to pick up.
    // Once picked up, attaches to spoon's side and sets PickaxeState.Found = true.
    public class Pickaxe : MonoBehaviour
    {
        Transform handle;
        Transform head;
        bool playerInside;
        Transform attachedTo;

        void Awake()
        {
            BuildVisual();
            var trig = gameObject.AddComponent<SphereCollider>();
            trig.isTrigger = true;
            trig.radius = 1.2f;
        }

        void BuildVisual()
        {
            handle = new GameObject("Handle").transform;
            handle.SetParent(transform, false);
            var stick = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stick.transform.SetParent(handle, false);
            stick.transform.localPosition = new Vector3(0f, 0.2f, 0f);
            stick.transform.localScale = new Vector3(0.07f, 0.4f, 0.07f);
            stick.transform.localRotation = Quaternion.Euler(0f, 0f, 25f);
            Destroy(stick.GetComponent<Collider>());
            stick.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.5f, 0.35f, 0.18f), 0f, 0.3f);

            head = new GameObject("Head").transform;
            head.SetParent(handle, false);
            head.localPosition = new Vector3(0.15f, 0.55f, 0f);
            head.localRotation = Quaternion.Euler(0f, 0f, 25f);
            var iron = GameObject.CreatePrimitive(PrimitiveType.Cube);
            iron.transform.SetParent(head, false);
            iron.transform.localScale = new Vector3(0.4f, 0.12f, 0.12f);
            Destroy(iron.GetComponent<Collider>());
            iron.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.35f, 0.35f, 0.4f), 0.9f, 0.4f);
        }

        void OnTriggerEnter(Collider other)
        {
            if (attachedTo != null) return;
            if (other.GetComponent<ProceduralSpoonBuilder>() != null)
            {
                playerInside = true;
                attachedTo = other.transform; // remember for hover prompt
            }
        }
        void OnTriggerExit(Collider other)
        {
            if (other.GetComponent<ProceduralSpoonBuilder>() != null)
            {
                playerInside = false;
                if (attachedTo == other.transform) attachedTo = null;
            }
        }

        void Update()
        {
            if (!playerInside || PickaxeState.Found) return;
            var kb = Keyboard.current;
            if (kb != null && kb.eKey.wasPressedThisFrame)
                PickUp();
        }

        void PickUp()
        {
            PickaxeState.Found = true;
            SoundFx.Instance.Chime();
            // attach to spoon
            transform.SetParent(attachedTo, false);
            transform.localPosition = new Vector3(0.5f, 0.5f, 0.1f);
            transform.localRotation = Quaternion.Euler(0f, 90f, -45f);
            transform.localScale = Vector3.one * 0.8f;
            var col = GetComponent<Collider>();
            if (col != null) Destroy(col);
        }

        void OnGUI()
        {
            if (PickaxeState.Found || !playerInside) return;
            var s = new GUIStyle(GUI.skin.label) { fontSize = UiScale.Font(24), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(1f, 0.85f, 0.3f) } };
            float w = 500f;
            var sh = new GUIStyle(s); sh.normal.textColor = Color.black;
            string t = Loc.T("prompt.pickaxe");
            GUI.Label(new Rect((Screen.width - w) * 0.5f + 2, Screen.height * 0.6f + 2, w, 32f), t, sh);
            GUI.Label(new Rect((Screen.width - w) * 0.5f, Screen.height * 0.6f, w, 32f), t, s);
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
