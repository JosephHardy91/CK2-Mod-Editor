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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CK2_Mod_Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Tooling.Map.Map map = new Tooling.Map.Map("test");
            setupScreen();
            
        }

        void setupScreen()
        {
            this.Height = (int)System.Windows.SystemParameters.PrimaryScreenHeight;
            this.Width = (int)System.Windows.SystemParameters.PrimaryScreenWidth;
            Console.WriteLine(this.Height + "," + this.Width);
            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
        }

        void OnDoubleClick()
        {

        }

    }
}
