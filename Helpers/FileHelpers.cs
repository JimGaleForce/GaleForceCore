using System.IO;
using System.Reflection;

namespace GaleForceCore.Helpers
{
    public static class FileHelpers
    {
        public static string WorkspaceFolder => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        /// <summary>
        /// Gets the local filenames, asp.net core friendly.
        /// </summary>
        /// <param name="spec">The spec.</param>
        /// <param name="folder">The folder.</param>
        /// <param name="createWebRootIfNeeded">if set to <c>true</c> [create web root if needed].</param>
        /// <returns>System.String[].</returns>
        public static string[] GetLocalFiles(string spec, string folder = "tmp", bool createWebRootIfNeeded = false)
        {
            var xspec = GetLocalFilename(spec, folder, createWebRootIfNeeded);
            return Directory.GetFiles(Path.GetDirectoryName(xspec), spec);
        }

        /// <summary>
        /// Gets the name of the local path, asp.net core friendly.
        /// </summary>
        /// <param name="folder">The folder.</param>
        /// <param name="createWebRootIfNeeded">if set to <c>true</c> [create web root if needed].</param>
        /// <returns>System.String.</returns>
        public static string GetLocalPathName(string folder = "tmp", bool createWebRootIfNeeded = false)
        {
            if(Directory.Exists(folder + "/"))
            {
                return folder + "/";
            }

            if(Directory.Exists(WorkspaceFolder + folder))
            {
                var dir = WorkspaceFolder + folder;
                return dir + "/";
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
                    return dir + "/";
                }
            }

            if(Directory.Exists("d:/home/site/wwwroot"))
            {
                return "d:/home/site/wwwroot/" + folder + "/";
            }

            if(!Directory.Exists("wwwroot/" + folder + "/") && createWebRootIfNeeded)
            {
                Directory.CreateDirectory("wwwroot/" + folder + "/");
                return "wwwroot/" + folder + "/";
            }

            return folder + "/"; //doesn't exist
        }

        /// <summary>
        /// Gets the local filename, asp.net core friendly.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="folder">The folder.</param>
        /// <param name="createWebRootIfNeeded">if set to <c>true</c> [create web root if needed].</param>
        /// <returns>System.String.</returns>
        public static string GetLocalFilename(
            string filename,
            string folder = "tmp",
            bool createWebRootIfNeeded = false)
        { return GetLocalPathName(folder, createWebRootIfNeeded) + filename; }
    }
}
