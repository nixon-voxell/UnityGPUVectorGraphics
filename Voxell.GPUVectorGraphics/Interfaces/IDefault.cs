namespace Voxell.GPUVectorGraphics
{
    public interface IDefault<T>
    where T : unmanaged
    {
        T Default();
    }
}
