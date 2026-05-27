using UnityEngine;
using UnityEngine.InputSystem;

namespace Spoonacci
{
    // Upright spoon inmate. Three states:
    //   - LockedInCell (until cell door is unlocked)
    //   - Unlocked (cell is open, not yet persuaded)
    //   - Persuaded (will dig)
    // Player interactions:
    //   - E when inside trigger + Unlocked → attempt persuade (70% comply, 30% refuse)
    //   - Q when inside trigger + Refused → bonk into compliance
    //   - E when Persuaded → flavor line
    public enum InmateState { LockedInCell, Unlocked, Refused, Persuaded }

    public class PrisonInmate : MonoBehaviour
    {
        public string inmateName = "Inmate Spoon";
        public Color color = new Color(0.8f, 0.8f, 0.85f);
        public float metallic = 0.95f;
        public float smoothness = 0.85f;
        public float bodyHeight = 0.9f;
        public float bodyRadius = 0.08f;
        public Vector3 bowlSize = new Vector3(0.42f, 0.16f, 0.55f);
        public string[] persuadeLines = new[]
        {
            "alright. lead the way.",
            "fine. but if we get caught, i blame you.",
            "freedom? finally.",
            "give me a pickaxe. i'm in.",
            "aye. for the spoons.",
        };
        public string[] refuseLines = new[]
        {
            "no thanks. i live here now.",
            "go away. i was sleeping.",
            "you're going to get us all killed.",
            "i HATE the outside. so much sun.",
            "hard pass, prince.",
        };
        public string[] postBonkLines = new[]
        {
            "ow. ow. okay. okay. i'll dig.",
            "you didn't have to do that. i'm in.",
            "FINE. fine. let's dig the stupid stone.",
        };

        public InmateState state = InmateState.LockedInCell;

        bool playerInside;
        Transform headTransform;
        Transform visualRoot;
        float lastInteract;
        SpoonMarkDecal mark;

        void Awake()
        {
            BuildVisual();
            var trig = gameObject.AddComponent<SphereCollider>();
            trig.isTrigger = true;
            trig.radius = 2.0f;

            // physical body (so player can't walk through)
            var col = gameObject.AddComponent<CapsuleCollider>();
            col.direction = 1;
            col.height = bodyHeight + bowlSize.y;
            col.radius = Mathf.Max(bodyRadius * 1.3f, 0.18f);
            col.center = new Vector3(0f, (bodyHeight + bowlSize.y) * 0.5f, 0f);
            col.isTrigger = false;
        }

        void BuildVisual()
        {
            var mat = MakeMat(color, metallic, smoothness);
            var vis = new GameObject("Visual");
            vis.transform.SetParent(transform, false);
            visualRoot = vis.transform;

            var handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            handle.transform.SetParent(vis.transform, false);
            handle.transform.localPosition = new Vector3(0f, bodyHeight * 0.5f, 0f);
            handle.transform.localScale = new Vector3(bodyRadius * 2f, bodyHeight * 0.5f, bodyRadius * 2f);
            Destroy(handle.GetComponent<Collider>());
            handle.GetComponent<Renderer>().sharedMaterial = mat;

            var neck = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            neck.transform.SetParent(vis.transform, false);
            neck.transform.localPosition = new Vector3(0f, bodyHeight, 0f);
            neck.transform.localScale = Vector3.one * (bodyRadius * 2.3f);
            Destroy(neck.GetComponent<Collider>());
            neck.GetComponent<Renderer>().sharedMaterial = mat;

            var bowl = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bowl.transform.SetParent(vis.transform, false);
            bowl.transform.localPosition = new Vector3(0f, bodyHeight + bowlSize.y * 0.55f, 0.02f);
            bowl.transform.localRotation = Quaternion.Euler(15f, 0f, 0f);
            bowl.transform.localScale = bowlSize;
            Destroy(bowl.GetComponent<Collider>());
            bowl.GetComponent<Renderer>().sharedMaterial = mat;
            headTransform = bowl.transform;

            // eyes
            for (int side = -1; side <= 1; side += 2)
            {
                var e = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                e.transform.SetParent(bowl.transform, false);
                e.transform.localPosition = new Vector3(side * 0.25f, 0.1f, 0.4f);
                e.transform.localScale = new Vector3(0.18f, 0.2f, 0.18f);
                Destroy(e.GetComponent<Collider>());
                e.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.02f, 0.02f, 0.02f), 0f, 0.1f);
            }
        }

        void Update()
        {
            // idle sway
            if (visualRoot != null)
                visualRoot.localRotation = Quaternion.Euler(0f, Mathf.Sin(Time.time * 0.8f + inmateName.GetHashCode() * 0.001f) * 4f, 0f);

            // hop happily if persuaded
            if (state == InmateState.Persuaded && visualRoot != null)
            {
                float hop = Mathf.Abs(Mathf.Sin(Time.time * 5f)) * 0.08f;
                visualRoot.localPosition = new Vector3(0f, hop, 0f);
            }
            else if (visualRoot != null)
            {
                visualRoot.localPosition = Vector3.zero;
            }

            if (!playerInside) return;
            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.eKey.wasPressedThisFrame && Time.time - lastInteract > 0.4f)
            {
                lastInteract = Time.time;
                if (state == InmateState.LockedInCell)
                    DialogueBox.Instance.Say(inmateName, "*muffled through bars* ...let me out first.");
                else if (state == InmateState.Unlocked)
                    AttemptPersuade();
                else if (state == InmateState.Refused)
                    DialogueBox.Instance.Say(inmateName, "i SAID NO. (try [Q] to bonk me into it.)");
                else if (state == InmateState.Persuaded)
                    DialogueBox.Instance.Say(inmateName, persuadeLines[Random.Range(0, persuadeLines.Length)]);
            }

            if (kb.qKey.wasPressedThisFrame && state == InmateState.Refused && Time.time - lastInteract > 0.4f)
            {
                lastInteract = Time.time;
                BonkIntoCompliance();
            }
        }

        void AttemptPersuade()
        {
            if (Random.value > 0.3f) // 70% comply
            {
                state = InmateState.Persuaded;
                DialogueBox.Instance.Say(inmateName, persuadeLines[Random.Range(0, persuadeLines.Length)]);
                SoundFx.Instance.Chime();
                PrisonState.OnInmatePersuaded();
            }
            else
            {
                state = InmateState.Refused;
                DialogueBox.Instance.Say(inmateName, refuseLines[Random.Range(0, refuseLines.Length)] + "  ([Q] bonk me)");
                SoundFx.Instance.Ouch();
            }
        }

        void BonkIntoCompliance()
        {
            state = InmateState.Persuaded;
            DialogueBox.Instance.Say(inmateName, postBonkLines[Random.Range(0, postBonkLines.Length)]);
            SoundFx.Instance.Bonk();
            if (headTransform != null)
            {
                BonkBurst.Spawn(headTransform.position);
                var go = new GameObject("DazeBirds");
                go.AddComponent<DazeBirds>().Attach(headTransform);
                if (mark == null)
                {
                    var mgo = new GameObject("SpoonMark");
                    mark = mgo.AddComponent<SpoonMarkDecal>();
                    mark.Attach(headTransform);
                }
            }
            PrisonState.OnInmatePersuaded();
        }

        public void Unlock()
        {
            if (state == InmateState.LockedInCell) state = InmateState.Unlocked;
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<ProceduralSpoonBuilder>() != null) playerInside = true;
        }
        void OnTriggerExit(Collider other)
        {
            if (other.GetComponent<ProceduralSpoonBuilder>() != null)
            {
                playerInside = false;
                DialogueBox.Instance.Hide();
            }
        }

        static Material MakeMat(Color c, float metallic, float smoothness)
        {
            var sh = Shader.Find("Universal Render Pipeline/Lit");
            var m = new Material(sh) { color = c };
            m.SetFloat("_Metallic", metallic);
            m.SetFloat("_Smoothness", smoothness);
            return m;
        }
    }

    public static class PrisonState
    {
        public static int Persuaded;
        public static int Total = 15;
        public static int Unlocked;
        public static int TotalCells = 5;
        public static System.Action OnPersuadedChanged;

        public static void OnInmatePersuaded() { Persuaded++; OnPersuadedChanged?.Invoke(); }
        public static void OnCellUnlocked() { Unlocked++; }
        public static void Reset() { Persuaded = 0; Unlocked = 0; }
    }
}
