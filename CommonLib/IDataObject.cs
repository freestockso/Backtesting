using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace CommonLib
{
    public interface IIdentifiedObject
    {
        
        Guid ObjectID { get; set; }
        //string MessageMemo { get; }
    }

    public interface IDataObject 
    {
        string Name { get; set; }
        string Memo { get; set; }
    }

    public interface IEditEnableObject
    {
        bool IsEditable { get; set; }
    }
}
