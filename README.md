# Vancouver City Simulator

A web-based city simulation game where players can build and manage their own version of Vancouver. Build residential, commercial, and industrial zones while managing your city's budget and population.

## Features

- Interactive map centered on Vancouver using OpenStreetMap
- Three building types: Residential, Commercial, and Industrial
- Building upgrade system with multiple levels
- Real-time income generation
- Budget management system
- Building placement validation based on map zones

## Setup

1. Clone this repository
2. Open `templates/index.html` in a web browser
3. Start building your city!

## How to Play

- Select a building type from the bottom menu
- Click on valid building zones (gray areas) on the map to place buildings
- Click existing buildings to upgrade them (up to level 3)
- Manage your budget while expanding your city
- Collect income from your buildings every 10 seconds

## Technologies Used

- HTML5
- CSS3
- JavaScript
- Leaflet.js for map rendering
- Font Awesome for icons
- OpenStreetMap for map data

## Features (Planned)
- 3D city visualization
- Resource management (budget, population, happiness)
- Building placement and management
- City services (police, fire, education, etc.)
- Traffic management
- Environmental impact
- Day/night cycle
- Weather system

## Requirements
- Unity 2022.3 LTS or later
- Visual Studio 2022 or VS Code with C# extensions
- Git (for version control)

## Project Structure
```
Assets/
├── Scripts/         # C# scripts for game logic
├── Models/          # 3D models and prefabs
├── Materials/       # Materials and textures
├── Scenes/          # Unity scenes
└── Resources/       # Runtime-loaded assets
```

## Getting Started
1. Clone this repository
2. Open the project in Unity Hub
3. Open the main scene in `Assets/Scenes/MainScene.unity`
4. Press Play to test the game

## Development
This project uses Unity's new Input System and follows SOLID principles for clean, maintainable code. 