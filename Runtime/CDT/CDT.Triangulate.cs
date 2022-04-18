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
        int pointCount = na_points.Length;

        // create temp arrays
        NativeList<Edge> na_edges = new NativeList<Edge>(Allocator.Temp);
        NativeList<Edge> na_blackListedEdges = new NativeList<Edge>(Allocator.Temp);
        NativeList<Circumcenter> na_circumcenters = new NativeList<Circumcenter>(Allocator.Temp);

        // create rect-triangle
        float2 marginedMinRect = minRect - MARGIN;
        float2 marginedMaxRect = maxRect + MARGIN;

        na_points[pointCount-4] = marginedMinRect;
        na_points[pointCount-3] = new float2(marginedMinRect.x, marginedMaxRect.y);
        na_points[pointCount-2] = marginedMaxRect;
        na_points[pointCount-1] = new float2(marginedMaxRect.x, marginedMinRect.y);

        AddTriAndCircum(
          in na_points, ref na_triangles, ref na_circumcenters,
          pointCount-4, pointCount-3, pointCount-2
        );
        AddTriAndCircum(
          in na_points, ref na_triangles, ref na_circumcenters,
          pointCount-4, pointCount-2, pointCount-1
        );

        for (int p=0, originPointCount=pointCount-4; p < originPointCount; p++)
        {
          na_edges.Clear();
          na_blackListedEdges.Clear();

          float2 point = na_points[p];

          // remove triangles that contains the current point in its circumcenter
          int circumCount = na_circumcenters.Length;
          int removeCount = 0;
          for (int c=0; c < circumCount; c++)
          {
            Circumcenter circumcenter = na_circumcenters[c-removeCount];
            if (circumcenter.ContainsPoint(point))
            {
              int t0, t1, t2;
              GetTriangleIndices(in na_triangles, c-removeCount, out t0, out t1, out t2);

              Edge edge = new Edge(t0, t1);
              if (na_edges.Contains(edge))
              {
                if (!na_blackListedEdges.Contains(edge))
                  na_blackListedEdges.Add(edge);
              } else na_edges.Add(edge);

              edge.SetEdge(t1, t2);
              if (na_edges.Contains(edge))
              {
                if (!na_blackListedEdges.Contains(edge))
                  na_blackListedEdges.Add(edge);
              } else na_edges.Add(edge);

              edge.SetEdge(t2, t0);
              if (na_edges.Contains(edge))
              {
                if (!na_blackListedEdges.Contains(edge))
                  na_blackListedEdges.Add(edge);
              } else na_edges.Add(edge);

              RemoveTriAndCircum(ref na_circumcenters, ref na_triangles, c-removeCount++);
            }
          }

          int edgeCount;
          int blackListedEdgeCount = na_blackListedEdges.Length;
          for (int b=0; b < blackListedEdgeCount; b++)
          {
            Edge blackListedEdge = na_blackListedEdges[b];
            edgeCount = na_edges.Length;

            for (int e=0; e < edgeCount; e++)
            {
              if (na_edges[e].Equals(blackListedEdge))
              {
                na_edges.RemoveAt(e);
                break;
              }
            }
          }

          // create new triangles out of it by connecting
          // each new edges to the current point
          edgeCount = na_edges.Length;
          for (int e=0; e < edgeCount; e++)
          {
            Edge edge = na_edges[e];
            if (edge.e0 == p || edge.e1 == p) continue;
            float2 p0 = na_points[edge.e0];
            float2 p1 = na_points[edge.e1];

            if (VGMath.IsClockwise(in point, in p0, in p1))
              AddTriAndCircum(
                in na_points, ref na_triangles, ref na_circumcenters, p, edge.e0, edge.e1
              );
            else
              AddTriAndCircum(
                in na_points, ref na_triangles, ref na_circumcenters, p, edge.e1, edge.e0
              );
          }
        }

        // remove rect-triangle
        for (int p=pointCount-4; p < pointCount; p++)
        {
          int circumCount = na_circumcenters.Length;
          int removeCount = 0;
          for (int c=0; c < circumCount; c++)
          {
            int t0, t1, t2;
            GetTriangleIndices(in na_triangles, c-removeCount, out t0, out t1, out t2);
            if (t0 == p || t1 == p || t2 == p)
              RemoveTriAndCircum(ref na_circumcenters, ref na_triangles, c-removeCount++);
          }
        }

        // dispose temp arrays
        na_edges.Dispose();
        na_blackListedEdges.Dispose();
        na_circumcenters.Dispose();
      }
    }
  }
}