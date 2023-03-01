#!/bin/bash
# For support auto update Razor .cshtml change content.
dotnet add package Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation --version 6.0.12
# For support use EF Core about command line.
dotnet add package Microsoft.EntityFrameworkCore --version 6.0.12
# For use postgresql database.
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 6.0.8
# For use 'dotnet ef migrations add' command line create table in table.
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 6.0.8
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 6.0.12
# For use 'dotnet aspnet-codegenerator' command create controller and view.
dotnet tool uninstall -g dotnet-aspnet-codegenerator
dotnet tool install -g dotnet-aspnet-codegenerator
dotnet add package Microsoft.VisualStudio.Web.CodeGeneration.Design --version 6.0.8
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore --version 6.0.12
dotnet add package Microsoft.AspNetCore.Identity.UI --version 6.0.12
dotnet add package Stripe.net --version 41.5.0
dotnet add package Microsoft.AspNetCore.Authentication.Facebook --version 6.0.12
dotnet add package Microsoft.AspNetCore.Authentication.Google --version 6.0.12
dotnet add package Microsoft.AspNetCore.Authentication.MicrosoftAccount --version 6.0.12
dotnet add package Quartz --version 3.6.2
dotnet add package Quartz.Extensions.Hosting --version 3.6.2