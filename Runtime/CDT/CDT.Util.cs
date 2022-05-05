using System.Runtime.CompilerServices;
using Unity.Mathematics;
using Unity.Collections;

namespace Voxell.GPUVectorGraphics
{
  public partial class CDT
  {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void GetTriangleIndices(
      in NativeList<int> na_triangles,
      int idx, out int t0, out int t1, out int t2
    )
    {
      int tIdx = idx*3;
      t0 = na_triangles[tIdx];
      t1 = na_triangles[tIdx + 1];
      t2 = na_triangles[tIdx + 2];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AddTriangle(ref NativeList<int> na_triangles, int t0, int t1, int t2)
    {
      na_triangles.Add(t0);
      na_triangles.Add(t1);
      na_triangles.Add(t2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void RemoveTriangle(ref NativeList<int> na_triangles, int idx)
    {
      int tIdx = idx*3;
      na_triangles.RemoveRange(tIdx, 3);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AddTriAndCircum(
      in NativeArray<float2> na_points, ref NativeList<int> na_triangles,
      ref NativeList<Cirumcircle> na_cirumcircles,
      int t0, int t1, int t2
    )
    {
      AddTriangle(ref na_triangles, t0, t1, t2);
      float2 p0 = na_points[t0];
      float2 p1 = na_points[t1];
      float2 p2 = na_points[t2];
      na_cirumcircles.Add(new Cirumcircle(p0, p1, p2));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void RemoveTriAndCircum(
      ref NativeList<Cirumcircle> na_cirumcircles, ref NativeList<int> na_triangles, int idx
    )
    {
      RemoveTriangle(ref na_triangles, idx);
      na_cirumcircles.RemoveAt(idx);
    }
  }
}