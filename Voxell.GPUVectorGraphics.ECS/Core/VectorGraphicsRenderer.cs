using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Entities;
using Unity.Transforms;

namespace Voxell.GPUVectorGraphics.ECS
{
    public static class VectorGraphicsRenderer
    {
        public struct RenderCompCache : System.IDisposable
        {
            public EntityQuery Query;
            public Material Material;
            public Mesh Mesh;

            public VectorGraphicsRenderer.RenderDelegate RenderDelegate;

            public void Dispose()
            {
                this.Query.Dispose();
            }
        }

        public static EntityQuery CreateQuery<Comp>(ref EntityManager manager)
        where Comp : unmanaged, IRenderComp
        {
            return manager.CreateEntityQuery(
                ComponentType.ReadOnly<LocalTransform>(),
                ComponentType.ReadOnly<Comp>()
            );
        }

        public delegate void RenderDelegate(ref RenderCompCache renderData);

        public static void Render<Comp>(ref RenderCompCache renderData)
        where Comp : unmanaged, IRenderComp
        {
            JobHandle job_transform;
            JobHandle job_comp;

            NativeList<LocalTransform> na_transforms = renderData.Query.ToComponentDataListAsync<LocalTransform>(Allocator.TempJob, out job_transform);
            NativeList<Comp> na_comps = renderData.Query.ToComponentDataListAsync<Comp>(Allocator.TempJob, out job_comp);

            JobHandle.CompleteAll(ref job_transform, ref job_comp);

            for (int c = 0; c < na_comps.Length; c++)
            {
                LocalTransform transform = na_transforms[c];
                Comp comp = na_comps[c];

                RenderParams renderParams = new RenderParams(renderData.Material);

                MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
                comp.SetPropertyBlock(propertyBlock);

                renderParams.matProps = propertyBlock;

                // render with the assumption that there is only 1 submesh
                Graphics.RenderMesh(in renderParams, renderData.Mesh, 0, transform.ToMatrix());
            }

            na_transforms.Dispose();
            na_comps.Dispose();
        }

        public static RenderCompCache CreateCache<Comp>(ref EntityManager manager, Material material, Mesh mesh)
        where Comp : unmanaged, IRenderComp
        {
            return new RenderCompCache
            {
                Query = CreateQuery<Comp>(ref manager),
                Material = material,
                Mesh = mesh,
                RenderDelegate = Render<Comp>,
            };
        }
    }
}
