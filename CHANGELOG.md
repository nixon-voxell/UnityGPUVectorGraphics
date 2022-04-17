## [0.4.0]

### New Features

<!-- - Removal of triangles that are outside the constraint contour. -->
<!-- - Adding control points into constraint triangulation. -->

### Changes

- Uses "rect-triangles" instead of "supra-triangles" for stability. This can prevent some points from excluding when the min and max rect is large.

## [0.3.0]

### New Features

- Implementation of constraint delaunay triangulation.

### Changes

- Removed `Voxell.GPUVectorGraphics.Delaunay` namespace.

## [0.2.0]

### New Features

- Bezier Properties for describing a shape made up of bezier curves.
- Open Font lightweight importer:
  - Native support.
  - Store data as glyphs. Each glyph will have a glyph contour containing a sequence of points in the order of p0-ctrl0-p1-ctrl1.
  - Character maps that maps each character to a glyph index.

### Bug Fixes

- Fixes uvw coordinate when being flipped.
- Prevent extra triangulation when encountering loop artifacts.

## [0.1.0]

- Initial release.