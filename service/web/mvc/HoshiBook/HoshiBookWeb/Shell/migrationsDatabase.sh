#!/bin/bash
dotnet build
dotnet ef migrations add $1
dotnet ef database update

dotnet ef migrations add AddCoverTypeToDb