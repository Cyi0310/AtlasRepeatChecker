# AtlasRepeatChecker
#### [README_ZH](https://github.com/Cyi0310/AtlasRepeatChecker/blob/main/README_ZH.md)

A Unity Editor tool that analyzes [Sprite Atlases](https://docs.unity3d.com/ScriptReference/U2D.SpriteAtlas.html) to detect repeat `Sprite/Texture` shared across multiple `atlases`.


## Why I made this tool
When `multiple atlases` contained the `same sprite`, several issues occur ([Unity Official Documentation](https://docs.unity3d.com/Documentation/Manual/sprite/atlas/distribution/resolve-different-sprite-atlas-scenarios.html)) :

1. Unity randomly loads sprites from one of the atlases at runtime, which may cause unexpected results
2. Each `atlas` reserves space for the duplicate `sprite`, leading to unnecessary resource duplication and increased build size
3. Console shows `xxx.Sprite matches more than one built-in atlases ... `warning log
4. During mid-to-late development phases, frequent changes to `atlas` and `sprite` relationships become difficult to track and manage


## Description
`AtlasRepeatChecker` helps Unity developers identify when `multiple atlases` contained the `same sprite`, which can lead to unnecessary resource duplication and increased build size. This tool analyzes sprite `GUIDs` within atlases to ensure accurate detection and verify that each `sprite` is contained by only one `atlas`.


## Demo
> ![](https://github.com/user-attachments/assets/f3bcfd8e-3aa9-4e89-bdbe-7e0d7d7f106f)


## Features
- Analyze `multiple folders` or `individual atlas files (.spriteatlas)` in your project
- Easy-to-use drag & drop interface
- Display sprites contained by multiple atlases with preview and direct navigation to their locations
- Simple search functionality to quickly find specific sprites


## Usage
1. Open the tool via (`Tools > AtlasRepeatChecker`)
2. Add sources by:
   - Dragging folders/atlases into `sources` area
   - Using "Add Folder" / "Add Atlas" buttons to select source
3. Click "Analyze Atlas Guids" to start analysis
4. Review results and navigate to duplicate atlases or sprites


## Installation
### Unity Package Manager (Recommended)
1. Open Unity Package Manager (`Window > Package Manager`)
2. Click the `+` button in the top-left corner
3. Select `Add package from git URL...`
4. Enter the following URL: `https://github.com/Cyi0310/AtlasRepeatChecker.git`
5. Click `Add`

## Requirements
- Unity 2021.3 or newer
- Windows/Mac/Linux compatible  
- Works in `Editor Mode` only, not supported in `Play Mode`

## License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
