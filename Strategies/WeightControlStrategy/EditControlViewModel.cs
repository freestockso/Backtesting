using BackTestingInterface;
using CommonLibForWPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeightControlStrategy
{
    class EditControlViewModel:ViewModelBase
    {
        public IPortfolio TargetPortfolio { get; set; }
        public void InitPortfolio(List<IPortfolio> portfolioList)
        {
            //TargetList.Clear();
            portfolioList.ForEach(v => InitPortfolio(v));
        }
        public void InitPortfolio(IPortfolio portfolio)
        {
            TargetPortfolio = portfolio;
            //TargetList.Clear();
            if (portfolio!=null)
                portfolio.PositionList.ForEach(v =>
                {
                    var pos = TargetList.FirstOrDefault(x => x.TargetTicker == v.Name);
                    if (pos == null)
                    {
                        PosWeightTarget posweight = new PosWeightTarget();
                        posweight.TargetTicker = v.Name;
                        posweight.Shares = v.Shares;
                        posweight.CurrentPrice = v.CurrentPrice;
                        posweight.CurrentWeight = v.CurrentValue / portfolio.CurrentValue;
                        posweight.DataTime = v.DataTime;
                        posweight.CurrentCurrency = v.CurrentCurrency;
                        posweight.TargetPortfolioName = portfolio.Name;
                        TargetList.Add(posweight);
                    }
                });
        }
        public void Init()
        {
            if (LoadValue != null)
            {
                var l = LoadValue();
                TargetList.Clear();
                if(l!=null)
                    TargetList.Add(l);
            }
        }
        public PosWeightTarget CurrentPosWeightTarget { get; set; }
        ObservableCollection<PosWeightTarget> _TargetList = new ObservableCollection<PosWeightTarget>();
        public ObservableCollection<PosWeightTarget> TargetList
        {
            get { return _TargetList; }
        }

        public Action<PosWeightTarget> SaveValue { get; set; }
        public Func<PosWeightTarget> LoadValue { get; set; }
        public CommonCommand AddValueCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    TargetList.Add(new PosWeightTarget());
                });
            }
        }
        public CommonCommand RemoveValueCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    if (CurrentPosWeightTarget!=null)
                        TargetList.Remove(CurrentPosWeightTarget);
                });
            }
        }
        public CommonCommand SaveValueCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    if (SaveValue != null)
                        SaveValue(CurrentPosWeightTarget);
                });
            }
        }
        public CommonCommand LoadValueCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    Init();
                        
                });
            }
        }
    }
}
