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
      ConstrainJob job_constrain = new ConstrainJob(ref na_contours, ref na_points, ref na_triangles);
      return job_constrain.Schedule(jobHandle);
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
  }
}