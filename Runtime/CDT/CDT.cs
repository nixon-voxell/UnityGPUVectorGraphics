using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;

namespace Voxell.GPUVectorGraphics
{
  public static partial class CDT
  {
    private const float MARGIN = 1.0f;

    // this is just for testing purposes, in practice,
    // we will be scheduling multiple jobs for every unique shape
    // and dispose the native list ourselves
    public static JobHandle Triangulate(
      float2 minRect, float2 maxRect, in float2[] points,
      out NativeArray<float2> na_points, out NativeList<int> na_triangles
    )
    {
      na_points = new NativeArray<float2>(points.Length + 3, Allocator.TempJob);
      na_triangles = new NativeList<int>(Allocator.TempJob);

      NativeSlice<float2> na_points_slice = na_points.Slice(0, points.Length);
      na_points_slice.CopyFrom(points);

      TriangulateJob job_triangulate = new TriangulateJob(minRect, maxRect, ref na_points, ref na_triangles);
      return job_triangulate.Schedule();
    }
  }
}