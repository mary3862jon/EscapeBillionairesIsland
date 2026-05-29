using UnityEngine;

namespace Spoonacci
{
    // Bike couriers riding the official east-west bike lane (z = -36, x from -62..62),
    // plus a couple looping past the hub plaza edge so the player spots cyclists instantly.
    // Each cyclist is a Patrol civilian sweeping up/down the lane, carries a bicycle rig,
    // and is marked as a DrugDealer courier.
    public static class CyclistsBuilder
    {
        public static void Build(Transform root)
        {
            var hub = BuildKit.Root("Cyclists", Vector3.zero);
            if (root != null) hub.transform.SetParent(root, true);

            Vector2 a = WorldLayout.BikeLaneA; // (-62, -36)
            Vector2 b = WorldLayout.BikeLaneB; // ( 62, -36)

            // Sporty jersey palette for variety.
            Color[] jerseys =
            {
                new Color(0.90f, 0.20f, 0.20f), // red
                new Color(0.20f, 0.55f, 0.95f), // blue
                new Color(0.95f, 0.80f, 0.15f), // yellow
                new Color(0.20f, 0.80f, 0.45f), // green
                new Color(0.95f, 0.45f, 0.10f), // orange
                new Color(0.75f, 0.25f, 0.85f), // purple
                new Color(0.10f, 0.85f, 0.85f), // cyan
            };
            Color darkShorts = new Color(0.12f, 0.12f, 0.15f);

            // --- 7 cyclists spaced ALONG the bike lane ---
            for (int i = 0; i < 7; i++)
            {
                float t = (i + 0.5f) / 7f;                 // 0.07 .. 0.93 along the lane
                Vector2 onLane = Vector2.Lerp(a, b, t);
                Vector3 spawn = new Vector3(onLane.x, 0f, onLane.y);

                Color jersey = jerseys[i % jerseys.Length];
                var c = BuildKit.Civ("Courier_" + i, spawn, Civilian.Mode.Patrol, jersey, darkShorts);
                if (c == null) continue;

                c.mode = Civilian.Mode.Patrol;
                c.walkSpeed = 3.5f;
                c.patrolCenter = spawn;                    // a point ON the lane
                c.patrolRadius = 22f;                      // sweeps up/down the lane
                c.shirtColor = jersey;
                c.pantsColor = darkShorts;
                c.gameObject.AddComponent<DrugDealer>();   // they ARE couriers

                bool withPackage = (i % 3 == 0);           // ~2-3 of 7 carry a package
                BuildBike(c.transform, jersey, withPackage);
            }

            // --- 2 extra cyclists looping right by the hub plaza edge ---
            Vector3 hubCenter = new Vector3(0f, 0f, -12f);
            for (int j = 0; j < 2; j++)
            {
                Vector3 spawn = hubCenter + new Vector3(j == 0 ? -4f : 4f, 0f, 0f);
                Color jersey = jerseys[(j + 2) % jerseys.Length];
                var c = BuildKit.Civ("HubCourier_" + j, spawn, Civilian.Mode.Patrol, jersey, darkShorts);
                if (c == null) continue;

                c.mode = Civilian.Mode.Patrol;
                c.walkSpeed = 3.5f;
                c.patrolCenter = hubCenter;                // loop near the plaza edge
                c.patrolRadius = 10f;
                c.shirtColor = jersey;
                c.pantsColor = darkShorts;
                c.gameObject.AddComponent<DrugDealer>();

                BuildBike(c.transform, jersey, j == 0);
            }

            // --- "BIKE COURIERS" sign on a post near the lane midpoint ---
            Vector2 mid = Vector2.Lerp(a, b, 0.5f);        // (0, -36)
            // nudge the post just off the lane line so it doesn't clip the riders
            float postX = mid.x;
            float postZ = mid.y - 4f;
            var post = BuildKit.Cylinder("CourierSignPost", hub.transform,
                new Vector3(postX, 2.0f, postZ),
                new Vector3(0.22f, 2.0f, 0.22f), new Color(0.30f, 0.22f, 0.14f), 0.2f, 0.3f);
            // keep the post collider (it is a solid post)

            var board = BuildKit.Cube("CourierSignBoard", hub.transform,
                new Vector3(postX, 4.0f, postZ),
                new Vector3(3.4f, 0.9f, 0.15f), new Color(0.95f, 0.85f, 0.25f), 0.1f, 0.4f);
            BuildKit.Label(board, "BIKE COURIERS", new Color(0.08f, 0.08f, 0.1f), 26,
                new Vector3(0f, 0.05f, -0.12f));
        }

        // Builds a bicycle rig as a CHILD of the cyclist's transform; all colliders removed.
        static void BuildBike(Transform cyclist, Color accent, bool withPackage)
        {
            var rigGo = new GameObject("BikeRig");
            rigGo.transform.SetParent(cyclist, false);
            rigGo.transform.localPosition = new Vector3(0f, 0f, 0f);
            var rig = rigGo.transform;

            Color tyre = new Color(0.08f, 0.08f, 0.10f);
            Color metal = new Color(0.60f, 0.62f, 0.66f);

            // Wheels: thin cylinders stood UPRIGHT (rotate 90deg about Z so they stand like wheels).
            var wheelRot = Quaternion.Euler(0f, 0f, 90f);
            float wheelR = 0.45f;
            BuildWheel(rig, "WheelFront", new Vector3(0f, wheelR, 0.58f), wheelR, wheelRot, tyre, accent);
            BuildWheel(rig, "WheelRear", new Vector3(0f, wheelR, -0.58f), wheelR, wheelRot, tyre, accent);

            // Frame: thin cubes.
            var topTube = BuildKit.Cube("FrameTop", rig,
                new Vector3(0f, 0.78f, 0f), new Vector3(0.06f, 0.06f, 1.05f),
                accent, 0.6f, 0.55f);
            StripCollider(topTube);

            var downTube = BuildKit.Cube("FrameDown", rig,
                new Vector3(0f, 0.55f, 0.05f), new Vector3(0.06f, 0.06f, 0.9f),
                accent, 0.6f, 0.55f);
            downTube.transform.localRotation = Quaternion.Euler(28f, 0f, 0f);
            StripCollider(downTube);

            var seatTube = BuildKit.Cube("FrameSeat", rig,
                new Vector3(0f, 0.6f, -0.45f), new Vector3(0.06f, 0.5f, 0.06f),
                metal, 0.7f, 0.6f);
            seatTube.transform.localRotation = Quaternion.Euler(12f, 0f, 0f);
            StripCollider(seatTube);

            var fork = BuildKit.Cube("Fork", rig,
                new Vector3(0f, 0.62f, 0.56f), new Vector3(0.06f, 0.62f, 0.06f),
                metal, 0.7f, 0.6f);
            fork.transform.localRotation = Quaternion.Euler(-16f, 0f, 0f);
            StripCollider(fork);

            // Handlebars: stem + cross bar.
            var stem = BuildKit.Cube("Stem", rig,
                new Vector3(0f, 1.0f, 0.56f), new Vector3(0.06f, 0.28f, 0.06f),
                metal, 0.7f, 0.6f);
            StripCollider(stem);
            var bars = BuildKit.Cube("Handlebars", rig,
                new Vector3(0f, 1.12f, 0.56f), new Vector3(0.62f, 0.06f, 0.08f),
                new Color(0.05f, 0.05f, 0.05f), 0.4f, 0.5f);
            StripCollider(bars);

            // Seat.
            var saddle = BuildKit.Cube("Saddle", rig,
                new Vector3(0f, 0.92f, -0.5f), new Vector3(0.16f, 0.06f, 0.32f),
                new Color(0.1f, 0.1f, 0.12f), 0f, 0.3f);
            StripCollider(saddle);

            // Crank hint.
            var crank = BuildKit.Cube("Crank", rig,
                new Vector3(0f, 0.3f, 0f), new Vector3(0.06f, 0.34f, 0.06f),
                new Color(0.2f, 0.2f, 0.22f), 0.5f, 0.5f);
            StripCollider(crank);

            if (withPackage)
            {
                // Rear rack + suspicious brown package.
                var rack = BuildKit.Cube("RearRack", rig,
                    new Vector3(0f, 0.9f, -0.6f), new Vector3(0.4f, 0.05f, 0.45f),
                    new Color(0.3f, 0.3f, 0.32f), 0.6f, 0.5f);
                StripCollider(rack);

                var pkg = BuildKit.Cube("CourierPackage", rig,
                    new Vector3(0f, 1.08f, -0.6f), new Vector3(0.4f, 0.3f, 0.4f),
                    new Color(0.55f, 0.36f, 0.18f), 0f, 0.22f);
                StripCollider(pkg);

                var tape = BuildKit.Cube("PackageTape", pkg.transform,
                    new Vector3(0f, 0.52f, 0f), new Vector3(1.05f, 0.12f, 0.3f),
                    new Color(0.82f, 0.72f, 0.5f), 0f, 0.2f);
                StripCollider(tape);
            }
        }

        static void BuildWheel(Transform parent, string name, Vector3 localPos, float radius,
                               Quaternion rot, Color tyre, Color hubColor)
        {
            var wheel = BuildKit.Cylinder(name, parent, localPos,
                new Vector3(radius * 2f, 0.07f, radius * 2f), tyre, 0.2f, 0.4f);
            wheel.transform.localRotation = rot;
            StripCollider(wheel);

            // Bright hub accent so the wheels read as bicycle wheels.
            var hub = BuildKit.Sphere(name + "_Hub", wheel.transform,
                Vector3.zero, new Vector3(0.45f, 0.45f, 0.45f), hubColor, 0.5f, 0.6f);
            StripCollider(hub);
        }

        static void StripCollider(GameObject go)
        {
            if (go == null) return;
            var col = go.GetComponent<Collider>();
            if (col != null) Object.Destroy(col);
        }
    }
}
