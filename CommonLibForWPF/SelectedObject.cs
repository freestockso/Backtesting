using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibForWPF
{
    public class SelectedObject<T>:ViewModelBase
    {
        public SelectedObject(string name,T targetObject,string memo=null)
        {
            Name = name;
            Memo = memo;
            TargetObject = targetObject;
        }
        bool _IsSelected = false;
        public bool IsSelected
        {
            get { return _IsSelected; }
            set { _IsSelected = value; OnPropertyChanged("IsSelected"); }
        }

        public string Name { get; set; }
        public string Memo { get; set; }
        public T TargetObject { get; set; }
    }
}
