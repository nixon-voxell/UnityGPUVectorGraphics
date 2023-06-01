using Unity.Entities;
using Unity.Burst;

namespace Voxell.GPUVectorGraphics.ECS
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial class VectorGraphicsInitSystem : SystemBase
    {
        protected override void OnCreate()
        {
            VectorGraphicsWorld.Initialize();
        }

        protected override void OnUpdate()
        {
            VectorGraphicsWorld.Dispose();
        }
    }
}
