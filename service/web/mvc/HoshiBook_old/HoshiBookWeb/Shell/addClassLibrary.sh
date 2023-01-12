#!/bin/bash
if [ -z "$1" ] || [ -z "$2" ]; then
    echo "ERROR: Please enter a valid class library name and solution project filename."
else
    echo "Starting switch to project directory $1."
    cd $1
    echo "Finish switch to project directory $1."
    echo "Starting create a class library $2."
    dotnet new classlib -o $2
    echo "Finish create a class library $2."
    echo "Starting add class library $2 to solution project $2."
    dotnet sln add $2/$2.csproj
    echo "Finish add class library $2 to solution project $2."
fi
