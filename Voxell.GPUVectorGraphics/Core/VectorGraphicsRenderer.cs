using System.Runtime.CompilerServices;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Entities;
using Unity.Transforms;

namespace Voxell.GPUVectorGraphics
{
    public static class VectorGraphicsRenderer
    {
        public delegate void RenderDelegate(ref RenderCompCache renderData);

        /// <summary>Cache of required data for rendering a IRenderComp.</summary>
        public struct RenderCompCache : System.IDisposable
        {
            public EntityQuery Query;
            public Material Material;
            public Mesh Mesh;

            public RenderDelegate RenderDelegate;

            public void Dispose()
            {
                this.Query.Dispose();
            }
        }

        /// <summary>Create an EntityQuery for a given IRenderComp.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EntityQuery CreateQuery<Comp>(ref EntityManager manager)
        where Comp : unmanaged, IRenderComp
        {
            return manager.CreateEntityQuery(
                ComponentType.ReadOnly<LocalTransform>(),
                ComponentType.ReadOnly<Comp>()
            );
        }

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

        /// <summary>Create a RenderCompCache for a given IRenderComp and its associated data.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
