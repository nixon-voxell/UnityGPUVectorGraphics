namespace Voxell.GPUVectorGraphics.Font
{
  [System.Serializable]
  public struct Table
  { 
    /// <summary>The 4 byte identifier.</summary>
    public string tag;

    /// <summary>The error detection checksum.</summary>
    public uint checksum;

    /// <summary>The offset from the start of the file, where the actual data payload is.</summary>
    public uint offset;

    /// <summary>The size, in bytes, of the data payload.</summary>
    public uint length;

    /// <summary>Read the table from a TTFReader.</summary>
    /// <param name="r">The reader.</param>
    public void Read(FontReader r)
    {
      this.tag = r.ReadString(4);
      r.ReadInt(out this.checksum);
      r.ReadInt(out this.offset);
      r.ReadInt(out this.length);
    }
  }
}