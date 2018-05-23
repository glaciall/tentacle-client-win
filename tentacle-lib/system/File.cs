using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cn.org.hentai.tentacle.system
{
    public class File
    {
        public bool isDirectory;
        public long length;
        public long lastModifiedTime;
        public string name;

        public File(bool isDirectory, long length, long lastModifiedTime, string name)
        {
            this.isDirectory = isDirectory;
            this.length = length;
            this.lastModifiedTime = lastModifiedTime;
            this.name = name;
        }

        public override String ToString()
        {
            return "File{" +
                    "isDirectory=" + isDirectory +
                    ", length=" + length +
                    ", lastModifiedTime=" + new DateTime(lastModifiedTime) +
                    ", name='" + name + '\'' +
                    '}';
        }
    }
}
