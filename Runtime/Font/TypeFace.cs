using UnityEngine;

namespace Voxell.GPUVectorGraphics.Font
{
  [System.Serializable]
  public struct TypeFace
  {
    [Tooltip("Name of the type face.")]
    public string name;

    [Tooltip("Bezier contour for each character.")]
    public Glyph[] glyphs;
  }
}