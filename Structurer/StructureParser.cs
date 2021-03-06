﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

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
                Queue<ExpanderTask> tasks = new Queue<ExpanderTask>();

                foreach (string command in structure.Split(new[] { Environment.NewLine, "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries))
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

                    if (expanderKey != null && this.Expanders.ContainsKey(expanderKey))
                    {
                        tasks.Enqueue(new ExpanderTask(this.Expanders[expanderKey], dir, file));
                    }
                    else if (file != null)
                    {
                        File.Create(file);
                    }
                }

                int numTasks = tasks.Count;
                bool error = false;
                object o = new object();
                while (tasks.Any())
                {
                    ExpanderTask task = tasks.Dequeue();
                    this.HandleExpander(task.Expander, task.Directory, task.File, b => { lock (o) numTasks--; if (!b) error = true; });
                }

                while (numTasks > 0)
                {
                    Thread.Sleep(500);
                }

                return !error;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void HandleExpander(Expander exp, string dir, string file, Action<bool> finished)
        {
            if (file != null)
            {
                switch (exp.Type)
                {
                    case ExpanderType.Text:
                        finished(WriteTextToFile(exp.Value, file));
                        break;
                    case ExpanderType.OnlineFile:
                        DownloadFile(exp.Value, file, finished);
                        break;
                    case ExpanderType.LocalFile:
                        finished(CopyFile(exp.Value, file));
                        break;
                }
            }
            else
            {
                switch (exp.Type)
                {
                    case ExpanderType.OnlineFile:
                        DownloadArchive(exp.Value, dir, finished);
                        break;
                    case ExpanderType.LocalDirectory:
                        finished(MoveDirectory(new DirectoryInfo(exp.Value), new DirectoryInfo(dir), false));
                        break;
                }
            }
        }

        private void DownloadArchive(string value, string file, Action<bool> callback)
        {
            string temp = Path.GetTempFileName();
            this.DownloadFile(value, temp, b =>
            {
                if (!b)
                {
                    callback(false);
                    return;
                }

                string tempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                DirectoryInfo tempDir = Directory.CreateDirectory(tempFolder);

                if (this.Extract(temp, tempFolder) != 0)
                {
                    callback(false);
                    return;
                }

                int folders = tempDir.GetDirectories().Length;
                int files = tempDir.GetFiles().Length;
                if (folders == 1 && (files == 0 || (files == 1 && tempDir.GetFiles().First().Name == "pax_global_header")))
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

                folders = tempDir.GetDirectories().Length;
                files = tempDir.GetFiles().Length;
                if (folders == 1 && (files == 0 || (files == 1 && tempDir.GetFiles().First().Name == "pax_global_header")))
                {
                    tempDir = tempDir.GetDirectories().First();
                }

                Action cleanUp = () =>
                                 {
                                     Directory.Delete(tempFolder, true);
                                     File.Delete(temp);
                                 };

                if (!this.MoveDirectory(tempDir, new DirectoryInfo(file), true))
                {
                    cleanUp();
                    callback(false);
                    return;
                }

                cleanUp();
                callback(true);
            });
        }

        private int Extract(string from, string to)
        {
            Process p7z = new Process();
            p7z.StartInfo.FileName = "7za.exe";
            p7z.StartInfo.Arguments = "x \"" + from + "\" -y -o\"" + to + "\"";
            p7z.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p7z.StartInfo.CreateNoWindow = true;
            if (!p7z.Start()) return -1;
            p7z.WaitForExit();
            return p7z.ExitCode;
        }

        private bool MoveDirectory(DirectoryInfo source, DirectoryInfo target, bool move)
        {
            try
            {
                if (!Directory.Exists(target.FullName)) Directory.CreateDirectory(target.FullName);

                foreach (FileInfo fi in source.GetFiles())
                {
                    string to = Path.Combine(target.ToString(), fi.Name);
                    if (!File.Exists(to)) if (move) fi.MoveTo(to); else fi.CopyTo(to);
                }

                bool all = true;

                foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
                {
                    DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                    if (!MoveDirectory(diSourceSubDir, nextTargetSubDir, move)) all = false;
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

        private bool DownloadFile(string value, string file, Action<bool> callback)
        {
            try
            {
                WebClient WC = new WebClient();
                WC.DownloadFileCompleted += (o, h) => callback(!h.Cancelled && h.Error == null);
                WC.DownloadFileAsync(new Uri(value), file);
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