#!/bin/bash
if [ -z "$1" ]; then
    echo "Error, please enter a valid options."
    exit 1
fi
if [ -z "$2" ]; then
    echo "Error, please enter a valid model name."
    exit 1
fi

dotnet aspnet-codegenerator view $1 Empty -outDir $2 -udl -f

if [ $? -eq 0 ]; then
    echo "Create empty view file $1 has been completed successfully."
else
    echo "Error, create empty view file $1 failed."
    exit 1
fi