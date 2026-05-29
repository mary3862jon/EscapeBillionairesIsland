using UnityEngine;

namespace Spoonacci
{
    // Single integration point for all island feature builders. Each builder lives in its
    // own file under World/Features and is fully self-contained, so they can be developed
    // in parallel without touching each other or the bootstrapper.
    public static class IslandExpansion
    {
        public static void Build(Transform _)
        {
            var root = new GameObject("[Island Expansion]").transform;

            PathsBuilder.Build(root);        // road/sidewalk network connecting the zones
            PoolBuilder.Build(root);         // lavish central infinity-pool complex
            AlleyBuilder.Build(root);        // gritty shady alley + dealers
            VegetationBuilder.Build(root);   // refined palms, bushes, hedges, flowers, rocks
            PerimeterBuilder.Build(root);    // in-world boundary: hedges/fences + crash roadblock
            CyclistsBuilder.Build(root);     // bike lane + cyclist drug couriers
            StreetPropsBuilder.Build(root);  // lamps, benches, fountain, stalls, billboards
        }
    }
}
