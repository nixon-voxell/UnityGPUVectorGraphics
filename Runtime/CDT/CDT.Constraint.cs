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
          GetTriangleIndices(in na_triangles, t, out t0, out t1, out t2);
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

        NativeList<int> na_outsideIndices = new NativeList<int>(2, Allocator.Temp);
        NativeList<int> na_insideIndices = new NativeList<int>(2, Allocator.Temp);
        NativeList<int> na_blackListedTris = new NativeList<int>(Allocator.Temp);

        NativeList<int> na_repairTriangles = new NativeList<int>(Allocator.Temp);
        NativeList<Edge> na_repairEdges = new NativeList<Edge>(Allocator.Temp);
        NativeList<Edge> na_blackListedRepairEdges = new NativeList<Edge>(Allocator.Temp);
        NativeList<Circumcenter> na_repairCircumcenters = new NativeList<Circumcenter>(Allocator.Temp);

        int segmentCount = na_contours.Length - 1;
        for (int s=0; s < segmentCount; s++)
        {
          na_insideIndices.Clear();
          na_outsideIndices.Clear();
          na_blackListedTris.Clear();

          ContourPoint c0 = na_contours[s];
          ContourPoint c1 = na_contours[s + 1];
          // only check for intersection if both points are in the same contour
          if (c0.contourIdx != c1.contourIdx) continue;

          int e0 = c0.pointIdx;
          int e1 = c1.pointIdx;

          // if edge already exist, continue to the next contour segment
          Edge edge = new Edge(e0, e1);
          if (na_edges.Contains(edge)) continue;

          // initialize point list with edge points
          na_outsideIndices.Add(e0); na_outsideIndices.Add(e1);
          na_insideIndices.Add(e0); na_insideIndices.Add(e1);
          // initialize min and max point with edge points
          float2x2 ePoints = new float2x2(na_points[e0], na_points[e1]);
          float2 minRect = math.min(ePoints[0], ePoints[1]);
          float2 maxRect = math.max(ePoints[0], ePoints[1]);
          // remove all blocking triangles
          int triangleCount = na_triangles.Length/3;
          for (int t=0; t < triangleCount; t++)
          {
            int3 tIdx;
            GetTriangleIndices(in na_triangles, t, out tIdx.x, out tIdx.y, out tIdx.z);

            bool3 diff_t;
            float2x3 tPoints;
            if (TriEdgeIntersect(in na_points, in tIdx, in edge, in ePoints, out diff_t, out tPoints))
            {
              // black list triangle to be removed later
              na_blackListedTris.Add(t);

              // sort points into outside and inside regions
              float2 n = math.normalize(float2x.perpendicular(ePoints[1] - ePoints[0]));
              for (int i=0; i < 3; i++)
              {
                if (diff_t[i])
                {
                  minRect = math.min(minRect, na_points[tIdx[i]]);
                  maxRect = math.max(maxRect, na_points[tIdx[i]]);
                  if (math.dot(n, tPoints[i] - ePoints[0]) < 0.0f) na_insideIndices.Add(tIdx[i]);
                  else na_outsideIndices.Add(tIdx[i]);
                }
              }
            }
          }

          // remove all blacklisted triangles
          int blackListedTriCount = na_blackListedTris.Length;
          int removeCount = 0;
          for (int t=0; t < blackListedTriCount; t++)
            RemoveTriangle(ref na_triangles, na_blackListedTris[t]-removeCount++);

          // retriangulate inside points
          if (na_insideIndices.Length > 2)
          {
            TriangulatePoints(
              in minRect, in maxRect, in na_insideIndices,
              ref na_repairTriangles, ref na_repairEdges, ref na_blackListedRepairEdges,
              ref na_repairCircumcenters
            );
          }

          // retriangulate outside points
          if (na_outsideIndices.Length > 2)
          {
            TriangulatePoints(
              in minRect, in maxRect, in na_outsideIndices,
              ref na_repairTriangles, ref na_repairEdges, ref na_blackListedRepairEdges,
              ref na_repairCircumcenters
            );
          }
        }

        // disposing all temp allocations
        na_edges.Dispose();

        na_outsideIndices.Dispose();
        na_insideIndices.Dispose();
        na_blackListedTris.Dispose();

        na_repairTriangles.Dispose();
        na_repairEdges.Dispose();
        na_blackListedRepairEdges.Dispose();
        na_repairCircumcenters.Dispose();
      }

      /// <summary>Delaunay triangulate a portion of points defined by an indices array.</summary>
      /// <param name="minRect">min AABB point</param>
      /// <param name="maxRect">max AABB point</param>
      /// <param name="na_indices">indices array indicating the portion of points to be triangulated</param>
      private void TriangulatePoints(
        in float2 minRect, in float2 maxRect, in NativeList<int> na_indices,
        ref NativeList<int> na_repairTriangles,
        ref NativeList<Edge> na_repairEdges,
        ref NativeList<Edge> na_blackListedRepairEdges,
        ref NativeList<Circumcenter> na_repairCircumcenters
      )
      {
        na_repairTriangles.Clear();
        na_repairCircumcenters.Clear();
        int pointCount = na_points.Length;
        int idxCount = na_indices.Length;

        // create rect-triangle
        float2 marginedMinRect = minRect - MARGIN;
        float2 marginedMaxRect = maxRect + MARGIN;

        na_points[pointCount-4] = marginedMinRect;
        na_points[pointCount-3] = new float2(marginedMinRect.x, marginedMaxRect.y);
        na_points[pointCount-2] = marginedMaxRect;
        na_points[pointCount-1] = new float2(marginedMaxRect.x, marginedMinRect.y);

        AddTriAndCircum(
          in na_points, ref na_repairTriangles, ref na_repairCircumcenters,
          pointCount-4, pointCount-3, pointCount-2
        );
        AddTriAndCircum(
          in na_points, ref na_repairTriangles, ref na_repairCircumcenters,
          pointCount-4, pointCount-2, pointCount-1
        );

        for (int i=0; i < idxCount; i++)
        {
          na_repairEdges.Clear();
          na_blackListedRepairEdges.Clear();

          int pIdx = na_indices[i];
          float2 point = na_points[pIdx];

          // remove triangles that contains the current point in its circumcenter
          int circumCount = na_repairCircumcenters.Length;
          int removeCount = 0;
          for (int c=0; c < circumCount; c++)
          {
            Circumcenter circumcenter = na_repairCircumcenters[c - removeCount];
            if (circumcenter.ContainsPoint(point))
            {
              int t0, t1, t2;
              GetTriangleIndices(in na_repairTriangles, c-removeCount, out t0, out t1, out t2);

              Edge edge = new Edge(t0, t1);
              if (na_repairEdges.Contains(edge))
              {
                if (!na_blackListedRepairEdges.Contains(edge))
                  na_blackListedRepairEdges.Add(edge);
              } else na_repairEdges.Add(edge);

              edge.SetEdge(t1, t2);
              if (na_repairEdges.Contains(edge))
              {
                if (!na_blackListedRepairEdges.Contains(edge))
                  na_blackListedRepairEdges.Add(edge);
              } else na_repairEdges.Add(edge);

              edge.SetEdge(t2, t0);
              if (na_repairEdges.Contains(edge))
              {
                if (!na_blackListedRepairEdges.Contains(edge))
                  na_blackListedRepairEdges.Add(edge);
              } else na_repairEdges.Add(edge);

              RemoveTriAndCircum(ref na_repairCircumcenters, ref na_repairTriangles, c-removeCount++);
            }
          }

          int edgeCount;
          int blackListedEdgeCount = na_blackListedRepairEdges.Length;
          for (int b=0; b < blackListedEdgeCount; b++)
          {
            Edge blackListedEdge = na_blackListedRepairEdges[b];
            edgeCount = na_repairEdges.Length;

            for (int e=0; e < edgeCount; e++)
            {
              if (na_repairEdges[e].Equals(blackListedEdge))
              {
                na_repairEdges.RemoveAt(e);
                break;
              }
            }
          }

          // create new triangles out of it by connecting
          // each new edges to the current point
          edgeCount = na_repairEdges.Length;
          for (int e=0; e < edgeCount; e++)
          {
            Edge edge = na_repairEdges[e];
            if (edge.e0 == pIdx || edge.e1 == pIdx) continue;
            float2 p0 = na_points[edge.e0];
            float2 p1 = na_points[edge.e1];

            if (VGMath.IsClockwise(in point, in p0, in p1))
              AddTriAndCircum(
                in na_points, ref na_repairTriangles, ref na_repairCircumcenters, pIdx, edge.e0, edge.e1
              );
            else
              AddTriAndCircum(
                in na_points, ref na_repairTriangles, ref na_repairCircumcenters, pIdx, edge.e1, edge.e0
              );
          }
        }

        // remove rect-triangle
        for (int p=pointCount-4; p < pointCount; p++)
        {
          int triCount = na_repairTriangles.Length/3;
          int removeCount = 0;
          for (int t=0; t < triCount; t++)
          {
            int t0, t1, t2;
            GetTriangleIndices(in na_repairTriangles, t-removeCount, out t0, out t1, out t2);
            if (t0 == p || t1 == p || t2 == p)
              RemoveTriangle(ref na_repairTriangles, t-removeCount++);
          }
        }

        na_triangles.AddRange(na_repairTriangles);
      }
    }
  }
}