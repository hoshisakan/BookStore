using System.Diagnostics;
using Quartz;

namespace HoshiBookWeb.QuartzPostgreSQLBackupScheduler;

public class BackupJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        string id = Guid.NewGuid().ToString();
        string? scheduleUTCExecuteTime = context.ScheduledFireTimeUtc.ToString();
        string scheduleLocalExecuteTime = DateTime.Parse(scheduleUTCExecuteTime).ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");

        string message = "This job will be executed postgreSQL database backup task at: " + scheduleLocalExecuteTime;
        // string message = "This job will be executed postgreSQL database backup task at: " + DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
        
        try
        {
            var process = new Process();
            var startInfo = new ProcessStartInfo();
            startInfo.FileName = "/bin/bash";
            startInfo.Arguments = "-c \"psql -U root -h 172.0.0.1 -p 5432 --no-password -d hoshi1 -n bookstore -f hosh1_test_backup.dump\"";
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            process.StartInfo = startInfo;
            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            await Console.Out.WriteLineAsync($"{id} - {message}, result:\n{result}");
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        // return Task.CompletedTask;
    }
}
