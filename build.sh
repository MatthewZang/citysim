#!/bin/bash

# Create build directory if it doesn't exist
mkdir -p build

# Build the game
echo "Building game..."
/Applications/Unity/Hub/Editor/2022.3.0f1/Unity.app/Contents/MacOS/Unity \
    -batchmode \
    -quit \
    -projectPath "$(pwd)" \
    -buildOSXUniversalPlayer "build/CitySim.app"

# Check if build was successful
if [ $? -eq 0 ]; then
    echo "Build successful!"
    echo "Starting game..."
    open "build/CitySim.app"
else
    echo "Build failed!"
    exit 1
fi 