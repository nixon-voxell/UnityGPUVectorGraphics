using Unity.Mathematics;
using Unity.Collections;

namespace Voxell.GPUVectorGraphics
{
  public partial class CDT
  {
    /// <summary>Create rect-triangles using the last 4 elements of the point array.</summary>
    private static void CreateRectTriangle(
      in float2 minRect, in float2 maxRect,
      ref NativeArray<float2> na_points,
      ref NativeList<int> na_triangles,
      ref NativeList<Circumcenter> na_circumcenters
    )
    {
      int pointCount = na_points.Length;
      float2 marginedMinRect = minRect - MARGIN;
      float2 marginedMaxRect = maxRect + MARGIN;
      int r0 = pointCount-4;
      int r1 = pointCount-3;
      int r2 = pointCount-2;
      int r3 = pointCount-1;

      na_points[r0] = marginedMinRect;
      na_points[r1] = new float2(marginedMinRect.x, marginedMaxRect.y);
      na_points[r2] = marginedMaxRect;
      na_points[r3] = new float2(marginedMaxRect.x, marginedMinRect.y);

      AddTriAndCircum(in na_points, ref na_triangles, ref na_circumcenters, r0, r1, r2);
      AddTriAndCircum(in na_points, ref na_triangles, ref na_circumcenters, r0, r2, r3);
    }

    /// <summary>Remove all triangles associated with the rect-triangle.</summary>
    private static void RemoveRectTriangle(in int pointCount, ref NativeList<int> na_triangles)
    {
      for (int p=pointCount-4; p < pointCount; p++)
      {
        int triangleCount = na_triangles.Length/3;
        int removeCount = 0;
        for (int t=0; t < triangleCount; t++)
        {
          int idx = t - removeCount;
          int t0, t1, t2;
          GetTriangleIndices(in na_triangles, idx, out t0, out t1, out t2);

          if (t0 == p || t1 == p || t2 == p)
          {
            RemoveTriangle(ref na_triangles, idx);
            removeCount++;
          }
        }
      }
    }

    /// <summary>Create new triangles out of a new point by connecting each new edges to it.</summary>
    /// <param name="pointIdx">index of the point</param>
    /// <param name="point">point location</param>
    private static void CreateTrianglesForNewPoint(
      in int pointIdx, in float2 point,
      in NativeList<Edge> na_edges, in NativeArray<float2> na_points,
      ref NativeList<int> na_triangles, ref NativeList<Circumcenter> na_circumcenters
    )
    {
      int edgeCount = na_edges.Length;
      for (int e=0; e < edgeCount; e++)
      {
        Edge edge = na_edges[e];
        if (edge.e0 == pointIdx || edge.e1 == pointIdx) continue;
        float2 p0 = na_points[edge.e0];
        float2 p1 = na_points[edge.e1];

        if (VGMath.IsClockwise(in point, in p0, in p1))
          AddTriAndCircum(in na_points, ref na_triangles, ref na_circumcenters, pointIdx, edge.e0, edge.e1);
        else
          AddTriAndCircum(in na_points, ref na_triangles, ref na_circumcenters, pointIdx, edge.e1, edge.e0);
      }
    }

    /// <summary>
    /// Add edges obtained from a triangle that is going to be removed
    /// because its circumcircle contains a point.
    /// </summary>
    /// <param name="na_edges">a list of all added edges</param>
    /// <param name="na_blackListedEdges">
    /// a list of indices of duplicated edges to be removed
    /// </param>
    private static void AddEdgesOfRemovedTriangle(
      in Edge edge, ref NativeList<Edge> na_edges,
      ref NativeList<int> na_blackListedEdges
    )
    {
      if (na_edges.Contains(edge))
      {
        int edgeIdx = na_edges.IndexOf(edge);
        if (!na_blackListedEdges.Contains(edgeIdx))
          na_blackListedEdges.Add(edgeIdx);
      } else na_edges.Add(edge);
    }

    /// <summary>Remove black listed triangles.</summary>
    /// <param name="na_blackListedTris">black listed triangle indices in ascending order</param>
    private static void RemoveBlacklistedTriangles(
      in NativeList<int> na_blackListedTris, ref NativeList<int> na_triangles
    )
    {
      int blacklistedTriCount = na_blackListedTris.Length;
      int removeCount = 0;

      for (int t=0; t < blacklistedTriCount; t++)
        RemoveTriangle(ref na_triangles, na_blackListedTris[t]-removeCount++);
    }

    /// <summary>Remove black listed edges.</summary>
    /// <param name="na_blackListedTris">black listed edge indices in ascending order</param>
    private static void RemoveBlacklistedEdges(
      in NativeList<int> na_blackListedEdges, ref NativeList<Edge> na_edges
    )
    {
      int blackListedEdgeCount = na_blackListedEdges.Length;
      int removeCount = 0;

      for (int b=0; b < blackListedEdgeCount; b++)
        na_edges.RemoveAt(na_blackListedEdges[b]-removeCount++);
    }
  }
}