using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;

namespace Voxell.GPUVectorGraphics
{
  public static partial class CubicBezier
  {
    public static void ComputeCubic(
      float2 p0, float2 p1, float2 p2, float2 p3,
      ref int vertexStart, ref NativeSlice<float2> vertexSlice,
      ref int coordsStart, ref NativeSlice<float3> coordsSlice,
      int recursiveType = -1
    )
    {
      float d0, d1, d2, d3;
      float3x4 coords = new float3x4();
      bool flip = false;
      // artifact on loop
      int errorLoop = -1;
      float splitParam = 0.0f;
      CurveType curveType = ClassifyCurve(p0, p1, p2, p3, out d0, out d1, out d2, out d3);
      Debug.Log(curveType);

      switch (curveType)
      {
        case CurveType.SERPENTINE:
          coords = Serpentine(d1, d2, d3, ref flip);
          break;

        case CurveType.LOOP:
          coords = Loop(d1, d2, d3, ref flip, ref errorLoop, ref splitParam, recursiveType);
          break;

        case CurveType.CUSP:
          coords = Cusp(d1, d2, d3);
          break;

        case CurveType.QUADRATIC:
          coords = Quadratic(d3, ref flip);
          break;

        default: return;
      }

      // recursive computation
      if(errorLoop != -1 && recursiveType == -1)
      {
        float2 p01 = (p1 - p0) * splitParam + p0;
        float2 p12 = (p2 - p1) * splitParam + p1;
        float2 p23 = (p3 - p2) * splitParam + p2;

        float2 p012 = (p12 - p01) * splitParam + p01;
        float2 p123 = (p23 - p12) * splitParam + p12;

        float2 p0123 = (p123 - p012) * splitParam + p012;

        if(errorLoop == 1) // flip second
        {
          Debug.Log("flip second");
          ComputeCubic(p0, p01, p012, p0123, ref vertexStart, ref vertexSlice, ref coordsStart, ref coordsSlice, 0);
          ComputeCubic(p0123, p123, p23, p3, ref vertexStart, ref vertexSlice, ref coordsStart, ref coordsSlice, 1);
        } else if(errorLoop == 2) // flip first
        {
          Debug.Log("flip first");
          ComputeCubic(p0, p01, p012, p0123, ref vertexStart, ref vertexSlice, ref coordsStart, ref coordsSlice, 1);
          ComputeCubic(p0123, p123, p23, p3, ref vertexStart, ref vertexSlice, ref coordsStart, ref coordsSlice, 0);
        }
        return;
      }

      if (recursiveType == 1) flip = !flip;
      if (flip)
      {
        coords[0].xy = -coords[0].xy;
        coords[1].xy = -coords[1].xy;
        coords[2].xy = -coords[2].xy;
        coords[3].xy = -coords[3].xy;
      }

      // triangulate
      Triangulate(
        new float2x4(p0, p1, p2, p3), coords,
        ref vertexStart, ref vertexSlice,
        ref coordsStart, ref coordsSlice
      );
    }
  }
}