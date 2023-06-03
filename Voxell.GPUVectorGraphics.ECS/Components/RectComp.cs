using Unity.Mathematics;
using Unity.Entities;

namespace Voxell.GPUVectorGraphics.ECS
{
    public struct RectComp : IComponentData, IDefault<RectComp>
    {
        public float2 Size;
        public float Radius;
        public float4 Tint;

        public RectComp Default()
        {
            return new RectComp
            {
                Size = 1.0f,
                Radius = 0.05f,
                Tint = 1.0f,
            };
        }
    }
}
