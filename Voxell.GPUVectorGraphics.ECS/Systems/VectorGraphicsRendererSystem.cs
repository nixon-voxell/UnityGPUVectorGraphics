using System.Collections.Generic;
using Unity.Entities;

namespace Voxell.GPUVectorGraphics.ECS
{
    using static VectorGraphicsRenderer;
    using static VectorGraphicsWorld;

    public partial class VectorGraphicsRendererSystem : SystemBase, System.IDisposable
    {
        private List<RenderCompCache> m_RenderCompCaches;

        protected override void OnCreate()
        {
            this.m_RenderCompCaches = new List<RenderCompCache>(128);

            EntityManager manager = this.EntityManager;
            this.m_RenderCompCaches.Add(CreateCache<RectComp>(ref manager, MaterialMap["RectUnlit"], Primitive.Quad));
            this.m_RenderCompCaches.Add(CreateCache<EllipseComp>(ref manager, MaterialMap["EllipseUnlit"], Primitive.Quad));
        }

        protected override void OnStartRunning()
        {
        }

        protected override void OnUpdate()
        {
            for (int c = 0, count = this.m_RenderCompCaches.Count; c < count; c++)
            {
                RenderCompCache cache = this.m_RenderCompCaches[c];
                cache.RenderDelegate(ref cache);
            }
        }

        public void OnStopRunning(ref SystemState state) {}

        protected override void OnDestroy()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            for (int c = 0, count = this.m_RenderCompCaches.Count; c < count; c++)
            {
                RenderCompCache cache = this.m_RenderCompCaches[c];
                cache.Dispose();
            }

            this.m_RenderCompCaches.Clear();
        }
    }
}
