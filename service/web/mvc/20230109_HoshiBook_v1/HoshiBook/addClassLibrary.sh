#!/bin/bash
if [ -z "$1" ]; then
    echo "ERROR: Please enter a valid class library name and solution project filename."
else
    echo "Starting create a class library $1."
    dotnet new classlib -o $1
    echo "Finish create a class library $1."
    echo "Starting add class library $1 to solution project $1."
    dotnet sln add $1/$1.csproj
    echo "Finish add class library $1 to solution project $1."
fi
