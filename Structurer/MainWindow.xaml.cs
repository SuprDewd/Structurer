using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
            this.UpdateTemplates();
        }

        private void CreateStructure(object sender, RoutedEventArgs e)
        {
            this.btnCreate.IsEnabled = false;
            this.Status.Content = "Creating Structure...";
            string baseDir = this.BaseDir.Text;
            string structure = this.Structure.Text;
            bool allFolders = this.AllFolders.IsChecked.HasValue && this.AllFolders.IsChecked.Value;
            if (baseDir.StartsWith("~\\")) baseDir = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), baseDir.Substring(2));

            new Thread(() =>
                       {
                           StructureParser parser = new StructureParser { AllFolders = allFolders };
                           parser.Expanders.Add("jquery", new Expander { Type = ExpanderType.OnlineFile, Value = "http://ajax.googleapis.com/ajax/libs/jquery/1/jquery.js" });
                           parser.Expanders.Add("960gs", new Expander { Type = ExpanderType.LocalDirectory, Value = @"C:\Users\SuprDewd\Documents\Projects\Old\BASE\style\960.gs" });
                           parser.Expanders.Add("cssreset", new Expander { Type = ExpanderType.LocalFile, Value = @"C:\Users\SuprDewd\Documents\Projects\Old\BASE\style\reset.css" });
                           parser.Expanders.Add("codeigniter", new Expander { Type = ExpanderType.OnlineFile, Value = @"http://codeigniter.com/download.php" });
                           parser.Expanders.Add("cakephp", new Expander { Type = ExpanderType.OnlineFile, Value = @"https://github.com/cakephp/cakephp/tarball/1.3-dev" });
                           parser.Expanders.Add("ltest", new Expander { Type = ExpanderType.OnlineFile, Value = @"http://localhost/Skilaverkefni1.zip" });

                           bool ok = parser.Parse(structure, baseDir);

                           this.Dispatcher.BeginInvoke(new Action(() =>
                                                                  {
                                                                      this.btnCreate.IsEnabled = true;
                                                                      this.Status.Content = ok ? "Structure Created." : "Error while creating structure.";
                                                                  }));
                       }).Start();
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

        private void Templates_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.Templates.SelectedItem == TemplateSeparator || this.Templates.SelectedItem == CustomTemplateItem) return;

            if (this.Templates.SelectedItem == SaveTemplateItem) this.SaveTemplate();
            // else if (this.Templates.SelectedItem == ManageTemplatesItem) return; // new NewTemplate(structure: this.Structure.Text).ShowDialog();

            this.Templates.SelectedIndex = 0;
        }

        private ComboBoxItem CustomTemplateItem = new ComboBoxItem { Content = "Custom" };
        private ComboBoxItem SaveTemplateItem = new ComboBoxItem { Content = "Save Template..." };
        private ComboBoxItem ManageTemplatesItem = new ComboBoxItem { Content = "Manage Templates..." };
        private Separator TemplateSeparator = new Separator();

        private void UpdateTemplates()
        {
            this.Templates.Items.Clear();
            this.Templates.Items.Add(CustomTemplateItem);

            // Add templates..

            this.Templates.Items.Add(TemplateSeparator);
            this.Templates.Items.Add(SaveTemplateItem);
            this.Templates.Items.Add(ManageTemplatesItem);
            this.Templates.SelectedIndex = 0;
        }

        private void SaveTemplate()
        {
            new NewTemplate(structure: this.Structure.Text).ShowDialog();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.SaveTemplate();
        }
    }
}