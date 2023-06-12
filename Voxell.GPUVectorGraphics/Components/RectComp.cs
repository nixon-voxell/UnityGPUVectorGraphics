using Unity.Mathematics;
using UnityEngine;

namespace Voxell.GPUVectorGraphics
{
    public struct RectComp : IRenderComp, IDefault<RectComp>
    {
        public float2 Size;
        public float Radius;
        public float4 Tint;

        public RectComp Default()
        {
            this.Size = 1.0f;
            this.Radius = 0.0f;
            this.Tint = 1.0f;

            return this;
        }

        public void SetPropertyBlock(MaterialPropertyBlock propertyBlock)
        {
            propertyBlock.SetVector(ShaderID._Size, new Vector4(this.Size.x, this.Size.y, 0.0f, 0.0f));
            propertyBlock.SetFloat(ShaderID._Radius, this.Radius);
            propertyBlock.SetVector(ShaderID._Tint, this.Tint);
        }
    }
}
