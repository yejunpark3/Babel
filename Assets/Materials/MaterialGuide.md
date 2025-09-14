# Materials for Hexagonal Tile Map System

## Required Materials

Create the following materials in the Materials folder to properly visualize the hex tile map:

### 1. HexTileDefault.mat
- Shader: Standard
- Albedo: White (1, 1, 1, 1)
- Metallic: 0
- Smoothness: 0.5

### 2. HexTileHighlight.mat
- Shader: Standard
- Albedo: Yellow (1, 1, 0, 1)
- Emission: Enabled
- Emission Color: Yellow (1, 1, 0, 1) * 0.2
- Metallic: 0
- Smoothness: 0.5

### 3. HexTileSelected.mat
- Shader: Standard
- Albedo: Cyan (0, 1, 1, 1)
- Emission: Enabled
- Emission Color: Cyan (0, 1, 1, 1) * 0.3
- Metallic: 0
- Smoothness: 0.5

### 4. HexTilePath.mat
- Shader: Standard
- Albedo: Red (1, 0, 0, 1)
- Emission: Enabled
- Emission Color: Red (1, 0, 0, 1) * 0.4
- Metallic: 0
- Smoothness: 0.5

## Usage

Assign these materials to the HexTileMapInteraction component:
- highlightMaterial: HexTileHighlight
- selectedMaterial: HexTileSelected
- pathMaterial: HexTilePath

The default material will be automatically assigned to HexTileMapManager's hexTileMaterial field.