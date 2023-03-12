#!/bin/bash

if [ -z "$1" ]; then
    echo "ERROR: Missing area name, please enter a valid area name."
    exit 1
else
    echo "Starting to create a new MVC Area '$1'."
    dotnet-aspnet-codegenerator area $1
    echo "Finish to create a new MVC Area '$1'."
fi