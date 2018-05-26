using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cn.org.hentai.tentacle.system
{
    public class FileSystem
    {
        public static File[] list(string path)
        {
            if ("".Equals(path))
            {
                string[] drivers = System.IO.Directory.GetLogicalDrives();
                File[] rootFiles = new File[drivers.Length];
                for (int i = 0; i < drivers.Length; i++)
                    rootFiles[i] = new File(true, 0, 0, drivers[i]);
                return rootFiles;
            }

            string[] directories = System.IO.Directory.GetDirectories(path);
            string[] files = System.IO.Directory.GetFiles(path);
            File[] subfiles = new File[directories.Length + files.Length];
            for (int i = 0; i < directories.Length; i++)
            {
                string dir = directories[i];
                if (dir.IndexOf('/') > -1) dir = dir.Substring(dir.LastIndexOf('/') + 1);
                else if (dir.IndexOf('\\') > -1) dir = dir.Substring(dir.LastIndexOf('\\') + 1);
                subfiles[i] = new File(true, 0, 0, dir);
            }
            for (int i = 0; i < files.Length; i++)
            {
                string fName = files[i];
                if (fName.IndexOf('/') > -1) fName = fName.Substring(fName.LastIndexOf('/') + 1);
                else if (fName.IndexOf('\\') > -1) fName = fName.Substring(fName.LastIndexOf('\\') + 1);
                subfiles[i + directories.Length] = new File(false, new FileInfo(path + fName).Length, 0, fName);
            }
            return subfiles;
        }
    }
}
