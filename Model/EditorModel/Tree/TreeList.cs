using DicomEditor.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace DicomEditor.Model.EditorModel.Tree
{
    public class TreeList : ListView
    {
		#region Properties

		/// <summary>
		/// Internal collection of rows representing visible nodes, actually displayed in the ListView
		/// </summary>
		internal ObservableCollectionAdv<TreeNode> Rows
		{
			get;
			private set;
		}

        // Register a custom routed event using the Bubble routing strategy.
        public static readonly RoutedEvent ModelChangedEvent = EventManager.RegisterRoutedEvent(
			name: "ModelChanged",
			routingStrategy: RoutingStrategy.Bubble,
			handlerType: typeof(RoutedEventHandler),
			ownerType: typeof(TreeList));

        // Provide CLR accessors for assigning an event handler.
        public event RoutedEventHandler ModelChanged
        {
            add { AddHandler(ModelChangedEvent, value); }
            remove { RemoveHandler(ModelChangedEvent, value); }
        }

        public static readonly DependencyProperty ModelProperty =
			DependencyProperty.Register("Model", typeof(ITreeModel), typeof(TreeList), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnModelChangedCallBack)));

		//private ITreeModel _model;
		public ITreeModel Model
		{
			get { return (ITreeModel)GetValue(ModelProperty); }
			set { SetValue(ModelProperty, value); }
		}

		private static void OnModelChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
			TreeList tl = sender as TreeList;
			if (tl != null)
            {
				tl.OnModelChanged();
            }
        }

		private void RaiseModelChangedEvent()
		{
            // Create a RoutedEventArgs instance.
            RoutedEventArgs routedEventArgs = new(routedEvent: ModelChangedEvent);

            // Raise the event, which will bubble up through the element tree.
            RaiseEvent(routedEventArgs);
        }

		protected virtual void OnModelChanged()
        {
			_root.Children.Clear();
			Rows.Clear();
			CreateChildrenNodes(_root);
			RaiseModelChangedEvent();
		}

		private TreeNode _root;
		internal TreeNode Root
		{
			get { return _root; }
		}

		public ReadOnlyCollection<TreeNode> Nodes
		{
			get { return Root.Nodes; }
		}

		internal TreeNode PendingFocusNode
		{
			get;
			set;
		}

		public ICollection<TreeNode> SelectedNodes
		{
			get
			{
				return SelectedItems.Cast<TreeNode>().ToArray();
			}
		}

		public TreeNode SelectedNode
		{
			get
			{
				if (SelectedItems.Count > 0)
					return SelectedItems[0] as TreeNode;
				else
					return null;
			}
		}
		#endregion

		public TreeList()
		{
			Rows = new ObservableCollectionAdv<TreeNode>();
			_root = new TreeNode(this, null);
			_root.IsExpanded = true;
			ItemsSource = Rows;
			ItemContainerGenerator.StatusChanged += ItemContainerGeneratorStatusChanged;
		}

		void ItemContainerGeneratorStatusChanged(object sender, EventArgs e)
		{
			if (ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated && PendingFocusNode != null)
			{
				var item = ItemContainerGenerator.ContainerFromItem(PendingFocusNode) as TreeListItem;
				if (item != null)
					item.Focus();
				PendingFocusNode = null;
			}
		}

		protected override DependencyObject GetContainerForItemOverride()
		{
			return new TreeListItem();
		}

		protected override bool IsItemItsOwnContainerOverride(object item)
		{
			return item is TreeListItem;
		}

		protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
		{
			var ti = element as TreeListItem;
			var node = item as TreeNode;
			if (ti != null && node != null)
			{
				ti.Node = item as TreeNode;
				base.PrepareContainerForItemOverride(element, node.Tag);
			}
		}

		internal void SetIsExpanded(TreeNode node, bool value)
		{
			if (value)
			{
				if (!node.IsExpandedOnce)
				{
					node.IsExpandedOnce = true;
					node.AssignIsExpanded(value);
					CreateChildrenNodes(node);
				}
				else
				{
					node.AssignIsExpanded(value);
					CreateChildrenRows(node);
				}
			}
			else
			{
				DropChildrenRows(node, false);
				node.AssignIsExpanded(value);
			}
		}

		internal void CreateChildrenNodes(TreeNode node)
		{
			var children = GetChildren(node);
			if (children != null)
			{
				int rowIndex = Rows.IndexOf(node);
				node.ChildrenSource = children as INotifyCollectionChanged;
				foreach (object obj in children)
				{
					TreeNode child = new TreeNode(this, obj);
					child.HasChildren = HasChildren(child);
					node.Children.Add(child);
				}
				Rows.InsertRange(rowIndex + 1, node.Children.ToArray());
			}
		}

		private void CreateChildrenRows(TreeNode node)
		{
			int index = Rows.IndexOf(node);
			if (index >= 0 || node == _root) // ignore invisible nodes
			{
				var nodes = node.AllVisibleChildren.ToArray();
				Rows.InsertRange(index + 1, nodes);
			}
		}

		internal void DropChildrenRows(TreeNode node, bool removeParent)
		{
			int start = Rows.IndexOf(node);
			if (start >= 0 || node == _root) // ignore invisible nodes
			{
				int count = node.VisibleChildrenCount;
				if (removeParent)
					count++;
				else
					start++;
				Rows.RemoveRange(start, count);
			}
		}

		private IEnumerable GetChildren(TreeNode parent)
		{
			if (Model != null)
				return Model.GetChildren(parent.Tag);
			else
				return null;
		}

		private bool HasChildren(TreeNode parent)
		{
			if (parent == Root)
				return true;
			else if (Model != null)
				return Model.HasChildren(parent.Tag);
			else
				return false;
		}

		internal void InsertNewNode(TreeNode parent, object tag, int rowIndex, int index)
		{
			TreeNode node = new TreeNode(this, tag);
			if (index >= 0 && index < parent.Children.Count)
				parent.Children.Insert(index, node);
			else
			{
				index = parent.Children.Count;
				parent.Children.Add(node);
			}
			Rows.Insert(rowIndex + index + 1, node);
		}
	}
}
