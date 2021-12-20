using UnityEngine;

namespace Voxell.GPUVectorGraphics.Font
{
  public class FontCurve : ScriptableObject
  {
    public Glyph[] Glyphs => _glyphs;

    /// <summary>Bezier contour for each character.</summary>
    [SerializeField] private Glyph[] _glyphs;

    /// <summary>Mapping index for each characters.</summary>
    [SerializeField] private int[] _charMap;

    public void Initialize(Glyph[] glyphs, int[] charMap)
    {
      _glyphs = glyphs;
      _charMap = charMap;
    }

    /// <summary>Try obtaining the glyph index of a certain character.</summary>
    /// <returns>-1 if character does not exsists, else index of the character's glyph.</returns>
    public int TryGetGlyhIndex(char character)
    {
      int index = -1;
      if (character < _charMap.Length) index = _charMap[character];
      return index;
    }
  }
}