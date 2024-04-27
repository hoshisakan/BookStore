#!/bin/bash
# Publish the dotnet web app

SolutionPath="HoshiBook"
WebAppPath="HoshiBookWeb"
OutputPath="/deploy/HoshiBookWeb"

echo "Starting publish dotnet web app."

# if [ -d "$SolutionPath" ]; then
#     echo "Directory $SolutionPath exists."
#     cd $SolutionPath
# else
#     echo "Directory $SolutionPath does not exist."
#     exit 1
# fi

if [ -d "$WebAppPath" ]; then
    echo "Directory $WebAppPath exists."
    cd $WebAppPath
else
    echo "Directory $WebAppPath does not exist."
    exit 1
fi

if [ -d "$OutputPath" ]; then
    echo "Directory $OutputPath exists, will be deleting the directory."
    rm -rf $OutputPath
else
    echo "Directory $OutputPath does not exist."
fi

dotnet publish -c Release -o $OutputPath

if [ $? -eq 0 ]; then
    echo "Publish dotnet web app successfully."
else
    mkdir -p $OutputPath
    echo "Publish dotnet web app failed."
    exit 1
fi

cd ..

echo "Finish publish dotnet web app."