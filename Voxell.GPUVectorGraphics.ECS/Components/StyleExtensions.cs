using Unity.Mathematics;
using Unity.Entities;

namespace Voxell.GPUVectorGraphics.ECS
{
    using static StyleComps;

    public static class StyleExtensions
    {
        public static void AddTint(this Entity entity, ref SystemState state, float4 color)
        {
            state.EntityManager.AddComponentData<StyleComps.TintComp>(
                entity,
                new TintComp
                {
                    Color = color
                }
            );
        }

        public static void AddStroke(this Entity entity, ref SystemState state, float width)
        {
            state.EntityManager.AddComponentData<StyleComps.StrokeComp>(
                entity,
                new StrokeComp
                {
                    Width = width
                }
            );
        }
    }
}
