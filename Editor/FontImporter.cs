using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor.AssetImporters;
using Unity.Mathematics;
using Voxell.Inspector;

namespace Voxell.GPUVectorGraphics.Font
{
  [ScriptedImporter(0,  new[] { "ttfvector", "otfVector" }, new[] { "ttf", "otf" })]
  public class FontImporter : ScriptedImporter
  {
    /// <summary>Format of the font.</summary>
    public enum FontFormat
    {
      /// <summary>True type format.</summary>
      TTF,
      /// <summary>Open type format.</summary>
      OTF
    }

    public const int FormatTrueType = 0x00010000;
    public const int FormatOTF = 0x4F54544F;

    [Tooltip("Format of the font."), InspectOnly]
    public FontFormat fontFormat;
    [Tooltip("0x00010000 (TTF) or 0x4F54544F (OTF)."),InspectOnly]
    public uint sfntVersion;
    [Tooltip("Number of tables."), InspectOnly]
    public ushort numTables;

    private Dictionary<string, Table> _tableMap;

    [Tooltip("The integer value for a unit distance in the font."), InspectOnly]
    public float unitsPerEm = 0;
    [Tooltip("Offset width, relevant when certain peices of data in the file."), InspectOnly]
    public int offsetByteWidth = 0;
    [Tooltip("The number of glyphs in the font file."), InspectOnly]
    public int glyphCount;

    [Tooltip("Name of the type face."), InspectOnly]
    public string fontName;
    [Tooltip("Bezier contour for each character."), HideInInspector]
    public Glyph[] glyphs;

    public override void OnImportAsset(AssetImportContext ctx)
    {
      string filePath = FileUtilx.GetAssetFilePath(ctx.assetPath);
      FontReaderFile fontReader = new FontReaderFile(filePath);
      fontReader.SetPosition(0);
      fontReader.ReadInt(out sfntVersion);


      if (sfntVersion != FormatTrueType && sfntVersion != FormatOTF)
      {
        Debug.LogWarning("Font type not supported!");
        return;
      }

      fontFormat = sfntVersion == FormatTrueType ? FontFormat.TTF : FontFormat.OTF;
      fontReader.ReadInt(out numTables);
      fontReader.SetPosition(6, SeekOrigin.Current);

      Table table;
      _tableMap = new Dictionary<string, Table>();
      for (int t=0; t < numTables; ++t)
      {
        table = new Table();
        table.Read(fontReader);
        _tableMap.Add(table.tag, table);
      }

      // head
      if (!_tableMap.TryGetValue(Head.TagName, out table))
      {
        Debug.LogError("Font file does not have a header!");
        return;
      }

      fontReader.SetPosition(table.offset);
      Head head = new Head();
      head.Read(fontReader);
      unitsPerEm = (float)head.unitsPerEm;
      offsetByteWidth = head.OffsetByteWidth;

      // maxp
      if (!_tableMap.TryGetValue(Maxp.TagName, out table))
      {
        Debug.LogError("Font file does not have maxp data!");
        return;
      }

      fontReader.SetPosition(table.offset);
      Maxp maxP = new Maxp();
      maxP.Read(fontReader);
      glyphCount = maxP.numGlyphs;

      // loca
      if (!_tableMap.TryGetValue(Loca.TagName, out table))
      {
        Debug.LogError("Font file does not have loca data!");
        return;
      }

      fontReader.SetPosition(table.offset);
      Loca loca = new Loca();
      loca.Read(fontReader, glyphCount, offsetByteWidth == 4);

      // glyf
      if (!_tableMap.TryGetValue(Glyf.TagName, out table))
      {
        Debug.LogError("Font file does not have glyf data!");
        return;
      }

      // keep a list of what's a composite, and we'll construct those when we're done.
      glyphs = new Glyph[glyphCount];
      fontName = FileUtilx.GetFilename(filePath).Split('.')[0];
      for (int g=0; g < glyphCount; g++)
      {
        uint glyphOffset = loca.GetGlyphOffset(table, g);
        uint glyphSize = loca.GetGlyphSize(g);
        // possibly a blank space or character does not exsit in this font
        // usually we treat it as a blank box
        if (glyphSize == 0) continue;

        fontReader.SetPosition(glyphOffset);
        Glyf glyf = new Glyf();
        glyf.Read(fontReader);
        int contourCount = glyf.numberOfContours;

        if (glyf.IsComplex)
        {
          // complex
          int compositeCount = glyf.compositeEntries.Count;
          glyphs[g].compositeReferences = new Glyph.CompositeReference[compositeCount];
          for (int c=0; c < compositeCount; c++)
          {
            Glyf.CompositeEntry ce = glyf.compositeEntries[c];
            Glyph.CompositeReference cref = new Font.Glyph.CompositeReference();
            cref.xAxis = new float2(ce.xscale, ce.scale01);
            cref.yAxis = new float2(ce.scale10, ce.yscale);
            cref.offset = new float2(ce.argument1, ce.argument2) / unitsPerEm;
            cref.glyphRef = ce.glyphIndex;

            glyphs[g].compositeReferences[c] = cref;
          }
          glyphs[g].isComplex = true;
        } else
        {
          // simple
          glyphs[g].glyphContours = new Glyph.GlyphContour[contourCount];
          int prevContourEnd = 0;
          int currContourEnd;

          for (int c=0; c < contourCount; c++)
          {
            currContourEnd = glyf.endPtsOfCountours[c];
            int segmentCount = currContourEnd - prevContourEnd;

            float2[] points = new float2[segmentCount];
            bool[] isControls = new bool[segmentCount];
            for (int s=0; s < segmentCount; s++)
            {
              int segmentIdx = prevContourEnd + s;

              points[s] = new float2(
                (float)glyf.xCoordinates[segmentIdx], (float)glyf.yCoordinates[segmentIdx]
              ) / unitsPerEm;

              isControls[s] = (glyf.simpflags[segmentIdx] & Glyf.ON_CURVE_POINT) == 0;
            }
            glyphs[g].glyphContours[c].points = points;
            glyphs[g].glyphContours[c].isControls = isControls;
            prevContourEnd = currContourEnd;
          }
          glyphs[g].isComplex = false;
        }

      }

      fontReader.Close();
    }
  }
}