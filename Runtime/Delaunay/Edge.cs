namespace Voxell.GPUVectorGraphics.Delaunay
{
  public class Edge
  {
    public Point Point1 { get; }
    public Point Point2 { get; }

    public Edge(Point point1, Point point2)
    {
      Point1 = point1;
      Point2 = point2;
    }

    public override bool Equals(object obj)
    {
      if (obj == null) return false;
      if (obj.GetType() != GetType()) return false;
      var edge = obj as Edge;

      var samePoints = Point1 == edge.Point1 && Point2 == edge.Point2;
      var samePointsReversed = Point1 == edge.Point2 && Point2 == edge.Point1;
      return samePoints || samePointsReversed;
    }

    public override int GetHashCode()
    {
      int hCode = (int)Point1.coordinate.x ^ (int)Point1.coordinate.y ^ (int)Point2.coordinate.x ^ (int)Point2.coordinate.y;
      return hCode.GetHashCode();
    }
  }
}