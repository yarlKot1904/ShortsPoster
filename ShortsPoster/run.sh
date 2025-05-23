#!/bin/bash

echo "Cleaning previous builds..."
dotnet clean

echo "Building the project..."
dotnet build

echo "Applying EF Core migrations..."
dotnet ef database update

echo "Starting ngrok tunnel..."
ngrok http --domain=ample-informally-crawdad.ngrok-free.app 5166 &

echo "Running ASP.NET Core app..."
dotnet run
