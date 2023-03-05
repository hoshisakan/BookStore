using HoshiBookWeb.Tools;

using System.Diagnostics;
using Quartz;


namespace HoshiBookWeb.QuartzPostgreSQLBackupScheduler;

public class BackupJob : IJob
{
    private readonly ILogger<BackupJob>? _logger;

    public BackupJob(ILogger<BackupJob> logger)
    {
        _logger = logger;
    }

    public Task Execute(IJobExecutionContext context)
    {
        try
        {
            _logger?.LogInformation("Backup job started.");
            string id = Guid.NewGuid().ToString();
            JobKey key = context.JobDetail.Key;
            JobDataMap dataMap = context.Trigger.JobDataMap;

            //TODO Get schedule execute time.
            string? scheduleUTCExecuteTime = context.ScheduledFireTimeUtc.ToString() ?? DateTime.UtcNow.ToString();
            string? scheduleLocalExecuteFormatTime = DateTime.Parse(scheduleUTCExecuteTime).ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
            // string? backupFileDateTime = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
            string? backupFileTime = DateTime.Now.ToString("HH_mm_ss");
            string backupFileDate = DateTime.Now.ToString("yyyy_MM_dd");

            //TODO Read variables from job data map by key.
            // string? backupStoragePath = dataMap.GetString("backupStoragePath");
            string? backupDbName = dataMap.GetString("backupTarget");

            //TODO Read variables from environment variables.
            string? backupStoragePath = Environment.GetEnvironmentVariable("POSTGRES_DATA_BACKUP_PATH");
            string? postgresUser = Environment.GetEnvironmentVariable("DOTNET_POSTGRES_USER");
            string? postgresHost = Environment.GetEnvironmentVariable("DOTNET_POSTGRES_HOST_IP");
            string? postgresPort = Environment.GetEnvironmentVariable("DOTNET_POSTGRES_PORT");

            //TODO Check POSTGRES_USER, POSTGRES_HOST_IP, POSTGRES_PORT whether null or not.
            if (string.IsNullOrEmpty(postgresUser) || string.IsNullOrEmpty(postgresHost) || string.IsNullOrEmpty(postgresPort))
            {
                throw new Exception(
                    $"POSTGRES_USER or POSTGRES_HOST_IP or POSTGRES_PORT is null that event occurred at {scheduleLocalExecuteFormatTime}."
                );
            }

            //TODO Check backupStoragePath and backupDbName whether null or not.
            if (string.IsNullOrEmpty(backupDbName) || string.IsNullOrEmpty(backupStoragePath))
            {
                throw new Exception($"backupDbName or backupStoragePath is null that event occurred at {scheduleLocalExecuteFormatTime}..");
            }

            //TODO Set backup file name and path and command.
            string backupFileName = $"{backupFileTime}_{backupDbName}_db_backup.dump";
            string backupDirectory = $"{backupStoragePath}/{backupDbName}/{backupFileDate}";
            string backupFullPath = Path.Combine(backupDirectory, backupFileName);
            string backupCommand = $" -U {postgresUser} -h {postgresHost} -p {postgresPort} -d {backupDbName} -f {backupFullPath}";

            //TODO Create backup directory if not exists.
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

            //TODO Check backup whether success or not by checking file exists.
            if (FileTool.CheckFileExists(backupFullPath))
            {
                _logger?.LogInformation($"Backup job {id} - {backupDbName} - {backupFullPath} executed at {scheduleLocalExecuteFormatTime}.");
            }
            else
            {
                throw new Exception($"Backup job {id} - {backupDbName} - {backupFullPath} failed at {scheduleLocalExecuteFormatTime}.");
            }
            _logger?.LogInformation("Backup job finish.");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex.Message);
        }
        return Task.CompletedTask;
    }
}
