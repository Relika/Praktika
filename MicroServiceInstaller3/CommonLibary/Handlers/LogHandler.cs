using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibary.Handlers
{
    public class LogHandler
    {
        /// <summary>
        /// Writes message to file
        /// </summary>
        /// <param name="path">File path </param>
        /// <param name="error">Message</param>
        public static void WriteLogMessage(string path, string error)
        {
            if (File.Exists(path))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.AppendText(path))
                {
                    sw.WriteLine(error);
                }
            }
        }
        /// <summary>
        /// Creates error message
        /// </summary>
        /// <param name="error">Error</param>
        /// <param name="addStackTrace"></param>
        /// <param name="stackTraceLen">Error message length</param>
        /// <returns>Returns string</returns>
        public static string CreateErrorMessage(Exception error, bool addStackTrace = false, int stackTraceLen = 1024)
        {
            if (error == null) return "Error object is null";
            if (error.GetType().ToString().Equals("System.NullReferenceException", StringComparison.OrdinalIgnoreCase)) addStackTrace = true;
            StringBuilder builder = new StringBuilder();
            builder.Append("Message:");
            string errorMessage = (error == null) ? string.Empty : error.Message;
            builder.Append(errorMessage).Append(Environment.NewLine);

            string innerExceptionMessage = "";
            if (error != null && error.InnerException != null)
            {
                innerExceptionMessage = (error.InnerException.Message == null) ? string.Empty : error.InnerException.Message;
            }
            builder.Append("InnerException Message:").Append(innerExceptionMessage).Append(Environment.NewLine);
            if (addStackTrace)
            {
                if (error != null && !string.IsNullOrWhiteSpace(error.StackTrace))builder.Append("ST:").Append(CutString(error.StackTrace, stackTraceLen));
            }
            return builder.ToString();
        }
        /// <summary>
        /// Cuts string
        /// </summary>
        /// <param name="initialString">String to cut</param>
        /// <param name="length">String lenght</param>
        /// <returns>Returns cut string</returns>
        public static string CutString(string initialString, int length)
        {
            if (string.IsNullOrWhiteSpace(initialString) || length < 1) return string.Empty;
            if (initialString.Length <= length) return initialString;
            return initialString.Substring(0, length);
        }
    }
}
