using DicomEditor.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows;
using DicomEditor.Model;
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

        private IList<ImageSource> _images;
        private readonly IEditorService _editorService;
        private readonly IList<Instance> _instances;
        

        public ImageViewDialogViewModel(IEditorService editorService, IList<Instance> instances)
        {
            Status = "N/A";
            ExecutionFinished = false;
            _editorService = editorService;
            _instances = instances;

            _images = _editorService.GetImages(_instances);
        }

        public ImageViewDialogViewModel()
        {
            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                throw new Exception("Use only for design mode");
            }
        }

        public void Execute() { }
    }
}
