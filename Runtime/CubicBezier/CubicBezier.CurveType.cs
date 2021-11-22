using Unity.Mathematics;

namespace Voxell.GPUVectorGraphics
{
  public static partial class CubicBezier
  {
    internal static readonly float SQRT3 = math.sqrt(3.0f);
    internal static readonly float DET_SQRT3 = 1/SQRT3;

    internal const float ONE_THIRD = 1.0f/3.0f;
    internal const float TWO_THIRDS = 2.0f/3.0f;

    internal enum CurveType
    {
      UNKNOWN = 0,
      SERPENTINE = 1,
      LOOP = 2,
      CUSP = 3,
      QUADRATIC = 4,
      LINE = 5
    }

    internal static CurveType ClassifyCurve(
      float2 p0, float2 p1, float2 p2, float2 p3,
      out float d0, out float d1, out float d2, out float d3
    )
    {
      float3 b0 = new float3(p0, 1.0f);
      float3 b1 = new float3(p1, 1.0f);
      float3 b2 = new float3(p2, 1.0f);
      float3 b3 = new float3(p3, 1.0f);

      float a1 = math.dot(b0, math.cross(b3, b2));
      float a2 = math.dot(b1, math.cross(b0, b3));
      float a3 = math.dot(b2, math.cross(b1, b0));

      d0 = 0.0f;
      d1 = a1 - 2.0f * a2 + 3.0f * a3;
      d2 = -a2 + 3.0f * a3;
      d3 = 3.0f * a3;

      float D = 3.0f * d2 * d2 - 4.0f * d1 * d3;
      float disc = d1 * d1 * D;

      if (disc == 0.0f)
      {
        if (d1 == 0.0f && d2 == 0.0f)
        {
          if (d3 == 0.0f) return CurveType.LINE;
          return CurveType.QUADRATIC;
        }

        if (d1 != 0.0f) return CurveType.CUSP;
        if (D < 0.0f) return CurveType.LOOP;

        return CurveType.SERPENTINE;
      }

      if (disc > 0.0f) return CurveType.SERPENTINE;
      else return CurveType.LOOP;
    }

    public static float3x4 Serpentine(float d1, float d2, float d3, ref bool flip)
    {
      float t1 = math.sqrt(9.0f * d2 * d2 - 12 * d1 * d3);
      float ls = 3.0f * d2 - t1;
      float lt = 6.0f * d1;
      float ms = 3.0f * d2 + t1;
      float mt = lt;
      float ltMinusLs = lt - ls;
      float mtMinusMs = mt - ms;

      float3x4 coords = new float3x4();
      coords.c0.x = ls * ms;
      coords.c0.y = ls * ls * ls;
      coords.c0.z = ms * ms * ms;

      coords.c1.x = ONE_THIRD * (3.0f * ls * ms - ls * mt - lt * ms);
      coords.c1.y = ls * ls * (ls - lt);
      coords.c1.z = ms * ms * (ms - mt);

      coords.c2.x = ONE_THIRD * (lt * (mt - 2.0f * ms) + ls * (3.0f * ms - 2.0f * mt));
      coords.c2.y = ltMinusLs * ltMinusLs * ls;
      coords.c2.z = mtMinusMs * mtMinusMs * ms;

      coords.c3.x = ltMinusLs * mtMinusMs;
      coords.c3.y = -(ltMinusLs * ltMinusLs * ltMinusLs);
      coords.c3.z = -(mtMinusMs * mtMinusMs * mtMinusMs);

      flip = d1 < 0.0f;
      return coords;
    }

    public static float3x4 Loop(
      float d1, float d2, float d3, ref bool flip,
      ref int loopArtifact, ref float splitParam, int recursiveType
    )
    {
      float t1 = math.sqrt(4.0f * d1 * d3 - 3.0f * d2 * d2);
      float ls = d2 - t1;
      float lt = 2.0f * d1;
      float ms = d2 + t1;
      float mt = lt;

      // Figure out whether there is a rendering artifact requiring
      // the curve to be subdivided by the caller.
      float ql = ls / lt;
      float qm = ms / mt;
      if (0.0f < ql && ql < 1.0f) 
      {
        loopArtifact = 1;
        splitParam = ql;
      }

      if (0.0f < qm && qm < 1.0f) 
      {
        loopArtifact = 2;
        splitParam = qm;
      }

      float ltMinusLs = lt - ls;
      float mtMinusMs = mt - ms;

      float3x4 coords = new float3x4();
      coords.c0.x = ls * ms;
      coords.c0.y = ls * ls * ms;
      coords.c0.z = ls * ms * ms;

      coords.c1.x = ONE_THIRD * (-ls * mt - lt * ms + 3.0f * ls * ms);
      coords.c1.y = -ONE_THIRD * ls * (ls * (mt - 3.0f * ms) + 2.0f * lt * ms);
      coords.c1.z = -ONE_THIRD * ms * (ls * (2.0f * mt - 3.0f * ms) + lt * ms);

      coords.c2.x = ONE_THIRD * (lt * (mt - 2.0f * ms) + ls * (3.0f * ms - 2.0f * mt));
      coords.c2.y = ONE_THIRD * (lt - ls) * (ls * (2.0f * mt - 3.0f * ms) + lt * ms);
      coords.c2.z = ONE_THIRD * (mt - ms) * (ls * (mt - 3.0f * ms) + 2.0f * lt * ms);

      coords.c3.x = ltMinusLs * mtMinusMs;
      coords.c3.y = -(ltMinusLs * ltMinusLs) * mtMinusMs;
      coords.c3.z = -ltMinusLs * mtMinusMs * mtMinusMs;

      if (recursiveType == -1)
        flip = (d1 > 0.0f && coords.c0.x < 0.0f) || (d1 < 0.0f && coords.c0.x > 0.0f);
      return coords;
    }

    public static float3x4 Cusp(float d1, float d2, float d3)
    {
      float ls = d3;
      float lt = 3.0f * d2;
      float lsMinusLt = ls - lt;

      float3x4 coords = new float3x4();
      coords.c0.x = ls;
      coords.c0.y = ls * ls * ls;
      coords.c0.z = 1.0f;

      coords.c1.x = ls - TWO_THIRDS * lt;
      coords.c1.y = ls * ls * lsMinusLt;
      coords.c1.z = 1.0f;

      coords.c2.x = ls - TWO_THIRDS * lt;
      coords.c2.y = lsMinusLt * lsMinusLt * ls;
      coords.c2.z = 1.0f;

      coords.c3.x = lsMinusLt;
      coords.c3.y = lsMinusLt * lsMinusLt * lsMinusLt;
      coords.c3.z = 1.0f;

      return coords;
    }

    public static float3x4 Quadratic(float d3, ref bool flip)
    {
      float3x4 coords = new float3x4(
        0.0f,       0.0f,      0.0f,
        ONE_THIRD,  0.0f,      ONE_THIRD,
        TWO_THIRDS, ONE_THIRD, TWO_THIRDS,
        1.0f,       1.0f,      1.0f
      );

      flip = d3 < 0.0f;
      return coords;
    }
  }
}