using UnityEngine;

namespace Spoonacci
{
    // Single integration point for all island feature builders. Each builder lives in its
    // own file under World/Features, is fully self-contained, and obeys WorldLayout so
    // nothing lands on a road / in a building / out of bounds. Built in dependency order:
    // roads first (define corridors), then boundaries, then scatter, then props & life.
    public static class IslandExpansion
    {
        public static void Build(Transform _)
        {
            var root = new GameObject("[Island Expansion]").transform;

            // 1. ROADS — define the corridors everything else avoids
            PathsBuilder.Build(root);

            // 2. BOUNDARY — keep the player on the island, diegetically
            PerimeterHedgeWallBuilder.Build(root);
            RoadblockSceneBuilder.Build(root);

            // 3. POOL PARTY
            PoolBasinBuilder.Build(root);
            PoolPartyBuilder.Build(root);

            // 4. SHADY ALLEY
            AlleyBuilder.Build(root);

            // 5. VEGETATION (scatter, all WorldLayout-aware)
            PalmsBuilder.Build(root);
            BushesBuilder.Build(root);
            FlowersBuilder.Build(root);
            HedgesBuilder.Build(root);
            RocksBuilder.Build(root);

            // 6. STREET FURNITURE (sidewalk-snapped, facing the road)
            BenchesBuilder.Build(root);
            StreetLampsBuilder.Build(root);
            TrashCansBuilder.Build(root);
            PlantersBuilder.Build(root);
            BillboardsBuilder.Build(root);
            FountainBuilder.Build(root);
            FoodStallsBuilder.Build(root);

            // 7. CYCLIST COURIERS
            BikeLaneBuilder.Build(root);
            CyclistsBuilder.Build(root);

            // 8. AMBIENT LIFE
            BeachGoersBuilder.Build(root);
            TouristsBuilder.Build(root);
        }
    }
}
