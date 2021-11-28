using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace Voxell.GPUVectorGraphics
{
  internal static class DelaunayMath
  {
    internal static void Circumcircle(
      float2 p0, float2 p1, float2 p2,
      out float2 circumcenter, out float squaredRadius
    )
    {
      float dA = math.dot(p0, p0);
      float dB = math.dot(p1, p1);
      float dC = math.dot(p2, p2);

      float aux1 = (dA * (p2.y - p1.y) + dB * (p0.y - p2.y) + dC * (p1.y - p0.y));
      float aux2 = -(dA * (p2.x - p1.x) + dB * (p0.x - p2.x) + dC * (p1.x - p0.x));
      float div = (2 * (p0.x * (p2.y - p1.y) + p1.x * (p0.y - p2.y) + p2.x * (p1.y - p0.y)));
      div += math.EPSILON;

      circumcenter = new float2(aux1/div, aux2/div);
      float2 diff = circumcenter - p0;
      squaredRadius = math.dot(diff, diff);
    }

    internal static bool PointInCircumcircle(float2 circumcenter, float squaredRadius, float2 point)
    {
      float squaredDistance = (point.x - circumcenter.x) * (point.x - circumcenter.x) +
        (point.y - circumcenter.y) * (point.y - circumcenter.y);
      return squaredDistance < squaredRadius;
    }

    internal static bool IsCounterClockwise(float2 p0, float2 p1, float2 p2)
    {
      return (p1.x - p0.x) * (p2.y - p0.y) - (p2.x - p0.x) * (p1.y - p0.y) > 0;
    }
  }
}