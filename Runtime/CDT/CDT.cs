using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;

namespace Voxell.GPUVectorGraphics
{
  public static partial class CDT
  {
    private const float MARGIN = 1.0f;

    public static JobHandle ConstraintTriangulate(
      float2 minRect, float2 maxRect, in float2[] points, in int[] contour,
      out NativeArray<float2> na_points, out NativeList<int> na_triangles, out NativeArray<int> na_contour
    )
    {
      na_contour = new NativeArray<int>(contour, Allocator.TempJob);
      JobHandle triangulateJobHandle = Triangulate(minRect, maxRect, in points, out na_points, out na_triangles);
      ConstraintJob job_constraint = new ConstraintJob(ref na_contour, ref na_points, ref na_triangles);
      JobHandle constraintJobHandle = job_constraint.Schedule(triangulateJobHandle);
      return constraintJobHandle;
    }

    /// <summary>Performs a delaunay triangulation on a set of points.</summary>
    /// <param name="minRect">minimum point of the point set</param>
    /// <param name="maxRect">maximum point of the point set</param>
    /// <param name="points">points to be triangulated</param>
    /// <param name="na_points">a copy of the input point array</param>
    /// <param name="na_triangles">output of the final triangle list</param>
    /// <returns>A JobHandle the is being scheduled for delaunay triangulation.</returns>
    public static JobHandle Triangulate(
      float2 minRect, float2 maxRect, in float2[] points,
      out NativeArray<float2> na_points, out NativeList<int> na_triangles
    )
    {
      na_points = new NativeArray<float2>(points.Length + 3, Allocator.TempJob);
      na_triangles = new NativeList<int>(Allocator.TempJob);

      NativeSlice<float2> na_points_slice = na_points.Slice(0, points.Length);
      na_points_slice.CopyFrom(points);

      TriangulateJob job_triangulate = new TriangulateJob(
        minRect, maxRect, ref na_points, ref na_triangles
      );
      return job_triangulate.Schedule();
    }

    #region Helper Functions
    private static void GetTriangleIndices(
      ref NativeList<int> na_triangles,
      int idx, out int t0, out int t1, out int t2
    )
    {
      int tIdx = idx*3;
      t0 = na_triangles[tIdx];
      t1 = na_triangles[tIdx + 1];
      t2 = na_triangles[tIdx + 2];
    }

    private static void AddTriangle(ref NativeList<int> na_triangles, int t0, int t1, int t2)
    {
      na_triangles.Add(t0);
      na_triangles.Add(t1);
      na_triangles.Add(t2);
    }

    private static void RemoveTriangle(ref NativeList<int> na_triangles, int idx)
    {
      int tIdx = idx*3;
      na_triangles.RemoveRange(tIdx, 3);
    }
    #endregion
  }
}