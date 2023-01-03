#!/bin/bash
dotnet build
dotnet ef migrations add $1
dotnet ef database update