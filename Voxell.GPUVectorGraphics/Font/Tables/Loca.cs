// MIT License
// 
// Copyright (c) 2020 Pixel Precision LLC
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Collections.Generic;

namespace Voxell.GPUVectorGraphics.Font
{
  /// <summary>
  /// loca — Index to Location
  /// https://docs.microsoft.com/en-us/typography/opentype/spec/loca
  /// 
  /// The indexToLoc table stores the offsets to the locations of the glyphs
  /// in the font, relative to the beginning of the glyphData table. In order
  /// to compute the length of the last glyph element, there is an extra entry
  /// after the last valid index.
  /// </summary>
  public struct Loca
  {
    public const string TagName = "loca";

    // If int16:
    //      The actual local offset divided by 2 is stored. The value of n 
    //      is numGlyphs + 1. The value for numGlyphs is found in the 'maxp' table.
    // If Int32:
    //      The actual local offset is stored. The value of n is numGlyphs + 1.
    //      The value for numGlyphs is found in the 'maxp' table.
    public List<uint> offset;

    public void Read(FontReader r, int numGlyphs, bool longVer)
    {
      int readCt = numGlyphs + 1;
      this.offset = new List<uint>();

      if (longVer == false)
      {
        for (int i = 0; i < readCt; ++i)
          this.offset.Add( (uint)(r.ReadUInt16() * 2));
      }
      else
      {
        for (int i = 0; i < readCt; ++i)
          this.offset.Add( r.ReadUInt32());
      }
    }

    public int GetGlyphCount() => this.offset.Count - 1;

    public uint GetGlyphSize(int idx) => this.offset[idx + 1] - this.offset[idx];

    public uint GetGlyphOffset(Table tableGlyf, int idx) => tableGlyf.offset + this.offset[idx];
  }
}