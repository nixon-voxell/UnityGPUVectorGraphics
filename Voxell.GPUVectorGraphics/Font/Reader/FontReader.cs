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

using System.IO;

namespace Voxell.GPUVectorGraphics.Font
{
  /// <summary>
  /// Utility class used by Berny to read a TrueType Font file and other
  /// related files and data streams.
  /// </summary>
  public abstract class FontReader
  {
    /// <summary>Returning access and resources back to the system.</summary>
    /// <returns>True if the reader was successfully closed. Else, false.</returns>
    /// <remark>May not be relevant on all platforms and implementations of TTFReader, but 
    /// best practice is to call Close() at the proper time, regardless of the implementation
    /// or usecase. The only exception is explicit use to TTFReaderBytes.</remark>
    public abstract bool Close();

    /// <summary>
    /// Read a 1 byte un signed integer from the read position.
    /// </summary>
    /// <returns>The read integer value.</returns>
    public abstract sbyte ReadInt8();

    /// <summary>
    /// Read a 1 byte signed integer from the read position.
    /// </summary>
    /// <returns>The read integer value.</returns>
    public abstract byte ReadUInt8();

    /// <summary>
    /// Returns the read position.
    /// </summary>
    /// <returns>The read position.</returns>
    public abstract long GetPosition();

    /// <summary>
    /// Set the read position of the reader.
    /// </summary>
    /// <param name="pos">The new read position.</param>
    /// <returns>If true, the read position was successfully set. Else, false.</returns>
    public abstract bool SetPosition(long pos, SeekOrigin seekOrigin = SeekOrigin.Begin);

    /// <summary>
    /// Read an array of bytes, starting from the read position.
    /// </summary>
    /// <param name="length">The number of bytes to read.</param>
    /// <returns>The read bytes.</returns>
    public abstract byte[] ReadBytes(int length);

    public sbyte[] ReadSBytes(int length)
    { 
      // Reading SBytes is far less-used than ReadBytes, so we just do 
      // our own implementation instead of forcing the implemenenters
      // to make their own.
      sbyte [] ret = new sbyte[length];

      for(int i = 0; i < length; ++i)
        ret[i] = this.ReadInt8();

      return ret;
    }

    /// <summary>
    /// If true, the read position is past the readable area of the data stream. 
    /// Else, false.
    /// </summary>
    /// <returns>If false, there is still data that can be read from where the read position is.</returns>
    public abstract bool AtEnd();

    /// <summary>
    /// Read a signed 16 int from the current read position.
    /// </summary>
    /// <returns>The read integer value.</returns>
    /// <returns>Current implementation is hard coded for little endian platforms.</returns>
    public ushort ReadUInt16()
    {
      return (ushort)(this.ReadUInt8() << 8 | this.ReadUInt8());
    }

    /// <summary>
    /// Read a 24 bit unsigned value from the current read position.
    /// </summary>
    /// <returns>The read integer value.</returns>
    /// <remarks>Current implementation is hard coded for little endian platforms.</remarks>
    public uint ReadUInt24()
    {
      return (uint)((this.ReadUInt8() << 16) | (this.ReadUInt8() << 8) | (this.ReadUInt8() << 0));
    }

    /// <summary>
    /// Read a 32 bit unsigned value from the current read position.
    /// </summary>
    /// <returns>Current implementation is hard coded for little endian platforms.</returns>
    public uint ReadUInt32()
    {
      return (uint)((this.ReadUInt8() << 24) | (this.ReadUInt8() << 16) | (this.ReadUInt8() << 8) | (this.ReadUInt8() << 0));
    }

    public long ReadUInt64()
    {
      return (
        ((long)this.ReadUInt8() << 56) | 
        ((long)this.ReadUInt8() << 48) | 
        ((long)this.ReadUInt8() << 40) | 
        ((long)this.ReadUInt8() << 32) |
        ((long)this.ReadUInt8() << 24) | 
        ((long)this.ReadUInt8() << 16) | 
        ((long)this.ReadUInt8() << 8) | 
        ((long)this.ReadUInt8() << 0));
    }

    /// <summary>
    /// Read a 16 bit signed value from the current read position.
    /// </summary>
    /// <returns></returns>
    /// <returns>Current implementation is hard coded for little endian platforms.</returns>
    public short ReadInt16()
    {
      return (short)(this.ReadUInt8() << 8 | this.ReadUInt8());
    }

    /// <summary>
    /// Read a 32 bit signed value from the current read position.
    /// </summary>
    /// <returns></returns>
    /// <returns>Current implementation is hard coded for little endian platforms.</returns>
    public int ReadInt32()
    {
      return (int)((this.ReadUInt8() << 24) | (this.ReadUInt8() << 16) | (this.ReadUInt8() << 8) | (this.ReadUInt8() << 0));
    }

    /// <summary>
    /// Read a TTF FWord from the current read position.
    /// </summary>
    /// <returns>The value read from the data stream.</returns>
    public short ReadFWord()
    {
      return this.ReadInt16();
    }

    /// <summary>
    /// Read a TTF UFWord from the current read position.
    /// </summary>
    /// <returns>The value read from the data stream.</returns>
    public ushort ReadUFWord()
    {
      return this.ReadUInt16();
    }

    /// <summary>
    /// Read a TTF Offset16 value from the current read position.
    /// </summary>
    /// <returns>The value read from the data stream.</returns>
    public ushort ReadOffset16()
    {
      return this.ReadUInt16();
    }

    /// <summary>
    /// Read a TTF Offset32 value from the current read position.
    /// </summary>
    /// <returns>The value read from the data stream.</returns>
    public int ReadOffset32()
    {
      return this.ReadOffset32();
    }

    /// <summary>
    /// Reads a TTF FDot14 value from the current read position.
    /// </summary>
    /// <returns>The number value read from the data stream and converted to a float.</returns>
    public float ReadFDot14()
    {
      return (float)this.ReadInt16() / (float)(1 << 14);
    }

    /// <summary>
    /// Reads a TTF Fixed value from the current read position.
    /// </summary>
    /// <returns>the number value read from the data stream and converted to a float.</returns>
    public float ReadFixed()
    {
      return (float)this.ReadInt32() / (float)(1 << 16);
    }

    /// <summary>
    /// Read an ASCII string of a known length from file.
    /// </summary>
    /// <param name="length">The length of the string.</param>
    /// <returns>The string read from the data stream.</returns>
    public string ReadString(int length)
    {
      byte[] rbStr = this.ReadBytes(length);
      return System.Text.ASCIIEncoding.ASCII.GetString(rbStr);
    }

    /// <summary>
    /// Read a record string from file. A record string is a pascal
    /// string with a 2 byte length.
    /// </summary>
    /// <returns>The string read from the data stream.</returns>
    public string ReadNameRecord()
    {
      ushort len = this.ReadUInt16();
      return this.ReadString(len);
    }

    /// <summary>
    /// Read a string from file in pascal format - where the first byte
    /// defines the length of the string, directly followed by the ASCII
    /// data.
    /// </summary>
    /// <returns>The string read from the datastream.</returns>
    public string ReadPascalString()
    {
      byte c = this.ReadUInt8();
      return this.ReadString(c);
    }

    // A strategy used in reading is to create file member variables
    // of the correct byte width and "signed-ness", and just use
    // ReadInt(out * i) and let the overloading resolve figure out
    // the proper Read*() function.

    /// <summary>
    /// Overload of ReadInt() for 1 byte unsigned int.
    /// </summary>
    /// <param name="i">The output variable.</param>
    public void ReadInt(out sbyte i)
    {
      i = this.ReadInt8();
    }

    /// <summary>
    /// Overload of ReadInt() for 1 byte signed int.
    /// </summary>
    /// <param name="i">The output variable.</param>
    public void ReadInt(out byte i)
    {
      i = this.ReadUInt8();
    }

    /// <summary>
    /// Overload of ReadInt() for 2 byte signed int.
    /// </summary>
    /// <param name="i">The output variable.</param>
    public void ReadInt(out short i)
    {
      i = this.ReadInt16();
    }

    /// <summary>
    /// Overload of ReadInt() for 2 byte unsigned int.
    /// </summary>
    /// <param name="i">The output variable.</param>
    public void ReadInt(out ushort i)
    {
      i = this.ReadUInt16();
    }

    /// <summary>
    /// Overload of ReadInt() for 4 byte signed int.
    /// </summary>
    /// <param name="i">The output variable.</param>
    public void ReadInt(out int i)
    {
      i = this.ReadInt32();
    }

    /// <summary>
    /// Overload of ReadInt() for 4 byte unsigned int.
    /// </summary>
    /// <param name="i">The output variable.</param>
    public void ReadInt(out uint i)
    {
      i = this.ReadUInt32();
    }

    /// <summary>
    /// Reads a date time from the TTF file.
    /// </summary>
    /// <returns>The datetime at the current read position.</returns>
    public System.DateTime ReadDate()
    {
      System.DateTime dt = new System.DateTime(1904, 1, 1);
      dt = dt.AddSeconds(this.ReadUInt64());

      return dt;
    }
  }
}