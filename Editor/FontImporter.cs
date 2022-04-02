using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor.AssetImporters;
using Unity.Mathematics;

namespace Voxell.GPUVectorGraphics.Font
{
  using Inspector;

  [ScriptedImporter(1,  new[] { "ttfvector", "otfVector" }, new[] { "ttf", "otf" })]
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
    [Tooltip("0x00010000 (TTF) or 0x4F54544F (OTF)."), InspectOnly]
    public uint sfntVersion;
    [Tooltip("Number of tables."), InspectOnly]
    public ushort numTables;

    private Dictionary<string, Table> _tableMap;

    // we convert the integer into a float for easier division operation purposes
    // we want a float division instead of a integer division (which looses all the floating point data)
    [Tooltip("The *integer value for a unit distance in the font."), InspectOnly]
    public float unitsPerEm = 0;
    [Tooltip("Offset width, relevant when certain peices of data in the file."), InspectOnly]
    public int offsetByteWidth = 0;
    [Tooltip("The number of glyphs in the font file."), InspectOnly]
    public int glyphCount;

    [Tooltip("Name of the type face."), InspectOnly]
    public string fontName;

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

      // head will tell us all table offset information
      ////////////////////////////////////////////////////////////////////////////////
      #region Head
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
      #endregion

      // maxp will tell us how many glyphs there are in the file
      ////////////////////////////////////////////////////////////////////////////////
      #region Maxp
      if (!_tableMap.TryGetValue(Maxp.TagName, out table))
      {
        Debug.LogError("Font file does not have maxp data!");
        return;
      }

      fontReader.SetPosition(table.offset);
      Maxp maxP = new Maxp();
      maxP.Read(fontReader);
      glyphCount = maxP.numGlyphs;
      #endregion

      // loca knows offsets of glyphs in the glyf table
      ////////////////////////////////////////////////////////////////////////////////
      #region Loca
      if (!_tableMap.TryGetValue(Loca.TagName, out table))
      {
        Debug.LogError("Font file does not have loca data!");
        return;
      }

      fontReader.SetPosition(table.offset);
      Loca loca = new Loca();
      loca.Read(fontReader, glyphCount, offsetByteWidth == 4);
      #endregion

      // glyf provides contour data
      ////////////////////////////////////////////////////////////////////////////////
      #region Glyf
      if (!_tableMap.TryGetValue(Glyf.TagName, out table))
      {
        Debug.LogError("Font file does not have glyf data!");
        return;
      }

      Glyph[] glyphs = new Glyph[glyphCount];
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
          glyphs[g].contours = new QuadraticContour[contourCount];
          int pointIdx = 0;

          for (int c=0; c < contourCount; c++)
          {
            int endPoint = glyf.endPtsOfCountours[c]+1;
            int pointCount = endPoint - pointIdx;

            // initialize with the minimum capacity needed (if there are no consecutive points)
            List<float2> points = new List<float2>(pointCount);
            List<bool> isControls = new List<bool>(pointCount);

            // populate lists with original data
            for (; pointIdx < endPoint; pointIdx++)
            {
              isControls.Add((glyf.simpflags[pointIdx] & Glyf.ON_CURVE_POINT) == 0);
              float2 point = new float2(
                (float)glyf.xCoordinates[pointIdx], (float)glyf.yCoordinates[pointIdx]
              ) / unitsPerEm;

              points.Add(point);
            }

            // insert missing vertex points and control points
            for (int p=0; p < points.Count; p++)
            {
              // reverts back to 0 when we reach the end of the array
              int nextIdx = (p + 1) % points.Count;

              if (isControls[p] && isControls[nextIdx])
              {
                // If 2 control points are next to each other, there's an implied
                // point in between them at their average.
                // We add them explicitly for better parallelization in the future.

                // average vector between previous point and current point
                float2 avgPoint = (points[p] + points[nextIdx]) * 0.5f;
                points.Insert(nextIdx, avgPoint);
                isControls.Insert(nextIdx, false);
              } else if (!isControls[p] && !isControls[nextIdx])
              {
                // If 2 vertex points are next to each other, it represents that
                // the segment is a line instead of a curve.
                // We add a dummy control point which has the exact same location
                // as the current vertex point to indicate that it is a line.

                points.Insert(nextIdx, points[p]);
                isControls.Insert(nextIdx, true);
              }
            }

            // convert point list into segment array
            int segmentCount = points.Count/2;
            QuadraticPathSegment[] segments = new QuadraticPathSegment[segmentCount];
            for (int s=0; s < segmentCount; s++)
            {
              int segmentIdx = s*2;
              segments[s].p0 = points[segmentIdx];
              segments[s].p1 = points[segmentIdx+1];
            }

            // store to glyph struct
            glyphs[g].contours[c].segments = segments;
            glyphs[g].contours[c].closed = true;
            glyphs[g].maxRect = new float2(glyf.xMax, glyf.yMax)/unitsPerEm;
            glyphs[g].minRect = new float2(glyf.xMin, glyf.yMin)/unitsPerEm;
          }
          glyphs[g].isComplex = false;
        }
      }
      #endregion

      // cmap tells us the mapping between character codes and glyph indices
      // used throughout the font file
      ////////////////////////////////////////////////////////////////////////////////
      #region cmap
      if (!_tableMap.TryGetValue(CMap.TagName, out table))
      {
        Debug.LogError("Font file does not have cmap data!");
        return;
      }

      fontReader.SetPosition(table.offset);
      CMap cMap = new CMap();
      cMap.Read(fontReader, table.offset);

      Dictionary<uint, uint> characterRemap = null;
      foreach (CMap.CharacterConversionMap ccm in cMap.EnumCharacterMaps())
      {
        characterRemap = ccm.MapCodeToIndex(fontReader);
        break;
      }

      int keyCount =  characterRemap.Keys.Count;
      int[] charMap = new int[keyCount];
      foreach (KeyValuePair<uint, uint> kvp in characterRemap)
      {
        int code = (int)kvp.Key;
        int idx = (int)kvp.Value;
        charMap[code] = idx;
      }
      #endregion

      _tableMap.Clear();
      fontReader.Close();

      FontCurve fontCurve = ScriptableObject.CreateInstance<FontCurve>();
      fontCurve.Initialize(glyphs, charMap);
      ctx.AddObjectToAsset("FontCurve", fontCurve);
    }
  }
}