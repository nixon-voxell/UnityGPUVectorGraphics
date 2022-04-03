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

        NativeList<int> na_outsidePoints = new NativeList<int>(Allocator.Temp);
        NativeList<int> na_insidePoints = new NativeList<int>(Allocator.Temp);
        NativeList<int> na_blackListedTris = new NativeList<int>(Allocator.Temp);

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

          // remove all blocking triangles
          int triangleCount = na_triangles.Length/3;
          for (int t=0; t < triangleCount; t++)
          {
            int3 tIdx;
            GetTriangleIndices(ref na_triangles, t, out tIdx.x, out tIdx.y, out tIdx.z);

            bool3 diff_t;
            float2x3 tPoints;
            float2x2 ePoints;
            if (TriEdgeIntersect(in tIdx, in edge, out diff_t, out tPoints, out ePoints))
            {
              // black list triangle to be removed later
              na_blackListedTris.Add(t);

              // sort points into outside and inside regions
              float2 n = math.normalize(float2x.perpendicular(ePoints[1] - ePoints[0]));
              for (int i=0; i < 3; i++)
              {
                if (diff_t[i])
                {
                  if (math.dot(n, tPoints[i] - ePoints[0]) > 0.0f) na_outsidePoints.Add(tIdx[i]);
                  else na_insidePoints.Add(tIdx[i]);
                }
              }
            }
          }

          // remove all blacklisted triangles
          int blackListedTriCount = na_blackListedTris.Length;
          int removeCount = 0;
          for (int t=0; t < blackListedTriCount; t++)
            RemoveTriangle(ref na_triangles, na_blackListedTris[t]-removeCount++);

          // retriangulate outside points

          // retriangulate inside points

          na_insidePoints.Clear();
          na_outsidePoints.Clear();
          na_blackListedTris.Clear();
        }

        na_edges.Dispose();
        na_outsidePoints.Dispose();
        na_insidePoints.Dispose();
        na_blackListedTris.Dispose();
      }

      private bool TriEdgeIntersect(
        in int3 tIdx, in Edge edge,
        out bool3 diff_t,
        out float2x3 tPoints,
        out float2x2 ePoints
      )
      {
        int e0 = edge.e0, e1 = edge.e1;
        ePoints = new float2x2(na_points[e0], na_points[e1]);

        diff_t = new bool3();
        tPoints = new float2x3();
        for (int i=0; i < 3; i++)
        {
          diff_t[i] = !(tIdx[i] == e0 || tIdx[i] == e1);
          tPoints[i] = na_points[tIdx[i]];
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
}