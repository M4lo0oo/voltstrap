using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voltstrap.UI.ViewModels.Settings;

namespace Voltstrap.UI.Elements.Settings.Pages
{
    /// <summary>
    /// Interaction logic for VoltstrapPage.xaml
    /// </summary>
    public partial class VoltstrapPage
    {
        public VoltstrapPage()
        {
            DataContext = new VoltstrapViewModel();
            InitializeComponent();
        }
    }
}
