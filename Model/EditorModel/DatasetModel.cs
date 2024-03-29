﻿using DicomEditor.Interfaces;
using FellowOakDicom;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DicomEditor.Model.EditorModel
{
    public class DatasetModel : IDatasetModel
    {
        public DicomTag Tag { get; set; }
        public string ValueRepresentation { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public ICollection<IDatasetModel> NestedDatasets { get; set; }
        public IDatasetModel ParentDataset { get; set; }
        public bool IsValid { get; set; }
        public string ValidationFailedMessage { get; set; }
        public bool IsSearchResult { get; set; }

        public DatasetModel(DicomTag tag, string vr, string name, string value, IDatasetModel parentDataset, bool isValid, string validationFailedMessage, bool isSearchResult)
        {
            Tag = tag;
            ValueRepresentation = vr;
            Name = name;
            Value = value;
            NestedDatasets = new ObservableCollection<IDatasetModel>();
            ParentDataset = parentDataset;
            IsValid = isValid;
            ValidationFailedMessage = validationFailedMessage;
            IsSearchResult = isSearchResult;
        }
    }
}