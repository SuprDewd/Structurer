using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Structurer
{
    public class StructureParser
    {
        public string BaseDirectory { get; set; }

        public string Indention { get; set; }

        public string ExpanderStart { get; set; }

        public bool AllFolders { get; set; }

        public Dictionary<string, Expander> Expanders = new Dictionary<string, Expander>();

        public StructureParser()
        {
            this.BaseDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            this.Indention = " ";
            this.ExpanderStart = ":";
            this.AllFolders = false;
        }

        public bool Parse(string structure, string baseDirectory = null)
        {
            try
            {
                if (baseDirectory == null) baseDirectory = this.BaseDirectory;

                string lastDir = baseDirectory;

                foreach (string command in structure.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (command.StartsWith("#") || command.Trim() == "") continue;
                    string cmd = command.Replace('/', '\\');

                    int exp = cmd.LastIndexOf(this.ExpanderStart);
                    string expanderKey = exp != -1 ? cmd.Substring(exp + 1) : null;
                    cmd = exp != -1 ? cmd.Substring(0, exp) : cmd;

                    if (cmd.Trim() == "")
                    {
                        if (exp != -1) cmd = "\\";
                        else continue;
                    }

                    bool useLast = cmd.StartsWith("\\");
                    cmd = cmd.TrimStart('\\');
                    cmd = Path.Combine((useLast ? lastDir : baseDirectory).TrimEnd('\\') + '\\', cmd);
                    string dir = (this.AllFolders ? cmd : Path.GetDirectoryName(cmd)).TrimEnd('\\') + '\\';

                    Directory.CreateDirectory(dir);
                    string file = null;
                    if (!cmd.EndsWith("\\") && !this.AllFolders) file = cmd;

                    lastDir = dir;

                    if (expanderKey != null)
                    {
                        this.HandleExpander(expanderKey, dir, file);
                    }
                    else if (file != null)
                    {
                        File.Create(file);
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public bool HandleExpander(string expander, string dir, string file)
        {
            if (!this.Expanders.ContainsKey(expander)) return false;
            Expander exp = this.Expanders[expander];

            if (file != null)
            {
                switch (exp.Type)
                {
                    case ExpanderType.Text:
                        return WriteTextToFile(exp.Value, file);
                    case ExpanderType.OnlineFile:
                        return DownloadFile(exp.Value, file);
                    case ExpanderType.LocalFile:
                        return CopyFile(exp.Value, file);
                }
            }
            else
            {
                switch (exp.Type)
                {
                    case ExpanderType.OnlineFile:
                        return DownloadArchive(exp.Value, dir);
                    case ExpanderType.LocalDirectory:
                        return CopyDirectory(new DirectoryInfo(exp.Value), new DirectoryInfo(dir));
                }
            }

            return false;
        }

        private bool DownloadArchive(string value, string file)
        {
            string temp = Path.GetTempFileName();
            this.DownloadFile(value, temp);
            string tempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            DirectoryInfo tempDir = Directory.CreateDirectory(tempFolder);

            if (this.Extract(temp, tempFolder) != 0) return false;

            int folders = tempDir.GetDirectories().Length;
            int files = tempDir.GetFiles().Length;
            if (folders == 1 && files == 0)
            {
                tempDir = tempDir.GetDirectories().First();
            }
            else if (folders == 0 && files == 1)
            {
                FileInfo f = tempDir.GetFiles().First();
                if (this.Extract(f.FullName, tempFolder) == 0)
                {
                    f.Delete();
                }
            }

            //this.MoveContents(tempDir, new DirectoryInfo(file));
            this.CopyDirectory(tempDir, new DirectoryInfo(file));
            Directory.Delete(tempFolder, true);
            File.Delete(temp);

            return true;
        }

        private int Extract(string from, string to)
        {
            Process p7z = new Process();
            p7z.StartInfo.FileName = "7za.exe";
            p7z.StartInfo.Arguments = "x \"" + from + "\" -y -o\"" + to + "\"";
            // p7z.StartInfo.CreateNoWindow = !p7z.StartInfo.CreateNoWindow;
            if (!p7z.Start()) return -1;
            p7z.WaitForExit();
            return p7z.ExitCode;
        }

        private void MoveContents(DirectoryInfo fromDir, DirectoryInfo toDir)
        {
            foreach (FileInfo file in fromDir.GetFiles())
            {
                file.MoveTo(toDir.ToString());
            }

            foreach (DirectoryInfo dir in fromDir.GetDirectories())
            {
                dir.MoveTo(toDir.ToString());
            }
        }

        private bool CopyDirectory(DirectoryInfo source, DirectoryInfo target)
        {
            try
            {
                if (!Directory.Exists(target.FullName)) Directory.CreateDirectory(target.FullName);

                foreach (FileInfo fi in source.GetFiles())
                {
                    fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);
                }

                bool all = true;

                foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
                {
                    DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                    if (!CopyDirectory(diSourceSubDir, nextTargetSubDir)) all = false;
                }

                return all;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool CopyFile(string value, string file)
        {
            try
            {
                File.Copy(value, file);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private bool DownloadFile(string value, string file)
        {
            try
            {
                /*HttpWebRequest req = (HttpWebRequest)WebRequest.Create(value);
                WebResponse res = req.GetResponse();

                using (StreamReader sr = new StreamReader(res.GetResponseStream()))
                {
                    using (StreamWriter sw = new StreamWriter(file))
                    {
                        int bufferSize = 8192, read;
                        char[] buffer = new char[bufferSize];

                        while ((read = sr.Read(buffer, 0, bufferSize)) > 0)
                        {
                            sw.Write(buffer, 0, read);
                        }
                    }
                }*/

                new WebClient().DownloadFile(value, file);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private bool WriteTextToFile(string value, string file)
        {
            try
            {
                File.WriteAllText(file, value);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}