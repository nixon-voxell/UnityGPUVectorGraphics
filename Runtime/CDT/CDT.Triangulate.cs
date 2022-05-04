using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;

namespace Voxell.GPUVectorGraphics
{
  public partial class CDT
  {
    /// <summary>Bowyer-Watson delaunay triangulation.</summary>
    [BurstCompile]
    private struct TriangulateJob : IJob
    {
      public float2 minRect;
      public float2 maxRect;

      public NativeArray<float2> na_points;
      public NativeList<int> na_triangles;

      public TriangulateJob(
        float2 minRect, float2 maxRect,
        ref NativeArray<float2> na_points,
        ref NativeList<int> na_triangles
      )
      {
        this.minRect = minRect;
        this.maxRect = maxRect;

        this.na_points = na_points;
        this.na_triangles = na_triangles;
      }

      public void Execute()
      {
        // create temp arrays
        NativeList<Edge> na_edges = new NativeList<Edge>(Allocator.Temp);
        NativeList<int> na_blackListedEdges = new NativeList<int>(Allocator.Temp);
        NativeList<Circumcenter> na_circumcenters = new NativeList<Circumcenter>(Allocator.Temp);

        // create rect-triangle
        CreateRectTriangle(in minRect, in maxRect, ref na_points, ref na_triangles, ref na_circumcenters);

        for (int p=0, pointCount=na_points.Length-4; p < pointCount; p++)
        {
          na_edges.Clear();
          na_blackListedEdges.Clear();

          float2 point = na_points[p];

          // prevent duplicated points (only triangulate the first point found)
          int tempIdx = na_points.IndexOf(point);
          if (tempIdx != p) continue;

          // remove triangles that contains the current point in its circumcenter
          int removeCount = 0;
          for (int c=0, circumCount=na_circumcenters.Length; c < circumCount; c++)
          {
            int idx = c - removeCount;
            Circumcenter circumcenter = na_circumcenters[idx];
            circumcenter.sqradius += 0.001f;
            if (circumcenter.ContainsPoint(point))
            {
              int t0, t1, t2;
              GetTriangleIndices(in na_triangles, idx, out t0, out t1, out t2);

              Edge edge = new Edge(t0, t1);
              AddEdgesOfRemovedTriangle(in edge, ref na_edges, ref na_blackListedEdges);

              edge.SetEdge(t1, t2);
              AddEdgesOfRemovedTriangle(in edge, ref na_edges, ref na_blackListedEdges);

              edge.SetEdge(t2, t0);
              AddEdgesOfRemovedTriangle(in edge, ref na_edges, ref na_blackListedEdges);

              RemoveTriAndCircum(ref na_circumcenters, ref na_triangles, idx);
              removeCount++;
            }
          }

          // sort black listed edge indices in ascending order and remove them
          na_blackListedEdges.Sort();
          RemoveBlacklistedEdges(in na_blackListedEdges, ref na_edges);

          // create new triangles out of the current point
          // by connecting each new edges to the current point
          CreateTrianglesForNewPoint(
            in p, in point, in na_edges, in na_points,
            ref na_triangles, ref na_circumcenters
          );
        }

        // remove all triangles associated with the rect-triangle
        RemoveRectTriangle(na_points.Length, ref na_triangles);

        // dispose all temp allocations
        na_edges.Dispose();
        na_blackListedEdges.Dispose();
        na_circumcenters.Dispose();
      }
    }
  }
}