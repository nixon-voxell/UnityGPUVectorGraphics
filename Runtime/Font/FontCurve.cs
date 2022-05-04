using System.Runtime.CompilerServices;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;

namespace Voxell.GPUVectorGraphics.Font
{
  public class FontCurve : ScriptableObject
  {
    public const float ENLARGE = 100.0f;
    public const float INV_ENLARGE = 1/ENLARGE;
    public Glyph[] Glyphs => _glyphs;
    public int[] CharCodes => _charCodes;
    public int[] GlyphIndices => _glyphIndices;

    /// <summary>Bezier contour for each character.</summary>
    [SerializeField, NonReorderable] private Glyph[] _glyphs;

    /// <summary>Character codes.</summary>
    [SerializeField, NonReorderable] private int[] _charCodes;
    /// <summary>Glyph index of the corresponding character.</summary>
    [SerializeField, NonReorderable] private int[] _glyphIndices;

    public void Initialize(Glyph[] glyphs, int[] charCodes, int[] glyphIndices)
    {
      _glyphs = glyphs;
      _charCodes = charCodes;
      _glyphIndices = glyphIndices;
    }

    /// <summary>Search for the glyph index of a certain character through binary search.</summary>
    /// <returns>0 if character does not exsists, else index of the character's glyph.</returns>
    public int SearchGlyhIndex(char character)
    {
      int __len = _charCodes.Length;
      int __first = 0;
      int __half;
      int __middle;

      // binary search
      while (__len > 0)
      {
        __half = __len >> 1;
        __middle = __first + __half;

        int midCode = _charCodes[__middle];
        if (character == midCode) return _glyphIndices[__middle];

        // search first half if it is lower than the mid point
        if (character < midCode)
          __len = __half;
        // search second half if it is higher than the mid point
        else
        {
          __first = __middle + 1;
          __len = __len - __half - 1;
        }
      }

      // index of "square" glyph (a glyph repersenting that the character is not supported)
      return 0;
    }

    /// <summary>Get character glyph through binary search.</summary>
    /// <returns>
    /// First glyph if character is not supported,
    /// else the glyph representing the shape of the character
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Glyph GetCharacterGlyph(char character) => _glyphs[SearchGlyhIndex(character)];

    /// <summary>Convert glyphs into points and contours.</summary>
    public static void ExtractGlyphData(
      in Glyph glyph, out float2[] points, out CDT.ContourPoint[] contours
    )
    {
      int contourCount = glyph.contours.Length;
      List<float2> pointList = new List<float2>();
      List<CDT.ContourPoint> contourList = new List<CDT.ContourPoint>();

      int contourStart = 0;
      for (int c=0; c < contourCount; c++)
      {
        QuadraticContour glyphContour = glyph.contours[c];
        int segmentCount = glyphContour.segments.Length;

        for (int s=0; s < segmentCount; s++)
        {
          pointList.Add(glyphContour.segments[s].p0 * ENLARGE);
          contourList.Add(new CDT.ContourPoint(contourStart+s, c));
        }
        contourList.Add(new CDT.ContourPoint(contourStart, c));
        contourStart += segmentCount;
      }

      points = pointList.ToArray();
      contours = contourList.ToArray();
    }

    public static Vector3[] PointsToVertices(in NativeArray<float2> points)
    {
      Vector3[] vertices = new Vector3[points.Length];
      for (int p=0; p < points.Length; p++)
      {
        float2 point = points[p] * INV_ENLARGE;
        vertices[p] = new Vector3(point.x, point.y, 0.0f);
      }

      return vertices;
    }
  }
}