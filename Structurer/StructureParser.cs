using System;
using System.Collections.Generic;
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

        public Dictionary<string, Expander> Expanders = new Dictionary<string, Expander>();

        public StructureParser()
        {
            this.BaseDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            this.Indention = " ";
            this.ExpanderStart = ":";
        }

        public bool Parse(string structure, string baseDirectory = null)
        {
            if (baseDirectory == null) baseDirectory = this.BaseDirectory;

            foreach (string command in structure.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
                int indent = 0;
                string path = "", cmd = command;

                while (cmd.StartsWith(this.Indention))
                {
                    cmd = cmd.Substring(this.Indention.Length);
                    indent++;
                }

                int exp = cmd.LastIndexOf(this.ExpanderStart);
                string expanderKey = exp != -1 ? cmd.Substring(exp + 1) : null;
                cmd = exp != -1 ? cmd.Substring(0, exp) : cmd;
                cmd = Path.Combine(baseDirectory, cmd);

                string file = null;
                if (!cmd.EndsWith("/")) file = cmd;
                string dir = Path.GetDirectoryName(cmd);
                Directory.CreateDirectory(dir);

                if (expanderKey != null)
                {
                    this.HandleExpander(expanderKey, dir, file);
                }
                else if (file != null)
                {
                    File.Create(file);
                }
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
                    case ExpanderType.LocalDirectory:
                        return false;
                }
            }
            else
            {
                switch (exp.Type)
                {
                    case ExpanderType.Text:
                        return false;
                    case ExpanderType.OnlineFile:
                        return false; // TODO: Support archives
                    case ExpanderType.LocalFile:
                        return false;
                    case ExpanderType.LocalDirectory:
                        return CopyDirectory(new DirectoryInfo(exp.Value), new DirectoryInfo(dir));
                }
            }

            return false;
        }

        public static bool CopyDirectory(DirectoryInfo source, DirectoryInfo target)
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
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(value);
                using (StreamReader sr = new StreamReader(req.GetResponse().GetResponseStream()))
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
                }
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