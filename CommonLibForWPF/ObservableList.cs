using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibForWPF
{
    public class ObservableList<T> : List<T>, INotifyCollectionChanged
    {
        bool _IsModifyList = false;
        public bool IsModifyList
        {
            get { return _IsModifyList; }
            set { _IsModifyList = true; }
        }
        public void Synchronize(List<T> list)
        {
            list.Clear();
            list.AddRange(this);
        }
        public ObservableList(){

        }
        public ObservableList(List<T> list)
        {
            AddRange(list);
        }
        public void LoadData(List<T> list)
        {
            AddRange(list);
        }
        public virtual void OnPropertyChanged(string name)
        {
            if (CollectionChanged != null)
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
    }

    public class ObservableDictionary<S,T> : Dictionary<S,T>, INotifyCollectionChanged
    {
        public ObservableDictionary(){

        }
        public ObservableDictionary(Dictionary<S,T> list)
        {
            foreach (var kv in list)
                Add(kv.Key, kv.Value);
        }
        public void AddRange(Dictionary<S, T> list)
        {
            foreach (var kv in list)
                Add(kv.Key, kv.Value);
        }
        public void Synchronize(Dictionary<S, T> list)
        {
            list.Clear();
            foreach (var kv in this)
                list.Add(kv.Key, kv.Value);
        }
        public void LoadData(Dictionary<S, T> list)
        {
            foreach (var kv in list)
                Add(kv.Key, kv.Value);
        }
        public virtual void OnPropertyChanged(string name)
        {
            if (CollectionChanged != null)
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
    }
}
