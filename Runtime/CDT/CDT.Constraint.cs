using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;

namespace Voxell.GPUVectorGraphics
{
  using Mathx;

  public partial class CDT
  {
    [BurstCompile]
    private struct ConstraintJob : IJob
    {
      public NativeArray<int> na_contour;
      public NativeArray<float2> na_points;
      public NativeList<int> na_triangles;

      public ConstraintJob(
        ref NativeArray<int> na_contour,
        ref NativeArray<float2> na_points, ref NativeList<int> na_triangles
      )
      {
        this.na_contour = na_contour;
        this.na_points = na_points;
        this.na_triangles = na_triangles;
      }

      public void Execute()
      {
        // create a list of all initial edges from the delaunay triangulation
        NativeList<Edge> na_edges = new NativeList<Edge>(Allocator.Temp);
        for (int t=0, triangleCount=na_triangles.Length/3; t < triangleCount; t++)
        {
          int t0, t1, t2;
          GetTriangleIndices(ref na_triangles, t, out t0, out t1, out t2);
          Edge edge0 = new Edge(t0, t1);
          Edge edge1 = new Edge(t1, t2);
          Edge edge2 = new Edge(t2, t0);

          // make sure no duplicate edges are added
          if (!na_edges.Contains(edge0))
            na_edges.Add(edge0);
          if (!na_edges.Contains(edge1))
            na_edges.Add(edge1);
          if (!na_edges.Contains(edge2))
            na_edges.Add(edge2);
        }

        int segmentCount = na_contour.Length - 1;

        NativeList<int> outsidePoints = new NativeList<int>(Allocator.Temp);
        NativeList<int> insidePoints = new NativeList<int>(Allocator.Temp);
        NativeList<int> blackListedTris = new NativeList<int>(Allocator.Temp);

        for (int s=0; s < segmentCount; s++)
        {
          int e0 = na_contour[s];
          int e1 = na_contour[s + 1];

          Edge edge = new Edge(e0, e1);
          if (na_edges.Contains(edge)) continue;

          // remove all blocking triangles
          int triangleCount = na_triangles.Length/3;
          for (int t=0; t < triangleCount; t++)
          {
            if (TriEdgeIntersectCheck(t, edge))
            {
              blackListedTris.Add(t);
            }
          }

          // remove all blacklisted triangles
          int blackListedTriCount = blackListedTris.Length;
          int removeCount = 0;
          for (int t=0; t < blackListedTriCount; t++)
            RemoveTriangle(ref na_triangles, blackListedTris[t]-removeCount++);

          // retriangulate outside points

          // retriangulate inside points

          insidePoints.Clear();
          outsidePoints.Clear();
          blackListedTris.Clear();
        }

        na_edges.Dispose();
        outsidePoints.Dispose();
        insidePoints.Dispose();
        blackListedTris.Dispose();
      }

      private bool TriEdgeIntersectCheck(int idx, Edge edge)
      {
        int t0, t1, t2;
        GetTriangleIndices(ref na_triangles, idx, out t0, out t1, out t2);

        // wrong checking method (this treats the edge as an infinite boundary)

        float2 t_p0 = na_points[t0];
        float2 t_p1 = na_points[t1];
        float2 t_p2 = na_points[t2];

        float2 e_p0 = na_points[edge.e0];
        float2 e_p1 = na_points[edge.e1];
        float2 n = float2x.perpendicular(e_p1 - e_p0);

        float d0 = math.dot(t_p0 - e_p0, n);
        float d1 = math.dot(t_p1 - e_p0, n);
        float d2 = math.dot(t_p2 - e_p0, n);

        return !((d0 >= 0.0f && d1 >= 0.0f && d2 >= 0.0f) || (d0 <= 0.0f && d1 <= 0.0f && d2 <= 0.0f));
      }
    }
  }
}