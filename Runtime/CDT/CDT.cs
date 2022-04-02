using Unity.Mathematics;
using Unity.Collections;
using Unity.Collections.NotBurstCompatible;
using Unity.Jobs;

namespace Voxell.GPUVectorGraphics
{
  public static partial class CDT
  {
    // this is just for testing purposes, in practice,
    // we will be scheduling multiple jobs for every unique shape
    // and dispose the native list ourselves
    public static JobHandle Triangulate(
      float2 minRect, float2 maxRect, in float2[] points,
      out NativeList<float2> na_points, out NativeList<int> na_triangles
    )
    {
      na_points = new NativeList<float2>(points.Length, Allocator.TempJob);
      na_triangles = new NativeList<int>(Allocator.TempJob);
      na_points.CopyFromNBC(points);

      TriangulateJob job_triangulate = new TriangulateJob(minRect, maxRect, ref na_points, ref na_triangles);
      return job_triangulate.Schedule();
    }
  }
}