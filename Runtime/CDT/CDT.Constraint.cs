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
      public NativeArray<float2> na_contour;
      public NativeArray<float2> na_points;
      public NativeList<int> na_triangles;

      public ConstraintJob(
        ref NativeArray<float2> na_contour,
        ref NativeArray<float2> na_points, ref NativeList<int> na_triangles
      )
      {
        this.na_contour = na_contour;
        this.na_points = na_points;
        this.na_triangles = na_triangles;
      }

      public void Execute()
      {
        
      }
    }
  }
}