#!/bin/bash
# syntax reference: https://stackoverflow.com/questions/49805204/adding-reference-to-another-project-from-visual-studio-code
# cd [Project2.Executable] => [Added Reference Project]
# dotnet add reference [../Project1.Api/Project1.Api.csproj] => [Your Reference Project Directory]
cd D:\\Files\\Learn\\ASP_Dotnet_Core_Learn\\BookStore\\service\\web\\mvc\\HoshiBook\\HoshiBook.DataAccess
dotnet add reference ../HoshiBook.Models/HoshiBook.Models.csproj