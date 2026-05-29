using UnityEngine;
using System.Collections.Generic;

namespace Spoonacci
{
    // A side-road bike lane with sporty cyclists who moonlight as drug couriers.
    // The lane runs roughly parallel to the main road from (-60,-30) to (60,-30).
    public static class CyclistsBuilder
    {
        // Lane geometry constants.
        const float LaneZ = -30f;     // center z of the bike lane
        const float LaneXMin = -60f;
        const float LaneXMax = 60f;
        const float LaneWidth = 3.2f;

        public static void Build(Transform root)
        {
            var hub = BuildKit.Root("CyclistSideRoad", new Vector3(0f, 0f, LaneZ));
            if (root != null) hub.transform.SetParent(root, true);

            BuildLane(hub.transform);
            BuildCyclists(hub.transform);
        }

        // ------------------------------------------------------------------ lane

        static void BuildLane(Transform parent)
        {
            float laneLen = LaneXMax - LaneXMin;
            float midX = (LaneXMin + LaneXMax) * 0.5f;

            // Asphalt-ish base strip with a distinct green/red bike-lane tint.
            var laneColor = new Color(0.18f, 0.42f, 0.22f); // bike-lane green
            var lane = BuildKit.Cube("BikeLaneStrip", parent,
                new Vector3(midX, 0.02f, 0f),
                new Vector3(laneLen, 0.04f, LaneWidth),
                laneColor, 0f, 0.18f);
            StripCollider(lane);

            // Red accent border slabs along each long edge of the lane.
            var red = new Color(0.55f, 0.14f, 0.12f);
            float edge = LaneWidth * 0.5f - 0.12f;
            for (int s = -1; s <= 1; s += 2)
            {
                var border = BuildKit.Cube("BikeLaneEdge", parent,
                    new Vector3(midX, 0.022f, edge * s),
                    new Vector3(laneLen, 0.045f, 0.18f),
                    red, 0f, 0.2f);
                StripCollider(border);
            }

            // White dashed center markings down the lane.
            var white = new Color(0.92f, 0.92f, 0.92f);
            float dashStep = 4.5f;
            for (float x = LaneXMin + 2f; x < LaneXMax - 2f; x += dashStep)
            {
                var dash = BuildKit.Cube("LaneDash", parent,
                    new Vector3(x, 0.03f, 0f),
                    new Vector3(1.6f, 0.04f, 0.18f),
                    white, 0f, 0.25f);
                StripCollider(dash);
            }

            // Painted little "bicycle" pictograms (two dots + a bar) every ~16m.
            for (float x = LaneXMin + 8f; x < LaneXMax - 4f; x += 16f)
            {
                BuildBikePictogram(parent, x, white);
            }
        }

        // A flat painted bike symbol: two wheel rings (squares) and a connecting bar.
        static void BuildBikePictogram(Transform parent, float x, Color col)
        {
            float wheelGap = 1.1f;
            for (int w = -1; w <= 1; w += 2)
            {
                var wheel = BuildKit.Cube("BikeMarkWheel", parent,
                    new Vector3(x + wheelGap * 0.5f * w, 0.031f, -0.5f),
                    new Vector3(0.55f, 0.04f, 0.55f),
                    col, 0f, 0.25f);
                StripCollider(wheel);
            }
            var bar = BuildKit.Cube("BikeMarkBar", parent,
                new Vector3(x, 0.031f, -0.1f),
                new Vector3(wheelGap, 0.04f, 0.16f),
                col, 0f, 0.25f);
            StripCollider(bar);
        }

        // --------------------------------------------------------------- cyclists

        static readonly Color[] JerseyColors =
        {
            new Color(0.90f, 0.20f, 0.20f), // red
            new Color(0.15f, 0.55f, 0.95f), // blue
            new Color(0.95f, 0.80f, 0.10f), // yellow
            new Color(0.20f, 0.80f, 0.35f), // green
            new Color(0.85f, 0.30f, 0.75f), // magenta
            new Color(0.10f, 0.85f, 0.85f), // cyan
        };

        static void BuildCyclists(Transform parent)
        {
            int count = 6;
            float span = LaneXMax - LaneXMin;
            for (int i = 0; i < count; i++)
            {
                // Spread their patrol centers evenly along the lane.
                float t = (i + 0.5f) / count;
                float cx = LaneXMin + 6f + t * (span - 12f);
                var worldCenter = new Vector3(cx, 0f, LaneZ);

                var jersey = JerseyColors[i % JerseyColors.Length];
                var pants = new Color(0.12f, 0.12f, 0.16f);

                var civ = BuildKit.Civ("Cyclist_" + i, worldCenter, Civilian.Mode.Patrol, jersey, pants);
                if (civ != null)
                {
                    civ.mode = Civilian.Mode.Patrol;
                    civ.patrolCenter = worldCenter;
                    civ.patrolRadius = 20f;
                    civ.walkSpeed = 3.2f + Random.value * 1.4f; // brisk cycling pace
                    civ.shirtColor = jersey;
                    civ.pantsColor = pants;

                    // They ARE the couriers.
                    civ.gameObject.AddComponent<DrugDealer>();

                    // Bike rig parented to the cyclist so it travels with them.
                    bool withPackage = (i % 3 == 0); // a couple carry visible packages
                    BuildBike(civ.transform, jersey, withPackage);
                }
            }
        }

        // Builds a simple bicycle rig as a child of the cyclist's transform.
        static void BuildBike(Transform cyclist, Color accent, bool withPackage)
        {
            var rigGo = new GameObject("BikeRig");
            rigGo.transform.SetParent(cyclist, false);
            // Sit the bike slightly below the rider and offset so wheels track the lane.
            rigGo.transform.localPosition = new Vector3(0f, -0.55f, 0f);
            var rig = rigGo.transform;

            var dark = new Color(0.08f, 0.08f, 0.10f); // tyres
            var metal = new Color(0.55f, 0.56f, 0.60f); // frame metal

            // Wheels: thin cylinders stood upright (rolling around local X axis).
            // A default cylinder's long axis is Y; rotate 90deg about Z so it lies flat as a disc facing X.
            var wheelRot = Quaternion.Euler(0f, 0f, 90f);
            float wheelR = 0.45f;
            BuildWheel(rig, "WheelFront", new Vector3(0.62f, wheelR, 0f), wheelR, wheelRot, dark, accent);
            BuildWheel(rig, "WheelRear", new Vector3(-0.62f, wheelR, 0f), wheelR, wheelRot, dark, accent);

            // Frame: a couple of thin diagonal/horizontal bars.
            var topTube = BuildKit.Cube("FrameTop", rig,
                new Vector3(0f, 0.78f, 0f), new Vector3(1.05f, 0.06f, 0.06f),
                metal, 0.7f, 0.6f);
            StripCollider(topTube);

            var downTube = BuildKit.Cube("FrameDown", rig,
                new Vector3(0.05f, 0.55f, 0f), new Vector3(0.9f, 0.06f, 0.06f),
                metal, 0.7f, 0.6f);
            downTube.transform.localRotation = Quaternion.Euler(0f, 0f, 28f);
            StripCollider(downTube);

            var seatTube = BuildKit.Cube("FrameSeat", rig,
                new Vector3(-0.45f, 0.6f, 0f), new Vector3(0.06f, 0.5f, 0.06f),
                metal, 0.7f, 0.6f);
            seatTube.transform.localRotation = Quaternion.Euler(0f, 0f, 12f);
            StripCollider(seatTube);

            // Seat post saddle.
            var saddle = BuildKit.Cube("Saddle", rig,
                new Vector3(-0.5f, 0.9f, 0f), new Vector3(0.32f, 0.06f, 0.16f),
                new Color(0.1f, 0.1f, 0.1f), 0f, 0.3f);
            StripCollider(saddle);

            // Fork to front wheel.
            var fork = BuildKit.Cube("Fork", rig,
                new Vector3(0.6f, 0.62f, 0f), new Vector3(0.06f, 0.62f, 0.06f),
                metal, 0.7f, 0.6f);
            fork.transform.localRotation = Quaternion.Euler(0f, 0f, -16f);
            StripCollider(fork);

            // Handlebars: a stem and a cross bar.
            var stem = BuildKit.Cube("Stem", rig,
                new Vector3(0.62f, 1.0f, 0f), new Vector3(0.06f, 0.28f, 0.06f),
                metal, 0.7f, 0.6f);
            StripCollider(stem);
            var bars = BuildKit.Cube("Handlebars", rig,
                new Vector3(0.62f, 1.12f, 0f), new Vector3(0.08f, 0.06f, 0.7f),
                new Color(0.05f, 0.05f, 0.05f), 0.4f, 0.5f);
            StripCollider(bars);

            // Pedals/crank hint.
            var crank = BuildKit.Cube("Crank", rig,
                new Vector3(0f, 0.3f, 0f), new Vector3(0.06f, 0.34f, 0.06f),
                new Color(0.2f, 0.2f, 0.22f), 0.5f, 0.5f);
            StripCollider(crank);

            // Rear rack.
            var rack = BuildKit.Cube("RearRack", rig,
                new Vector3(-0.62f, 0.92f, 0f), new Vector3(0.5f, 0.05f, 0.34f),
                new Color(0.3f, 0.3f, 0.32f), 0.6f, 0.5f);
            StripCollider(rack);

            if (withPackage)
            {
                // A suspicious brown package strapped to the rear rack.
                var pkg = BuildKit.Cube("CourierPackage", rig,
                    new Vector3(-0.62f, 1.08f, 0f), new Vector3(0.42f, 0.28f, 0.3f),
                    new Color(0.45f, 0.30f, 0.16f), 0f, 0.25f);
                StripCollider(pkg);
                // Tape cross on top.
                var tape = BuildKit.Cube("PackageTape", pkg.transform,
                    new Vector3(0f, 0.5f, 0f), new Vector3(1.05f, 0.12f, 0.3f),
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

            // Bright hub/spoke accent center.
            var hub = BuildKit.Sphere(name + "_Hub", wheel.transform,
                Vector3.zero, new Vector3(0.5f, 0.5f, 0.5f), hubColor, 0.5f, 0.6f);
            // Sphere already has collider removed by default.
            if (hub != null) StripCollider(hub);
        }

        static void StripCollider(GameObject go)
        {
            if (go == null) return;
            var col = go.GetComponent<Collider>();
            if (col != null) Object.Destroy(col);
        }
    }
}
