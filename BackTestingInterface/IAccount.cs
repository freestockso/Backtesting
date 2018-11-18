using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonLib;

namespace BackTestingInterface
{
    //save monsy for portfolio
    public interface IAccount : IFxRateObject,ICloneable, IChangeNotify
    {
        bool IsUnlimited { get; set; }
        bool IsValidModify(Money target);
        bool ObtainMoney(Money target);//add money
        bool CommitMoney(Money target);//spend money

        Money CurrentValue { get; }
        double Quantity
        {
            get;
            set;
        }

        bool IsEnable
        {
            get;
            set;
        }

        double MaxValue
        {
            get;
            set;
        }

        double MinValue
        {
            get;
            set;
        }

    }
}
