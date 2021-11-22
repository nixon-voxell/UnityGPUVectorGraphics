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
using UnityEngine;

namespace Voxell.GPUVectorGraphics.Font
{
  public struct Glyf
  {
    public struct CompositeEntry
    {
      public ushort flags;       // composite flag
      public ushort glyphIndex;  // glyph index of component
      public int argument1;      // x-offset for component or point number; type depends on bits 0 and 1 in component flags
      public int argument2;      // y-offset for component or point number; type depends on bits 0 and 1 in component flags

      public float scale;
      public float xscale;
      public float scale01;
      public float scale10;
      public float yscale;

      public bool Read(FontReader r)
      {
        // Set defaults in case we don't read them
        // because it's filtered out from the flags.
        this.argument1 = 0;
        this.argument2 = 0;
        this.scale = 1.0f;
        this.xscale = 1.0f;
        this.yscale = 1.0f;
        this.scale01 = 0.0f;
        this.scale10 = 0.0f;

        r.ReadInt(out this.flags);
        r.ReadInt(out this.glyphIndex);

        if ((this.flags & ARG_1_AND_2_ARE_WORDS) != 0)
        {
          short a1 = r.ReadInt16();
          short a2 = r.ReadInt16();

          if((this.flags & ARGS_ARE_XY_VALUES) != 0)
          {
            this.argument1 = a1;
            this.argument2 = a2;
          }
          else
          { 
            this.argument1 = (ushort)a1;
            this.argument2 = (ushort)a2;
          }
        }
        else
        {
          sbyte a1 = r.ReadInt8();
          sbyte a2 = r.ReadInt8();

          if ((this.flags & ARGS_ARE_XY_VALUES) != 0)
          {
            this.argument1 = a1;
            this.argument2 = a2;
          }
          else
          {
            this.argument1 = (byte)a1;
            this.argument2 = (byte)a2;
          }
        }
        if ((this.flags & WE_HAVE_A_SCALE) != 0)
        {
          this.scale = r.ReadFDot14();
        }
        else if ((this.flags & WE_HAVE_AN_X_AND_Y_SCALE) != 0)
        {
          this.xscale = r.ReadFDot14();   // Format 2.14
          this.yscale = r.ReadFDot14();   // Format 2.14
        }
        else if ((this.flags & WE_HAVE_A_TWO_BY_TWO) != 0)
        {
          this.xscale = r.ReadFDot14();   // Format 2.14
          this.scale01 = r.ReadFDot14();   // Format 2.14
          this.scale10 = r.ReadFDot14();   // Format 2.14
          this.yscale = r.ReadFDot14();   // Format 2.14
        }

        return (this.flags & MORE_COMPONENTS) != 0;
      }
    }

    // Simple Glyph Flags
    public const byte ON_CURVE_POINT                        = 0x01; // Bit 0: If set, the point is on the curve; otherwise, it is off the curve.
    public const byte X_SHORT_VECTOR                        = 0x02; // Bit 1: If set, the corresponding x-coordinate is 1 byte long. If not set, it is two bytes long. For the sign of this value, see the description of the X_IS_SAME_OR_POSITIVE_X_SHORT_VECTOR flag.
    public const byte Y_SHORT_VECTOR                        = 0x04; // Bit 2: If set, the corresponding y-coordinate is 1 byte long. If not set, it is two bytes long. For the sign of this value, see the description of the Y_IS_SAME_OR_POSITIVE_Y_SHORT_VECTOR flag.
    public const byte REPEAT_FLAG                           = 0x08; // Bit 3: If set, the next byte (read as unsigned) specifies the number of additional times this flag byte is to be repeated in the logical flags array — that is, the number of additional logical flag entries inserted after this entry. (In the expanded logical array, this bit is ignored.) In this way, the number of flags listed can be smaller than the number of points in the glyph description.
    public const byte X_IS_SAME_OR_POSITIVE_X_SHORT_VECTOR  = 0x10; // Bit 4: This flag has two meanings, depending on how the X_SHORT_VECTOR flag is set. If X_SHORT_VECTOR is set, this bit describes the sign of the value, with 1 equaling positive and 0 negative.If X_SHORT_VECTOR is not set and this bit is set, then the current x-coordinate is the same as the previous x-coordinate.If X_SHORT_VECTOR is not set and this bit is also not set, the current x - coordinate is a signed 16 - bit delta vector.
    public const byte Y_IS_SAME_OR_POSITIVE_Y_SHORT_VECTOR  = 0x20; // Bit 5: This flag has two meanings, depending on how the Y_SHORT_VECTOR flag is set.If Y_SHORT_VECTOR is set, this bit describes the sign of the value, with 1 equaling positive and 0 negative.If Y_SHORT_VECTOR is not set and this bit is set, then the current y-coordinate is the same as the previous y-coordinate.If Y_SHORT_VECTOR is not set and this bit is also not set, the current y - coordinate is a signed 16 - bit delta vector.
    public const byte OVERLAP_SIMPLE                        = 0x40; // Bit 6: If set, contours in the glyph description may overlap.Use of this flag is not required in OpenType — that is, it is valid to have contours overlap without having this flag set. It may affect behaviors in some platforms, however. (See the discussion of “Overlapping contours” in Apple’s specification for details regarding behavior in Apple platforms.) When used, it must be set on the first flag byte for the glyph. See additional details below.
    public const byte SIMPLE_RESERVED                       = 0x80; // Bit 7 is reserved: set to zero.

    //Composite Glyph Flags
    public const ushort ARG_1_AND_2_ARE_WORDS               = 0x0001; // Bit 0: If this is set, the arguments are 16 - bit(uint16 or int16); otherwise, they are bytes(uint8 or int8).
    public const ushort ARGS_ARE_XY_VALUES                  = 0x0002; // Bit 1: If this is set, the arguments are signed xy values; otherwise, they are unsigned point numbers.
    public const ushort ROUND_XY_TO_GRID                    = 0x0004; // Bit 2: For the xy values if the preceding is true.
    public const ushort WE_HAVE_A_SCALE                     = 0x0008; // Bit 3: This indicates that there is a simple scale for the component. Otherwise, scale = 1.0.
    public const ushort MORE_COMPONENTS                     = 0x0020; // Bit 5: Indicates at least one more glyph after this one.
    public const ushort WE_HAVE_AN_X_AND_Y_SCALE            = 0x0040; // Bit 6: The x direction will use a different scale from the y direction.
    public const ushort WE_HAVE_A_TWO_BY_TWO                = 0x0080; // Bit 7: There is a 2 by 2 transformation that will be used to scale the component.
    public const ushort WE_HAVE_INSTRUCTIONS                = 0x0100; // Bit 8: Following the last component are instructions for the composite character.
    public const ushort USE_MY_METRICS                      = 0x0200; // Bit 9: If set, this forces the aw and lsb(and rsb) for the composite to be equal to those from this original glyph.This works for hinted and unhinted characters.
    public const ushort OVERLAP_COMPOUND                    = 0x0400; // Bit 10: If set, the components of the compound glyph overlap.Use of this flag is not required in OpenType — that is, it is valid to have components overlap without having this flag set.It may affect behaviors in some platforms, however. (See Apple’s specification for details regarding behavior in Apple platforms.) When used, it must be set on the flag word for the first component.See additional remarks, above, for the similar OVERLAP_SIMPLE flag used in simple - glyph descriptions.
    public const ushort SCALED_COMPONENT_OFFSET             = 0x0800; // Bit 11: The composite is designed to have the component offset scaled.
    public const ushort UNSCALED_COMPONENT_OFFSET           = 0x1000; // Bit 12: The composite is designed not to have the component offset scaled.
    public const ushort COMPOSITE_RESERVED                  = 0xE010; // Bits 4, 13, 14 and 15 are reserved: set to 0.

    public const string TagName = "glyf";

    public short numberOfContours;          // If the number of contours is greater than or equal to zero, this is a simple glyph. If negative, this is a composite glyph — the value -1 should be used for composite glyphs.
    public short xMin;                      // Minimum x for coordinate data.
    public short yMin;                      // Minimum y for coordinate data.
    public short xMax;                      // Maximum x for coordinate data.
    public short yMax;                      // Maximum y for coordinate data.

    // Simple Glyph
    public List<ushort> endPtsOfCountours;  // Array of point indices for the last point of each contour, in increasing numeric order.
    public ushort instructionLength;        // Total number of bytes for instructions. If instructionLength is zero, no instructions are present for this glyph, and this field is followed directly by the flags field.
    public List<byte> instructions;         // Array of instruction byte code for the glyph.
    public List<byte> simpflags;            // Array of flag elements. See below for details regarding the number of flag array elements.
    public List<int> xCoordinates;          // Contour point x-coordinates. See below for details regarding the number of coordinate array elements. Coordinate for the first point is relative to (0,0); others are relative to previous point.
    public List<int> yCoordinates;          // Contour point y-coordinates. See below for details regarding the number of coordinate array elements. Coordinate for the first point is relative to (0,0); others are relative to previous point.

    public bool IsComplex { get => this.numberOfContours < 0; } // In general, values are complex; Ideally negative values should be -1

    public List<CompositeEntry> compositeEntries;
    public ushort numInstr;
    public List<byte> instr;

    public void Read(FontReader r)
    {
      r.ReadInt(out this.numberOfContours);
      r.ReadInt(out this.xMin);
      r.ReadInt(out this.yMin);
      r.ReadInt(out this.xMax);
      r.ReadInt(out this.yMax);

      if (this.IsComplex == false)
      {
        // Simple

        this.endPtsOfCountours = new List<ushort>();
        for(int i = 0; i < this.numberOfContours; ++i)
          this.endPtsOfCountours.Add( r.ReadUInt16() );

        r.ReadInt(out this.instructionLength);
        this.instructions = new List<byte>();
        for(int i = 0; i < this.instructionLength; ++i)
          this.instructions.Add(r.ReadUInt8());

        int numPoints = 0;
        // http://stevehanov.ca/blog/?id=143
        foreach(ushort us in this.endPtsOfCountours)
          numPoints = Mathf.Max(numPoints, us);
        numPoints += 1;

        this.simpflags = new List<byte>();
        this.xCoordinates = new List<int>();
        this.yCoordinates = new List<int>();
        for (int i = 0; i < numPoints; ++i)
        {
          byte flag = r.ReadUInt8();
          this.simpflags.Add(flag);

          if ((flag & REPEAT_FLAG) != 0)
          {
            byte repeatCount = r.ReadUInt8();
            i += repeatCount;

            for (int j = 0; j < repeatCount; ++j)
              this.simpflags.Add(flag);

          }
        }

        int val = 0;
        for (int i = 0; i < numPoints; ++i)
        {
          byte flag = this.simpflags[i];
          if ((flag & X_SHORT_VECTOR) != 0)
          {
            if((flag & X_IS_SAME_OR_POSITIVE_X_SHORT_VECTOR) != 0)
              val += r.ReadUInt8();
            else
              val -= r.ReadUInt8();
          }
          else if ((~flag & X_IS_SAME_OR_POSITIVE_X_SHORT_VECTOR) != 0)
            val += r.ReadInt16();

          this.xCoordinates.Add(val);
        }

        val = 0;
        for (int i = 0; i < numPoints; ++i)
        {
          byte flag = this.simpflags[i];
          if ((flag & Y_SHORT_VECTOR) != 0)
          {
            if ((flag & Y_IS_SAME_OR_POSITIVE_Y_SHORT_VECTOR) != 0)
              val += r.ReadUInt8();
            else
              val -= r.ReadUInt8();
          }
          else if ((~flag & Y_IS_SAME_OR_POSITIVE_Y_SHORT_VECTOR) != 0)
            val += r.ReadInt16();
          else
          { }  // Value is unchanged

          this.yCoordinates.Add(val);
        }
      }
      else
      {
        // Composite
        this.compositeEntries = new List<CompositeEntry>();

        bool moreComps = true;
        while(moreComps == true)
        {
          CompositeEntry compE = new CompositeEntry();
          moreComps = compE.Read(r);

          this.compositeEntries.Add(compE);
        } 

        CompositeEntry ceLast = this.compositeEntries[compositeEntries.Count - 1];
        if ((ceLast.flags & WE_HAVE_INSTRUCTIONS) != 0)
        {
          r.ReadInt(out this.numInstr);

          this.instr = new List<byte>();
          for (int i = 0; i < this.instr.Count; ++i)
            this.instr.Add(r.ReadUInt8());

        }
      }
    }
  }
}
