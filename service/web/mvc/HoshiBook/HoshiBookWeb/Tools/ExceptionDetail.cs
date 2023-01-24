using System;


namespace HoshiBookWeb.Tools {
    public class ExceptionDetail {
        public string? errorFileName { get; set; }
        // public string errorClassName { get; set; }
        public int errorFileColumnNumber { get; set; }
        public int errorFileLineNumber { get; set; }
        public string? errorMethod { get; set; }
        public string? errorType { get; set; }
        public string? errorMessage { get; set; }
        public string? errorStackTrace { get; set; }
        public DateTime errorOccurredTime { get; set; }
    }
}