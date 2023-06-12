using UnityEngine;

namespace Voxell.GPUVectorGraphics
{
    public static class ShaderID
    {
        public static readonly int _Size = Shader.PropertyToID("_Size");
        public static readonly int _Radius = Shader.PropertyToID("_Radius");
        public static readonly int _Tint = Shader.PropertyToID("_Tint");
        public static readonly int _Sides = Shader.PropertyToID("_Sides");
    }
}
