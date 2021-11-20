using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace Voxell.GPUVectorGraphics
{
  internal static class BezierMath
  {
    /// <summary>Determines if a point is inside a triangle.</summary>
    internal static bool PointInTriangle(
      float2 point, float2 a, float2 b, float2 c
    )
    {
      float2 p0 = c - a;
      float2 p1 = b - a;
      float2 p2 = point - a;

      float dot00 = math.dot(p0, p0);
      float dot01 = math.dot(p0, p1);
      float dot02 = math.dot(p0, p2);
      float dot11 = math.dot(p1, p1);
      float dot12 = math.dot(p1, p2);
      float denominator = dot00 * dot11 - dot01 * dot01;

      // triangle has zero-area.
      // treat query point as not being inside.
      if (denominator == 0.0f) return false;

      // compute
      float inverseDenominator = 1.0f / denominator;
      float u = (dot11 * dot02 - dot01 * dot12) * inverseDenominator;
      float v = (dot00 * dot12 - dot01 * dot02) * inverseDenominator;

      return (u > 0.0f) && (v > 0.0f) && (u + v < 1.0f);
    }

    internal static bool LinesIntersect(float2 p1, float2 q1, float2 p2, float2 q2)
    {
      return (Orientation(p1, q1, p2) != Orientation(p1, q1, q2)
        && Orientation(p2, q2, p1) != Orientation(p2, q2, q1));
    }

    internal static int Orientation(float2 p1, float2 p2, float2 p3)
    {
      float crossProduct = (p2.y - p1.y) * (p3.x - p2.x) - (p3.y - p2.y) * (p2.x - p1.x);
      return (crossProduct < 0.0f) ? -1 : ((crossProduct > 0.0f) ? 1 : 0);
    }

    /// <summary>Forces both values to be within the range of -1 and 1.</summary>
    internal static void NormalizeTwo(ref float a, ref float b)
    {
      float max = math.abs(a);
      if (math.abs(b) > max) max = math.abs(b);

      a /= max;
      b /= max;
    }

    internal static void NormalizeCoords (ref float2 p0, ref float2 p1, ref float2 p2, ref float2 p3)
    {
      float maxx = MaxOutOfFour(p0.x, p1.x, p2.x, p3.x);
      float minx = MinOutOfFour(p0.x, p1.x, p2.x, p3.x);
      float maxy = MaxOutOfFour(p0.y, p1.y, p2.y, p3.y);
      float miny = MinOutOfFour(p0.y, p1.y, p2.y, p3.y);

      float2 span = new float2(maxx - minx, maxy - miny);

      p0 = (p0 - minx) / span;
      p1 = (p1 - minx) / span;
      p2 = (p2 - minx) / span;
      p3 = (p3 - minx) / span;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static float MaxOutOfFour(float x0, float x1, float x2, float x3)
      => math.max(math.max(math.max(x0, x1), x2), x3);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static float MinOutOfFour(float x0, float x1, float x2, float x3)
      => math.min(math.min(math.min(x0, x1), x2), x3);
  }
}