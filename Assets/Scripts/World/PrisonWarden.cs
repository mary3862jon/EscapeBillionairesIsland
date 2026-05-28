using UnityEngine;
using UnityEngine.InputSystem;

namespace Spoonacci
{
    // Human warden — tall civilian with peaked cap + uniform.
    // Patrols between waypoints. Has a view cone (60° / 12m).
    // If he SEES anyone digging → triggers alarm (flashing red lights + siren).
    // Player can:
    //   • Stay behind him (digging only resumes when he can't see the loose stone)
    //   • Q-bonk him (knocks out for 15s)
    //   • E-bribe him (costs 50 Troll Tokens, knocks out for 60s)
    public class PrisonWarden : MonoBehaviour
    {
        public Transform[] waypoints;
        public Transform watchTarget;        // the loose stone — what he's protecting
        public float patrolSpeed = 2.0f;
        public float turnSpeed = 180f;
        public float viewAngleDeg = 60f;
        public float viewDistance = 14f;
        public float bribeCost = 50;

        Transform head;
        Transform bodyVisual;
        int wpIdx;
        float knockedOutUntil;
        bool playerInTrigger;

        public bool KnockedOut => Time.time < knockedOutUntil;

        void Awake()
        {
            BuildVisual();
            var trig = gameObject.AddComponent<SphereCollider>();
            trig.isTrigger = true;
            trig.radius = 2.2f;
        }

        void BuildVisual()
        {
            var bodyGo = new GameObject("WardenBody");
            bodyGo.transform.SetParent(transform, false);
            bodyVisual = bodyGo.transform;

            var skin   = MakeMat(new Color(0.95f, 0.78f, 0.65f), 0f, 0.35f);
            var navy   = MakeMat(new Color(0.07f, 0.12f, 0.28f), 0.15f, 0.55f);
            var pants  = MakeMat(new Color(0.05f, 0.08f, 0.18f), 0.1f, 0.5f);
            var black  = MakeMat(new Color(0.02f, 0.02f, 0.02f), 0f, 0.1f);
            var gold   = MakeMat(new Color(0.95f, 0.78f, 0.2f),  1f, 0.9f);

            // boots
            for (int side = -1; side <= 1; side += 2)
            {
                var foot = GameObject.CreatePrimitive(PrimitiveType.Cube);
                foot.transform.SetParent(bodyVisual, false);
                foot.transform.localPosition = new Vector3(side * 0.15f, 0.1f, 0.06f);
                foot.transform.localScale = new Vector3(0.22f, 0.18f, 0.5f);
                Destroy(foot.GetComponent<Collider>());
                foot.GetComponent<Renderer>().sharedMaterial = black;
            }
            // legs (pants)
            var legs = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            legs.transform.SetParent(bodyVisual, false);
            legs.transform.localPosition = new Vector3(0f, 0.75f, 0f);
            legs.transform.localScale = new Vector3(0.45f, 0.65f, 0.45f);
            Destroy(legs.GetComponent<Collider>());
            legs.GetComponent<Renderer>().sharedMaterial = pants;
            // torso (navy uniform shirt)
            var torso = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            torso.transform.SetParent(bodyVisual, false);
            torso.transform.localPosition = new Vector3(0f, 1.7f, 0f);
            torso.transform.localScale = new Vector3(0.7f, 0.65f, 0.55f);
            Destroy(torso.GetComponent<Collider>());
            torso.GetComponent<Renderer>().sharedMaterial = navy;
            // brass buttons (3 down the front)
            for (int i = 0; i < 3; i++)
            {
                var btn = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                btn.transform.SetParent(bodyVisual, false);
                btn.transform.localPosition = new Vector3(0f, 1.95f - i * 0.18f, 0.3f);
                btn.transform.localScale = Vector3.one * 0.08f;
                Destroy(btn.GetComponent<Collider>());
                btn.GetComponent<Renderer>().sharedMaterial = gold;
            }
            // shoulders/epaulets (gold trim)
            for (int side = -1; side <= 1; side += 2)
            {
                var epaulet = GameObject.CreatePrimitive(PrimitiveType.Cube);
                epaulet.transform.SetParent(bodyVisual, false);
                epaulet.transform.localPosition = new Vector3(side * 0.32f, 2.05f, 0f);
                epaulet.transform.localScale = new Vector3(0.16f, 0.05f, 0.18f);
                Destroy(epaulet.GetComponent<Collider>());
                epaulet.GetComponent<Renderer>().sharedMaterial = gold;
            }
            // head
            var headGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            headGo.transform.SetParent(bodyVisual, false);
            headGo.transform.localPosition = new Vector3(0f, 2.35f, 0f);
            headGo.transform.localScale = new Vector3(0.4f, 0.45f, 0.4f);
            Destroy(headGo.GetComponent<Collider>());
            headGo.GetComponent<Renderer>().sharedMaterial = skin;
            head = headGo.transform;
            // peaked cap (cylinder + brim)
            var cap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cap.transform.SetParent(head, false);
            cap.transform.localPosition = new Vector3(0f, 0.32f, 0f);
            cap.transform.localScale = new Vector3(1.05f, 0.25f, 1.05f);
            Destroy(cap.GetComponent<Collider>());
            cap.GetComponent<Renderer>().sharedMaterial = navy;
            var brim = GameObject.CreatePrimitive(PrimitiveType.Cube);
            brim.transform.SetParent(head, false);
            brim.transform.localPosition = new Vector3(0f, 0.18f, 0.4f);
            brim.transform.localScale = new Vector3(1.3f, 0.08f, 0.8f);
            Destroy(brim.GetComponent<Collider>());
            brim.GetComponent<Renderer>().sharedMaterial = black;
            // cap badge (small gold disc)
            var badge = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            badge.transform.SetParent(head, false);
            badge.transform.localPosition = new Vector3(0f, 0.4f, 0.4f);
            badge.transform.localScale = new Vector3(0.25f, 0.25f, 0.08f);
            Destroy(badge.GetComponent<Collider>());
            badge.GetComponent<Renderer>().sharedMaterial = gold;
            // sunglasses
            var glasses = GameObject.CreatePrimitive(PrimitiveType.Cube);
            glasses.transform.SetParent(head, false);
            glasses.transform.localPosition = new Vector3(0f, 0.05f, 0.42f);
            glasses.transform.localScale = new Vector3(0.85f, 0.18f, 0.08f);
            Destroy(glasses.GetComponent<Collider>());
            glasses.GetComponent<Renderer>().sharedMaterial = black;
            // baton at right hip
            var baton = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            baton.transform.SetParent(bodyVisual, false);
            baton.transform.localPosition = new Vector3(0.42f, 1.25f, 0f);
            baton.transform.localScale = new Vector3(0.06f, 0.35f, 0.06f);
            baton.transform.localRotation = Quaternion.Euler(20f, 0f, 0f);
            Destroy(baton.GetComponent<Collider>());
            baton.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.25f, 0.18f, 0.12f), 0f, 0.4f);

            // name tag above head
            var labelGo = new GameObject("WardenLabel");
            labelGo.transform.SetParent(transform, false);
            labelGo.transform.localPosition = new Vector3(0f, 3.2f, 0f);
            var lbl = labelGo.AddComponent<WorldLabel>();
            lbl.text = "warden.name"; lbl.color = new Color(1f, 0.8f, 0.3f); lbl.fontSize = 22;

            // physics (kinematic so the spoon can't shove him around)
            var rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            var col = gameObject.AddComponent<CapsuleCollider>();
            col.direction = 1; col.height = 2.6f; col.radius = 0.4f; col.center = new Vector3(0f, 1.3f, 0f);
        }

        void Update()
        {
            if (KnockedOut) { ProcessInteract(); return; }

            // patrol
            if (waypoints != null && waypoints.Length > 0)
            {
                var wp = waypoints[wpIdx];
                if (wp != null)
                {
                    Vector3 to = wp.position - transform.position; to.y = 0f;
                    if (to.magnitude < 0.5f)
                    {
                        wpIdx = (wpIdx + 1) % waypoints.Length;
                    }
                    else
                    {
                        transform.position += to.normalized * patrolSpeed * Time.deltaTime;
                        Quaternion face = Quaternion.LookRotation(to.normalized, Vector3.up);
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, face, turnSpeed * Time.deltaTime);
                    }
                }
            }

            // view-cone detection toward the loose stone — if player is digging and warden faces it → ALARM
            if (watchTarget != null)
            {
                Vector3 to = watchTarget.position - transform.position; to.y = 0f;
                float dist = to.magnitude;
                if (dist < viewDistance)
                {
                    Vector3 fwd = transform.forward; fwd.y = 0f;
                    float ang = Vector3.Angle(fwd, to);
                    bool sees = ang < viewAngleDeg * 0.5f;
                    PrisonAlarm.SetWardenSeesDig(sees && PrisonAlarm.PlayerIsDigging);
                }
            }

            ProcessInteract();
        }

        void ProcessInteract()
        {
            if (!playerInTrigger) return;
            var kb = Keyboard.current;
            if (kb == null) return;
            if (kb.qKey.wasPressedThisFrame) BonkOut();
            else if (kb.eKey.wasPressedThisFrame && GameState.TrollTokens >= bribeCost) Bribe();
        }

        void BonkOut()
        {
            knockedOutUntil = Time.time + 15f;
            SoundFx.Instance.Bonk();
            CameraShake.Shake(0.9f);
            BonkBurst.Spawn(transform.position + Vector3.up * 2.4f);
            PrisonAlarm.OnWardenKnocked();
            // tip him over visually
            if (bodyVisual != null) bodyVisual.localRotation = Quaternion.Euler(0f, 0f, 90f);
            Invoke(nameof(StandBackUp), 15f);
        }

        void Bribe()
        {
            GameState.SpendTokens((int)bribeCost);
            knockedOutUntil = Time.time + 60f;
            SoundFx.Instance.Chime();
            // turn his back to the stone for 60s
            if (watchTarget != null && bodyVisual != null)
            {
                Vector3 away = transform.position - watchTarget.position; away.y = 0f;
                transform.rotation = Quaternion.LookRotation(away.normalized, Vector3.up);
            }
        }

        void StandBackUp() { if (bodyVisual != null) bodyVisual.localRotation = Quaternion.identity; }

        void OnTriggerEnter(Collider other) { if (other.GetComponent<ProceduralSpoonBuilder>() != null) playerInTrigger = true; }
        void OnTriggerExit(Collider other)  { if (other.GetComponent<ProceduralSpoonBuilder>() != null) playerInTrigger = false; }

        void OnGUI()
        {
            if (!playerInTrigger) return;
            var s = new GUIStyle(GUI.skin.label) { fontSize = UiScale.Font(22), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(1f, 0.85f, 0.3f) } };
            var sh = new GUIStyle(s); sh.normal.textColor = Color.black;
            string txt = KnockedOut ? Loc.T("warden.knocked") : Loc.Tf("warden.choose", (int)bribeCost, GameState.TrollTokens);
            float w = 700f;
            GUI.Label(new Rect((Screen.width - w) * 0.5f + 2, Screen.height * 0.62f + 2, w, 32f), txt, sh);
            GUI.Label(new Rect((Screen.width - w) * 0.5f, Screen.height * 0.62f, w, 32f), txt, s);
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
