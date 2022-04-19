using System.Runtime.CompilerServices;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;

namespace Voxell.GPUVectorGraphics
{
  public static partial class CDT
  {
    private const float MARGIN = 10.0f;

    /// <summary>Constraint delaunay triangulation based on a contour.</summary>
    /// <param name="minRect">minimum point of the point set</param>
    /// <param name="maxRect">maximum point of the point set</param>
    /// <param name="points">points to be triangulated</param>
    /// <param name="contours">contour defining the polygon boundary</param>
    /// <param name="na_points">a copy of the input point array</param>
    /// <param name="na_triangles">output of the final triangle list</param>
    /// <param name="na_contours">a copy of the input contour array</param>
    /// <returns></returns>
    public static JobHandle ConstraintTriangulate(
      float2 minRect, float2 maxRect, in float2[] points, in ContourPoint[] contours,
      out NativeArray<float2> na_points, out NativeList<int> na_triangles,
      out NativeArray<ContourPoint> na_contours
    )
    {
      na_contours = new NativeArray<ContourPoint>(contours, Allocator.TempJob);
      JobHandle jobHandle = Triangulate(minRect, maxRect, in points, out na_points, out na_triangles);
      ConstraintJob job_constraint = new ConstraintJob(ref na_contours, ref na_points, ref na_triangles);
      return job_constraint.Schedule(jobHandle);
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
      // last 4 points are for the rect-triangle (will be used throughout the CDT process too)
      na_points = new NativeArray<float2>(points.Length + 4, Allocator.TempJob);
      na_triangles = new NativeList<int>(Allocator.TempJob);

      NativeSlice<float2> na_points_slice = na_points.Slice(0, points.Length);
      na_points_slice.CopyFrom(points);

      TriangulateJob job_triangulate = new TriangulateJob(
        minRect, maxRect, ref na_points, ref na_triangles
      );
      return job_triangulate.Schedule();
    }

    #region Helper Functions
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
      ref NativeList<Circumcenter> na_circumcenters,
      int t0, int t1, int t2
    )
    {
      AddTriangle(ref na_triangles, t0, t1, t2);
      float2 p0 = na_points[t0];
      float2 p1 = na_points[t1];
      float2 p2 = na_points[t2];
      na_circumcenters.Add(new Circumcenter(p0, p1, p2));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void RemoveTriAndCircum(
      ref NativeList<Circumcenter> na_circumcenters, ref NativeList<int> na_triangles, int idx
    )
    {
      RemoveTriangle(ref na_triangles, idx);
      na_circumcenters.RemoveAt(idx);
    }
    #endregion
  }
}