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
            Entity entity = TransformEntity(ref manager);
            manager.AddComponentData<Comp>(entity, Default<Comp>());

            return entity;
        }

        public static Entity DefaultEntity<Comp>(ref EntityManager manager, Entity parent)
        where Comp : unmanaged, IComponentData, IDefault<Comp>
        {
            Entity entity = TransformEntity(ref manager, parent);
            manager.AddComponentData<Comp>(entity, Default<Comp>());

            return entity;
        }

        public static Entity TransformEntity(ref EntityManager manager)
        {
            return TransformEntity(ref manager, LocalTransform.Identity);
        }

        public static Entity TransformEntity(ref EntityManager manager, Entity parent)
        {
            Entity entity = TransformEntity(ref manager, LocalTransform.Identity);
            manager.AddComponentData<Parent>(entity, new Parent { Value = parent });

            return entity;
        }

        public static Entity TransformEntity(ref EntityManager manager, LocalTransform localTransform)
        {
            Entity entity = manager.CreateEntity();
            manager.AddComponentData<LocalTransform>(entity, localTransform);
            manager.AddComponent<LocalToWorld>(entity);

            return entity;
        }
    }
}
