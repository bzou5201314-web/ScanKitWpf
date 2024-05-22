
using System.Windows;
using Caliburn.Micro;
using HandyControlProject1.ViewModels;
using HandyControlProject1.Views;
namespace HandyControlProject1
{
    public class Bootstrapper : BootstrapperBase
    {
        public Bootstrapper()
        {
            Initialize();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewForAsync<MainWindowViewModel>();
        }
    }
}
