using System.Collections.Generic;
using System.Linq;
// using UnityEngine;
using Unity.Mathematics;

namespace Voxell.GPUVectorGraphics.Delaunay
{
  public class DelaunayTriangulator
  {
    private List<float2> _points;
    private List<int> _triangles;
    private List<int> _edges;
    private List<float2> _circumcenters;
    private List<float> _squaredRadiuses;

    public DelaunayTriangulator(float2 minRect, float2 maxRect)
    {
      float2 p0 = new float2(minRect);
      float2 p1 = new float2(0.0f, maxRect.y);
      float2 p2 = new float2(maxRect);
      float2 p3 = new float2(maxRect.x, 0.0f);

      _points.AddRange(new float2[4]{ p0, p1, p2, p3 });
      _triangles.AddRange(new int[3] { 0, 1, 2 });
      _triangles.AddRange(new int[3] { 0, 2, 3 });
    }

    public IEnumerable<Triangle> BowyerWatsonTriangulation(IEnumerable<Point> points)
    {
      HashSet<Triangle> triangulation = new HashSet<Triangle>();

      foreach (var point in points)
      {
        HashSet<Triangle> badTriangles = FindBadTriangles(point, triangulation);
        List<Edge> polygon = FindHoleBoundaries(badTriangles);
        triangulation.RemoveWhere(o => badTriangles.Contains(o));

        foreach (var edge in polygon.Where(possibleEdge => possibleEdge.Point1 != point && possibleEdge.Point2 != point))
        {
          var triangle = new Triangle(point, edge.Point1, edge.Point2);
          triangulation.Add(triangle);
        }
      }

      return triangulation;
    }

    private List<Edge> FindHoleBoundaries(HashSet<Triangle> badTriangles)
    {
      List<Edge> edges = new List<Edge>();
      foreach (var triangle in badTriangles)
      {
        edges.Add(new Edge(triangle.Vertices[0], triangle.Vertices[1]));
        edges.Add(new Edge(triangle.Vertices[1], triangle.Vertices[2]));
        edges.Add(new Edge(triangle.Vertices[2], triangle.Vertices[0]));
      }
      // IEnumerable<IGrouping<Edge, Edge>> grouped = edges.GroupBy(o => o);
      IEnumerable<Edge> boundaryEdges = edges.GroupBy(o => o).Where(o => o.Count() == 1).Select(o => o.First());
      return boundaryEdges.ToList();
    }

    private HashSet<Triangle> FindBadTriangles(Point point, HashSet<Triangle> triangles)
    {
      IEnumerable<Triangle> badTriangles = triangles.Where(o => o.IsPointInsideCircumcircle(point));
      return new HashSet<Triangle>(badTriangles);
    }
  }
}