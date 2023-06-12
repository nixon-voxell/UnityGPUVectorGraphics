using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;

namespace Voxell.GPUVectorGraphics
{
    using Util;

    public partial class CDT
    {
        // inserting quadratic control points from the contour
        [BurstCompile]
        private struct QuadraticInsertJob : IJob
        {
            public NativeArray<float2> na_points;
            public NativeArray<float2> na_controlPoints;
            public NativeList<int> na_triangles;

            public void Execute()
            {
                NativeParallelMultiHashMap<int, int> na_pointTriMap = new NativeParallelMultiHashMap<int, int>(
                  na_points.Length, Allocator.Temp
                );

                // create point idx to related triangles map
                for (int t = 0, triangleCount = na_triangles.Length / 3; t < triangleCount; t++)
                {
                    int t0, t1, t2;
                    GetTriangleIndices(in na_triangles, t, out t0, out t1, out t2);

                    na_pointTriMap.Add(t0, t);
                    na_pointTriMap.Add(t1, t);
                    na_pointTriMap.Add(t2, t);
                }

                for (int p = 0, pointCount = na_controlPoints.Length; p < pointCount; p++)
                {
                    float2 controlPoint = na_controlPoints[p];
                    // ignore if control point location is exactly same as the triangulation point
                    if (na_points[p].Equals(controlPoint)) continue;

                    // contour edge
                    int nextP = (p + 1) % pointCount;
                    Edge edge = new Edge(p, nextP);
                    float2 p0 = na_points[edge.e0];
                    float2 p1 = na_points[edge.e1];
                    float2 n = math.normalize(mathx.perpendicular(p1 - p0));

                    // if point is outside the contour, directly triangulate it as it will not overlap any triangles
                    if (math.dot(controlPoint - p0, n) > 0.0f)
                    {
                        // 
                    }
                    else
                    // if point is inside the contour, check for triangle intersections
                    // remove them and then retriangulate them
                    {
                        NativeParallelMultiHashMap<int, int>.Enumerator enumerator0 = na_pointTriMap.GetValuesForKey(edge.e0);
                        NativeParallelMultiHashMap<int, int>.Enumerator enumerator1 = na_pointTriMap.GetValuesForKey(edge.e1);

                        // line 0: p0, controlPoint
                        // line 1: p1, controlPoint

                        NativeList<int> na_intersectedTriangles = new NativeList<int>(Allocator.Temp);

                        foreach (int t in enumerator0)
                        {
                            int t0, t1, t2;
                            GetTriangleIndices(in na_triangles, t, out t0, out t1, out t2);
                        }
                    }
                }
            }
        }
    }
}
