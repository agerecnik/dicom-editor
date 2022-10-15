using DicomEditor.Interfaces;
using FellowOakDicom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicomEditor.Model.EditorModel.Tree
{
    public class DatasetTree : ITreeModel
    {
		public static DatasetTree CreateTree(DicomDataset dataset)
		{
			var tree = new DatasetTree();
            ReadDataset(dataset, tree.Root, null);
            return tree;
		}

        private static void ReadDataset(DicomDataset dataset, IDatasetModel datasetModel, IDatasetModel parent)
        {
            IList<DicomItem> items = dataset.ToList();
            foreach (DicomItem item in items)
            {
                bool test;
                DicomTag tag;
                string name;
                if (item.Tag.IsPrivate)
                {
                    tag = dataset.GetPrivateTag(item.Tag);
                    name = tag.DictionaryEntry.Keyword;
                }
                else
                {
                    tag = item.Tag;
                    name = tag.DictionaryEntry.Keyword;
                }

                test = dataset.TryGetString(tag, out string str);
                if (test)
                {
                    IDatasetModel attribute = new DatasetModel(tag, item.ValueRepresentation.ToString(), name, str, parent);
                    datasetModel.NestedDatasets.Add(attribute);
                    continue;
                }

                test = dataset.TryGetSequence(tag, out DicomSequence seq);
                if (test)
                {
                    IDatasetModel sequenceModel = new DatasetModel(tag, item.ValueRepresentation.ToString(), name, "", parent);
                    datasetModel.NestedDatasets.Add(sequenceModel);
                    int counter = 0;
                    foreach (DicomDataset sequenceItem in seq.Items)
                    {
                        IDatasetModel nestedDatasetModel = new DatasetModel(null, null, "Item", counter.ToString(), sequenceModel);
                        ReadDataset(sequenceItem, nestedDatasetModel, nestedDatasetModel);
                        sequenceModel.NestedDatasets.Add(nestedDatasetModel);
                        counter++;
                    }
                }
            }
        }

		public IDatasetModel Root { get; private set; }

		public DatasetTree()
		{
			Root = new DatasetModel(null, "", "", "", null);
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
