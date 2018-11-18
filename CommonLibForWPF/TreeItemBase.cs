using CommonLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace CommonLibForWPF
{
    public class TreeItemBase : ViewModelBase
    {
        ObservableCollection<CommonDataObjectCommand> _ValidOperationList = new ObservableCollection<CommonDataObjectCommand>();
        public ObservableCollection<CommonDataObjectCommand> ValidOperationList
        {
            get { return _ValidOperationList; }
        }
        ObservableCollection<TreeItemBase> _SubItemList = new ObservableCollection<TreeItemBase>();
        public ObservableCollection<TreeItemBase> SubItemList { get { return _SubItemList; } }

        public TreeItemBase FindByName(string name)
        {
            if (name == Name) return this;
            foreach (var t in SubItemList)
            {
                if (t.FindByName(name) != null) return t.FindByName(name);
            }
            return null;
        }

        private IDataObject _TargetObject;
        public IDataObject TargetObject
        {
            get { return _TargetObject; }
            set 
            { 
                _TargetObject = value; 
                OnPropertyChanged("Icon"); 
                OnPropertyChanged("Name"); 
                OnPropertyChanged("Memo"); 

            }
        }

        private bool _IsUnify = true;
        public bool IsUnify
        {
            get { return _IsUnify; }
            set { _IsUnify = value;OnPropertyChanged("IsUnify"); }
        }
        //private BitmapImage _Icon;
        //public BitmapImage Icon
        //{
        //    get
        //    {
        //        if (IsUnify&&TargetObject != null) return TargetObject.Icon;
        //        return _Icon;
        //    }
        //    set
        //    {
        //        if (!IsUnify || TargetObject==null)
        //            _Icon = value;
        //        else
        //        {
        //            TargetObject.Icon = value;
        //        }
        //        OnPropertyChanged("Icon");
        //    }
        //}

        private string _Name = "";
        public string Name
        {
            get
            {
                if (IsUnify && TargetObject != null)
                    return TargetObject.Name;
                return _Name;
            }
            set
            {
                if (IsUnify && TargetObject!=null) TargetObject.Name = value;
                else _Name = value;
                OnPropertyChanged("Name");
            }
        }
        private string _Memo = "";
        public string Memo
        {
            get
            {
                if (IsUnify && TargetObject != null)
                    return TargetObject.Memo; 
                return _Memo;
            }
            set
            {
                if (IsUnify && TargetObject != null) TargetObject.Memo = value;
                else _Memo = value;
                OnPropertyChanged("Memo");
            }
        }
    }

    public class EntityTreeItem:TreeItemBase
    {
        
    }

    public class FolderTreeItem : TreeItemBase
    {

    }

    public class CommonDataObjectCommand : CommonCommand
    {
        public CommonDataObjectCommand(Action<object> action, IDataObject sender, string name, string memo = "")
            : base(action)
        {
            Sender = sender;
            Name = name;
            Memo = memo;
        }

        public CommonDataObjectCommand(Action<object> action, Predicate<object> predicate, IDataObject sender, string name, string memo = "")
            : base(action, predicate)
        {
            Sender = sender;
            Name = name;
            Memo = memo;
        }

        public IDataObject Sender { get; set; }

    }
}
