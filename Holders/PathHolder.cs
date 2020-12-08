using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GaleForceCore.Holders
{
    public class PathHolder
    {
        private string BasePath { get; set; }

        private Dictionary<string, string> SubPaths = new Dictionary<string, string>();

        public void SetDataPath(string basePath, Dictionary<string, string> subPaths = null)
        {
            BasePath = basePath;
            if(subPaths != null)
            {
                SubPaths = subPaths;
            }
        }

        public void SetSubPath(string key, string path)
        {
            if(SubPaths.ContainsKey(key))
            {
                SubPaths[key] = path;
            }
            else
            {
                SubPaths.Add(key, path);
            }
        }

        public string GetFullPath(string key = null, string defaultSubPath = null)
        {
            if(key == null)
            {
                return BasePath;
            }

            if(SubPaths.ContainsKey(key))
            {
                var sub = SubPaths[key];
                return sub.Contains(":") ? sub : Path.Combine(BasePath, sub);
            }
            else
            {
                return Path.Combine(BasePath, defaultSubPath ?? string.Empty);
            }
        }
    }
}
