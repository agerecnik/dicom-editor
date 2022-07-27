using DicomEditor.Model;
using DicomEditor.Model.Interfaces;
using DicomEditor.Model.Services;
using DicomEditor.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace DicomEditor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Services = ConfigureServices();
        }

        public new static App Current => (App)Application.Current;

        public IServiceProvider Services { get; }

        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IImportService, ImportService>();
            services.AddSingleton<IEditorService, EditorService>();
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<ICache, Cache>();
            services.AddSingleton<MainViewModel>();

            return services.BuildServiceProvider();
        }
    }
}
