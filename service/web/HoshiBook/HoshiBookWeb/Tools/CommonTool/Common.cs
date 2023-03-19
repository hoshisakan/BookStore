using HoshiBookWeb.Tools.RuntimeInfoTool;

namespace HoshiBookWeb.Tools.CommonTool
{
    public class Common : ICommon
    {
        private readonly IConfiguration? _config;

        public Common(IConfiguration config)
        {
            _config = config;
        }

        public string GetProductImageStoragePath()
        {
            string productImageStoragePath = "";
            var runtimeInfo = new RuntimeInfo();
            runtimeInfo.GetEnvironmentOSPlatform();

            if (_config == null)
            {
                throw new Exception("Configuration of ProgramInitializer class is null");
            }

            switch (runtimeInfo.GetEnvironmentOSPlatform())
            {
                case 0:
                    productImageStoragePath = Path.Combine(_config["StaticFiles:LocalTest:Path"], @"staticfiles\images\products");
                    break;
                case 1:
                    productImageStoragePath = Path.Combine(_config["StaticFiles:Deployment:Path"], @"staticfiles/images/products");
                    break;
                case 2:
                    throw new Exception("Not Support OS Platform 'OSX'");
                default:
                    throw new Exception("Unknown OS Platform");
            }
            return productImageStoragePath;
        }

        public string GetUploadFilesStoragePath()
        {
            string productImageStoragePath = "";
            var runtimeInfo = new RuntimeInfo();
            runtimeInfo.GetEnvironmentOSPlatform();

            if (_config == null)
            {
                throw new Exception("Configuration of ProgramInitializer class is null");
            }

            switch (runtimeInfo.GetEnvironmentOSPlatform())
            {
                case 0:
                    productImageStoragePath = Path.Combine(_config["StaticFiles:LocalTest:Path"], @"staticfiles\images\upload_files");
                    break;
                case 1:
                    productImageStoragePath = Path.Combine(_config["StaticFiles:Deployment:Path"], @"staticfiles/images/upload_files");
                    break;
                case 2:
                    throw new Exception("Not Support OS Platform 'OSX'");
                default:
                    throw new Exception("Unknown OS Platform");
            }
            return productImageStoragePath;
        }
    }
}