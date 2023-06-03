using Unity.Entities;
using Unity.Transforms;

namespace Voxell.GPUVectorGraphics.ECS
{
    public static partial class VectorGraphics
    {
        public static T Default<T>()
        where T : unmanaged, IComponentData, IDefault<T>
        {
            return new T().Default();
        }

        public static Entity DefaultEntity<T>(ref EntityManager manager)
        where T : unmanaged, IComponentData, IDefault<T>
        {
            Entity entity = TransEntity(ref manager);
            manager.AddComponentData<T>(entity, Default<T>());

            return entity;
        }

        public static Entity TransEntity(ref EntityManager manager)
        {
            return TransEntity(ref manager, LocalTransform.Identity);
        }

        public static Entity TransEntity(ref EntityManager manager, LocalTransform localTransform)
        {
            Entity entity = manager.CreateEntity();
            manager.AddComponentData<LocalTransform>(entity, localTransform);

            return entity;
        }
    }
}
