namespace HoshiBookWeb.Tools
{
    public class FileUploadTool
    {
        // public static string UploadImage(IFormFile file, string webRootPath, string folderName)
        // {
        //     string fileName = Guid.NewGuid().ToString();
        //     var uploads = Path.Combine(webRootPath, folderName);
        //     var extension = Path.GetExtension(file.FileName);
        //     using (var fileStream = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
        //     {
        //         file.CopyTo(fileStream);
        //     }
        //     return Path.Combine(folderName, fileName + extension);
        // }

        // public static bool UploadImage(IFormFile file, string uploads)
        // {
        //     string filename = Guid.NewGuid().ToString();
        //     string extension = Path.GetExtension(file.FileName);
        //     string? storagePath = Path.Combine(uploads, filename + extension);

        //     using (var fileStream = new FileStream(storagePath, FileMode.Create))
        //     {
        //         file.CopyTo(fileStream);
        //     }
        //     return FileTool.CheckFileExists(storagePath);
        // }

        public static bool UploadImage(IFormFile file, string filename, string extension, string uploads)
        {
            string? storagePath = Path.Combine(uploads, filename + extension);

            using (var fileStream = new FileStream(storagePath, FileMode.Create))
            {
                file.CopyTo(fileStream);
            }
            return FileTool.CheckFileExists(storagePath);
        }
    }
}