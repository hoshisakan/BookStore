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



-------------------------------------------------------------------------------------------