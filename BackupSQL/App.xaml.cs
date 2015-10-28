using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace BackupSQL
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup_1(object sender, StartupEventArgs e) {
            if (e.Args.Count() > 0) {
                BackupViewModel vm = new BackupViewModel();
                switch (e.Args[0]) {
                    case "-b":
                        vm.Backup();
                        
                        break;
                    case "-r":
                        if (MessageBox.Show("restore engbase?","restore",MessageBoxButton.YesNo,MessageBoxImage.Asterisk,MessageBoxResult.No) == MessageBoxResult.Yes) {
                            vm.RestoreEngBase();
                        }
                        break;
                }
                Application.Current.Shutdown();
            }
        }
    }
}
