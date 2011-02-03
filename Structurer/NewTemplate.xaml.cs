using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Structurer
{
    /// <summary>
    /// Interaction logic for NewTemplate.xaml
    /// </summary>
    public partial class NewTemplate : Window
    {
        private Func<string, string, bool> Save { get; set; }

        public NewTemplate(Func<string, string, bool> save, string name = "", string structure = "")
        {
            this.Save = save;
            InitializeComponent();
            this.TemplateName.Text = name;
            this.TemplateStructure.Text = structure;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (this.Save(this.TemplateName.Text, this.TemplateStructure.Text)) this.Close();
            else
            {
                MessageBox.Show("Template name in use." + Environment.NewLine + "Please specify another.", "Template name taken", MessageBoxButton.OK);
            }
        }
    }
}