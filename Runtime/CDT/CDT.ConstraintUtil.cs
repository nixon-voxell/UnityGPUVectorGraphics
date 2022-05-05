using Unity.Mathematics;
using Unity.Collections;

namespace Voxell.GPUVectorGraphics
{
  public partial class CDT
  {
    /// <summary>
    /// Find the other triangle that is connected to this edge by looking up
    /// at a hash map from any of the point related to the edge.
    /// </summary>
    private static void FindEdgeTriangleAndExtraPoint(
      in NativeMultiHashMap<int, int>.Enumerator enumerator,
      in NativeList<int> na_triangles,
      in Edge edge, out int2 tris, out int2 extraPoints)
    {
      int foundCount = 0;
      tris = new int2(-1, -1);
      extraPoints = new int2(-1, -1);

      // UnityEngine.Debug.Log($"edge: {edge.e0}:{edge.e1}");
      int t0, t1, t2;
      foreach (int t in enumerator)
      {
        GetTriangleIndices(in na_triangles, t, out t0, out t1, out t2);
        Edge edge0 = new Edge(t0, t1);
        Edge edge1 = new Edge(t1, t2);
        Edge edge2 = new Edge(t2, t0);

        if (edge.Equals(edge0) || edge.Equals(edge1) || edge.Equals(edge2))
        {
          if (foundCount == 2)
          {
            UnityEngine.Debug.Log($"{foundCount}: {t0}, {t1}, {t2}");
            UnityEngine.Debug.Log($"edge: {edge.e0}:{edge.e1}");
          }
          tris[foundCount] = t;

          // find the odd one out (the point that is not related to the given edge)
          if (t0 != edge.e0 && t0 != edge.e1) extraPoints[foundCount] = t0;
          else if (t1 != edge.e0 && t1 != edge.e1) extraPoints[foundCount] = t1;
          else extraPoints[foundCount] = t2;

          foundCount++;
        }
      }
    }

    /// <summary>
    /// Find the other triangle that is connected to this edge by looking up
    /// at a hash map from any of the point related to the edge.
    /// </summary>
    private static void FindEdgeTriangles(
      in NativeMultiHashMap<int, int>.Enumerator enumerator,
      in NativeList<int> na_triangles,
      in Edge edge, out int2 tris)
    {
      int foundCount = 0;
      tris = new int2(-1, -1);

      // UnityEngine.Debug.Log($"edge: {edge.e0}:{edge.e1}");
      int t0, t1, t2;
      foreach (int t in enumerator)
      {
        GetTriangleIndices(in na_triangles, t, out t0, out t1, out t2);
        Edge edge0 = new Edge(t0, t1);
        Edge edge1 = new Edge(t1, t2);
        Edge edge2 = new Edge(t2, t0);

        if (edge.Equals(edge0) || edge.Equals(edge1) || edge.Equals(edge2))
        {
          if (foundCount == 2)
          {
            UnityEngine.Debug.Log($"{foundCount}: {t0}, {t1}, {t2}");
            UnityEngine.Debug.Log($"edge: {edge.e0}:{edge.e1}");
          }
          tris[foundCount++] = t;
        }
      }
    }

    /// <summary>Checks if an edge intersects a triangle.</summary>
    /// <param name="na_points">point pool</param>
    /// <param name="tIdx">triangle index</param>
    /// <param name="edge">edge indices</param>
    /// <param name="ePoints">2 points that makes up the edge</param>
    /// <param name="diff_t">if triangle point is part of the edge</param>
    /// <param name="tPoints">triangle points</param>
    private static bool TriEdgeIntersect(
      in NativeArray<float2> na_points,
      in int3 tIdx, in Edge edge, in float2x2 ePoints,
      out bool3 diff_t, out float2x3 tPoints
    )
    {
      diff_t = new bool3();
      tPoints = new float2x3();
      for (int i=0; i < 3; i++)
      {
        tPoints[i] = na_points[tIdx[i]];
        // compare point position rather than index
        // as there might be duplicated points with different index
        bool same = (tPoints[i].Equals(ePoints[0]) || tPoints[i].Equals(ePoints[1]));
        diff_t[i] = !same;
      }

      // only check for edge intersection when both edge are not connected
      // return a true, if either one of it intersects
      for (int i=0; i < 3; i++)
      {
        int nextIdx = (i + 1) % 3;
        if (diff_t[i] && diff_t[nextIdx])
          if (VGMath.LinesIntersect(tPoints[i], tPoints[nextIdx], ePoints[0], ePoints[1]))
            return true;
      }

      return false;
    }
  }
}