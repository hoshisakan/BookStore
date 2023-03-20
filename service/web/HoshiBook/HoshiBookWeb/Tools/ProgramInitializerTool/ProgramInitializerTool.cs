namespace HoshiBookWeb.Tools.ProgramInitializerTool
{
    public class ProgramInitializer : IProgramInitializer
    {
        private readonly ILogger<ProgramInitializer>? _logger;
        private readonly IConfiguration? _config;
        private readonly int _osPlatform;
        
        public ProgramInitializer(
            ILogger<ProgramInitializer> logger, IConfiguration config
        )
        {
            _logger = logger;
            _config = config;
            _osPlatform = new RuntimeInfoTool.RuntimeInfo().GetEnvironmentOSPlatform();
        }

        public string GetStaticFileStoragePath()
        {
            string staticFilesPath = "";

            if (_config == null)
            {
                throw new Exception("Configuration of ProgramInitializer class is null");
            }

            // var runtimeInfo = new RuntimeInfoTool.RuntimeInfo();
            // switch (runtimeInfo.GetEnvironmentOSPlatform())

            switch (_osPlatform)
            {
                case 0:
                    _logger?.LogInformation("OS Platform: Windows");
                    staticFilesPath = Path.Combine(
                        _config.GetSection("StaticFiles:StoragePath:LocalTest:Path").Get<string>(),
                        _config.GetSection("StaticFiles:RequestPath").Get<string>()
                    );
                    break;
                case 1:
                    _logger?.LogInformation("OS Platform: Linux");
                    staticFilesPath = Path.Combine(
                        _config.GetSection("StaticFiles:StoragePath:Deployment:Path").Get<string>(),
                        _config.GetSection("StaticFiles:RequestPath").Get<string>()
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