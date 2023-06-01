using Unity.Mathematics;
using Unity.Entities;
using Unity.Transforms;

namespace Voxell.GPUVectorGraphics.ECS
{
    public struct RectComp : IComponentData
    {
        public float2 Size;
        public float Radius;
        public float4 Tint;
    }

    public static partial class VectorGraphics
    {
        public static class Rect
        {
            public static EntityArchetype GetDefaultArchetype(ref SystemState state)
            {
                return state.EntityManager.CreateArchetype(
                    typeof(LocalTransform),
                    typeof(RectComp)
                );
            }

            public static Entity Create(ref SystemState state)
            {
                return state.EntityManager.CreateEntity(GetDefaultArchetype(ref state));
            }
        }
    }

}
