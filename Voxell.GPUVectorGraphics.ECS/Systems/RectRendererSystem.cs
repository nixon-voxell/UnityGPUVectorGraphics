using UnityEngine;
using Unity.Entities;
using Unity.Transforms;

namespace Voxell.GPUVectorGraphics.ECS
{
    using static VectorGraphicsWorld;
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class RectRendererSystem : SystemBase, System.IDisposable
    {
        private Material m_mat_RectUnlit;

        protected override void OnCreate()
        {
            this.RequireForUpdate<RectComp>();
        }

        protected override void OnStartRunning()
        {
            this.m_mat_RectUnlit = MaterialMap["RectUnlit"];
        }

        protected override void OnUpdate()
        {
            foreach (
                var (transform, rect) in
                SystemAPI.Query<RefRO<LocalTransform>, RefRO<RectComp>>()
            ) {
                RenderParams renderParams = new RenderParams(this.m_mat_RectUnlit);

                MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
                propertyBlock.SetVector(ShaderID._Size, new Vector4(rect.ValueRO.Size.x, rect.ValueRO.Size.y, 0.0f, 0.0f));
                propertyBlock.SetFloat(ShaderID._Radius, rect.ValueRO.Radius);
                propertyBlock.SetVector(ShaderID._Tint, rect.ValueRO.Tint);

                renderParams.matProps = propertyBlock;

                Graphics.RenderMesh(in renderParams, Primitive.Quad, 0, transform.ValueRO.ToMatrix());
            }
        }

        public void OnStopRunning(ref SystemState state) {}

        protected override void OnDestroy()
        {
            this.Dispose();
        }

        public void Dispose()
        {
        }
    }
}
