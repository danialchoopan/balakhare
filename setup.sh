#!/bin/bash

# Exit on error
set -e

echo "Setting up Balakhare Application..."

# 1. Copy .env.example to .env if not exists
if [ ! -f .env ]; then
    echo "Creating .env file from .env.example..."
    cp .env.example .env
fi

# 2. Install dependencies and build
echo "Building the project..."
dotnet build src/Balakhare.slnx

# 3. Inform the user
echo ""
echo "Setup complete!"
echo "To run the application, use: dotnet run --project src/Balakhare.Web"
