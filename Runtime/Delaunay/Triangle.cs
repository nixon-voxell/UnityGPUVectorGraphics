using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Mathematics;

namespace Voxell.GPUVectorGraphics.Delaunay
{
  public class Triangle
  {
    public Point[] Vertices { get; } = new Point[3];
    public float2 Circumcenter { get; private set; }
    public float RadiusSquared;

    public Triangle(Point point1, Point point2, Point point3)
    {
      // In theory this shouldn't happen, but it was at one point so this at least makes sure we're getting a
      // relatively easily-recognised error message, and provides a handy breakpoint for debugging.
      if (point1 == point2 || point1 == point3 || point2 == point3)
      {
        throw new ArgumentException("Must be 3 distinct points");
      }

      if (!IsCounterClockwise(point1, point2, point3))
      {
        Vertices[0] = point1;
        Vertices[1] = point3;
        Vertices[2] = point2;
      }
      else
      {
        Vertices[0] = point1;
        Vertices[1] = point2;
        Vertices[2] = point3;
      }
      UpdateCircumcircle();
    }

    private void UpdateCircumcircle()
    {
      // https://codefound.wordpress.com/2013/02/21/how-to-compute-a-circumcircle/#more-58
      // https://en.wikipedia.org/wiki/Circumscribed_circle
      Point p0 = Vertices[0];
      Point p1 = Vertices[1];
      Point p2 = Vertices[2];
      float dA = p0.coordinate.x * p0.coordinate.x + p0.coordinate.y * p0.coordinate.y;
      float dB = p1.coordinate.x * p1.coordinate.x + p1.coordinate.y * p1.coordinate.y;
      float dC = p2.coordinate.x * p2.coordinate.x + p2.coordinate.y * p2.coordinate.y;

      float aux1 = (dA * (p2.coordinate.y - p1.coordinate.y) + dB * (p0.coordinate.y - p2.coordinate.y) + dC * (p1.coordinate.y - p0.coordinate.y));
      float aux2 = -(dA * (p2.coordinate.x - p1.coordinate.x) + dB * (p0.coordinate.x - p2.coordinate.x) + dC * (p1.coordinate.x - p0.coordinate.x));
      float div = (2 * (p0.coordinate.x * (p2.coordinate.y - p1.coordinate.y) + p1.coordinate.x * (p0.coordinate.y - p2.coordinate.y) + p2.coordinate.x * (p1.coordinate.y - p0.coordinate.y)));
      div += Mathf.Epsilon;

      float2 center = new float2(aux1 / div, aux2 / div);
      Circumcenter = center;
      RadiusSquared = (center.x - p0.coordinate.x) * (center.x - p0.coordinate.x) + (center.y - p0.coordinate.y) * (center.y - p0.coordinate.y);
    }

    private bool IsCounterClockwise(Point point1, Point point2, Point point3)
    {
      var result = (point2.coordinate.x - point1.coordinate.x) * (point3.coordinate.y - point1.coordinate.y) -
        (point3.coordinate.x - point1.coordinate.x) * (point2.coordinate.y - point1.coordinate.y);
      return result > 0;
    }

    public bool IsPointInsideCircumcircle(Point point)
    {
      var d_squared = (point.coordinate.x - Circumcenter.x) * (point.coordinate.x - Circumcenter.x) +
        (point.coordinate.y - Circumcenter.y) * (point.coordinate.y - Circumcenter.y);
      return d_squared < RadiusSquared;
    }
  }
}