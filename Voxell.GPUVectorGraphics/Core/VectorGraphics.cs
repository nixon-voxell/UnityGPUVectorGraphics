using Unity.Entities;
using Unity.Transforms;

namespace Voxell.GPUVectorGraphics
{
    using Util.Interface;

    public static partial class VectorGraphics
    {
        public static Comp Default<Comp>()
        where Comp : unmanaged, IComponentData, IDefault<Comp>
        {
            return new Comp().Default();
        }

        public static Entity DefaultEntity<Comp>(ref EntityManager manager)
        where Comp : unmanaged, IComponentData, IDefault<Comp>
        {
            Entity entity = TransEntity(ref manager);
            manager.AddComponentData<Comp>(entity, Default<Comp>());

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
