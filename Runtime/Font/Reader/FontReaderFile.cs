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
  /// An implementation of the TTFReader that pulls data from a file.
  /// </summary>
  public class FontReaderFile : FontReader
  {
    /// <summary>The file reader.</summary>
    BinaryReader reader = null;

    /// <summary>The file stream used by the binary reader.</summary>
    FileStream filestream = null;

    /// <summary>File path constructor.</summary>
    /// <param name="path">The file path to open.</param>
    public FontReaderFile(string path)
    {
      if (this.Open(path) == false)
        throw new System.Exception("Could not open file");
    }

    /// <summary>Opens a file.</summary>
    /// <param name="path">The file path to open.</param>
    /// <returns>If true, the file was successfully opened. Else, false.</returns>
    public bool Open(string path)
    {
      this.Close();

      this.filestream = System.IO.File.Open(path, System.IO.FileMode.Open);
      reader = new System.IO.BinaryReader(this.filestream);
      return true;
    }

    public override bool Close()
    {
      if (this.filestream != null)
      {
        this.filestream.Close();
        this.filestream = null;
        this.reader = null;

        return true;
      }
      return false;
    }

    public override bool AtEnd() => this.filestream.Position < this.filestream.Length;

    public override sbyte ReadInt8() => this.reader.ReadSByte();

    public override byte ReadUInt8() => this.reader.ReadByte();

    public override long GetPosition() => this.filestream.Position;

    public override bool SetPosition(long pos, SeekOrigin seekOrigin = SeekOrigin.Begin)
    {
      if (this.filestream == null) return false;
      return this.filestream.Seek(pos, seekOrigin) == pos;
    }

    public override byte [] ReadBytes(int length) => this.reader.ReadBytes(length);
  }
}