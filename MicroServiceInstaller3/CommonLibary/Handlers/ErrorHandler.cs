using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibary.Handlers
{
    public class ErrorHandler
    {
        public static void WriteErrorMessage(string path, string error)
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
                if (error != null && !string.IsNullOrWhiteSpace(error.StackTrace)) ;
                    //builder.Append("ST:").Append(CutString(error.StackTrace, stackTraceLen));

            }

            return builder.ToString();
        }
    }
}
