using BackTestingInterface;
using CommonLib;
using CommonLibForWPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UUBacktesting.ViewModel
{
    public abstract class ParameterObjectViewModelBase<T> : ViewModelBase, IEditableViewModel where T : IParameterSupportObject
    {
        T _TargetObject;
        public T TargetObject
        {
            get { return _TargetObject; }
            set
            {
                if (value != null)
                {
                    _TargetObject = value;
                    Load();
                }
                OnPropertyChanged("TargetObject");
            }
        }

        ObservableCollection<Parameter> _ParameterList = new ObservableCollection<Parameter>();
        public ObservableCollection<Parameter> ParameterList { get { return _ParameterList; } }
        public Parameter CurrentParameter { get; set; }

        public virtual CommonCommand AddParameterCommand
        {
            get
            {
                return new CommonCommand((o) => {
                    AddParameter();
                });
            }
        }
        public virtual CommonCommand DeleteParameterCommand
        {
            get
            {
                return new CommonCommand((o) => {
                    DeleteParameter();
                });
            }
        }

        public virtual void AddParameter()
        {
            var p = new Parameter();
            ParameterList.Add(p);
            TargetObject.ParameterList.Add(p);
        }

        public virtual void DeleteParameter()
        {
            if (CurrentParameter != null && ParameterList.Contains(CurrentParameter))
            {
                
                if (TargetObject.ParameterList.Contains(CurrentParameter))
                    TargetObject.ParameterList.Remove(CurrentParameter);
                ParameterList.Remove(CurrentParameter);
            }
        }

        public virtual void Load()
        {
            if (TargetObject == null) return;
            TargetObject.SaveToParameterList();
            ParameterList.Clear();
            TargetObject.ParameterList.ForEach(v => ParameterList.Add(v));
            RefreshView();
        }

        public virtual void Save()
        {
            if (TargetObject == null) return;
            TargetObject.ParameterList.Clear();
            foreach (var v in ParameterList)
                TargetObject.ParameterList.Add(v);
            TargetObject.LoadFromParameterList();
            RefreshView();
        }

        public virtual CommonCommand LoadCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    Load();
                });
            }
        }
        public virtual CommonCommand SaveCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    Save();
                });
            }
        }

        public virtual void RefreshView()
        {
            OnPropertyChanged("Name");
            OnPropertyChanged("Memo");
        }
    }
}
