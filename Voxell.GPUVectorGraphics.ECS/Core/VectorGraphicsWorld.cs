using System.Collections.Generic;
using UnityEngine;

namespace Voxell.GPUVectorGraphics.ECS
{
    public static class VectorGraphicsWorld
    {
        public static Dictionary<string, Material> MaterialMap;
        public static PrimitiveMesh Primitive;

        public static void Initialize()
        {
            MaterialMap = new Dictionary<string, Material>(1024);

            Material[] materials = Resources.LoadAll<Material>("GPUVectorGraphics/Materials");

            foreach (Material mat in materials)
            {
                MaterialMap.Add(mat.name, mat);
            }

            Primitive.Initialize();
        }

        public static void Dispose()
        {
            MaterialMap.Clear();
        }
    }

    public struct PrimitiveMesh
    {
        public Mesh Quad => m_Quad;
        private Mesh m_Quad;

        public void Initialize()
        {
            this.m_Quad = GeneratePrimitive(PrimitiveType.Quad);
        }

        private Mesh GeneratePrimitive(PrimitiveType primitiveType)
        {
            // create temporary game object and discard it
            GameObject tempObj = GameObject.CreatePrimitive(primitiveType);
            Mesh mesh = tempObj.GetComponent<MeshFilter>().sharedMesh;
            Object.Destroy(tempObj);

            return mesh;
        }
    }
}
