using DicomEditor.Interfaces;
using DicomEditor.Services;
using DicomEditor.View;
using DicomEditor.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using System;
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
            DialogService.RegisterDialog<QueryDialog, QueryDialogViewModel>();
            DialogService.RegisterDialog<ImportDialog, ImportDialogViewModel>();
            DialogService.RegisterDialog<ExportDialog, ExportDialogViewModel>();
            DialogService.RegisterDialog<MessageDialog, MessageDialogViewModel>();
        }

        public new static App Current => (App)Application.Current;

        public IServiceProvider Services { get; }

        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IImportService, ImportService>();
            services.AddSingleton<IEditorService, EditorService>();
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<IDICOMService, DICOMService>();
            services.AddSingleton<ICache, Cache>();
            services.AddSingleton<MainViewModel>();

            return services.BuildServiceProvider();
        }
    }
}
