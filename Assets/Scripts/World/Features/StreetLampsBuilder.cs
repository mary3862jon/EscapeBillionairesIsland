using UnityEngine;
using System.Collections.Generic;
namespace Spoonacci
{
    // Warm sidewalk street lamps lining the island's walkways.
    // Tall dark poles with a curved arm and a glowing amber lamp head;
    // roughly half carry a real point Light for nighttime pools of warmth.
    public static class StreetLampsBuilder
    {
        private static readonly Color PoleCol = new Color(0.16f, 0.17f, 0.20f);
        private static readonly Color BaseCol = new Color(0.11f, 0.11f, 0.13f);
        private static readonly Color WarmGlass = new Color(1f, 0.86f, 0.55f);
        private static readonly Color WarmLight = new Color(1f, 0.82f, 0.52f);

        public static void Build(Transform root)
        {
            var hub = BuildKit.Root("StreetLamps", Vector3.zero).transform;
            hub.SetParent(root, true);

            // Sidewalk slots already sit off-road, just beside the carriageway.
            List<WorldLayout.Slot> slots = WorldLayout.SidewalkSlots(11f, 6.0f);

            int placed = 0;
            int target = 18;
            int lightCount = 0;

            for (int i = 0; i < slots.Count && placed < target; i++)
            {
                // spread across the available slots so lamps aren't all clustered
                var slot = slots[i];
                Vector2 p = new Vector2(slot.pos.x, slot.pos.z);

                // Safety: never let a lamp end up on a road / footprint / out of bounds.
                if (WorldLayout.Blocked(p, 0.6f)) continue;

                // alternate which side the arm/head leans toward the road
                bool lit = (placed % 2 == 0) && lightCount < 10;
                BuildLamp(hub, slot.pos, slot.yaw, lit, placed);
                if (lit) lightCount++;
                placed++;
            }
        }

        private static void BuildLamp(Transform hub, Vector3 footPos, float yaw, bool lit, int id)
        {
            // Lamp local rig so we can rotate the whole thing to face the road.
            var lamp = BuildKit.Root("Lamp_" + id, footPos).transform;
            lamp.SetParent(hub, true);
            lamp.localRotation = Quaternion.Euler(0f, yaw, 0f);

            // --- chunky cast base (solid, keep collider) ---
            BuildKit.Cube("Base", lamp, new Vector3(0f, 0.14f, 0f),
                new Vector3(0.62f, 0.28f, 0.62f), BaseCol, 0.5f, 0.3f);
            // little tapered collar above the base
            BuildKit.Cube("Collar", lamp, new Vector3(0f, 0.34f, 0f),
                new Vector3(0.40f, 0.16f, 0.40f), PoleCol, 0.6f, 0.35f);

            // --- tall pole (cylinder, KEEP collider) ---
            float poleH = 4.4f;
            BuildKit.Cylinder("Pole", lamp, new Vector3(0f, poleH * 0.5f + 0.42f, 0f),
                new Vector3(0.16f, poleH * 0.5f, 0.16f), PoleCol, 0.65f, 0.4f);

            float topY = poleH + 0.42f;

            // --- curved arm reaching out over the sidewalk toward the road (local -Z faces road after yaw) ---
            // upright knuckle
            BuildKit.Cylinder("ArmRise", lamp, new Vector3(0f, topY + 0.18f, 0f),
                new Vector3(0.12f, 0.22f, 0.12f), PoleCol, 0.6f, 0.4f);
            // horizontal reach (slight downward tilt at the end)
            var arm = BuildKit.Cube("Arm", lamp, new Vector3(0f, topY + 0.30f, -0.55f),
                new Vector3(0.12f, 0.12f, 1.25f), PoleCol, 0.6f, 0.4f);
            arm.transform.localRotation = Quaternion.Euler(12f, 0f, 0f);

            // head mount point out at the end of the arm, hanging down a touch
            Vector3 headLocal = new Vector3(0f, topY + 0.06f, -1.18f);

            // --- decorative housing shell (pass-through, remove collider) ---
            var shell = BuildKit.Cube("Housing", lamp, headLocal + new Vector3(0f, 0.18f, 0f),
                new Vector3(0.42f, 0.16f, 0.42f), PoleCol, 0.55f, 0.45f);
            RemoveCollider(shell);

            // --- glowing warm lamp head (emissive, collider removed) ---
            var head = BuildKit.Sphere("LampHead", lamp, headLocal,
                new Vector3(0.34f, 0.30f, 0.34f), WarmGlass, 0.0f, 0.85f);
            // Sphere already strips its collider (keepCollider defaults false), but be explicit/safe.
            RemoveCollider(head);
            MakeGlow(head, lit ? 2.4f : 1.1f);

            // --- optional real light cast down onto the sidewalk ---
            if (lit)
            {
                var lightGo = new GameObject("LampLight");
                lightGo.transform.SetParent(lamp, false);
                lightGo.transform.localPosition = headLocal + new Vector3(0f, -0.12f, 0f);
                var l = lightGo.AddComponent<Light>();
                l.type = LightType.Point;
                l.color = WarmLight;
                l.intensity = 2.2f;
                l.range = 9f;
                l.shadows = LightShadows.None;
            }
        }

        private static void RemoveCollider(GameObject go)
        {
            var c = go.GetComponent<Collider>();
            if (c != null) Object.Destroy(c);
        }

        private static void MakeGlow(GameObject go, float emission)
        {
            var r = go.GetComponent<Renderer>();
            if (r == null) return;
            var m = r.material; // instance
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", WarmGlass * emission);
        }
    }
}
