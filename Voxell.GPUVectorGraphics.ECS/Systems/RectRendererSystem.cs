using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace Voxell.GPUVectorGraphics.ECS
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct RectRendererSystem : ISystem
    {
        // private Mesh m_Quad;

        private void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<RectComp>();
            // this.m_Quad = 
        }

        [BurstCompile]
        private void OnUpdate(ref SystemState state)
        {
            foreach (
                var (transform, rect) in
                SystemAPI.Query<RefRO<LocalTransform>, RefRO<RectComp>>()
            ) {
                // Graphics.RenderMesh();
            }
        }
    }
}
