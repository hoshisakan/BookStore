#!/bin/bash
if [ -z "$1" ]; then
    echo "Error, please enter a valid options."
    exit 1
fi
if [ -z "$2" ]; then
    echo "Error, please enter a valid model name."
    exit 1
fi
if [ -z "$3" ]; then
    echo "Error, please enter a valid view file storage directory name."
    exit 1
fi
if [ -z "$4" ]; then
    echo "Error, please enter a valid dbContext filename."
    exit 1
fi

# Example: dotnet aspnet-codegenerator view MyEdit Edit -m Movie -dc MovieContext -outDir Views/Movies
# Create an empty view page file.
if [ ! -z "$5" ]; then
    # dotnet aspnet-codegenerator view [arguments] [options] -m [model] -outDir Views\\[view_dirname] -dc [dbContext_file]
    if [ ! -z "$1" ] && [ "$1" = "-e" ]; then
        dotnet aspnet-codegenerator view $5 Empty -m $2 -outDir Views\\$3 -dc $4 -udl -f
    else
        echo "Error, invalid option '$1'."
        exit 1
    fi
# Create specify arguments view page file.
else
    # dotnet aspnet-codegenerator view [arguments] [options] -m [model] -outDir Views\\[view_dirname] -dc [dbContext_file]
    if [ ! -z "$1" ] && [ "$1" = "-l" ]; then
        dotnet aspnet-codegenerator view Index List -m $2 -outDir Views\\$3 -dc $4 -udl -f
    elif [ ! -z "$1" ] && [ "$1" = "-c" ]; then
        dotnet aspnet-codegenerator view Create Create -m $2 -outDir Views\\$3 -dc $4 -udl -f
    elif [ ! -z "$1" ] && [ "$1" = "-u" ]; then
        dotnet aspnet-codegenerator view Edit Edit -m $2 -outDir Views\\$3 -dc $4 -udl -f
    elif [ ! -z "$1" ] && [ "$1" = "-d" ]; then
        dotnet aspnet-codegenerator view Details Details -m $2 -outDir Views\\$3 -dc $4 -udl -f
    elif [ ! -z "$1" ] && [ "$1" = "-r" ]; then
        dotnet aspnet-codegenerator view Delete Delete -m $2 -outDir Views\\$3 -dc $4 -udl -f
    else
        echo "Error, invalid option '$1'."
        exit 1
    fi
fi

if [ $? -eq 0 ]; then
    echo "Create view file has been completed successfully."
else
    echo "Error, create view file failed."
    exit 1
fi