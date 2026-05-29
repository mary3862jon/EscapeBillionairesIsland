using UnityEngine;

namespace Spoonacci
{
    // Detailed, vivid palm trees scattered across the open sand of the island.
    // Each palm has a gently CURVED trunk (3 stacked, slightly tilted Wood
    // cylinders), a leafy crown lump, 6 drooping fronds fanned around the top,
    // and a couple of coconuts. Placement is delegated to WorldLayout so no palm
    // ever lands on a road, in a building footprint, in the pool, or out of bounds.
    public static class PalmsBuilder
    {
        private const int PalmCount = 26;

        // Bark colours for trunk variety.
        private static readonly Color BarkLight = new Color(0.62f, 0.46f, 0.30f);
        private static readonly Color BarkDark  = new Color(0.50f, 0.36f, 0.22f);

        // Frond / crown greens.
        private static readonly Color LeafGreen      = new Color(0.18f, 0.46f, 0.16f);
        private static readonly Color LeafGreenDark  = new Color(0.12f, 0.34f, 0.12f);
        private static readonly Color LeafGreenLight = new Color(0.30f, 0.58f, 0.22f);

        // Coconut brown.
        private static readonly Color Coconut = new Color(0.32f, 0.22f, 0.12f);

        public static void Build(Transform root)
        {
            var hub = BuildKit.Root("Palms", Vector3.zero);
            hub.transform.SetParent(root, false);
            var parent = hub.transform;

            for (int i = 0; i < PalmCount; i++)
            {
                // WorldLayout is the spatial authority — extra 2.5 pad keeps the
                // wide crown clear of roads/buildings/pool/bounds.
                Vector3 pos;
                if (!WorldLayout.TryOpenSpot(out pos, 2.5f)) continue;

                float scale = Random.Range(0.85f, 1.45f);
                BuildPalm(parent, pos, scale, i);
            }
        }

        // ---------- one palm ----------

        private static void BuildPalm(Transform parent, Vector3 basePos, float scale, int id)
        {
            var palm = BuildKit.Root("Palm" + id, basePos);
            palm.transform.SetParent(parent, false);
            var t = palm.transform;

            // Random yaw so palms don't all curve in the same visual direction.
            t.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

            float trunkRadius = 0.30f * scale;
            const int segments = 3;            // exactly 3 stacked tilted cylinders
            float segLen = Random.Range(2.4f, 3.0f) * scale;

            // One global lean direction for the whole curving trunk.
            float leanDir = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            Vector3 lean = new Vector3(Mathf.Cos(leanDir), 0f, Mathf.Sin(leanDir));

            Vector3 cursor = Vector3.zero;     // local, bottom of the next segment
            float curTilt = Random.Range(2f, 5f);
            Vector3 topCenter = Vector3.zero;
            float topRadius = trunkRadius;

            for (int s = 0; s < segments; s++)
            {
                // Tilt grows toward the top -> gentle, natural curve.
                curTilt += Random.Range(4f, 8f);
                float tiltRad = curTilt * Mathf.Deg2Rad;
                Vector3 dir = (Vector3.up * Mathf.Cos(tiltRad) + lean * Mathf.Sin(tiltRad)).normalized;

                float r = Mathf.Lerp(trunkRadius, trunkRadius * 0.62f, (float)s / segments);
                Vector3 segCenter = cursor + dir * (segLen * 0.5f);

                Color bark = (s % 2 == 0) ? BarkLight : BarkDark;
                var seg = BuildKit.Cylinder("Trunk" + s, t, segCenter,
                    new Vector3(r, segLen * 0.5f, r), bark, 0f, 0.18f);
                Retexture(seg, ProceduralTextures.Wood, bark, 0f, 0.2f, new Vector2(1f, 2f));
                // Orient the cylinder along its tilted direction.
                seg.transform.localRotation = Quaternion.FromToRotation(Vector3.up, dir);
                // Trunk is a SOLID obstacle — keep the collider.

                cursor += dir * segLen;
                topCenter = cursor;
                topRadius = r;
            }

            // Leafy crown lump at the top of the trunk.
            var crown = BuildKit.Sphere("Crown", t, topCenter,
                Vector3.one * (topRadius * 2.6f), LeafGreenDark, 0f, 0.25f, false);
            Retexture(crown, ProceduralTextures.Leaves, LeafGreenDark, 0f, 0.25f, Vector2.one);

            // ---- 6 drooping fronds fanned around the crown ----
            const int fronds = 6;
            for (int f = 0; f < fronds; f++)
            {
                float ang = (360f / fronds) * f + Random.Range(-14f, 14f);
                float aRad = ang * Mathf.Deg2Rad;
                // Fronds reach outward then droop downward.
                Vector3 outDir = new Vector3(Mathf.Cos(aRad), -0.30f, Mathf.Sin(aRad)).normalized;
                float frondLen = Random.Range(2.8f, 3.8f) * scale;

                Color leaf = (f % 2 == 0) ? LeafGreen : LeafGreenLight;

                // Mid blade: an elongated flattened Leaves sphere lifted a touch.
                Vector3 mid = topCenter + outDir * (frondLen * 0.45f) + Vector3.up * (0.35f * scale);
                var fMid = BuildKit.Sphere("FrondMid" + f, t, mid,
                    new Vector3(1.6f, 0.40f, 1.6f) * scale, leaf, 0f, 0.30f, false);
                fMid.transform.localRotation = Quaternion.LookRotation(outDir, Vector3.up);
                Retexture(fMid, ProceduralTextures.Leaves, leaf, 0f, 0.30f, Vector2.one);
                StripCollider(fMid); // pass-through foliage

                // Drooping tip: a flattened Leaves cube dipping below the mid blade.
                Vector3 tip = topCenter + outDir * frondLen + Vector3.down * (frondLen * 0.22f);
                var fTip = BuildKit.CubeTex("FrondTip" + f, t, tip,
                    new Vector3(0.9f, 0.18f, 1.5f) * scale, ProceduralTextures.Leaves,
                    leaf, 0f, 0.30f, Vector2.one);
                fTip.transform.localRotation = Quaternion.LookRotation(outDir, Vector3.up);
                StripCollider(fTip); // pass-through foliage
            }

            // ---- a couple of coconuts clustered under the crown ----
            int nuts = Random.Range(2, 4);
            for (int n = 0; n < nuts; n++)
            {
                float a = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                Vector3 np = topCenter +
                    new Vector3(Mathf.Cos(a), -0.25f, Mathf.Sin(a)) * (topRadius * 1.7f);
                BuildKit.Sphere("Coconut" + n, t, np,
                    Vector3.one * 0.42f * scale, Coconut, 0f, 0.28f, false);
            }
        }

        // ---------- util ----------

        private static void StripCollider(GameObject go)
        {
            if (go == null) return;
            var col = go.GetComponent<Collider>();
            if (col != null) Object.Destroy(col);
        }

        // Apply a textured Lit material to an existing renderer.
        private static void Retexture(GameObject go, Texture2D tex, Color tint, float metallic, float smoothness, Vector2 tile)
        {
            if (go == null) return;
            var rend = go.GetComponent<Renderer>();
            if (rend == null) return;
            rend.sharedMaterial = ShaderCache.MakeTextured(tex, tint, metallic, smoothness, tile);
        }
    }
}
