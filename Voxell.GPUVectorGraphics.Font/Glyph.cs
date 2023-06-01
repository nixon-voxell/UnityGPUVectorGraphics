using Unity.Mathematics;

namespace Voxell.GPUVectorGraphics.Font
{
    [System.Serializable]
    public struct Glyph
    {
        [System.Serializable]
        public struct CompositeReference
        {
            /// <summary>The offset for the referenced glyph.</summary>
            public float2 offset;

            /// <summary>The X axis for the referenced glyph.</summary>
            public float2 xAxis;

            /// <summary>The Y axis for the referenced glyph.</summary>
            public float2 yAxis;

            /// <summary>The index of the glyph being referenced.</summary>
            public int glyphRef;
        }

        /// <summary>All contours in the glyph.</summary>
        public QuadraticContour[] contours;

        /// <summary>Composition of referenes of other glyphs.</summary>
        public CompositeReference[] compositeReferences;

        /// <summary>Determines if this glyph is made up of other glyph or a glyph on its own.</summary>
        public bool isComplex;

        /// <summary>Bottom left of the glyph's bounding box.</summary>
        public float2 minRect;
        /// <summary>Top right of the glyph's bounding box.</summary>
        public float2 maxRect;
    }
}
