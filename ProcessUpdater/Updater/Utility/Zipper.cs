using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Updater.Utility
{
    public class Zipper
    {
        public static bool Unzip(byte[] bytes, string destination)
        {
            try
            {
                using var ms = new MemoryStream(bytes);
                using var zs = new ZipInputStream(ms);

                ZipEntry entry = zs.GetNextEntry();
                while (entry != null)
                {
                    FileInfo fi = new FileInfo(Path.Combine(destination, entry.Name));
                    fi.Directory.Create();

                    if (entry.IsFile)
                    {
                        using FileStream fs = new FileStream(fi.FullName, FileMode.Create, FileAccess.Write);
                        byte[] buffer = new byte[4096];
                        int size = 0;
                        do
                        {
                            size = zs.Read(buffer, 0, buffer.Length);
                            fs.Write(buffer, 0, size);
                        }
                        while (size > 0);
                    }

                    entry = zs.GetNextEntry();
                }
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }
    }
}
