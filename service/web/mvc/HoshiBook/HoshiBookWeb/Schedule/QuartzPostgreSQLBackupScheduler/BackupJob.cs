using System.Diagnostics;
using Quartz;
using HoshiBookWeb.Tools;


namespace HoshiBookWeb.QuartzPostgreSQLBackupScheduler;

public class BackupJob : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        string id = Guid.NewGuid().ToString();

        try
        {
            JobKey key = context.JobDetail.Key;
            JobDataMap dataMap = context.Trigger.JobDataMap;

            string? scheduleUTCExecuteTime = context.ScheduledFireTimeUtc.ToString() ?? DateTime.UtcNow.ToString();
            string? scheduleLocalExecuteFormatTime = DateTime.Parse(scheduleUTCExecuteTime).ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
            string? backupFileGeneratedFormatTime = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
            // Read variables from job data map by key.
            // string? backupStoragePath = dataMap.GetString("backupStoragePath");
            string? backupStoragePath = Environment.GetEnvironmentVariable("POSTGRES_DATA_BACKUP_PATH");
            string? backupDbName = dataMap.GetString("backupTarget");
            string? postgresUser = Environment.GetEnvironmentVariable("DOTNET_POSTGRES_USER");
            string? postgresHost = Environment.GetEnvironmentVariable("DOTNET_POSTGRES_HOST_IP");
            string? postgresPort = Environment.GetEnvironmentVariable("DOTNET_POSTGRES_PORT");

            // Check POSTGRES_USER, POSTGRES_HOST_IP, POSTGRES_PORT whether null or not
            if (string.IsNullOrEmpty(postgresUser) || string.IsNullOrEmpty(postgresHost) || string.IsNullOrEmpty(postgresPort))
            {
                throw new Exception(
                    $"POSTGRES_USER or POSTGRES_HOST_IP or POSTGRES_PORT is null that event occurred at {scheduleLocalExecuteFormatTime}."
                );
            }

            // Check backupStoragePath and backupDbName whether null or not
            if (string.IsNullOrEmpty(backupDbName) || string.IsNullOrEmpty(backupStoragePath))
            {
                throw new Exception($"backupDbName or backupStoragePath is null that event occurred at {scheduleLocalExecuteFormatTime}..");
            }

            string backupFileName = $"{backupFileGeneratedFormatTime}_{backupDbName}_db_backup.dump";
            string backupFileDate = DateTime.Now.ToString("yyyy_MM_dd");
            string backupDirectory = $"{backupStoragePath}/{backupDbName}/{backupFileDate}";
            string backupFullPath = Path.Combine(backupDirectory, backupFileName);
            // string backupCommand = $" -U root -h postgresql-dev -p 5432 -d {backupDbName} -f {backupFullPath}";
            string backupCommand = $" -U {postgresUser} -h {postgresHost} -p {postgresPort} -d {backupDbName} -f {backupFullPath}";

            // Create backup directory if not exists
            if (!FileTool.CheckDirExists(backupDirectory))
            {
                FileTool.CreateDirectory(backupDirectory);
            }

            var process = new Process();
            var startInfo = new ProcessStartInfo();
            startInfo.FileName = "pg_dump";
            startInfo.Arguments = backupCommand;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.CreateNoWindow = true;
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
            process.Close();

            // Check backup whether success or not by checking file exists
            if (FileTool.CheckFileExists(backupFullPath))
            {
                Console.WriteLine($"Backup job {id} - {backupDbName} - {backupFullPath} executed at {scheduleLocalExecuteFormatTime}.");
            }
            else
            {
                throw new Exception($"Backup job {id} - {backupDbName} - {backupFullPath} failed at {scheduleLocalExecuteFormatTime}.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        return Task.CompletedTask;
    }
}
