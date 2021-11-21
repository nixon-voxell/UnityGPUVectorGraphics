using Unity.Mathematics;
using Unity.Collections;

namespace Voxell.GPUVectorGraphics
{
  public static partial class CubicBezier
  {
    internal static void Triangulate(
      float2x4 points, float3x4 coords,
      ref int vertexStart, ref NativeSlice<float2> vertexSlice,
      ref int coordsStart, ref NativeSlice<float3> coordsSlice
    )
    {
      // test for degenerate cases.
      for (int i=0; i < 4; i++)
      {
        for (int j=i + 1; j < 4; j++)
        {
          if (math.distance(points[i], points[j]) == 0.0f)
          {
            // Two of the points are coincident, so we can eliminate at
            // least one triangle. We might be able to eliminate the other
            // as well, but this seems sufficient to avoid degenerate triangulations.

            NativeArray<int> indices = new NativeArray<int>(3, Allocator.Temp);
            int index = 0;
            for (int k=0; k < 4; ++k)
              if (k != j) indices[index++] = k;

            CreateTriangleIndices(
              ref vertexStart, ref vertexSlice,
              ref coordsStart, ref coordsSlice,
              indices[0], indices[1], indices[2],
              points, coords
            );

            indices.Dispose();
            return;
          }
        }
      }

      // see whether any of the points are fully contained in the
      // triangle defined by the other three.
      for (int i=0; i < 4; ++i) 
      {
        NativeArray<int> indices = new NativeArray<int>(3, Allocator.Temp);
        int index = 0;
        for (int j=0; j < 4; ++j)
          if (i != j) indices[index++] = j;

        if (BezierMath.PointInTriangle(points[i], points[indices[0]], points[indices[1]], points[indices[2]]))
        {
          // produce three triangles surrounding this interior vertex.
          for (int j=0; j < 3; ++j)
          {
            CreateTriangleIndices(
              ref vertexStart, ref vertexSlice,
              ref coordsStart, ref coordsSlice,
              indices[j % 3], indices[(j + 1) % 3], i,
              points, coords
            );
          }

          indices.Dispose();
          return;
        }
      }

      // There are only a few permutations of the points, ignoring
      // rotations, which are irrelevant:

      //  0--3  0--2  0--3  0--1  0--2  0--1
      //  |  |  |  |  |  |  |  |  |  |  |  |
      //  |  |  |  |  |  |  |  |  |  |  |  |
      //  1--2  1--3  2--1  2--3  3--1  3--2

      // Note that three of these are reflections of each other.
      // Therefore there are only three possible triangulations:

      //  0--3  0--2  0--3
      //  |\ |  |\ |  |\ |
      //  | \|  | \|  | \|
      //  1--2  1--3  2--1

      // From which we can choose by seeing which of the potential
      // diagonals intersect. Note that we choose the shortest diagonal
      // to split the quad.
      if (BezierMath.LinesIntersect(points[0], points[2], points[1], points[3]))
      {
        if (math.lengthsq(points[2] - points[0]) < math.lengthsq(points[3] - points[1]))
        {
          CreateTriangleIndices(
            ref vertexStart, ref vertexSlice,
            ref coordsStart, ref coordsSlice,
            0, 1, 2, points, coords
          );
          CreateTriangleIndices(
            ref vertexStart, ref vertexSlice,
            ref coordsStart, ref coordsSlice,
            0, 2, 3, points, coords
          );
        } else
        {
          CreateTriangleIndices(
            ref vertexStart, ref vertexSlice,
            ref coordsStart, ref coordsSlice,
            0, 1, 3, points, coords
          );
          CreateTriangleIndices(
            ref vertexStart, ref vertexSlice,
            ref coordsStart, ref coordsSlice,
            1, 2, 3, points, coords
          );
        }
      } else if (BezierMath.LinesIntersect(points[0], points[3], points[1], points[2]))
      {
        if (math.lengthsq(points[3] - points[0]) < math.lengthsq(points[2] - points[1]))
        {
          CreateTriangleIndices(
            ref vertexStart, ref vertexSlice,
            ref coordsStart, ref coordsSlice,
            0, 1, 3, points, coords
          );
          CreateTriangleIndices(
            ref vertexStart, ref vertexSlice,
            ref coordsStart, ref coordsSlice,
            0, 3, 2, points, coords
          );
        } else
        {
          CreateTriangleIndices(
            ref vertexStart, ref vertexSlice,
            ref coordsStart, ref coordsSlice,
            0, 1, 2, points, coords
          );
          CreateTriangleIndices(
            ref vertexStart, ref vertexSlice,
            ref coordsStart, ref coordsSlice,
            2, 1, 3, points, coords
          );
        }
      } else
      {
        // Lines (0->1), (2->3) intersect -- or should, modulo numerical
        // precision issues
        if (math.lengthsq(points[1] - points[0]) < math.lengthsq(points[3] - points[2]))
        {
          CreateTriangleIndices(
            ref vertexStart, ref vertexSlice,
            ref coordsStart, ref coordsSlice,
            0, 2, 1, points, coords
          );
          CreateTriangleIndices(
            ref vertexStart, ref vertexSlice,
            ref coordsStart, ref coordsSlice,
            0, 1, 3, points, coords
          );
        } else
        {
          CreateTriangleIndices(
            ref vertexStart, ref vertexSlice,
            ref coordsStart, ref coordsSlice,
            0, 2, 3, points, coords
          );
          CreateTriangleIndices(
            ref vertexStart, ref vertexSlice,
            ref coordsStart, ref coordsSlice,
            3, 2, 1, points, coords
          );
        }
      }
    }

    private static void CreateTriangleIndices(
      ref int vertexStart, ref NativeSlice<float2> vertexSlice,
      ref int coordsStart, ref NativeSlice<float3> coordsSlice,
      int idx0, int idx1, int idx2, float2x4 points, float3x4 coords
    )
    {
      vertexSlice[vertexStart] = points[idx0];
      coordsSlice[vertexStart++] = coords[idx0];

      vertexSlice[vertexStart] = points[idx1];
      coordsSlice[vertexStart++] = coords[idx1];

      vertexSlice[vertexStart] = points[idx2];
      coordsSlice[vertexStart++] = coords[idx2];
    }
  }
}