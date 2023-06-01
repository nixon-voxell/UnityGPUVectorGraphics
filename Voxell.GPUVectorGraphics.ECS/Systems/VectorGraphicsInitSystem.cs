using Unity.Entities;
using Unity.Burst;

namespace Voxell.GPUVectorGraphics.ECS
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct VectorGraphicsInitSystem : ISystem
    {
        [BurstCompile]
        private void OnCreate()
        {
            
        }

        [BurstCompile]
        private void OnDestroy()
        {
            
        }
    }
}
