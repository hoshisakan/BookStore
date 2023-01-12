-------------------------------------------------------------------------------------------
#TODO create asp.net core 6.0 project and add that to the solution project.
sh ..\..\shell\mvc_create.sh HoshiBook HoshiBookWeb 6.0
-------------------------------------------------------------------------------------------
#TODO create table in specify database.
sh Shell\migrationsDatabase.sh AddCategoryToDatabase
-------------------------------------------------------------------------------------------
#TODO create an empty controller in project.
sh Shell\controllerCreate.sh CategoryController
-------------------------------------------------------------------------------------------
#TODO create an specify empty view page file in project.
#TODO -e is 'Empty'
sh Shell\viewCreate.sh -e Category Category ApplicationDbContext Index
-------------------------------------------------------------------------------------------
#TODO create an specify arguments view page file in project.
#TODO -l is 'List', '-c' is 'Create', '-u' is 'Update', '-r' is 'Delete',
sh Shell\viewCreate.sh -e Category Category ApplicationDbContext
-------------------------------------------------------------------------------------------
sh Shell\addClassLibrary.sh HoshiBook.DataAccess
sh Shell\addClassLibrary.sh HoshiBook.Models
sh Shell\addClassLibrary.sh HoshiBook.Utility
-------------------------------------------------------------------------------------------
example:
=> sh Shell\addClassLibrary.sh [Your Project Directory Path] [Create Class Library Name]

sh Shell\addClassLibrary.sh D:\Files\Learn\ASP_Dotnet_Core_Learn\BookStore\service\web\mvc\HoshiBook HoshiBook.DataAccess
sh Shell\addClassLibrary.sh D:\Files\Learn\ASP_Dotnet_Core_Learn\BookStore\service\web\mvc\HoshiBook HoshiBook.Models
sh Shell\addClassLibrary.sh D:\Files\Learn\ASP_Dotnet_Core_Learn\BookStore\service\web\mvc\HoshiBook HoshiBook.Utility
-------------------------------------------------------------------------------------------
# create class library and add that to the solution.
https://learn.microsoft.com/en-us/dotnet/core/tutorials/library-with-visual-studio-code?pivots=dotnet-6-0
dotnet new classlib -o HoshiBook.DataAccess
dotnet sln add HoshiBook.DataAccess/HoshiBook.DataAccess.csproj