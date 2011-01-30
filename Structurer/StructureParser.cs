using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Structurer
{
    public class StructureParser
    {
        public string[] Structure { get; set; }

        public DirectoryInfo BaseDirectory { get; set; }

        private StructureParser(string structure, DirectoryInfo baseDir)
        {
            this.Structure = structure.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            this.BaseDirectory = baseDir;
        }

        public bool Execute()
        {
        }

        public static bool Parse(string structure, DirectoryInfo baseDir = null)
        {
            if (baseDir == null) baseDir = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
            StructureParser parser = new StructureParser(structure, baseDir);
            return parser.Execute();
        }
    }
}