using UnityEngine;
using Unity.Entities;

namespace Voxell.GPUVectorGraphics.ECS
{
    public interface IRenderComp : IComponentData
    {
        void SetPropertyBlock(MaterialPropertyBlock propertyBlock);
    }
}
