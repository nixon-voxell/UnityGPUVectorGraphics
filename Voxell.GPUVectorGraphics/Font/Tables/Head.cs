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

namespace Voxell.GPUVectorGraphics.Font
{
  /// <summary>
  /// head — Font Header Table
  /// https://docs.microsoft.com/en-us/typography/opentype/spec/head
  /// 
  /// This table gives global information about the font. The bounding box values 
  /// should be computed using only glyphs that have contours. Glyphs with no 
  /// contours should be ignored for the purposes of these calculations.
  /// </summary>
  public struct Head
  {
    // A different style than the rest of the flags, but if they don't
    // give me text to explicitly copy, it's going to be a flag enum
    // with matching style.
    [System.Flags]
    public enum Flags
    {
      Baseline                = (1 << 0),
      LeftSidebearing         = (1 << 1),
      InstrDepOnPtSz          = (1 << 2),
      ForcePPEMIntForScalar   = (1 << 3),
      InstrMayAlterAdvWd      = (1 << 4),
      Unused                  = (1 << 5),
      Lossless                = (1 << 11),
      Converted               = (1 << 12),
      Optimized               = (1 << 13),
      LastResort              = (1 << 14),
    }

    [System.Flags]
    public enum MacStyle
    {
      Bold                    = (1 << 0),
      Italic                  = (1 << 1),
      Underline               = (1 << 2),
      Outline                 = (1 << 3),
      Shadow                  = (1 << 4),
      Condensed               = (1 << 5),
      Extended                = (1 << 6)
    }

    public const string TagName = "head";

    // See member head.magicNumber.
    public const int ExpectedMagicNumber = 0x5F0F3CF5;

    public ushort majorVersion;             // Major version number of the font header table — set to 1.
    public ushort minorVersion;             // Minor version number of the font header table — set to 0.
    public float fontRevision;              // Set by font manufacturer.
    public uint checksumAdjustment;         // To compute: set it to 0, sum the entire font as uint32, then store 0xB1B0AFBA - sum. If the font is used as a component in a font collection file, the value of this field will be invalidated by changes to the file structure and font table directory, and must be ignored.
    public uint magicNumber;                //  Set to 0x5F0F3CF5.
    public ushort flags;                    
    public ushort unitsPerEm;               // Set to a value from 16 to 16384. Any value in this range is valid. In fonts that have TrueType outlines, a power of 2 is recommended as this allows performance optimizations in some rasterizers.
    public System.DateTime created;         // Number of seconds since 12:00 midnight that started January 1st 1904 in GMT/UTC time zone.
    public System.DateTime modified;        // Number of seconds since 12:00 midnight that started January 1st 1904 in GMT/UTC time zone.
    public short xMin;                      // For all glyph bounding boxes. (Glyphs without contours are ignored.)
    public short yMin;                      // For all glyph bounding boxes. (Glyphs without contours are ignored.)
    public short xMax;                      // For all glyph bounding boxes. (Glyphs without contours are ignored.)
    public short yMax;                      // For all glyph bounding boxes. (Glyphs without contours are ignored.)
    public ushort macStyle;                 // 
    public ushort lowestRecPPEM;            // Smallest readable size in pixels.
    public ushort fontDirectionHint;        // 
    public short indexToLocFormat;          // 0 for short offsets (Offset16), 1 for long (Offset32).
    public short glyphDataFormat;           // 0 for current format.

    public int OffsetByteWidth
    {
      get
      {
        switch (this.indexToLocFormat)
        {
          case 0:
            return 2;   // Offset16 / uint16
          case 1:
            return 4;   // Offset32 / uint32
        }

        return 0;
      }

    }

    public void Read(FontReader r)
    {
      r.ReadInt(out this.majorVersion);
      r.ReadInt(out this.minorVersion);
      this.fontRevision = r.ReadFixed();
      r.ReadInt(out this.checksumAdjustment);
      r.ReadInt(out this.magicNumber);
      r.ReadInt(out this.flags);
      r.ReadInt(out this.unitsPerEm);
      this.created = r.ReadDate();
      this.modified = r.ReadDate();
      r.ReadInt(out this.xMin);
      r.ReadInt(out this.yMin);
      r.ReadInt(out this.xMax);
      r.ReadInt(out this.yMax);
      r.ReadInt(out this.macStyle);
      r.ReadInt(out this.lowestRecPPEM);
      r.ReadInt(out this.fontDirectionHint);
      r.ReadInt(out this.indexToLocFormat);
      r.ReadInt(out this.glyphDataFormat);
    }
  }
}