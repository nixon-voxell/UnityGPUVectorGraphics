using UnityEngine;

namespace Voxell.GPUVectorGraphics.ECS
{
    public static class ShaderID
    {
        public static readonly int _Size = Shader.PropertyToID("_Size");
        public static readonly int _Radius = Shader.PropertyToID("_Radius");
        public static readonly int _Tint = Shader.PropertyToID("_Tint");
    }
}
