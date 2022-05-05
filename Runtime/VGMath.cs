using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace Voxell.GPUVectorGraphics
{
  internal static class VGMath
  {
    /// <summary>Determines if a point is inside a triangle.</summary>
    /// <remarks>
    /// https://stackoverflow.com/questions/2049582/how-to-determine-if-a-point-is-in-a-2d-triangle
    /// </remarks>
    internal static bool PointInTriangle(float2 p, float2 p0, float2 p1, float2 p2)
    {
      float d1, d2, d3;
      bool hasNeg, hasPos;

      d1 = VertEdgeSign(p, p0, p1);
      d2 = VertEdgeSign(p, p1, p2);
      d3 = VertEdgeSign(p, p2, p0);

      hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
      hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);

      return !(hasNeg && hasPos);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static float VertEdgeSign(float2 p, float2 p0, float2 p1)
      => (p.x - p1.x) * (p0.y - p1.y) - (p0.x - p1.x) * (p.y - p1.y);

    internal static bool LinesIntersect(float2 p1, float2 q1, float2 p2, float2 q2)
    {
      return (Orientation(p1, q1, p2) != Orientation(p1, q1, q2)
        && Orientation(p2, q2, p1) != Orientation(p2, q2, q1));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int Orientation(float2 p1, float2 p2, float2 p3)
    {
      float crossProduct = (p2.y - p1.y) * (p3.x - p2.x) - (p3.y - p2.y) * (p2.x - p1.x);
      return (crossProduct < 0.0f) ? -1 : ((crossProduct > 0.0f) ? 1 : 0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsClockwise(in float2 p0, in float2 p1, in float2 p2)
    {
      return (p1.x - p0.x) * (p2.y - p0.y) - (p2.x - p0.x) * (p1.y - p0.y) < 0.0f;
    }
  }
}