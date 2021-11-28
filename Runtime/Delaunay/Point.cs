using System.Collections.Generic;
using Unity.Mathematics;

namespace Voxell.GPUVectorGraphics.Delaunay
{
  public class Point
  {
    public float2 coordinate;
    // public HashSet<Triangle> adjacentTriangles;

    public Point(float x, float y) : this(new float2(x, y)) {}
    public Point(float2 coordinate)
    {
      this.coordinate = coordinate;
    }
  }
}