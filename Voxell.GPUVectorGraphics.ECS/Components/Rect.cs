using Unity.Mathematics;
using Unity.Entities;
using Unity.Transforms;

namespace Voxell.GPUVectorGraphics.ECS
{
    public struct RectComp : IComponentData
    {
        public float2 Size;
    }

    public static partial class VectorGraphics
    {
        public static class Rect
        {
            public static EntityArchetype GetDefaultArchetype(ref SystemState state)
            {
                return state.EntityManager.CreateArchetype(
                    typeof(LocalTransform),
                    typeof(RectComp),
                    typeof(StyleComps.TintComp),
                    typeof(StyleComps.BorderComp)
                    // typeof(StyleComps.Stroke),
                    // typeof(StyleComps.StrokeTint),
                );
            }

            public static Entity Create(ref SystemState state)
            {
                return state.EntityManager.CreateEntity(GetDefaultArchetype(ref state));
            }
        }
    }

}
