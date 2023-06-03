namespace Voxell.GPUVectorGraphics.ECS
{
    public interface IDefault<T>
    where T : unmanaged
    {
        T Default();
    }
}
