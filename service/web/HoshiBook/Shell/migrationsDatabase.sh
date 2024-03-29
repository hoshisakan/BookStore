#!/bin/bash
# Check ApplicationDbContext.cs have models of want to create before Use the shell script migrations database
# Usage: dotnet ef migrations add [arguments] [options]
# Arguments:
#   <NAME>  The name of the migration.
# Options:
#   -o|--output-dir <PATH>                 The directory (and sub-namespace) to use. Paths are relative to the project directory. Defaults to "Migrations".
#   --json                                 Show JSON output.
#   -c|--context <DBCONTEXT>               The DbContext to use.
#   -p|--project <PROJECT>                 The project to use.
#   -s|--startup-project <PROJECT>         The startup project to use.
#   --framework <FRAMEWORK>                The target framework.
#   --configuration <CONFIGURATION>        The configuration to use.
#   --msbuildprojectextensionspath <PATH>  The MSBuild project extensions path. Defaults to "obj".
#   -e|--environment <NAME>                The environment to use. Defaults to "Development".
#   -h|--help                              Show help information
#   -v|--verbose                           Show verbose output.
#   --no-color                             Don't colorize output.
#   --prefix-output   
if [ -z "$1" ]; then
    echo "ERROR: Please enter a valid migrations name."
    exit 1
else
    dotnet build
    # dotnet ef migrations add AddCategory --project HoshiBook.DataAccess -s HoshiBookWeb -c ApplicationDbContext --verbose
    dotnet ef migrations add $1 --project HoshiBook.DataAccess -s HoshiBookWeb -c ApplicationDbContext --verbose
    dotnet ef database update --project HoshiBook.DataAccess -s HoshiBookWeb -c ApplicationDbContext --verbose
fi
