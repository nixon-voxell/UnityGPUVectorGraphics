using UnityEngine;
using Unity.Mathematics;

namespace Voxell.GPUVectorGraphics.Font
{
  [System.Serializable]
  public struct Glyph
  {
    [System.Serializable]
    public struct CompositeReference
    {
      /// <summary>The offset for the referenced glyph.</summary>
      public float2 offset;

      /// <summary>The X axis for the referenced glyph.</summary>
      public float2 xAxis;

      /// <summary>The Y axis for the referenced glyph.</summary>
      public float2 yAxis;

      /// <summary>The index of the glyph being referenced.</summary>
      public int glyphRef;
    }

    [System.Serializable]
    public struct GlyphContour
    {
      /// <summary>All points in the glyph.</summary>
      public float2[] points;

      /// <summary>Determine if a point is a control point.</summary>
      public bool[] isControls;
    }

    [Tooltip("All contours in the glyph.")]
    public GlyphContour[] glyphContours;

    [Tooltip("Composition of referenes of other glyphs.")]
    public CompositeReference[] compositeReferences;

    [Tooltip("Determines if this glyph is made up of other glyph or a glyph on its own.")]
    public bool isComplex;

    [Tooltip("Bottom left of the glyph's bounding box.")]
    public float2 minRect;
    [Tooltip("Top right of the glyph's bounding box.")]
    public float2 maxRect;
  }
}