using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCloud
{
    internal class VolumeStructure
    {
        private Dictionary<string, string> volumes = new Dictionary<string, string>();

        public void addVolume(string name, string path)
        { 
            volumes[name] = path; 
        }

        public void removeVolume(string name)
        {
            volumes.Remove(name);
        }

        public void generateStructure()
        {
            foreach(var volume in volumes.Values)
            {
                Directory.CreateDirectory(volume);
            }
        }
    }
}
