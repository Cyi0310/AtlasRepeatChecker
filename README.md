# AtlasRepeatChecker
A Unity Editor tool to detect duplicate textures across multiple Sprite Atlases by analyzing their GUIDs.

## Description
AtlasRepeatChecker helps Unity developers identify textures that are being used by multiple Sprite Atlases, which can lead to unnecessary asset repeat and increased build size. The tool analyzes atlas files at the GUID level to ensure accurate detection of shared resources.

## Demo

<!-- > ![](https://github.com/user-attachments/assets/cff8a344-2218-4361-ad31-a73f6cea1345) -->

## Features
- Multi-Source Support: Analyze multiple folders and individual atlas files
- Drag & Drop Interface: Easy-to-use interface with drag and drop support
- GUID-Based Detection: Accurate duplicate detection using Unity's internal GUID system
- Visual Results: Display duplicate textures with previews and usage information
- Search & Filter: Quick search functionality to find specific duplicates
- Asset Navigation: One-click navigation to atlas files and textures

## Usage
1. Open the tool via `Tools > AtlasRepeatChecker`
2. Add sources by:
   - Dragging folders/atlases into the interface
   - Using "Add Folder" or "Add Atlas" buttons
3. Click "Analyze Atlas Guids" to start analysis
4. Review results and navigate to duplicate textures

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
- Requires Unity Editor (Runtime not supported)

## License
This tool is provided as-is for educational and development purposes.
Contributing
Feel free to modify and improve the tool according to your project needs. 
