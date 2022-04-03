using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;

namespace Voxell.GPUVectorGraphics
{
  public partial class CDT
  {
    [BurstCompile]
    private struct ConstraintJob : IJob
    {
      public NativeArray<ContourPoint> na_contours;
      public NativeArray<float2> na_points;
      public NativeList<int> na_triangles;

      public ConstraintJob(
        ref NativeArray<ContourPoint> na_contours,
        ref NativeArray<float2> na_points, ref NativeList<int> na_triangles
      )
      {
        this.na_contours = na_contours;
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

        NativeList<int> outsidePoints = new NativeList<int>(Allocator.Temp);
        NativeList<int> insidePoints = new NativeList<int>(Allocator.Temp);
        NativeList<int> blackListedTris = new NativeList<int>(Allocator.Temp);

        int segmentCount = na_contours.Length - 1;
        for (int s=0; s < segmentCount; s++)
        {
          ContourPoint c0 = na_contours[s];
          ContourPoint c1 = na_contours[s + 1];
          // only check for intersection if both points are in the same contour
          if (c0.contourIdx != c1.contourIdx) continue;

          int e0 = c0.pointIdx;
          int e1 = c1.pointIdx;

          Edge edge = new Edge(e0, e1);
          if (na_edges.Contains(edge)) continue;

          UnityEngine.Debug.Log($"#{s}: {edge.e0}, {edge.e1}");

          // remove all blocking triangles
          int triangleCount = na_triangles.Length/3;
          for (int t=0; t < triangleCount; t++)
          {
            if (TriEdgeIntersect(t, edge))
            {
              // black list triangle to be removed later
              blackListedTris.Add(t);

              // sort points into outside and inside regions
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

      private bool TriEdgeIntersect(int idx, Edge edge)
      {
        int t0, t1, t2;
        GetTriangleIndices(ref na_triangles, idx, out t0, out t1, out t2);

        float2 t_p0 = na_points[t0];
        float2 t_p1 = na_points[t1];
        float2 t_p2 = na_points[t2];

        float2 e_p0 = na_points[edge.e0];
        float2 e_p1 = na_points[edge.e1];

        // only check for edge intersection when both edge are not connected
        // return a true, if either one of it intersects
        if (!EdgeConnected(t0, t1, edge.e0, edge.e1))
          if (VGMath.LinesIntersect(t_p0, t_p1, e_p0, e_p1)) return true;
        if (!EdgeConnected(t1, t2, edge.e0, edge.e1))
          if (VGMath.LinesIntersect(t_p1, t_p2, e_p0, e_p1)) return true;
        if (!EdgeConnected(t2, t0, edge.e0, edge.e1))
          if (VGMath.LinesIntersect(t_p2, t_p0, e_p0, e_p1)) return true;

        return false;
      }
    }
  }
}