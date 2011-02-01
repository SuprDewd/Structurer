using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Structurer
{
    public struct ExpanderTask
    {
        public Expander Expander;
        public string Directory;
        public string File;

        public ExpanderTask(Expander exp, string dir, string file)
        {
            Expander = exp;
            Directory = dir;
            File = file;
        }
    }
}