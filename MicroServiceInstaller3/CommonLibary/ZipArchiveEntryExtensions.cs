using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibary
{
    public static class ZipArchiveEntryExtensions
    {
        public static bool IsFolder(this ZipArchiveEntry entry)
        {
            return entry.FullName.EndsWith("/");
        }

        //public static bool EndsWith(this ZipArchiveEntry entry, string end)
        //{
        //    return entry.FullName.EndsWith(end);
        //}
    }
}
