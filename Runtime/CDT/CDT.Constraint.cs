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
      public NativeArray<int> na_contour;
      public NativeArray<float2> na_points;
      public NativeList<int> na_triangles;

      public ConstraintJob(
        ref NativeArray<int> na_contour,
        ref NativeArray<float2> na_points, ref NativeList<int> na_triangles
      )
      {
        this.na_contour = na_contour;
        this.na_points = na_points;
        this.na_triangles = na_triangles;
      }

      public void Execute()
      {
        int segmentCount = na_contour.Length - 1;

        for (int s=0; s < segmentCount; s++)
        {
          int c0 = na_contour[s];
          int c1 = na_contour[s + 1];
        }
      }

      private void TriangleContainsEdge(int idx, ref NativeList<Circumcenter> na_circumcenters)
      {
        int tIdx = idx*3;
        na_triangles.RemoveRange(tIdx, 3);
        na_circumcenters.RemoveAt(idx);
      }
    }
  }
}