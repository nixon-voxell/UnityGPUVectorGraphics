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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Voxell.GPUVectorGraphics.Font
{
  /// <summary>
  /// maxp — Maximum Profile
  /// https://docs.microsoft.com/en-us/typography/opentype/spec/maxp
  /// 
  /// This table establishes the memory requirements for this font. Fonts with CFF 
  /// data must use Version 0.5 of this table, specifying only the numGlyphs field. 
  /// Fonts with TrueType outlines must use Version 1.0 of this table, where all 
  /// data is required.
  /// </summary>
  public struct Maxp
  {
    public const string TagName = "maxp";

    public ushort majorVersion;
    public ushort minorVersion;
    public ushort numGlyphs;

    public ushort maxPoints;
    public ushort maxCountours;
    public ushort maxCompositePoints;
    public ushort maxCompositeContours;
    public ushort maxZones;
    public ushort maxTwilightPoints;
    public ushort maxStorage;
    public ushort maxFunctionDefs;
    public ushort maxIInstructionDefs;
    public ushort maxStackElements;
    public ushort maxSizeOfInstructions;
    public ushort maxComponentElements;
    public ushort maxComponentDepth;

    public void Read(FontReader r)
    {
      r.ReadInt(out this.majorVersion);
      r.ReadInt(out this.minorVersion);
      r.ReadInt(out this.numGlyphs);

      if (majorVersion == 0) return;

      r.ReadInt(out this.maxPoints);
      r.ReadInt(out this.maxCountours);
      r.ReadInt(out this.maxCompositePoints);
      r.ReadInt(out this.maxCompositeContours);
      r.ReadInt(out this.maxTwilightPoints);
      r.ReadInt(out this.maxStorage);
      r.ReadInt(out this.maxFunctionDefs);
      r.ReadInt(out this.maxIInstructionDefs);
      r.ReadInt(out this.maxStackElements);
      r.ReadInt(out this.maxSizeOfInstructions);
      r.ReadInt(out this.maxComponentElements);
      r.ReadInt(out this.maxComponentDepth);
    }
  }
}
