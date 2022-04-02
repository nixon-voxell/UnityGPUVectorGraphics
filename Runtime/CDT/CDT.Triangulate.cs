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

        // create supra-triangle
        float2 rectDiff = maxRect - minRect;

        na_points[pointCount-3] = new float2(
          -2.0f*rectDiff.x + minRect.x - MARGIN, -2.0f*rectDiff.y + minRect.y - MARGIN
        );
        na_points[pointCount-2] = new float2(
          0.5f * rectDiff.x + minRect.x, 2.0f*rectDiff.y + minRect.y + MARGIN
        );
        na_points[pointCount-1] = new float2(
          2.0f*rectDiff.x + maxRect.x + MARGIN, -2.0f*rectDiff.y + minRect.y - MARGIN
        );
        AddTriangle(pointCount - 3, pointCount - 2, pointCount - 1, ref na_circumcenters);


        for (int p=0; p < pointCount; p++)
        {
          float2 point = na_points[p];
          na_edges.Clear();
          na_blackListedEdges.Clear();

          // remove triangles that contains other points in its circumcenter
          // and create new triangles out of it by connecting
          // each new edges to the current point
          int circumCount = na_circumcenters.Length;
          int removeCount = 0;
          for (int c=0; c < circumCount; c++)
          {
            Circumcenter circumcenter = na_circumcenters[c - removeCount];
            if (circumcenter.ContainsPoint(point))
            {
              int t0, t1, t2;
              GetTriangleIndices(c - removeCount, out t0, out t1, out t2);

              Edge edge;
              edge = new Edge(t0, t1);
              if (na_edges.Contains(edge))
              {
                if (!na_blackListedEdges.Contains(edge))
                  na_blackListedEdges.Add(edge);
              } else na_edges.Add(edge);

              edge = new Edge(t1, t2);
              if (na_edges.Contains(edge))
              {
                if (!na_blackListedEdges.Contains(edge))
                  na_blackListedEdges.Add(edge);
              } else na_edges.Add(edge);

              edge = new Edge(t2, t0);
              if (na_edges.Contains(edge))
              {
                if (!na_blackListedEdges.Contains(edge))
                  na_blackListedEdges.Add(edge);
              } else na_edges.Add(edge);

              RemoveTriangle(c - removeCount++, ref na_circumcenters);
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

          edgeCount = na_edges.Length;
          for (int e=0; e < edgeCount; e++)
          {
            Edge edge = na_edges[e];
            if (edge.e0 == p || edge.e1 == p) continue;
            float2 p0 = na_points[edge.e0];
            float2 p1 = na_points[edge.e1];

            if (VGMath.IsClockwise(in point, in p0, in p1)) AddTriangle(p, edge.e0, edge.e1, ref na_circumcenters);
            else AddTriangle(p, edge.e1, edge.e0, ref na_circumcenters);
          }
        }

        // remove supra-triangle
        for (int p = pointCount-3; p < pointCount; p++)
        {
          int circumCount = na_circumcenters.Length;
          int removeCount = 0;
          for (int c=0; c < circumCount; c++)
          {
            int t0, t1, t2;
            GetTriangleIndices(c - removeCount, out t0, out t1, out t2);
            if (t0 == p || t1 == p || t2 == p) RemoveTriangle(c - removeCount++, ref na_circumcenters);
          }
        }

        // dispose temp arrays
        na_edges.Dispose();
        na_blackListedEdges.Dispose();
        na_circumcenters.Dispose();
      }

      private void GetTriangleIndices(int idx, out int t0, out int t1, out int t2)
      {
        int tIdx = idx*3;
        t0 = na_triangles[tIdx];
        t1 = na_triangles[tIdx + 1];
        t2 = na_triangles[tIdx + 2];
      }

      private void AddTriangle(int t0, int t1, int t2, ref NativeList<Circumcenter> na_circumcenters)
      {
        na_triangles.Add(t0);
        na_triangles.Add(t1);
        na_triangles.Add(t2);
        float2 p0 = na_points[t0];
        float2 p1 = na_points[t1];
        float2 p2 = na_points[t2];
        na_circumcenters.Add(new Circumcenter(p0, p1, p2));
      }

      private void RemoveTriangle(int idx, ref NativeList<Circumcenter> na_circumcenters)
      {
        int tIdx = idx*3;
        na_triangles.RemoveRange(tIdx, 3);
        na_circumcenters.RemoveAt(idx);
      }
    }
  }
}