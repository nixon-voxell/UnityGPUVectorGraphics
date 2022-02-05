using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Voxell.Mathx;

namespace Voxell.GPUVectorGraphics
{
  public partial class DelaunayTriangulation
  {
    // this is just for testing purposes, in practice,
    // we will be scheduling multiple jobs for every unique shape
    // and dispose the native list ourselves
    public static void Triangulate(
      float2 minRect, float2 maxRect,
      ref NativeList<float2> na_points, ref NativeList<int> na_triangles
    )
    {
      NativeList<Edge> na_edges = new NativeList<Edge>(Allocator.TempJob);
      NativeList<Edge> na_blackListedEdges = new NativeList<Edge>(Allocator.TempJob);
      NativeList<Circumcenter> na_circumcenters = new NativeList<Circumcenter>(Allocator.TempJob);

      TriangulateJob triangulateJob = new TriangulateJob(
        minRect, maxRect,
        ref na_points, ref na_triangles,
        ref na_edges, ref na_blackListedEdges, ref na_circumcenters
      );
      JobHandle jobHandle = triangulateJob.Schedule();
      jobHandle.Complete();

      na_edges.Dispose();
      na_blackListedEdges.Dispose();
      na_circumcenters.Dispose();
    }

    /// <summary>Bowyer-Watson delaunay triangulation.</summary>
    [BurstCompile]
    private struct TriangulateJob : IJob
    {
      private const float MARGIN = 1.0f;
      public NativeList<float2> na_points;
      public NativeList<int> na_triangles;
      public NativeList<Edge> na_edges;
      public NativeList<Edge> na_blackListedEdges;
      public NativeList<Circumcenter> na_circumcenters;

      public TriangulateJob(
        float2 minRect, float2 maxRect,
        ref NativeList<float2> na_points,
        ref NativeList<int> na_triangles,
        ref NativeList<Edge> na_edges,
        ref NativeList<Edge> na_blackListedEdges,
        ref NativeList<Circumcenter> na_circumcenters
      )
      {
        float2 rectDiff = maxRect - minRect;
        float2 p0 = new float2(-2.0f*rectDiff.x + minRect.x - MARGIN, -2.0f*rectDiff.y + minRect.y - MARGIN);
        float2 p1 = new float2(0.5f * rectDiff.x + minRect.x, 2.0f*rectDiff.y + minRect.y + MARGIN);
        float2 p2 = new float2(2.0f*rectDiff.x + maxRect.x + MARGIN, -2.0f*rectDiff.y + minRect.y - MARGIN);

        this.na_points = na_points;
        int originPointCount = na_points.Length;
        this.na_triangles = na_triangles;
        this.na_edges = na_edges;
        this.na_blackListedEdges = na_blackListedEdges;
        this.na_circumcenters = na_circumcenters;
        na_points.Add(p0);
        na_points.Add(p1);
        na_points.Add(p2);
        // na_points.Add(p3);

        AddTriangle(originPointCount, originPointCount + 1, originPointCount + 2);
        // AddTriangle(originPointCount, originPointCount + 2, originPointCount + 3);
      }

      public void Execute()
      {
        int p = 0;
        int pointCount = na_points.Length;
        int originPointCount = pointCount-3;
        for (; p < pointCount; p++)
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

              RemoveTriangle(c - removeCount++);
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

            if (IsFacingFront(in point, in p0, in p1)) AddTriangle(p, edge.e0, edge.e1);
            else AddTriangle(p, edge.e1, edge.e0);
          }
        }

        // remove triangles that are related to the points of the "super trianlges"
        for (p = originPointCount; p < pointCount; p++)
        {
          int circumCount = na_circumcenters.Length;
          int removeCount = 0;
          for (int c=0; c < circumCount; c++)
          {
            int t0, t1, t2;
            GetTriangleIndices(c - removeCount, out t0, out t1, out t2);
            if (t0 == p || t1 == p || t2 == p) RemoveTriangle(c - removeCount++);
          }
        }
        na_points.RemoveRange(pointCount-3, 3);
      }

      private void GetTriangleIndices(int idx, out int t0, out int t1, out int t2)
      {
        int tIdx = idx*3;
        t0 = na_triangles[tIdx];
        t1 = na_triangles[tIdx + 1];
        t2 = na_triangles[tIdx + 2];
      }

      private void AddTriangle(int t0, int t1, int t2)
      {
        na_triangles.Add(t0);
        na_triangles.Add(t1);
        na_triangles.Add(t2);
        float2 p0 = na_points[t0];
        float2 p1 = na_points[t1];
        float2 p2 = na_points[t2];
        na_circumcenters.Add(new Circumcenter(p0, p1, p2));
      }

      private void RemoveTriangle(int idx)
      {
        int tIdx = idx*3;
        na_triangles.RemoveRange(tIdx, 3);
        na_circumcenters.RemoveAt(idx);
      }

      private bool IsFacingFront(in float2 p0, in float2 p1, in float2 p2)
      {
        float3 direction = math.cross(new float3(p1 - p0, 0.0f), new float3(p2 - p0, 0.0f));
        return math.dot(direction, float3x.back) > 0.0f;
      }
    }
  }
}