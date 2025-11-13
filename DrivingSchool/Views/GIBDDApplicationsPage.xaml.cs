using DrivingSchool.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DrivingSchool.Views
{
    
    public partial class GIBDDApplicationsPage : Window
    {
        private readonly XmlDataService _dataService;
        public GIBDDApplicationsPage(XmlDataService dataService)
        {
            InitializeComponent();
            _dataService = dataService;
        }
    }
}
