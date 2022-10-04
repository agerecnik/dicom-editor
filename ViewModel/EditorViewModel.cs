﻿using DicomEditor.Commands;
using DicomEditor.Interfaces;
using DicomEditor.Model;
using DicomEditor.Model.EditorModel.Tree;
using DicomEditor.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace DicomEditor.ViewModel
{
    public class EditorViewModel : ViewModelBase
    {
        private IEditorService _editorService;
        private IDialogService _dialogService;

        private ICollection<Series> _loadedSeriesList;
        public ICollection<Series> LoadedSeriesList
        {
            get => _loadedSeriesList;
            set
            {
                SetProperty(ref _loadedSeriesList, value);
            }
        }

        private Series _selectedSeries;
        public Series SelectedSeries
        {
            get => _selectedSeries;
            set
            {
                if (value != null)
                {
                    SetProperty(ref _selectedSeries, value);
                }
            }
        }

        private Instance _selectedInstance;
        public Instance SelectedInstance
        {
            get => _selectedInstance;
            set
            {
                if (value != null)
                {
                    SetProperty(ref _selectedInstance, value);
                    UpdateListOfAttributes();
                }
            }
        }

        private ITreeModel _selectedInstanceAttributes;
        public ITreeModel SelectedInstanceAttributes
        {
            get => _selectedInstanceAttributes;
            set
            {
                if (value != null)
                {
                    SetProperty(ref _selectedInstanceAttributes, value);
                }
            }
        }

        public ICommand StoreCommand { get; }

        public EditorViewModel(IEditorService editorService, IDialogService dialogService)
        {
            _editorService = editorService;
            _dialogService = dialogService;

            StoreCommand = new RelayCommand(o =>
            {
                if (SelectedSeries is not null)
                {
                    _dialogService.ShowDialog<StoreDialogViewModel>("Store in progress", editorService, new List<Series> { SelectedSeries });
                }
            }, CanUseStoreCommand);
        }

        public EditorViewModel()
        {
            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                throw new Exception("Use only for design mode");
            }
        }

        public void UpdateLoadedSeriesList()
        {
            LoadedSeriesList = new ObservableCollection<Series>(_editorService.GetLoadedSeries());
        }

        private void UpdateListOfAttributes()
        {
            SelectedInstanceAttributes = _editorService.GetInstance(SelectedInstance.InstanceUID);
        }

        private bool CanUseStoreCommand(object o)
        {
            if (SelectedSeries is null)
            {
                return false;
            }
            return true;
        }
    }
}
