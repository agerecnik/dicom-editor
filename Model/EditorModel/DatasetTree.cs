using DicomEditor.Interfaces;
using FellowOakDicom;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DicomEditor.Model.EditorModel.Tree
{
    public class DatasetTree : ITreeModel
    {
        public static async Task<DatasetTree> CreateTree(DicomDataset dataset, bool validate, string searchTerm, CancellationToken cancellationToken)
        {
            var tree = new DatasetTree();
            await Task.Run(() => ReadDataset(dataset, tree.Root, null, validate, searchTerm, cancellationToken), cancellationToken);
            return cancellationToken.IsCancellationRequested ? new DatasetTree() : tree;
        }

        private static void ReadDataset(DicomDataset dataset, IDatasetModel datasetModel, IDatasetModel parent, bool validate, string searchTerm, CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                IList<DicomItem> items = dataset.ToList();
                foreach (DicomItem item in items)
                {
                    bool test;
                    DicomTag tag;
                    string name;
                    DicomVR vr;
                    bool isValid = true;
                    string validationFailedMessage = string.Empty;
                    bool isSearchResult = false;

                    if (item.Tag.IsPrivate)
                    {
                        tag = dataset.GetPrivateTag(item.Tag);
                    }
                    else
                    {
                        tag = item.Tag;
                    }

                    name = tag.DictionaryEntry.Keyword;
                    vr = item.ValueRepresentation;

                    if (validate)
                    {
                        try
                        {
                            item.Validate();
                        }
                        catch (DicomValidationException e)
                        {
                            isValid = false;
                            validationFailedMessage = e.Message;
                        }
                    }

                    if (!string.IsNullOrEmpty(searchTerm) && (tag.ToString().Contains(searchTerm, System.StringComparison.CurrentCultureIgnoreCase) || name.Contains(searchTerm, System.StringComparison.CurrentCultureIgnoreCase)))
                    {
                        isSearchResult = true;
                    }

                    if (vr != DicomVR.SQ)
                    {
                        test = dataset.TryGetValues<string>(tag, out string[] values);
                        if (test)
                        {
                            string value = string.Join("\\", values);
                            if (!string.IsNullOrEmpty(searchTerm) && value.Contains(searchTerm, System.StringComparison.CurrentCultureIgnoreCase))
                            {
                                isSearchResult = true;
                            }
                            IDatasetModel attribute = new DatasetModel(tag, item.ValueRepresentation.ToString(), name, value, parent, isValid, validationFailedMessage, isSearchResult);
                            datasetModel.NestedDatasets.Add(attribute);
                        }
                    }
                    else
                    {
                        test = dataset.TryGetSequence(tag, out DicomSequence seq);
                        if (test)
                        {
                            IDatasetModel sequenceModel = new DatasetModel(tag, item.ValueRepresentation.ToString(), name, string.Empty, parent, isValid, string.Empty, isSearchResult);
                            datasetModel.NestedDatasets.Add(sequenceModel);
                            int counter = 0;
                            foreach (DicomDataset sequenceItem in seq.Items)
                            {
                                isValid = true;
                                if (validate)
                                {
                                    try
                                    {
                                        sequenceItem.Validate();
                                    }
                                    catch (DicomValidationException)
                                    {
                                        isValid = false;
                                    }
                                }
                                IDatasetModel nestedDatasetModel = new DatasetModel(null, null, "Item", counter.ToString(), sequenceModel, isValid, string.Empty, false);
                                ReadDataset(sequenceItem, nestedDatasetModel, nestedDatasetModel, validate, searchTerm, cancellationToken);
                                sequenceModel.NestedDatasets.Add(nestedDatasetModel);
                                counter++;
                            }
                        }
                    }
                }
            }
        }

        public IDatasetModel Root { get; private set; }

        public DatasetTree()
        {
            Root = new DatasetModel(null, string.Empty, string.Empty, string.Empty, null, true, string.Empty, false);
        }

        public System.Collections.IEnumerable GetChildren(object parent)
        {
            if (parent == null)
                parent = Root;
            return (parent as IDatasetModel).NestedDatasets;
        }

        public bool HasChildren(object parent)
        {
            return (parent as IDatasetModel).NestedDatasets.Count > 0;
        }
    }
}
