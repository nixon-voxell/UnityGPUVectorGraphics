using Unity.Mathematics;

namespace Voxell.GPUVectorGraphics
{
  public partial class CDT
  {
    public struct Edge : System.IEquatable<Edge>
    {
      public int e0, e1;

      public Edge(int e0, int e1)
      {
        this.e0 = e0;
        this.e1 = e1;
      }

      public void SetEdge(int e0, int e1)
      {
        this.e0 = e0;
        this.e1 = e1;
      }

      public bool Equals(Edge other)
        => (this.e0 == other.e0 && this.e1 == other.e1) || 
        (this.e0 == other.e1 && this.e1 == other.e0);
    }

    public struct Circumcenter
    {
      public float2 center;
      public float sqradius;

      public Circumcenter(float2 p0, float2 p1, float2 p2)
      {
        float dA = p0.x * p0.x + p0.y * p0.y;
        float dB = p1.x * p1.x + p1.y * p1.y;
        float dC = p2.x * p2.x + p2.y * p2.y;

        float aux1 = dA * (p2.y - p1.y) + dB * (p0.y - p2.y) + dC * (p1.y - p0.y);
        float aux2 = -(dA * (p2.x - p1.x) + dB * (p0.x - p2.x) + dC * (p1.x - p0.x));
        float div = 2.0f * (p0.x * (p2.y - p1.y) + p1.x * (p0.y - p2.y) + p2.x * (p1.y - p0.y));
        div += math.EPSILON;
        div = 1.0f / div;

        center = new float2(aux1, aux2) * div;
        sqradius = math.lengthsq(center - p0);
      }

      public bool ContainsPoint(float2 p)
      {
        float sqlength = math.lengthsq(center - p);
        return sqlength < sqradius;
      }
    }
  }
}