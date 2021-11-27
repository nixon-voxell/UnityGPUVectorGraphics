using System;
using UnityEngine;
using Unity.Mathematics;

namespace Voxell.GPUVectorGraphics
{
  /// <summary>
  /// The various ways corners in an SVG file can be formed.
  /// </summary>
  public enum Corner
  { 
    /// <summary>A half circle to round the end.</summary>
    Round,

    /// <summary>A flat connection.</summary>
    Bevel,

    /// <summary>Extrapolate sides with straight lines to where they collide.</summary>
    Miter,

    /// <summary>Extrapolate sides continuing their curve to where they collide.</summary>
    Arc
  }

  /// <summary>The style of an unconnected edge's end.</summary>
  public enum Cap
  { 
    /// <summary>Stop instantly.</summary>
    Butt,

    /// <summary>Round it out with a half circle.</summary>
    Round,

    /// <summary>Add an additional half square, based off the width of the edge.</summary>
    Square
  }

  [Serializable]
  public struct CubicSegment
  {
    /// <summary>Origin point of the segment.</summary>
    public float2 p0;

    /// <summary>First control point of the segment.</summary>
    public float2 p1;

    /// <summary>Second control point of the segment.</summary>
    public float2 p2;

    /// <summary>Ending point of the segment.</summary>
    public float2 p3;

    public CubicSegment(float2 p0, float2 p1, float2 p2, float2 p3)
    {
      this.p0 = p0;
      this.p1 = p1;
      this.p2 = p2;
      this.p3 = p3;
    }
  }

  [Serializable]
  public struct QuadraticSegment
  {
    /// <summary>Origin point of the segment.</summary>
    public float2 p0;

    /// <summary>First control point of the segment.</summary>
    public float2 p1;

    /// <summary>Ending point of the segment.</summary>
    public float2 p2;

    public QuadraticSegment(float2 p0, float2 p1, float2 p2)
    {
      this.p0 = p0;
      this.p1 = p1;
      this.p2 = p2;
    }
  }

  [Serializable]
  public struct CubicPathSegment
  {
    /// <summary>Origin point of the segment.</summary>
    public float2 p0;

    /// <summary>First control point of the segment.</summary>
    public float2 p1;

    /// <summary>Second control point of the segment.</summary>
    public float2 p2;

    public CubicPathSegment(float2 p0, float2 p1, float2 p2)
    {
      this.p0 = p0;
      this.p1 = p1;
      this.p2 = p2;
    }
  }

  [Serializable]
  public struct QuadraticPathSegment
  {
    /// <summary>Origin point of the segment.</summary>
    public float2 p0;

    /// <summary>First control point of the segment.</summary>
    public float2 p1;

    public QuadraticPathSegment(float2 p0, float2 p1)
    {
      this.p0 = p0;
      this.p1 = p1;
    }
  }

  [Serializable]
  public struct CubicContour
  {
    /// <summary>An array of every cubic path segments on the contour.</summary>
    public CubicPathSegment[] segments;

    /// <summary>A closed loop contour.</summary>
    public bool closed;
  }

  [Serializable]
  public struct QuadraticContour
  {
    /// <summary>An array of every quadratic path segments on the contour.</summary>
    public QuadraticPathSegment[] segments;

    /// <summary>A closed loop contour.</summary>
    public bool closed;
  }

  [Serializable]
  public struct PathProperties
  {
    /// <summary>How the beginning of the path should be displayed.</summary>
    public Cap head;

    /// <summary>How the end of the path should be displayed.</summary>
    public Cap tail;

    /// <summary>How the corners of the path should be displayed.</summary>
    public Corner corner;
  }
}
