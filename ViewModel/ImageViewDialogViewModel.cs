using DicomEditor.Commands;
using DicomEditor.Interfaces;
using DicomEditor.Model;
using DicomEditor.Services;
using FellowOakDicom.Imaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace DicomEditor.ViewModel
{
    public class ImageViewDialogViewModel : ViewModelBase, IDialogViewModel
    {
        private bool _executionFinished;
        public bool ExecutionFinished
        {
            get => _executionFinished;
            set => SetProperty(ref _executionFinished, value);
        }

        private string _status;
        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public object Payload => throw new NotImplementedException();

        public ICommand CancelCommand => throw new NotImplementedException();

        private int _currentImageIndex;
        public int CurrentImageIndex
        {
            get => _currentImageIndex;
            set
            {
                SetProperty(ref _currentImageIndex, value);
                CurrentImage = _images[value];
            }
        }

        private ImageSource _currentImage;
        public ImageSource CurrentImage
        {
            get => _currentImage;
            set => SetProperty(ref _currentImage, value);
        }

        private int _numberOfImages;
        public int NumberOfImages
        {
            get => _numberOfImages;
            set
            {
                SetProperty(ref _numberOfImages, value);
            }
        }

        private string _windowCenter;
        public string WindowCenter
        {
            get => _windowCenter;
            set
            {
                if (ValidateValue(value))
                {
                    SetProperty(ref _windowCenter, value);
                }
                else
                {
                    SetProperty(ref _windowCenter, _windowCenter);
                }
            }
        }

        private string _windowWidth;
        public string WindowWidth
        {
            get => _windowWidth;
            set
            {
                if (ValidateValue(value))
                {
                    SetProperty(ref _windowWidth, value);
                }
                else
                {
                    SetProperty(ref _windowWidth, _windowWidth);
                }
            }
        }

        public ICommand ApplyWindowCenterAndWidthCommand { get; }

        private IList<ImageSource> _images;
        private readonly IEditorService _editorService;
        private readonly IDialogService _dialogService;
        private readonly IList<Instance> _instances;


        public ImageViewDialogViewModel(IEditorService editorService, IDialogService dialogService, IList<Instance> instances)
        {
            Status = "N/A";
            ExecutionFinished = false;
            _editorService = editorService;
            _dialogService = dialogService;
            _instances = instances;

            try
            {
                var imagesAndWCWW = _editorService.GetImages(_instances);
                _images = imagesAndWCWW.Item1;
                NumberOfImages = _images.Count - 1;
                CurrentImageIndex = 0;
                if (imagesAndWCWW.Item2.Length > 1)
                {
                    WindowCenter = imagesAndWCWW.Item2[0].ToString();
                    WindowWidth = imagesAndWCWW.Item2[1].ToString();
                }

                ApplyWindowCenterAndWidthCommand = new RelayCommand(o => UpdateWindowCenterAndWidth(), CanUseApplyWindowCenterAndWidthCommand);
            }
            catch (DicomImagingException e)
            {
                ExecutionFinished = true;
                _dialogService.ShowDialog<MessageDialogViewModel>("Notification", e.Message);
            }
        }

        public ImageViewDialogViewModel()
        {
            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                throw new Exception("Use only for design mode");
            }
        }

        public void Execute() { }

        public void UpdateWindowCenterAndWidth()
        {
            _images = _editorService.GetImages(_instances, double.Parse(WindowCenter), double.Parse(WindowWidth));
            CurrentImageIndex = _currentImageIndex;
        }

        private bool CanUseApplyWindowCenterAndWidthCommand(object o)
        {
            if (string.IsNullOrWhiteSpace(WindowCenter) || string.IsNullOrWhiteSpace(WindowWidth) || WindowCenter == "-" || WindowWidth == "-")
            {
                return false;
            }
            return true;
        }

        private static bool ValidateValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return true;
            }
            if (value == "-")
            {
                return true;
            }
            if (double.TryParse(value, out _))
            {
                return true;
            }
            return false;
        }
    }
}
