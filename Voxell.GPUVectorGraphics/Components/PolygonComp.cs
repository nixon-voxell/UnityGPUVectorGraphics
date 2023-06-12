using Unity.Mathematics;
using UnityEngine;

namespace Voxell.GPUVectorGraphics
{
    public struct PolygonComp : IRenderComp, IDefault<PolygonComp>
    {
        public float2 Size;
        public float Radius;
        public float4 Tint;
        public uint Sides;

        public PolygonComp Default()
        {
            this.Size = 1.0f;
            this.Radius = 0.0f;
            this.Tint = 1.0f;
            this.Sides = 3u;

            return this;
        }

        public void SetPropertyBlock(MaterialPropertyBlock propertyBlock)
        {
            propertyBlock.SetVector(ShaderID._Size, new Vector4(this.Size.x, this.Size.y, 0.0f, 0.0f));
            propertyBlock.SetFloat(ShaderID._Radius, this.Radius);
            propertyBlock.SetVector(ShaderID._Tint, this.Tint);
            propertyBlock.SetInteger(ShaderID._Sides, (int)this.Sides);
        }
    }
}
