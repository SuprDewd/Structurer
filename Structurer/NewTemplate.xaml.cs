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
        public NewTemplate(string name = "", string structure = "")
        {
            InitializeComponent();
            this.TemplateName.Text = name;
            this.TemplateStructure.Text = structure;
        }
    }
}