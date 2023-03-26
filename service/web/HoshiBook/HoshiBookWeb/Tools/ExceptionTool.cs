using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;


namespace HoshiBookWeb.Tools {
    public class ExceptionTool {
        public static void CollectDetailMessage(Exception ex, bool isShow = true)
        {
            Dictionary<string, string> errorDetailInfo = new Dictionary<string, string>();
            //TODO Get stack trace for the exception with source file information
            StackTrace stackTrace = new StackTrace(ex, true);
            //TODO Get the top stack frame
            StackFrame? stackFrame = stackTrace.GetFrame(0);

            // StackFrame frame = new StackFrame(true);
            var errorFileName = stackFrame?.GetFileName();
            // var errorNativeIP = stackFrames.GetNativeIP().ToString();
            //TODO Get the error file column number
            string errorFileColumnNumber = stackFrame?.GetFileColumnNumber().ToString() ?? "";
            //TODO Get the erro line number from the stack frame
            string errorLineNumber = stackFrame?.GetFileLineNumber().ToString() ?? "";
            //TODO Get the error class name from the stack frame
            //TODO Get the error method name from the stack frame
            string? errorMethodName = stackFrame?.GetMethod()?.ToString() ?? "";
            //TODO Get the error occurred type from exception object
            string errorType = ex.GetType().Name;
            //TODO Get the error message
            string errorMessage = ex.Message;
            errorDetailInfo["errorMessage"] = errorMessage;
            errorDetailInfo["errorFileName"] = errorFileName ?? "";
            // errorDetailInfo["errorNativeIP"] = errorNativeIP;
            errorDetailInfo["errorLineNumber"] = errorLineNumber;
            errorDetailInfo["errorFileColumnNumber"] = errorFileColumnNumber;
            errorDetailInfo["errorMethodName"] = errorMethodName;
            errorDetailInfo["errorType"] = errorType;
            errorDetailInfo["errorStackTrace"] = ex.StackTrace ?? "";
            errorDetailInfo["errorOccurredTime"] = DateTime.Now.ToString("yyyy-MM-dd HH':'mm':'ss");
            if (isShow) {
                IterationTool.ReadDictItems(errorDetailInfo);
            }
        }
        public static string CollectDetailMessage(Exception ex)
        {
            //TODO Get stack trace for the exception with source file information
            StackTrace stackTrace = new StackTrace(ex, true);
            //TODO Get the top stack frame
            StackFrame? stackFrame = stackTrace.GetFrame(0);

            // StackFrame frame = new StackFrame(true);
            var errorFileName = stackFrame?.GetFileName();
            // var errorNativeIP = stackFrames.GetNativeIP().ToString();
            //TODO Get the error file column number
            string errorFileColumnNumber = stackFrame?.GetFileColumnNumber().ToString() ?? "";
            //TODO Get the erro line number from the stack frame
            string errorLineNumber = stackFrame?.GetFileLineNumber().ToString() ?? "";
            //TODO Get the error class name from the stack frame
            //TODO Get the error method name from the stack frame
            string? errorMethodName = stackFrame?.GetMethod()?.ToString() ?? "";
            //TODO Get the error occurred type from exception object
            string errorType = ex.GetType().Name;
            //TODO Get the error message
            string errorMessage = ex.Message;
            string errorStackTrace = ex.StackTrace ?? "";
            string errorDetails = $"\nException Stack Trace: {errorStackTrace}\nException Line Number: {errorLineNumber}\nException Type: {errorType}\nException Method: {errorMethodName}\nException Message: {errorMessage}";
            return errorDetails;
        }
    }
}