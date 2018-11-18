using CommonLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;


namespace CommonLibForWPF
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        bool _IsChanged = false;
        public virtual bool IsChanged { get { return _IsChanged; } set { _IsChanged = value; } }

        public ViewModelBase()
        {

        }

        public virtual void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
                IsChanged = true;

            }
        }

        public event PropertyChangedEventHandler PropertyChanged;



    }

    public abstract class SupportConsistentViewModel : ViewModelBase, IStatusSupportObject
    {
        public virtual string GetStatus()
        {
            return CommonProc.ConvertObjectToString(this);
        }

        public abstract void SetStatus(string status); //should overwrite this function

        private DateTime _CreateTime=DateTime.Now;
        public DateTime CreateTime
        {
            get { return _CreateTime; }
            set { _CreateTime = value; OnPropertyChanged("CreateTime"); }
        }

        protected DateTime _LastModifyTime = DateTime.Now;
        public DateTime LastModifyTime
        {
            get { return _LastModifyTime; }
            set { _LastModifyTime = value; OnPropertyChanged("LastModifyTime"); }
        }


        public void UpdateStatus()
        {
            Status = GetStatus();
        }

        public void LoadStatus()
        {
            SetStatus(Status);
        }

        public string Status { get; set; }
    }

    public class EntityBase : SupportConsistentViewModel,IDataObject
    {
        private BitmapImage _Icon;
        public BitmapImage Icon
        {
            get { return _Icon; }
            set { _Icon = value;OnPropertyChanged("Icon");
                LastModifyTime = DateTime.Now;
            }
        }

        private string _Name = "";
        public string Name
        {
            get { return _Name; }
            set 
            { 
                _Name = value;
                LastModifyTime = DateTime.Now; 
                OnPropertyChanged("Name");
            }
        }
        private string _Memo = "";
        public string Memo
        {
            get { return _Memo; }
            set
            {
                _Memo = value;
                LastModifyTime = DateTime.Now;
                OnPropertyChanged("Memo");
            }
        }
        public override void OnPropertyChanged(string name)
        {
            base.OnPropertyChanged(name);
            _LastModifyTime = DateTime.Now;
            base.OnPropertyChanged("LastModifyTime");
        }
        public override void SetStatus(string status){}

        private Guid _ObjectID = Guid.NewGuid();
        public Guid ObjectID
        {
            get { return _ObjectID; }
            set { _ObjectID = value; OnPropertyChanged("ObjectID"); }
        }


    }

    public class ControlViewModel : ViewModelBase, IIdentifiedObject
    {
        private Guid _ObjectID = Guid.NewGuid();//control view model is different to dtatobject, so use different id
        public Guid ObjectID
        {
            get { return _ObjectID; }
            set { _ObjectID = value; OnPropertyChanged("ObjectID"); }
        }

        private EntityBase _DataObject = null;
        public EntityBase DataObject
        {
            get { return _DataObject; }
            set
            {
                _DataObject = value;
                OnPropertyChanged("CreateTime");
                OnPropertyChanged("LastModifyTime");
                OnPropertyChanged("Icon");
                OnPropertyChanged("Name");
                OnPropertyChanged("Memo");
            }
        }

        public DateTime CreateTime
        {
            get { return DataObject.CreateTime; }
            set { DataObject.CreateTime = value; OnPropertyChanged("CreateTime"); }
        }

        public DateTime LastModifyTime
        {
            get { return DataObject.LastModifyTime; }
            set { DataObject.LastModifyTime = value; OnPropertyChanged("LastModifyTime"); }
        }
        public BitmapImage Icon
        {
            get { return DataObject.Icon; }
            set
            {
                DataObject.Icon = value; 
                OnPropertyChanged("Icon");
                OnPropertyChanged("LastModifyTime");
            }
        }

        public string Name
        {
            get { return DataObject.Name; }
            set
            {
                DataObject.Name = value;
                OnPropertyChanged("LastModifyTime");
                OnPropertyChanged("Name");
            }
        }

        public string Memo
        {
            get { return DataObject.Memo; }
            set
            {
                DataObject.Memo = value;
                OnPropertyChanged("LastModifyTime");
                OnPropertyChanged("Memo");
            }
        }

    }
}
