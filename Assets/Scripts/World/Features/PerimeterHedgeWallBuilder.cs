using UnityEngine;

namespace Spoonacci
{
    // PerimeterHedgeWallBuilder — a continuous, IMPASSABLE boundary ring styled as
    // tall manicured hedges (Leaves CubeTex) studded with stone/iron fence posts,
    // ornamental corner pillars, and a grand locked gate arch facing the ocean.
    //
    // The ring sits at x,z = +/-92 — just OUTSIDE the playable bounds (WorldLayout.Bounds=90),
    // so it never conflicts with interior content and never blocks the main roads.
    // Hedge segments OVERLAP (length ~6, step ~5.5) so there are zero gaps the player
    // could squeeze through. All hedge/fence/pillar pieces KEEP their colliders — this
    // is the real wall.
    public static class PerimeterHedgeWallBuilder
    {
        const float Ring = 92f;     // distance from origin to the wall line
        const float Height = 3.5f;  // hedge height
        const float SegLen = 6f;    // hedge segment length (overlaps neighbour)
        const float Step = 5.5f;    // spacing between segment centers (< SegLen => overlap)
        const float Thick = 1.1f;   // hedge depth

        public static void Build(Transform root)
        {
            var holder = BuildKit.Root("PerimeterHedgeWall", Vector3.zero);
            holder.transform.SetParent(root, false);
            var T = holder.transform;

            Color leafTint = new Color(0.34f, 0.55f, 0.28f);
            Color baseTint = new Color(0.22f, 0.34f, 0.18f); // darker hedge skirt
            Vector2 tile = new Vector2(3f, 1.5f);

            // ---- Four sides. Each side is a run of overlapping hedge boxes. ----
            // South edge (z = -Ring), running along x
            BuildSide(T, leafTint, baseTint, tile, axisIsX: true, fixedCoord: -Ring);
            // North edge (z = +Ring)
            BuildSide(T, leafTint, baseTint, tile, axisIsX: true, fixedCoord: +Ring);
            // West edge (x = -Ring), running along z
            BuildSide(T, leafTint, baseTint, tile, axisIsX: false, fixedCoord: -Ring);
            // East edge (x = +Ring)
            BuildSide(T, leafTint, baseTint, tile, axisIsX: false, fixedCoord: +Ring);

            // ---- Ornamental corner pillars ----
            BuildCornerPillar(T, new Vector3(+Ring, 0f, +Ring));
            BuildCornerPillar(T, new Vector3(+Ring, 0f, -Ring));
            BuildCornerPillar(T, new Vector3(-Ring, 0f, +Ring));
            BuildCornerPillar(T, new Vector3(-Ring, 0f, -Ring));

            // ---- Grand locked gate arch facing the ocean (south, toward -Z) ----
            BuildGate(T, new Vector3(0f, 0f, -Ring), facingZ: true);
        }

        // Builds one full side of overlapping hedge boxes plus periodic fence posts.
        static void BuildSide(Transform parent, Color leaf, Color skirt, Vector2 tile, bool axisIsX, float fixedCoord)
        {
            // Run from -Ring..+Ring along the moving axis. Leave a small gap centered
            // at 0 on the south side for the gate; everything else is solid.
            float start = -Ring;
            float end = +Ring;
            bool isGateSide = axisIsX && fixedCoord < 0f; // south side carries the gate
            float gateHalf = 5.5f; // opening width/2 spanned by the (closed, locked) gate

            int idx = 0;
            for (float t = start; t <= end + 0.01f; t += Step)
            {
                // Skip hedge boxes inside the gate opening — the gate arch fills it.
                if (isGateSide && Mathf.Abs(t) < gateHalf) { idx++; continue; }

                Vector3 pos;
                Vector3 scale;
                if (axisIsX)
                {
                    pos = new Vector3(t, Height * 0.5f, fixedCoord);
                    scale = new Vector3(SegLen, Height, Thick);
                }
                else
                {
                    pos = new Vector3(fixedCoord, Height * 0.5f, t);
                    scale = new Vector3(Thick, Height, SegLen);
                }

                // Main leafy hedge body (KEEP collider — this is the wall).
                BuildKit.CubeTex($"Hedge_{(axisIsX ? "X" : "Z")}_{fixedCoord:0}_{idx}",
                    parent, pos, scale, ProceduralTextures.Leaves, leaf, 0f, 0.18f, tile);

                // Darker trimmed skirt at the base for a manicured look (decoration, drop collider).
                Vector3 skirtPos = pos; skirtPos.y = 0.35f;
                Vector3 skirtScale = scale; skirtScale.y = 0.7f;
                if (axisIsX) skirtScale.z += 0.25f; else skirtScale.x += 0.25f;
                var sk = BuildKit.CubeTex($"HedgeSkirt_{(axisIsX ? "X" : "Z")}_{fixedCoord:0}_{idx}",
                    parent, skirtPos, skirtScale, ProceduralTextures.Leaves, skirt, 0f, 0.15f, tile);
                var skc = sk.GetComponent<Collider>(); if (skc != null) Object.Destroy(skc);

                // Decorative fence post every 3rd segment (stone base + iron cap, KEEP colliders).
                if (idx % 3 == 0)
                {
                    Vector3 postPos = pos; postPos.y = 0f;
                    BuildFencePost(parent, postPos, $"{(axisIsX ? "X" : "Z")}_{fixedCoord:0}_{idx}");
                }
                idx++;
            }
        }

        // A stone post with a dark iron spear cap. Solid.
        static void BuildFencePost(Transform parent, Vector3 footPos, string id)
        {
            Color stone = new Color(0.55f, 0.54f, 0.5f);
            Color iron = new Color(0.16f, 0.16f, 0.18f);

            // Stone pillar, slightly taller than the hedge so it reads as a post.
            float h = Height + 0.6f;
            BuildKit.Cube($"Post_{id}", parent,
                new Vector3(footPos.x, h * 0.5f, footPos.z),
                new Vector3(0.55f, h, 0.55f), stone, 0.05f, 0.35f);

            // Iron cap.
            BuildKit.Cube($"PostCap_{id}", parent,
                new Vector3(footPos.x, h + 0.12f, footPos.z),
                new Vector3(0.7f, 0.24f, 0.7f), iron, 0.85f, 0.55f);

            // Iron spear finial.
            var spear = BuildKit.Cube($"PostSpear_{id}", parent,
                new Vector3(footPos.x, h + 0.55f, footPos.z),
                new Vector3(0.22f, 0.55f, 0.22f), iron, 0.85f, 0.6f);
            spear.transform.localRotation = Quaternion.Euler(0f, 45f, 0f);
        }

        // Tall ornamental corner pillar with a tiered stone cap and a golden orb.
        static void BuildCornerPillar(Transform parent, Vector3 corner)
        {
            Color stone = new Color(0.6f, 0.58f, 0.53f);
            Color gold = new Color(0.85f, 0.7f, 0.25f);
            string id = $"{corner.x:0}_{corner.z:0}";

            float h = Height + 1.6f;
            BuildKit.Cube($"CornerPillar_{id}", parent,
                new Vector3(corner.x, h * 0.5f, corner.z),
                new Vector3(1.5f, h, 1.5f), stone, 0.05f, 0.3f);

            // Tiered cap.
            BuildKit.Cube($"CornerCapA_{id}", parent,
                new Vector3(corner.x, h + 0.2f, corner.z),
                new Vector3(1.9f, 0.4f, 1.9f), stone, 0.05f, 0.35f);
            BuildKit.Cube($"CornerCapB_{id}", parent,
                new Vector3(corner.x, h + 0.55f, corner.z),
                new Vector3(1.3f, 0.35f, 1.3f), stone, 0.05f, 0.4f);

            // Golden decorative orb (drop collider — pure ornament on top).
            BuildKit.Sphere($"CornerOrb_{id}", parent,
                new Vector3(corner.x, h + 1.05f, corner.z),
                new Vector3(0.7f, 0.7f, 0.7f), gold, 1f, 0.85f);
        }

        // Grand locked gate: two thick stone piers, a heavy lintel/arch beam spanning
        // the opening, twin dark-iron gate leaves (closed & locked), and a brass lock plate.
        static void BuildGate(Transform parent, Vector3 center, bool facingZ)
        {
            Color stone = new Color(0.62f, 0.6f, 0.55f);
            Color iron = new Color(0.13f, 0.13f, 0.15f);
            Color brass = new Color(0.78f, 0.6f, 0.22f);

            float opening = 9f;      // total clear span between piers
            float pierH = Height + 2.5f;
            float pierW = 1.4f;
            float halfOpen = opening * 0.5f;

            // Piers either side of the opening (along X since gate is on the south Z-edge).
            BuildKit.Cube("GatePierL", parent,
                new Vector3(center.x - halfOpen - pierW * 0.5f, pierH * 0.5f, center.z),
                new Vector3(pierW, pierH, 1.6f), stone, 0.05f, 0.3f);
            BuildKit.Cube("GatePierR", parent,
                new Vector3(center.x + halfOpen + pierW * 0.5f, pierH * 0.5f, center.z),
                new Vector3(pierW, pierH, 1.6f), stone, 0.05f, 0.3f);

            // Solid filler walls bridging each pier to the adjacent hedge run, so the
            // boundary is CONTINUOUS regardless of where the hedge loop indices fall.
            // The hedge skip is symmetric (|t|<5.5) but hedge centers are not perfectly
            // symmetric, so without these fillers a small gap can open beside a pier.
            // Span generously from the pier outer face out to |x|=14 (well past the
            // nearest hedge inner edge); the overlap with hedges is harmless and the
            // wall is guaranteed gap-free. KEEP colliders — this is the wall.
            float pierOuterL = center.x - halfOpen - pierW;        // left pier outer face
            float pierOuterR = center.x + halfOpen + pierW;        // right pier outer face
            float fillTo = 14f;                                    // reach out past nearest hedge
            float fillLenL = Mathf.Abs(pierOuterL) > fillTo ? 0f : (fillTo - Mathf.Abs(pierOuterL));
            float fillLenR = Mathf.Abs(pierOuterR) > fillTo ? 0f : (fillTo - Mathf.Abs(pierOuterR));
            if (fillLenL > 0.01f)
                BuildKit.Cube("GateFillerL", parent,
                    new Vector3(pierOuterL - fillLenL * 0.5f, Height * 0.5f, center.z),
                    new Vector3(fillLenL, Height, Thick), stone, 0.05f, 0.3f);
            if (fillLenR > 0.01f)
                BuildKit.Cube("GateFillerR", parent,
                    new Vector3(pierOuterR + fillLenR * 0.5f, Height * 0.5f, center.z),
                    new Vector3(fillLenR, Height, Thick), stone, 0.05f, 0.3f);

            // Pier caps.
            BuildKit.Cube("GatePierCapL", parent,
                new Vector3(center.x - halfOpen - pierW * 0.5f, pierH + 0.25f, center.z),
                new Vector3(pierW + 0.5f, 0.5f, 2.1f), stone, 0.05f, 0.35f);
            BuildKit.Cube("GatePierCapR", parent,
                new Vector3(center.x + halfOpen + pierW * 0.5f, pierH + 0.25f, center.z),
                new Vector3(pierW + 0.5f, 0.5f, 2.1f), stone, 0.05f, 0.35f);

            // Heavy lintel / arch beam spanning the opening (the "arch").
            float lintelY = pierH - 0.3f;
            BuildKit.Cube("GateLintel", parent,
                new Vector3(center.x, lintelY, center.z),
                new Vector3(opening + pierW * 2f, 1.0f, 1.3f), stone, 0.05f, 0.32f);

            // Arched keystone block on top center.
            BuildKit.Cube("GateKeystone", parent,
                new Vector3(center.x, lintelY + 0.7f, center.z),
                new Vector3(1.2f, 0.9f, 1.5f), stone, 0.05f, 0.35f);

            // Twin iron gate leaves filling the opening (closed, locked). KEEP colliders.
            float leafW = halfOpen - 0.1f;
            float leafH = pierH - 0.8f;
            BuildKit.Cube("GateLeafL", parent,
                new Vector3(center.x - halfOpen * 0.5f, leafH * 0.5f, center.z),
                new Vector3(leafW, leafH, 0.35f), iron, 0.8f, 0.5f);
            BuildKit.Cube("GateLeafR", parent,
                new Vector3(center.x + halfOpen * 0.5f, leafH * 0.5f, center.z),
                new Vector3(leafW, leafH, 0.35f), iron, 0.8f, 0.5f);

            // Vertical iron bars decorating the leaves (drop colliders — leaves already block).
            for (int i = -3; i <= 3; i++)
            {
                float bx = center.x + i * 1.25f;
                if (Mathf.Abs(bx - center.x) < 0.05f) continue; // gap at the seam for the lock
                var bar = BuildKit.Cube($"GateBar_{i}", parent,
                    new Vector3(bx, leafH * 0.5f, center.z),
                    new Vector3(0.14f, leafH - 0.3f, 0.14f), iron, 0.85f, 0.6f);
                var bc = bar.GetComponent<Collider>(); if (bc != null) Object.Destroy(bc);
            }

            // Brass lock plate + handle ring at the seam (decoration).
            var plate = BuildKit.Cube("GateLockPlate", parent,
                new Vector3(center.x, leafH * 0.5f, center.z - 0.22f),
                new Vector3(0.5f, 0.7f, 0.12f), brass, 0.9f, 0.7f);
            var pc = plate.GetComponent<Collider>(); if (pc != null) Object.Destroy(pc);

            var ring = BuildKit.Sphere("GateLockRing", parent,
                new Vector3(center.x, leafH * 0.5f, center.z - 0.32f),
                new Vector3(0.4f, 0.4f, 0.12f), brass, 0.95f, 0.8f);
            // (sphere collider already removed by BuildKit)

            // "PRIVATE — KEEP OUT" sign on the lintel facing inward (toward +Z, the player).
            var signParent = new GameObject("GateSign");
            signParent.transform.SetParent(parent, false);
            signParent.transform.position = new Vector3(center.x, lintelY, center.z + 0.7f);
            BuildKit.Label(signParent, "PRIVATE — NO EXIT", new Color(0.95f, 0.9f, 0.7f), 22, Vector3.zero);
        }
    }
}
