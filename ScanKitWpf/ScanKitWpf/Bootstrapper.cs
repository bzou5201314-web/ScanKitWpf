using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Caliburn.Micro;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ScanKitWpf.Models;
using ScanKitWpf.ViewModels;

namespace ScanKitWpf
{
    public class Bootstrapper:BootstrapperBase
    {
        private SimpleContainer _container=new SimpleContainer();

        ServiceCollection services= new ServiceCollection();
        public Bootstrapper() 
        { 
            Initialize();
            

        }

        protected override void Configure()
        {
            base.Configure();
            services.AddSingleton<IWindowManager, WindowManager>();
            services.AddSingleton<IEventAggregator, EventAggregator>();
            services.AddSingleton<MainViewModel>();

            //_container.Singleton<IWindowManager, WindowManager>();
            //_container.Singleton<IEventAggregator, EventAggregator>();
            var config = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("appsettings.json", optional: false, true)
                .Build();
           // _container.RegisterInstance(typeof(IConfiguration), "", config);
            
            services.AddOptions<AppConfig>().Bind(config);
            //_container.Instance<IOptions<AppConfig>>(services.AddOptions<AppConfig>().Bind(config.GetSection("RotateCoordinate")));
            //_container.Singleton<MainViewModel>();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewForAsync<MainViewModel>();
        }

        protected override object GetInstance(Type service, string key)
        {
            return services.BuildServiceProvider().GetRequiredKeyedService(service, key);
            //return _container.GetInstance(service, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return services.BuildServiceProvider().GetServices(service);
            //return _container.GetAllInstances(service);
        }

        protected override void BuildUp(object instance)
        {
            services.BuildServiceProvider();
            //_container.BuildUp(instance);
        }

        protected override void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.Message);
            return;
        }

        protected override void OnExit(object sender, EventArgs e)
        {
            IoC.Get<MainViewModel>().CloseCamera();
        }
    }
}
