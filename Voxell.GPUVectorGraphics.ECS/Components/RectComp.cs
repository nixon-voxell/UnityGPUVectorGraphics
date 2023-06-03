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
            this.Size = 1.0f;
            this.Radius = 0.05f;
            this.Tint = 1.0f;

            return this;
        }
    }
}
