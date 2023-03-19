namespace HoshiBookWeb.Tools
{
    public class FileUploadTool
    {
        public FileUploadTool()
        {
        }

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

        public static bool IsContainsExtension(string fileExtension, string mode)
        {
            string[] allowedImageExtensions = new string[]{};
            string[] allowedDocumentExtensions = new string[]{};
            string[] allowedImportExtensions = new string[]{};

            if (string.IsNullOrEmpty(fileExtension))
            {
                return false;
            }

            fileExtension = fileExtension.ToLower();
            allowedImageExtensions = new string[] { ".jpg", ".jpeg", ".png", ".webp" };
            allowedDocumentExtensions = new string[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx" };
            allowedImportExtensions = new string[] { ".xls", ".xlsx" };

            if (mode == "image")
            {
                return allowedImageExtensions.Contains(fileExtension);
            }
            else if (mode == "document")
            {
                return allowedDocumentExtensions.Contains(fileExtension);
            }
            else if (mode == "import")
            {
                return allowedImportExtensions.Contains(fileExtension);
            }
            else
            {
                return false;
            }
        }

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