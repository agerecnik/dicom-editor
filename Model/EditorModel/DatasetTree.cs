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
            ReadDataset(dataset, tree.Root);
            return tree;
		}

        private static void ReadDataset(DicomDataset dataset, IDatasetModel datasetModel)
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

                string str;
                test = dataset.TryGetString(tag, out str);
                if (test)
                {
                    IDatasetModel attribute = new DatasetModel(tag.ToString(), item.ValueRepresentation.ToString(), name, str);
                    datasetModel.NestedDatasets.Add(attribute);
                    continue;
                }

                DicomSequence seq;
                test = dataset.TryGetSequence(tag, out seq);
                if (test)
                {
                    IDatasetModel sequenceModel = new DatasetModel(tag.ToString(), item.ValueRepresentation.ToString(), name, "");
                    datasetModel.NestedDatasets.Add(sequenceModel);
                    int counter = 0;
                    foreach (DicomDataset sequenceItem in seq.Items)
                    {
                        IDatasetModel nestedDatasetModel = new DatasetModel("", "", "Item " + counter, "");
                        ReadDataset(sequenceItem, nestedDatasetModel);
                        sequenceModel.NestedDatasets.Add(nestedDatasetModel);
                        counter++;
                    }
                }
            }
        }

		public IDatasetModel Root { get; private set; }

		public DatasetTree()
		{
			Root = new DatasetModel("", "", "", "");
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
