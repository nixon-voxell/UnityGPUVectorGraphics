using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace Voxell.GPUVectorGraphics
{
  /// <summary>The path corner types, for joining path segments together.</summary>
  public enum PathCorner
  {
    /// <summary>A tipped corner with a sharp edge.</summary>
    Tipped,

    /// <summary>A rounded corner.</summary>
    Round,

    /// <summary>A beveled corner.</summary>
    Beveled
  }

  /// <summary>The path ending types.</summary>
  public enum PathEnding
  {
    /// <summary>A square path ending.</summary>
    Chop,

    /// <summary>A square path ending with a small extrusion.</summary>
    Square,

    /// <summary>A rounded path ending.</summary>
    Round
  }

  /// <summary>The fill mode types.</summary>
  public enum FillMode
  {
    /// <summary>
    /// Determines the "insideness" of the shape by evaluating the direction of the edges crossed.
    /// </summary>
    NonZero,

    /// <summary>
    /// Determines the "insideness" of the shape by counting the number of edges crossed.
    /// </summary>
    OddEven
  }

  [Serializable]
  public struct BezierSegment
  {
    [Tooltip("Origin point of the segment.")]
    public float2 p0;

    [Tooltip("First control point of the segment.")]
    public float2 p1;

    [Tooltip("Second control point of the segment.")]
    public float2 p2;

    [Tooltip("Ending point of the segment.")]
    public float2 p3;

    public BezierSegment(float2 p0, float2 p1, float2 p2, float2 p3)
    {
      this.p0 = p0;
      this.p1 = p1;
      this.p2 = p2;
      this.p3 = p3;
    }
  }

  [Serializable]
  public struct BezierPathSegment
  {
    [Tooltip("Origin point of the segment.")]
    public float2 p0;

    [Tooltip("First control point of the segment.")]
    public float2 p1;

    [Tooltip("Second control point of the segment.")]
    public float2 p2;

    public BezierPathSegment(float2 p0, float2 p1, float2 p2)
    {
      this.p0 = p0;
      this.p1 = p1;
      this.p2 = p2;
    }
  }

  [Serializable]
  public struct BezierContour
  {
    [Tooltip("An array of every path segments on the contour. " +
    "Closed paths should not add a dedicated closing segment. It is implied by the 'closed' property.")]
    public BezierPathSegment[] segments;

    [Tooltip("A boolean indicating if the contour should be closed. " +
    "When set to true, closed path will connect the last path segment to the first path segment, by using the " +
    "last path segment's P1 and P2 as control points.")]
    public bool closed;
  }

  [Serializable]
  public struct PathProperties
  {
    [Tooltip("How the beginning of the path should be displayed.")]
    public PathEnding head;

    [Tooltip("How the end of the path should be displayed.")]
    public PathEnding tail;

    [Tooltip("How the corners of the path should be displayed.")]
    public PathCorner corners;
  }
}
