using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonLib
{
    public interface IStatusSupportObject
    {
        string GetStatus();
        void SetStatus(string status);

        void UpdateStatus();
        void LoadStatus();
        string Status { get; set; }
    }
}
