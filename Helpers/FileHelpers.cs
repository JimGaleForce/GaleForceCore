using System.IO;
using System.Reflection;

namespace GaleForceCore.Helpers
{
    public static class FileHelpers
    {
        public static string WorkspaceFolder => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public static string[] GetLocalFiles(string spec, string folder = "tmp")
        {
            var xspec = GetLocalFilename(spec, folder);
            return Directory.GetFiles(Path.GetDirectoryName(xspec), spec);
        }

        public static string GetLocalFilename(string filename, string folder = "tmp")
        {
            if(Directory.Exists(folder + "/"))
            {
                return folder + "/" + filename;
            }

            if(Directory.Exists(WorkspaceFolder + folder))
            {
                var dir = WorkspaceFolder + folder;
                return dir + "/" + filename;
            }
            else
            {
                var last = "bin\\";
                var dir = WorkspaceFolder;
                if(!dir.EndsWith(last))
                {
                    last = "\\bin";
                }

                if(dir.EndsWith(last))
                {
                    dir = dir.Substring(0, dir.LastIndexOf(last));
                }

                dir = dir + folder;
                if(Directory.Exists(dir))
                {
                    return dir + "/" + filename;
                }
            }

            if(Directory.Exists("d:/home/site/wwwroot"))
            {
                return "d:/home/site/wwwroot/" + folder + "/" + filename;
            }

            if(!Directory.Exists("wwwroot/" + folder + "/"))
            {
                Directory.CreateDirectory("wwwroot/" + folder + "/");
            }

            return "wwwroot/" + folder + "/" + filename;
        }
    }
}
