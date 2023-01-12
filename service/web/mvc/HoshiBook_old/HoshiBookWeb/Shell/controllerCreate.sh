#!/bin/bash
if [ -z "$1" ]; then
    echo "Error, please specify a valid controller name and model name and dbcontext filename."
    exit 1
else
    echo "Starting create controller '$1'."
    dotnet aspnet-codegenerator controller -name $1 -outDir Controllers -f
    echo "Finish create controller '$1'."
fi

if [ $? -eq 0 ]; then
    echo "Create controller '$1' completed successfully."
else
    echo "Error, create controller failed."
    exit 1
fi