#!/bin/bash
if [ -z "$1" ] || [ -z "$2" ] || [ -z "$3" ]; then
    echo "Error, please specify a valid controller name and model name and dbcontext filename."
    exit 1
else
    echo "Starting create controller '$1' by model $2."
    dotnet aspnet-codegenerator controller -name $1 -outDir Controllers -async -m $2 -dc $3 -udl -f
    echo "Finish create controller '$1' by model $2."
fi

if [ $? -eq 0 ]; then
    echo "Create controller '$1' has been completed successfully."
else
    echo "Error, create controller file failed by model $2."
    exit 1
fi