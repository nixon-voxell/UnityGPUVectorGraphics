using UnityEngine;
using Unity.Entities;

namespace Voxell.GPUVectorGraphics
{
    public interface IRenderComp : IComponentData
    {
        void SetPropertyBlock(MaterialPropertyBlock propertyBlock);
    }
}
