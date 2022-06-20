## [0.3.3]

### New Features

- `FontCurve` stores an array of precomputed meshes on import.
- Implementation of binary search to search for character's glyph/mesh index.

### Changes

- Burst Compile uses high precision float and strict float mode.
- Reduced `MARGIN` from 10.0f to 1.0f.
- Renamed `Circumcenter` to `Circumcircle`.
- Renamed `GenerateMeshDataFromGlyph` to `ExtractGlyphData`.
- Increased `FontImporter` verion from 2 to 3.
- Instead of `charMaps`, `FontCurve` now stores 2 separate arrays:
  - `charCodes`: an array of supported characters.
  - `glyphIndices`: an array of glyph indices corresponding to the supported character codes.
- Uses "super-triangle" instead of "rect-triangles" as there is an elegant way to create a huge triangle that encapsulates all points inside.
  1. Calculate bounding-box (min/max rect) of the points.
  2. Place the bounding-box on a line.
   ```
    __
   |__|
   ----
   ```
  3. Place 2 more of the same box at the top and bottom of the current box.
   ```
    __
   |__|
    __
   |__|
    __
   |__|
   ----
   ```
  4. Place another 2 more of the same box at the left and right of the bottom most box.
   ```
         __
        |__|
         __
        |__|
    __   __   __
   |__| |__| |__|
   --------------
   ```
  5. Now, connect the "left-bottom most" point, "right-bottom most" point, and the "middle-top-most" point together.
   ```
         __                 /\      
        |__|               /__\     
         __               / __ \    
        |__|             / |__| \   
    __   __   __       _/   __   \_ 
   |__| |__| |__|     |/_| |__| |_\|
   ----------------------------------
   ```
  6. And now you get a "super-triangle" that encapsulates all the points in the middle box! (of course, you can add some kind of margin to the box itself to enlarge the "super-triangle" just to make sure!)
- Added "alpha" to package version.

### Bug Fixes

- `PointInTriangle` method in `VGMath` is much more accurate (to handle edge cases) and computationally cheap.
- Removed addition of `EPSILON` to `div` in `Cirumcircle` to prevent numerical inaccuracy.
- `TriEdgeIntersect` method now checks for similarity based on point position rather than index as there might be duplicated points that have a different index.

## [0.3.2]

### Changes

- Major code refactoring for CDT triangulation and constraint:
  - Generalize procedures into static function calls.
  - Create functions to prevent repetitive code.
- CDT utility functions are now separated into 3 files:
  - `CDT.Util` for general utility purposes.
  - `CDT.TriangulationUtil` for delaunay triangulation utility purposes.
  - `CDT.ConstraintUtil` for constrained delaunay triangulation utility purposes.

### Bug Fixes

- Removed `Debug.Log` calls from a static function that is being called in a burst compiled code.

## [0.3.1]

### New Features

- Removal of triangles that are outside the constraint contour.

### Changes

- Uses "rect-triangles" instead of "super-triangles" for stability. This can prevent some points from being excluded when the min and max rect is large.

## [0.3.0]

### New Features

- Implementation of constrained delaunay triangulation.

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