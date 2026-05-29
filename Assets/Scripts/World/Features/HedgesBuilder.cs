using UnityEngine;

namespace Spoonacci
{
    // Lays neat manicured HEDGE ROWS flanking the four main diagonal roads so they
    // read as proper landscaped avenues, plus a few decorative hedge cubes framing
    // the hub plaza corners. Each hedge box is a Leaves-textured cube (~2 long, ~1.4
    // tall) sitting at a perpendicular offset of ~8 from the road centerline on BOTH
    // sides. Hedges are SOLID, so colliders are kept. Segments that would fall inside
    // a building/zone footprint, out of bounds, or right next to a crosswalk (near the
    // plaza) are skipped so the avenues breathe.
    public static class HedgesBuilder
    {
        // Tunables for the avenue rows.
        const float Offset    = 8.0f;   // perpendicular distance from road centerline
        const float HedgeLen  = 2.0f;   // box length along the road
        const float HedgeH    = 1.4f;   // hedge height
        const float HedgeW    = 0.9f;   // hedge thickness (cross-road)
        const float StepStart = 14f;    // begin this far out from origin
        const float EndPad    = 20f;    // stop this far before the road's far end
        const float StepGap   = 2.4f;   // spacing between hedge box centers along the road

        public static void Build(Transform root)
        {
            var hedgeRoot = BuildKit.Root("Hedges", Vector3.zero);
            if (root != null) hedgeRoot.transform.SetParent(root, true);

            // Rich manicured-boxwood greens. A couple of tints keep rows from looking flat.
            var leafTint = new Color(0.22f, 0.46f, 0.18f, 1f);
            var tile = new Vector2(1f, 1f);

            // ---- avenue rows along the four MAIN roads ----------------------
            for (int r = 0; r < 4 && r < WorldLayout.Roads.Length; r++)
            {
                var seg = WorldLayout.Roads[r];
                Vector2 a = seg.a, b = seg.b;
                Vector2 along = b - a;
                float length = along.magnitude;
                if (length < 1e-3f) continue;

                Vector2 dir = along / length;            // unit direction down the road
                Vector2 perp = new Vector2(-dir.y, dir.x); // unit perpendicular

                // Orient the box so its long axis runs along the road.
                float yaw = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;

                float tEnd = length - EndPad;
                int idx = 0;
                for (float t = StepStart; t <= tEnd; t += StepGap, idx++)
                {
                    // Leave gaps near the plaza crosswalk: skip the first couple of
                    // boxes nearest origin so pedestrians have an open approach.
                    if (idx < 2) continue;

                    Vector2 centerOnRoad = a + dir * t;

                    // Both sides of the avenue.
                    for (int side = -1; side <= 1; side += 2)
                    {
                        Vector2 p = centerOnRoad + perp * (Offset * side);

                        // Respect footprints and bounds; these rows intentionally hug
                        // the road so we do NOT skip on OnRoad (offset 8 already clears
                        // the 5.0 corridor half-width), but we honor everything else.
                        if (WorldLayout.OutOfBounds(p, 2f)) continue;
                        if (WorldLayout.InFootprint(p, 0.5f)) continue;

                        var pos = new Vector3(p.x, HedgeH * 0.5f, p.y);
                        var go = BuildKit.CubeTex(
                            "Hedge_R" + r + "_" + idx + (side < 0 ? "_L" : "_R"),
                            hedgeRoot.transform, Vector3.zero,
                            new Vector3(HedgeLen, HedgeH, HedgeW),
                            ProceduralTextures.Leaves, leafTint, 0f, 0.18f, tile);
                        go.transform.position = pos;
                        go.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
                        // Hedges are solid obstacles — KEEP the collider (do nothing).
                    }
                }
            }

            // ---- decorative corner hedges framing the hub plaza ------------
            // Tidy boxwood cubes just outside each plaza corner so the central square
            // feels landscaped. Plaza half-extent is ~9; sit them a touch beyond.
            float c = WorldLayout.PlazaHalf + 1.6f;
            Vector2[] corners =
            {
                new Vector2( c,  c), new Vector2(-c,  c),
                new Vector2( c, -c), new Vector2(-c, -c),
            };
            float cubeH = 1.6f, cubeS = 1.8f;
            for (int i = 0; i < corners.Length; i++)
            {
                Vector2 p = corners[i];
                if (WorldLayout.OutOfBounds(p, 2f)) continue;
                if (WorldLayout.InFootprint(p, 0.3f)) continue;
                if (WorldLayout.OnRoad(p, 0f)) continue; // keep plaza corners off the diagonals

                var pos = new Vector3(p.x, cubeH * 0.5f, p.y);
                var go = BuildKit.CubeTex(
                    "PlazaCornerHedge_" + i, hedgeRoot.transform, Vector3.zero,
                    new Vector3(cubeS, cubeH, cubeS),
                    ProceduralTextures.Leaves, leafTint, 0f, 0.2f, tile);
                go.transform.position = pos;
                // Solid topiary cube — keep collider.
            }
        }
    }
}
