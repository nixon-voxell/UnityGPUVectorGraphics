using Unity.Entities;

namespace Voxell.GPUVectorGraphics.ECS
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial class VectorGraphicsInitSystem : SystemBase
    {
        protected override void OnCreate()
        {
            VectorGraphicsWorld.Initialize();
        }

        protected override void OnUpdate() {}

        protected override void OnDestroy()
        {
            VectorGraphicsWorld.Dispose();
        }
    }
}
