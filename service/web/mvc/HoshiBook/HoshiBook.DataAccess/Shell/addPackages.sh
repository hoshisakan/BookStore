#!/bin/bash
dotnet add package Microsoft.EntityFrameworkCore --version 6.0.12
dotnet add package Microsoft.EntityFrameworkCore.Relational --version 6.0.12
# For use postgresql database.
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 6.0.8
# For use 'dotnet ef migrations add' command line create table in table.
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 6.0.8
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 6.0.12
# For use identity in 'dbContext' file.
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore --version 6.0.12
dotnet add package Microsoft.AspNetCore.Identity.UI --version 6.0.12