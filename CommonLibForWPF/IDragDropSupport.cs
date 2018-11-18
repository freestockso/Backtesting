using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace CommonLibForWPF
{
    public class DragDropSupport
    {
        static List<object> _DragingData=new List<object>();
        public static List<object> DragingData
        {
            get { return _DragingData; }
        }

        static FrameworkElement dragSource;
        static Action<List<object>> endDropAction;
        public static void OnDrag(List<object> dataList, FrameworkElement element, Action<List<object>> EndDrop = null)
        {
            if (dataList != null && dataList.Count > 0)
            {
                DragDrop.DoDragDrop(element, dataList, DragDropEffects.Copy);
                dragSource = element;
                endDropAction = EndDrop;
            }
        }

        public static void OnDrop<T>(DragEventArgs e,Action<List<T>> dropAction)
        {
            var tl = GetData<T>(e);
            if (tl.Count > 0)
            {
                dropAction(tl);
                if (endDropAction != null)
                    endDropAction(tl.Cast<object>().ToList());
            }
        }
        static List<T> GetData<T>(DragEventArgs e)
        {
            var ol = e.Data.GetData(typeof(List<object>)) as List<object>;
            if (ol == null) return new List<T>();
            var tl = new List<T>();
            ol.ForEach(o =>
            {
                if (o is T)
                    tl.Add((T)o);
            });
            return tl;
        }
    }

    public interface IDragSupport
    {
        void DragStart(List<object> dataList);
    }

    public interface IDropSupport
    {
        bool CanDrop(List<object> dataList);
        void Dropped(List<object> dataList);
    }
}
