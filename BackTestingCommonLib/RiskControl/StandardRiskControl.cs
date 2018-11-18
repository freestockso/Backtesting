using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackTestingInterface;

namespace BackTestingCommonLib.RiskControl
{
    [SerialObjectAttribute (Name="Standard Risk Control",Key = "StandardRiskControl")]
    public class StandardRiskControl:RiskControlBase
    {
        public override string Memo
        {
            get;set;
        }

        

        public override List<IOrder> AdjustRisk(IPortfolio portfolio)
        {
            var orderList = new List<IOrder>();
            var l = AdjustWeight(portfolio);
            if (l != null)
                orderList.AddRange(l);

            PositionControlList.ForEach(v =>
            {
                var o = v.AdjustRisk(portfolio);
                if (o != null)
                    orderList.Add(o);
            });

            return orderList;
        }

        public override IRiskControl CreateInstance()
        {
            return new StandardRiskControl();
        }
    }
}
