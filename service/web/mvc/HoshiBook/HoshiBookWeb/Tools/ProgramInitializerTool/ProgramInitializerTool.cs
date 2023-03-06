namespace HoshiBookWeb.Tools.ProgramInitializerTool
{
    public class ProgramInitializer : IProgramInitializer
    {
        private readonly ILogger<ProgramInitializer>? _logger;
        private readonly IConfiguration? _config;
        
        public ProgramInitializer(
            ILogger<ProgramInitializer> logger, IConfiguration config
        )
        {
            _logger = logger;
            _config = config;
        }

        public string GetStaticFileStoragePath()
        {
            string staticFilesPath = "";

            if (_config == null)
            {
                throw new Exception("Configuration of ProgramInitializer class is null");
            }

            var runtimeInfo = new RuntimeInfoTool.RuntimeInfo();

            switch (runtimeInfo.GetEnvironmentOSPlatform())
            {
                case 0:
                    _logger?.LogInformation("OS Platform: Windows");
                    staticFilesPath = Path.Combine(
                        _config.GetSection("StaticFiles:LocalTest:Path").Get<string>(),
                        "staticfiles"
                    );
                    break;
                case 1:
                    _logger?.LogInformation("OS Platform: Linux");
                    staticFilesPath = Path.Combine(
                        _config.GetSection("StaticFiles:Deployment:Path").Get<string>(),
                        "staticfiles"
                    );
                    break;
                case 2:
                    throw new Exception("Not Support OS Platform 'OSX'");
                default:
                    throw new Exception("Unknown OS Platform");
            }
            _logger?.LogInformation("Static Files Path: {0}", staticFilesPath);
            return staticFilesPath;
        }
    }
}