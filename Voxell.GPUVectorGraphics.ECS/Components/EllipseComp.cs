using Unity.Mathematics;
using Unity.Entities;

namespace Voxell.GPUVectorGraphics.ECS
{
    public struct EllipseComp : IComponentData, IDefault<EllipseComp>
    {
        public float2 Size;
        public float4 Tint;

        public EllipseComp Default()
        {
            this.Size = 1.0f;
            this.Tint = 1.0f;

            return this;
        }
    }
}
