using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Voxell.Mathx;

namespace Voxell.GPUVectorGraphics
{
  // should it be static or partial?
  public static class Triangulator
  {
    // triangulate a given contour??
    public static void TriangulateContour()
    {
      // 
    }

    private struct Edge : System.IEquatable<Edge>
    {
      public int e0, e1;

      public Edge(int e0, int e1)
      {
        this.e0 = e0;
        this.e1 = e1;
      }

      public bool Equals(Edge other) => this.e0 == other.e0 && this.e1 == other.e1;
    }

    private struct Circumcenter
    {
      public float2 center;
      public float sqradius;

      public Circumcenter(float2 p0, float2 p1, float2 p2)
      {
        float dA = p0.x * p0.x + p0.y * p0.y;
        float dB = p1.x * p1.x + p1.y * p1.y;
        float dC = p2.x * p2.x + p2.y * p2.y;

        float aux1 = dA * (p2.y - p1.y) + dB * (p0.y - p2.y) + dC * (p1.y - p0.y);
        float aux2 = -(dA * (p2.x - p1.x) + dB * (p0.x - p2.x) + dC * (p1.x - p0.x));
        float div = 2.0f * (p0.x * (p2.y - p1.y) + p1.x * (p0.y - p2.y) + p2.x * (p1.y - p0.y));
        div += math.EPSILON;
        div = 1.0f / div;

        center = new float2(aux1, aux2) * div;
        sqradius = math.lengthsq(p0 - center);
      }

      public bool ContainsPoint(float2 p)
      {
        float sqlength = math.lengthsq(p - center);
        return sqlength < sqradius;
      }
    }

    [BurstCompile]
    private struct TriangulateJob : IJob
    {
      public NativeList<float2> na_points;
      public NativeList<int> na_triangles;
      public NativeHashSet<Edge> na_edges;
      public NativeList<Circumcenter> na_circumcenters;

      public TriangulateJob(
        float2 minRect, float2 maxRect,
        ref NativeList<float2> na_points,
        ref NativeList<int> na_triangles,
        ref NativeHashSet<Edge> na_edges,
        ref NativeList<Circumcenter> na_circumcenters
      )
      {
        float2 p0 = new float2(minRect);
        float2 p1 = new float2(0.0f, maxRect.y);
        float2 p2 = new float2(maxRect);
        float2 p3 = new float2(maxRect.x, 0.0f);

        this.na_points = na_points;
        this.na_triangles = na_triangles;
        this.na_edges = na_edges;
        this.na_circumcenters = na_circumcenters;
        na_points.Add(p0);
        na_points.Add(p1);
        na_points.Add(p2);
        na_points.Add(p3);

        AddTriangle(0, 1, 2);
        AddTriangle(0, 2, 3);
      }

      public void Execute()
      {
        int pointCount = na_points.Length-4;
        for (int p=0; p < pointCount; p++)
        {
          float2 point = na_points[p];
          na_edges.Clear();

          // remove triangles that contains other points and
          // and create new triangles out of it by connecting
          // each new edge to the current point
          int circumCount = na_circumcenters.Length;
          int removeCount = 0;
          for (int c=0; c < circumCount; c++)
          {
            Circumcenter circumcenter = na_circumcenters[c - removeCount];
            if (circumcenter.ContainsPoint(point))
            {
              int t0, t1, t2;
              GetTriangleIndices(c, out t0, out t1, out t2);

              if (t0 != p && t1 != p) na_edges.Add(new Edge(t0, t1));
              if (t1 != p && t2 != p) na_edges.Add(new Edge(t1, t2));
              if (t2 != p && t0 != p) na_edges.Add(new Edge(t2, t0));

              RemoveTriangle(c - removeCount++);
            }
          }

          foreach (Edge edge in na_edges)
            AddTriangle(p, edge.e0, edge.e1);
        }
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

      private void GetTrianglePoints(int idx, out float2 p0, out float2 p1, out float2 p2)
      {
        int t0, t1, t2;
        GetTriangleIndices(idx, out t0, out t1, out t2);

        p0 = na_points[t0];
        p1 = na_points[t1];
        p2 = na_points[t2];
      }

      private void GetTriangleIndices(int idx, out int t0, out int t1, out int t2)
      {
        int tIdx = idx*3;
        t0 = na_triangles[tIdx];
        t1 = na_triangles[tIdx + 1];
        t2 = na_triangles[tIdx + 2];
      }

      private void RemoveTriangle(int idx)
      {
        int tIdx = idx*3;
        na_triangles.RemoveRange(tIdx, 3);
        na_circumcenters.RemoveAt(idx);
      }
    }
  }
}
