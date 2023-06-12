using Unity.Mathematics;
using UnityEngine;

namespace Voxell.GPUVectorGraphics
{
    public struct EllipseComp : IRenderComp, IDefault<EllipseComp>
    {
        public float2 Size;
        public float4 Tint;

        public EllipseComp Default()
        {
            this.Size = 1.0f;
            this.Tint = 1.0f;

            return this;
        }

        public void SetPropertyBlock(MaterialPropertyBlock propertyBlock)
        {
            propertyBlock.SetVector(ShaderID._Size, new Vector4(this.Size.x, this.Size.y, 0.0f, 0.0f));
            propertyBlock.SetVector(ShaderID._Tint, this.Tint);
        }
    }
}
