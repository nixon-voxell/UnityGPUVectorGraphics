using Unity.Mathematics;
using Unity.Entities;

namespace Voxell.GPUVectorGraphics.ECS
{
    public static class StyleComps
    {
        public struct TintComp : IComponentData
        {
            public float4 Color;
        }

        public struct StrokeComp : IComponentData
        {
            public float Width;
        }

        public struct StrokeTintComp : IComponentData
        {
            public float4 Color;
        }

        public struct BorderComp : IComponentData
        {
            public float Radius;
        }
    }

}
