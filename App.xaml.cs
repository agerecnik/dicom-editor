using DicomEditor.Interfaces;
using DicomEditor.Services;
using DicomEditor.View;
using DicomEditor.ViewModel;
using FellowOakDicom;
using FellowOakDicom.Imaging;
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
            DialogService.RegisterDialog<SpinnerDialog, QueryDialogViewModel>();
            DialogService.RegisterDialog<SpinnerDialog, GetInstanceTreeDialogViewModel>();
            DialogService.RegisterDialog<ProgressBarDialog, ImportDialogViewModel>();
            DialogService.RegisterDialog<ProgressBarDialog, ExportDialogViewModel>();
            DialogService.RegisterDialog<MessageDialog, MessageDialogViewModel>();
            DialogService.RegisterDialog<ImageViewDialog, ImageViewDialogViewModel>();
            new DicomSetupBuilder().RegisterServices(s => s.AddFellowOakDicom().AddImageManager<WPFImageManager>()).Build();
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
