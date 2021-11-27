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

// Attempts were made to apply as much of the Microsoft documentation to this
// implementation with as little effort as possible. If the implementation wasn't
// straight-forward and the documentation says the format isn't popular, it's
// probably left incomplete in this source.
// (wleu 12/16/2020)

namespace Voxell.GPUVectorGraphics.Font
{
  /// <summary>
  /// cmap — Character to Glyph Index Mapping Table
  /// https://docs.microsoft.com/en-us/typography/opentype/spec/cmap
  /// 
  /// This table defines the mapping of character codes to the glyph index 
  /// values used in the font. It may contain more than one subtable, in order 
  /// to support more than one character encoding scheme.
  /// </summary>
  public struct CMap
  {
    /// <summary>
    /// If a font includes encoding records for Unicode subtables of the same 
    /// format but with different platform IDs, an application may choose which 
    /// to select, but should make this selection consistently each time the 
    /// font is used.
    /// </summary>
    public enum PlatformID
    {
      UnicodeVarious = 0,    // Various
      Macintosh      = 1,    // Script manager code
      ISO            = 2,    // ISO encoding [deprecated]
      Windows        = 3,    // Windows encoding
      Custom         = 4     // Custom
    }

    /// <summary>
    /// Unicode platform (platform ID = 0)
    /// The following encoding IDs are defined for use with the Unicode platform.
    /// </summary>
    public enum UniPlatform
    {
      Unicode0        = 0,    // Unicode 1.0 semantics—deprecated
      Unicode1_1      = 1,    // Unicode 1.1 semantics—deprecated
      ISOIEC10646     = 2,    // ISO/IEC 10646 semantics—deprecated
      Unicode2BMP     = 3,    // Unicode 2.0 and onwards semantics, Unicode BMP only
      Unicode2Full    = 4,    // Unicode 2.0 and onwards semantics, Unicode full repertoire
      UnicodeVarSub   = 5,    // Unicode Variation Sequences—for use with subtable format 14
      UnicodeVarFull  = 6     // Unicode full repertoire—for use with subtable format 13

    }

    // Macintosh platform(platform ID = 1)
    // Older Macintosh versions required fonts to have a 'cmap' subtable for platform ID 1. 
    // For current Apple platforms, use of platform ID 1 is discouraged.See the 'name' table 
    // chapter for details regarding encoding IDs defined for the Macintosh platform.

    // ISO platform (platform ID = 2)
    // Note: use of this platform ID is deprecated.
    // The following encoding IDs are defined for use with the ISO platform.
    public enum ISOPlatform
    {
      ISO7bit     = 0,       // 7-bit ASCII
      ISO10646    = 1,       // ISO 10646
      ISO8859_1   = 2	       // ISO 8859-1
    }

    // Windows platform (platform ID = 3)
    // The Windows platform supports several encodings. When creating fonts for Windows, 
    // Unicode 'cmap' subtables should always be used—platform ID 3 with encoding ID 1 or 
    // encoding ID 10. See below for additional details.
    public enum WindowsPlatform
    {
      Symbol          = 0,	 // Symbol
      UnicodeBMP      = 1,	 // Unicode BMP
      ShiftJIS        = 2,	 // ShiftJIS
      PRC             = 3,	 // PRC
      Big5            = 4,	 // Big5
      Wansung         = 5,	 // Wansung
      Johab           = 6,	 // Johab
      Reserved_7      = 7,	 // Reserved
      Reserved_8      = 8,	 // Reserved
      Reserved_9      = 9,	 // Reserved
      Unicode         = 10,	 // Unicode full repertoire
    }

    /// <summary>
    /// Each sequential map group record specifies a character range and the starting 
    /// glyph ID mapped from the first character. Glyph IDs for subsequent characters 
    /// follow in sequence.
    /// </summary>
    public struct SequentialMapGroup
    {
      public uint startCharCode;          // First character code in this group; note that if this group is for one or more 16-bit character codes (which is determined from the is32 array), this 32-bit value will have the high 16-bits set to zero
      public uint endCharCode;            // Last character code in this group; same condition as listed above for the startCharCode
      public uint startGlyphID;           // Glyph index corresponding to the starting character code

      public void Read(FontReader r)
      { 
        r.ReadInt(out this.startCharCode);
        r.ReadInt(out this.endCharCode);
        r.ReadInt(out this.startGlyphID);
      }
    }

    /// <summary>
    /// The constant map group record has the same structure as the sequential map group 
    /// record, with start and end character codes and a mapped glyph ID. However, the 
    /// same glyph ID applies to all characters in the specified range rather than sequential 
    /// glyph IDs.
    /// </summary>
    public struct ConstantMapGroup
    { 
      // It's similarity to the SequeltialMapGroup is not lost on me - but if I'm
      // dedicated towards implementing the font parsing off the documentation, I 
      // guess this is what we're doing. (wleu 12/16/2020)

      public uint startCharCode;          // First character code in this group
      public uint endCharCode;            // Last character code in this group
      public uint glyphID;                // Glyph index to be used for all the characters in the group’s range.

      public void Read(FontReader r)
      { 
        r.ReadInt(out this.startCharCode);
        r.ReadInt(out this.endCharCode);
        r.ReadInt(out this.glyphID);
      }
    }

    public interface CharacterConversionMap
    {
      Dictionary<uint, uint> MapCodeToIndex(FontReader r);
      int Format();
    }

    /// <summary>
    /// Format 0 was the standard mapping subtable used on older Macintosh 
    /// platforms but is not required on newer Apple platforms.
    /// </summary>
    public struct Format0 : CharacterConversionMap
    {
      // https://docs.microsoft.com/en-us/typography/opentype/spec/cmap#format-0-byte-encoding-table

      public ushort format;               // Format number is set to 0.
      public ushort length;              // This is the length in bytes of the subtable.
      public ushort language;            // For requirements on use of the language field, see “Use of the language field in 'cmap' subtables” in this document.
      public List<byte> glyphIdArray;     // An array that maps character codes to glyph index values.

      public void Read(FontReader r, bool readformat = false)
      { 
        if (readformat == true)
          r.ReadInt(out this.format);
        else
          this.format = 0;

        r.ReadInt(out this.length);
        r.ReadInt(out this.language);

        this.glyphIdArray = new List<byte>();
        for(int i = 0; i < 256; ++i)
          this.glyphIdArray.Add(r.ReadUInt8());
      }

      Dictionary<uint, uint> CharacterConversionMap.MapCodeToIndex(FontReader r)
      { 
        Dictionary<uint, uint> ret = new Dictionary<uint, uint>();

        for(uint i = 0; i < 256; ++i)
          ret[i] = this.glyphIdArray[(int)i];

        return ret;
      }

      int CharacterConversionMap.Format() => 0;
    }

    /// <summary>
    /// This subtable format was created for “double-byte” encodings following 
    /// the national character code standards used for Japanese, Chinese, and 
    /// Korean characters. These code standards use a mixed 8-/16-bit encoding. 
    /// This format is not commonly used today.
    /// </summary>
    public struct Format2 : CharacterConversionMap
    {
      // https://docs.microsoft.com/en-us/typography/opentype/spec/cmap#format-2-high-byte-mapping-through-table

      public struct SubHeader
      { 
        public ushort firstCode;
        public ushort entryCount;
        public short idDelta;
        public ushort idRangeOffset;

        public void Read(FontReader r)
        {
          r.ReadInt(out this.firstCode);
          r.ReadInt(out this.entryCount);
          r.ReadInt(out this.idDelta);
          r.ReadInt(out this.idRangeOffset);
        }
      }

      public ushort format;                  // Format number is set to 2.
      public ushort length;                  // This is the length in bytes of the subtable.
      public ushort language;                // For requirements on use of the language field, see “Use of the language field in 'cmap' subtables” in this document.
      public List<ushort> subHeaderKeys;     // Array that maps high bytes to subHeaders: value is subHeader index × 8.
      public List<SubHeader> subHeaders;     // Variable-length array of SubHeader records.
      public List<ushort> glyphIdArray;      // Variable-length array containing subarrays used for mapping the low byte of 2-byte characters.

      public void Read(FontReader r, bool readformat = false)
      {
        if (readformat == true)
          r.ReadInt(out this.format);
        else
          this.format = 2;

        r.ReadInt(out this.length);
        r.ReadInt(out this.language);

        this.subHeaderKeys = new List<ushort>();
        for(int i = 0; i < 256; ++i)
          this.subHeaderKeys.Add( r.ReadUInt16());

        // !NOTE: This parser is unfinished - don't feel like decrypting the documentation
        // on how to get the sizes for this.
        // this.subHeaders = new List<SubHeader>();
        // 
        // this.glyphIdArray = new List<ushort>();
      }

      Dictionary<uint, uint> CharacterConversionMap.MapCodeToIndex(FontReader r)
      {
        // !UNIMPLEMENTED
        Dictionary<uint, uint> ret = new Dictionary<uint, uint>();
        return ret;
      }

      int CharacterConversionMap.Format() => 2;
    }

    /// <summary>
    /// This is the standard character-to-glyph-index mapping subtable for 
    /// fonts that support only Unicode Basic Multilingual Plane characters 
    /// (U+0000 to U+FFFF).
    /// </summary>
    public struct Format4 : CharacterConversionMap
    {
      // https://docs.microsoft.com/en-us/typography/opentype/spec/cmap#format-4-segment-mapping-to-delta-values

      public ushort format;               // Format number is set to 4.
      public ushort length;               // This is the length in bytes of the subtable.
      public ushort language;             // For requirements on use of the language field, see “Use of the language field in 'cmap' subtables” in this document.
      public ushort segCountX2;           // 2 × segCount.
      public ushort searchRange;          // Maximum power of 2 less than or equal to segCount, times 2 ((2**floor(log2(segCount))) * 2, where “**” is an exponentiation operator)
      public ushort entrySelector;        // Log2 of the maximum power of 2 less than or equal to numTables (log2(searchRange/2), which is equal to floor(log2(numTables)))
      public ushort rangeShift;           // segCount times 2, minus searchRange ((segCount * 2) - searchRange)
      public List<ushort> endCode;        // End characterCode for each segment, last=0xFFFF.
      public ushort reservePad;           // Set to 0.
      public List<ushort> startCode;      // Start character code for each segment.
      public List<short> idDelta;        // Delta for all character codes in segment.
      public List<uint> idRangeOffsets; // Offsets into glyphIdArray or 0

      //public List<ushort> glyphIdArray;   // Glyph index array (arbitrary length)

      public void Read(FontReader r, bool readformat = false)
      { 
        if (readformat == true)
          r.ReadInt(out this.format);
        else
          this.format = 4;

        r.ReadInt(out this.length);
        r.ReadInt(out this.language);
        r.ReadInt(out this.segCountX2);
        r.ReadInt(out this.searchRange);
        r.ReadInt(out this.entrySelector);
        r.ReadInt(out this.rangeShift);

        uint segCt = (uint)this.segCountX2/2;

        this.endCode = new List<ushort>();
        for(int i = 0; i < segCt; ++i)
          this.endCode.Add(r.ReadUInt16());

        r.ReadInt(out this.reservePad);

        this.startCode = new List<ushort>();
        for(int i = 0; i < segCt; ++i)
          this.startCode.Add(r.ReadUInt16());

        this.idDelta = new List<short>();
        for(int i = 0; i < segCt; ++i)
          this.idDelta.Add(r.ReadInt16());

        this.idRangeOffsets = new List<uint>();
        for(int i = 0; i < segCt; ++i)
        {
          uint ro = r.ReadUInt16();

          if (ro != 0)
          {
            // glyphId = *(idRangeOffset[i] / 2
            //              + (c - startCode[i])
            //              + &idRangeOffset[i])
            uint addr = (uint)r.GetPosition() - 2;
            ro = addr + ro;
          }
          this.idRangeOffsets.Add(ro);
        }
      }

      Dictionary<uint, uint> CharacterConversionMap.MapCodeToIndex(FontReader r)
      { 
        Dictionary<uint, uint> ret = new Dictionary<uint, uint>();

        uint segCt = (uint)this.segCountX2 / 2;
        for (int i = 0; i < segCt; ++i)
        {
          // https://gist.github.com/smhanov/f009a02c00eb27d99479a1e37c1b3354

          int start = this.startCode[i];
          int end = this.endCode[i];
          if (start == 0xffff || end == 0xffff)
            break;

          uint ro = this.idRangeOffsets[i];

          if (ro == 0)
          {
            for (int j = start; j <= end; ++j)
              ret.Add((uint)j, (uint)(j + this.idDelta[i]));
          }
          else
          {
            r.SetPosition(ro);
            for (int j = start; j <= end; ++j)
              ret.Add((uint)j, r.ReadUInt16());
          }
        }

        return ret;
      }

      int CharacterConversionMap.Format() => 4;
    }

    /// <summary>
    /// Format 6 was designed to map 16-bit characters to glyph indexes when the character codes 
    /// for a font fall into a single contiguous range.
    /// </summary>
    public struct Format6 : CharacterConversionMap
    {
      // https://docs.microsoft.com/en-us/typography/opentype/spec/cmap#format-6-trimmed-table-mapping

      public ushort format;
      public ushort length;
      public ushort language;
      public ushort firstCode;
      public ushort entryCount;
      public List<ushort> glyphIdArray;

      public void Read(FontReader r, bool readformat = false)
      { 
        if (readformat == true)
          r.ReadInt(out this.format);
        else
          this.format = 6;

        r.ReadInt(out this.length);
        r.ReadInt(out this.firstCode);
        r.ReadInt(out this.entryCount);

        const int knownTableSz = 
          2 + // format
          2 + // length
          2 + // language
          2 + // firstCode
          2; // entryCount

        int glyphsCt = (length - knownTableSz)/2;
        this.glyphIdArray = new List<ushort>();
        for (int i = 0; i < glyphsCt; ++i)
          this.glyphIdArray.Add( r.ReadUInt16());
      }

      Dictionary<uint, uint> CharacterConversionMap.MapCodeToIndex(FontReader r)
      {
        Dictionary<uint, uint> ret = new Dictionary<uint, uint>();

        for(uint i = 0; i < (uint)this.glyphIdArray.Count; ++i)
          ret[i] = this.glyphIdArray[(int)i];

        return ret;
      }

      int CharacterConversionMap.Format() => 6;
    }

    /// <summary>
    /// Subtable format 8 was designed to support Unicode supplementary-plane characters in 
    /// UTF-16 encoding, though it is not commonly used. Format 8 is similar to format 2, 
    /// in that it provides for mixed-length character codes. Instead of allowing for 8- and 
    /// 16-bit character codes, however, it allows for 16- and 32-bit character codes.
    /// </summary>
    public struct Format8 : CharacterConversionMap
    {
      public ushort format;           // Subtable format; set to 8.
      public ushort reserved;         // Reserved; set to 0
      public uint length;             // Byte length of this subtable (including the header)
      public uint language;           // For requirements on use of the language field, see “Use of the language field in 'cmap' subtables” in this document.
      public List<byte> is32;         // Tightly packed array of bits (8K bytes total) indicating whether the particular 16-bit (index) value is the start of a 32-bit character code
      public uint numGroups;          // Number of groupings which follow
      public List<SequentialMapGroup> groups; // Array of SequentialMapGroup records.

      public void Read(FontReader r, bool readformat = false)
      { 
        if (readformat == true)
          r.ReadInt(out this.format);
        else
          this.format = 8;

        r.ReadInt(out this.reserved);
        r.ReadInt(out this.length);
        r.ReadInt(out this.language);

        this.is32 = new List<byte>();
        for(int i = 0; i < 8192; ++i)
          this.is32.Add(r.ReadUInt8());

        r.ReadInt(out this.numGroups);
                            
        this.groups = new List<SequentialMapGroup>();
        for(int i = 0; i < this.numGroups; ++i)
        { 
          SequentialMapGroup smg = new SequentialMapGroup();
          smg.Read(r);
          this.groups.Add(smg);
        }
      }

      Dictionary<uint, uint> CharacterConversionMap.MapCodeToIndex(FontReader r)
      {
        Dictionary<uint, uint> ret = new Dictionary<uint, uint>();

        // !UNIMPLEMENTED

        return ret;
      }

      int CharacterConversionMap.Format() => 8;
    }

    /// <summary>
    /// Subtable format 8 was designed to support Unicode supplementary-plane characters, though 
    /// it is not commonly used. Format 10 is similar to format 6, in that it defines a trimmed 
    /// array for a tight range of character codes. It differs, however, in that is uses 32-bit 
    /// character codes.
    /// </summary>
    public struct Format10 : CharacterConversionMap
    { 
      public ushort format;               // Subtable format; set to 10.
      public ushort reserved;             // Reserved; set to 0
      public uint length;                 // Byte length of this subtable (including the header)
      public uint language;               // For requirements on use of the language field, see “Use of the language field in 'cmap' subtables” in this document.
      public uint startCharCode;          // First character code covered
      public uint numChars;               // Number of character codes covered
      public List<ushort> glyphIdArray;   // Array of glyph indices for the character codes covered

      public void Read(FontReader r, bool readformat = false)
      { 
        if (readformat == true)
          r.ReadInt(out this.format);
        else
          this.format = 10;

        r.ReadInt(out this.reserved);
        r.ReadInt(out this.length);
        r.ReadInt(out this.language);
        r.ReadInt(out this.startCharCode);
        r.ReadInt(out this.numChars);

        this.glyphIdArray = new List<ushort>();
        for(int i = 0; i < this.numChars; ++i)
          this.glyphIdArray.Add(r.ReadUInt16());
      }

      Dictionary<uint, uint> CharacterConversionMap.MapCodeToIndex(FontReader r)
      {
        Dictionary<uint, uint> ret = new Dictionary<uint, uint>();

        // !UNTESTED
        for(uint i = 0; i < glyphIdArray.Count; ++i)
          ret.Add(i, glyphIdArray[(int)i]);

        return ret;
      }

      int CharacterConversionMap.Format() => 10;
    }

    /// <summary>
    /// This is the standard character-to-glyph-index mapping subtable for fonts supporting Unicode 
    /// character repertoires that include supplementary-plane characters (U+10000 to U+10FFFF).
    /// 
    /// Format 12 is similar to format 4 in that it defines segments for sparse representation. 
    /// It differs, however, in that it uses 32-bit character codes.
    /// </summary>
    public struct Format12 : CharacterConversionMap
    { 
      public ushort format;           // Subtable format; set to 12.
      public ushort reserved;         // Reserved; set to 0
      public uint length;             // Byte length of this subtable (including the header)
      public uint language;           // For requirements on use of the language field, see “Use of the language field in 'cmap' subtables” in this document.
      public uint numGroups;          // Number of groupings which follow
      public List<SequentialMapGroup> groups; // Array of SequentialMapGroup records.

      public void Read(FontReader r, bool readformat = false)
      { 
        if (readformat == true)
          r.ReadInt(out this.format);
        else
          this.format = 12;

        r.ReadInt(out this.reserved);
        r.ReadInt(out this.length);
        r.ReadInt(out this.language);
        r.ReadInt(out this.numGroups);

        this.groups = new List<SequentialMapGroup>();
        for(int i = 0; i < this.numGroups; ++i)
        { 
          SequentialMapGroup smg = new SequentialMapGroup();
          smg.Read(r);
          this.groups.Add(smg);
        }
      }

      Dictionary<uint, uint> CharacterConversionMap.MapCodeToIndex(FontReader r)
      {
        // !UNIMPLEMENTED
        Dictionary<uint, uint> ret = new Dictionary<uint, uint>();
        return ret;
      }

      int CharacterConversionMap.Format() => 12;
    }

    /// <summary>
    /// This subtable provides for situations in which the same glyph is used for 
    /// hundreds or even thousands of consecutive characters spanning across multiple 
    /// ranges of the code space. This subtable format may be useful for “last resort” 
    /// fonts, although these fonts may use other suitable subtable formats as well. 
    /// (For “last-resort” fonts, see also the 'head' table flags, bit 14.)
    /// </summary>
    public struct Format13 : CharacterConversionMap
    { 
      public ushort format;           // Subtable format; set to 13.
      public ushort reserved;         // Reserved; set to 0
      public uint length;             // Byte length of this subtable (including the header)
      public uint language;           // For requirements on use of the language field, see “Use of the language field in 'cmap' subtables” in this document.
      public uint numGroups;          // Number of groupings which follow
      List<ConstantMapGroup> groups;  // Array of ConstantMapGroup records.

      public void Read(FontReader r, bool readformat = false)
      { 
        if (readformat == true)
          r.ReadInt(out this.format);
        else
          this.format = 13;

        r.ReadInt(out this.reserved);
        r.ReadInt(out this.length);
        r.ReadInt(out this.language);
        r.ReadInt(out this.numGroups);

        this.groups = new List<ConstantMapGroup>();
        for(int i = 0; i < this.numGroups; ++i)
        { 
          ConstantMapGroup cmg = new ConstantMapGroup();
          cmg.Read(r);
          this.groups.Add(cmg);
        }
      }

      Dictionary<uint, uint> CharacterConversionMap.MapCodeToIndex(FontReader r)
      {
        // !UNIMPLEMENTED
        Dictionary<uint, uint> ret = new Dictionary<uint, uint>();
        return ret;
      }

      int CharacterConversionMap.Format() => 13;
    }

    /// <summary>
    /// The subtable partitions the UVSes supported by the font into two categories: “default” and “non-default” 
    /// UVSes. Given a UVS, if the glyph obtained by looking up the base character of that sequence in the Unicode 
    /// 'cmap' subtable (i.e. the BMP subtable or the BMP + supplementary-planes subtable) is the glyph to use for 
    /// that sequence, then the sequence is a default UVS; otherwise it is a non-default UVS, and the glyph to use 
    /// for that sequence is specified in the format 14 subtable itself.
    /// </summary>
    public struct Format14 : CharacterConversionMap
    { 
      public ushort format;                       // Subtable format. Set to 14.
      public uint length;                         // Byte length of this subtable (including this header)
      public uint numVarSelectorRecords;          // // Number of variation Selector Records
      public List<VariationSelector> varSelector; // Array of VariationSelector records.

      public void Read(FontReader r, bool readformat = false)
      {
        if (readformat == true)
          r.ReadInt(out this.format);
        else
          this.format = 14;

        r.ReadInt(out this.length);
        r.ReadInt(out this.numVarSelectorRecords);

        this.varSelector = new List<VariationSelector>();
        for(int i = 0; i < this.numVarSelectorRecords; ++i)
        {
          VariationSelector vs = new VariationSelector();
          vs.Read(r);
          this.varSelector.Add(vs);
        }
      }

      Dictionary<uint, uint> CharacterConversionMap.MapCodeToIndex(FontReader r)
      {
        // !UNIMPLEMENTED
        Dictionary<uint, uint> ret = new Dictionary<uint, uint>();
        return ret;
      }

      int CharacterConversionMap.Format() => 14;
    }

    /// <summary>
    /// Each variation selector records specifies a variation selector character, 
    /// and offsets to default and non-default tables used to map variation sequences 
    /// using that variation selector.
    /// </summary>
    public struct VariationSelector
    {
      public uint varSelector;                    // Variation selector. NOTE: loaded as 24 bit!
      public uint defaultUVSOffset;               // Offset from the start of the format 14 subtable to Default UVS Table. May be 0.
      public uint nonDefaultUVSOffset;            // Offset from the start of the format 14 subtable to Non-Default UVS Table. May be 0.

      public void Read(FontReader r)
      { 
        this.varSelector = r.ReadUInt24();
        r.ReadInt(out this.defaultUVSOffset);
        r.ReadInt(out this.nonDefaultUVSOffset);
      }
    }

    public const string TagName = "cmap";

    /// <summary>
    /// The array of encoding records specifies particular encodings and the 
    /// offset to the subtable for each encoding.
    /// </summary>
    public struct EncodingRecord
    {
      public ushort platformID;       // Platform ID.
      public ushort encodingID;       // Platform-specific encoding ID.
      public uint subtableOffset;     // Byte offset from beginning of table to the subtable for this encoding.

      public bool IsWindowsPlatform()
      {
        // Not 100% sure why we're filtering these items, but its from this source:
        // https://tchayen.github.io/ttf-file-parsing/
        // More info probably on the article or TTF docs
        return
          this.platformID == (ushort)PlatformID.Windows && 
          (
            this.encodingID == (ushort)WindowsPlatform.Symbol || 
            this.encodingID == (ushort)WindowsPlatform.UnicodeBMP || 
            this.encodingID == (ushort)WindowsPlatform.Unicode
          );
      }

      public bool IsUnicodePlatform()
      {
        // Not 100% sure why we're filtering these items, but its from this source:
        // https://tchayen.github.io/ttf-file-parsing/
        // More info probably on the article or TTF docs
        return this.platformID == 
          (ushort)PlatformID.UnicodeVarious &&
          (
            this.encodingID == (ushort)UniPlatform.Unicode0     ||
            this.encodingID == (ushort)UniPlatform.Unicode1_1   ||
            this.encodingID == (ushort)UniPlatform.ISOIEC10646  ||
            this.encodingID == (ushort)UniPlatform.Unicode2BMP  ||
            this.encodingID == (ushort)UniPlatform.Unicode2Full
          );
      }

      public void Read(FontReader r)
      {
        r.ReadInt(out this.platformID);
        r.ReadInt(out this.encodingID);
        r.ReadInt(out this.subtableOffset);
      }
    }

    public ushort version;      // Table version number (0).
    public ushort numTables;    // Number of encoding tables that follow.
    public List<EncodingRecord> encodingRecords;

    public List<Format0> format0;
    public List<Format2> format2;
    public List<Format4> format4;
    public List<Format6> format6;
    public List<Format8> format8;
    public List<Format10> format10;
    public List<Format12> format12;
    public List<Format13> format13;
    public List<Format14> format14;

    public void Read(FontReader r, uint tableStart)
    {
      r.ReadInt(out this.version);
      r.ReadInt(out this.numTables);

      this.encodingRecords = new List<EncodingRecord>();
      for (int i = 0; i < this.numTables; ++i)
      {
        EncodingRecord er = new EncodingRecord();
        er.Read(r);
        this.encodingRecords.Add(er);
      }

      foreach(EncodingRecord rc in this.encodingRecords)
      {
        r.SetPosition(tableStart + rc.subtableOffset);

        ushort format = r.ReadUInt16();

        if (format == 0)
        { 
          if (this.format0 == null)
            this.format0 = new List<Format0>();
                            
          Format0 f0 = new Format0();
          f0.Read(r);
          this.format0.Add(f0);
        }
        else if (format == 2)
        {
          if (this.format2 == null)
            this.format2 = new List<Format2>();
                            
          Format2 f2 = new Format2();
          f2.Read(r);
          this.format2.Add(f2);
        }
        else if (format == 4)
        {
          if (this.format4 == null)
            this.format4 = new List<Format4>();
                            
          Format4 f4 = new Format4();
          f4.Read(r);
          this.format4.Add(f4);
        }
        else if (format == 6)
        {
          if (this.format6 == null)
            this.format6 = new List<Format6>();
                            
          Format6 f6 = new Format6();
          f6.Read(r); 
          this.format6.Add(f6);
        }
        else if (format == 8)
        {
          if (this.format8 == null)
            this.format8 = new List<Format8>();
                            
          Format8 f8 = new Format8();
          f8.Read(r);
          this.format8.Add(f8);
        }
        else if (format == 10)
        {
          if (this.format10 == null)
            this.format10 = new List<Format10>();
                            
          Format10 f10 = new Format10();
          f10.Read(r);
          this.format10.Add(f10);
        }
        else if (format == 12)
        {
          if (this.format12 == null)
            this.format12 = new List<Format12>();
                            
          Format12 f12 = new Format12();
          f12.Read(r);
          this.format12.Add(f12);
        }
        else if (format == 13)
        {
          if (this.format13 == null)
            this.format13 = new List<Format13>();
                            
          Format13 f13 = new Format13();
          f13.Read(r);
          this.format13.Add(f13);
        }
        else if (format == 14)
        {
          if (this.format14 == null)
            this.format14 = new List<Format14>();
                            
          Format14 f14 = new Format14();
          f14.Read(r);
          this.format14.Add(f14);
        }
      }
    }

    public IEnumerable<CharacterConversionMap> EnumCharacterMaps()
    {
      if (this.format0 != null)
      {
        foreach(CharacterConversionMap ccm in this.format0)
          yield return ccm;
      }

      if (this.format2 != null)
      {
        foreach (CharacterConversionMap ccm in this.format2)
          yield return ccm;
      }

      if (this.format4 != null)
      {
        foreach (CharacterConversionMap ccm in this.format4)
          yield return ccm;
      }

      if (this.format6 != null)
      {
        foreach (CharacterConversionMap ccm in this.format6)
          yield return ccm;
      }

      if (this.format8 != null)
      {
        foreach (CharacterConversionMap ccm in this.format8)
          yield return ccm;
      }

      if (this.format10 != null)
      {
        foreach (CharacterConversionMap ccm in this.format10)
          yield return ccm;
      }

      if (this.format12 != null)
      {
        foreach (CharacterConversionMap ccm in this.format12)
          yield return ccm;
      }

      if (this.format13 != null)
      {
        foreach (CharacterConversionMap ccm in this.format13)
          yield return ccm;
      }

      if (this.format14 != null)
      {
        foreach (CharacterConversionMap ccm in this.format14)
          yield return ccm;
      }
    }
  }
}