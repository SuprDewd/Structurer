using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using WF = System.Windows.Forms;

namespace Structurer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void CreateStructure(object sender, RoutedEventArgs e)
        {
            string baseDir = this.BaseDir.Text;
            if (baseDir.StartsWith("~\\")) baseDir = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), baseDir.Substring(2));

            StructureParser parser = new StructureParser();
            parser.Expanders.Add("jquery", new Expander { Type = ExpanderType.OnlineFile, Value = "http://ajax.googleapis.com/ajax/libs/jquery/1/jquery.js" });
            parser.Expanders.Add("960gs", new Expander { Type = ExpanderType.LocalDirectory, Value = @"C:\Users\SuprDewd\Documents\Projects\Old\BASE\style\960.gs" });
            parser.Expanders.Add("cssreset", new Expander { Type = ExpanderType.LocalFile, Value = @"C:\Users\SuprDewd\Documents\Projects\Old\BASE\style\reset.css" });

            parser.Parse(this.Structure.Text, baseDir);
        }

        private void SelectBasePath(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fb = new FolderBrowserDialog();

            if (fb.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.BaseDir.Text = fb.SelectedPath;
            }
        }

        private void ExitProgram(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}