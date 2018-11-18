using BackTestingCommonLib;
using BackTestingInterface;
using CommonLibForWPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace UUBacktesting.ViewModel
{
    class PortfolioViewModel:ViewModelBase,IObersverSupport
    {
        IPortfolio _TargetObject;
        public IPortfolio TargetObject
        {
            get { return _TargetObject; }
            set
            {
                if (value != null)
                {
                    _TargetObject = value;
                    Refresh();
                }
                OnPropertyChanged("TargetObject");
            }
        }
        ObservableCollection<IPosition> _PositionList = new ObservableCollection<IPosition>();
        public ObservableCollection<IPosition> PositionList { get { return _PositionList; } }

        ObservableCollection<IAccount> _AccountList = new ObservableCollection<IAccount>();
        public ObservableCollection<IAccount> AccountList { get { return _AccountList; } }
        public IPosition CurrentPosition { get; set; }
        public IAccount CurrentMoney { get; set; }
        public void Save()
        {
            if (TargetObject == null) return;
            TargetObject.PositionList.Clear();
            foreach (var v in PositionList)
            {
                
                TargetObject.PositionList.Add(v);
            }
            TargetObject.AccountList.Clear();
            foreach (var v in AccountList)
            {
                TargetObject.AccountList.Add(v);
            }
        }

        public void Refresh()
        {
            if (TargetObject == null) return;
            var o = TargetObject as IPortfolio;
            if (o == null) return;

            PositionList.Clear();
            AccountList.Clear();

            o.PositionList.ForEach(v =>
            {
                    PositionList.Add(v);
            });

            o.AccountList.ForEach(v =>
            {
                    AccountList.Add(v);
            });


        }

        public bool NeedRefresh()
        {
            return true;
        }

        public CommonCommand AddMoneyCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {

                        AccountList.Add(new Account() { Quantity = 1000 });
                });
            }
        }
        public CommonCommand DeleteMoneyCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    if (CurrentMoney != null && AccountList.Contains(CurrentMoney))
                        AccountList.Remove(CurrentMoney);
                });
            }
        }
        public CommonCommand DeletePositionCommand
        {
            get {
                return new CommonCommand((o) =>
          {
              if (CurrentPosition != null && PositionList.Contains(CurrentPosition))
                  PositionList.Remove(CurrentPosition);
          });
            }
        }
        public CommonCommand SaveOrigenalCommand
        {
            get {
                return new CommonCommand((o) =>
                  {
                      if (TargetObject == null) return;
                      Save();
                      TargetObject.SaveOriginalStatus();
                  });
            }
        }
        public CommonCommand LoadOrigenalCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    if (TargetObject == null) return;
                    
                    TargetObject.LoadOriginalStatus();
                    Refresh();
                });
            }
        }
        public CommonCommand SaveCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    Save();
                });
            }
        }
        public CommonCommand RefreshCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    Refresh();
                });
            }
        }
        public CommonCommand AddInstrumentCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    var l = AllInstrumentWindowViewModel.GetInstrumentList();
                    if (l != null && l.Count > 0)
                    {
                        l.ForEach(v =>
                        {
                            if (!PositionList.Any(i => i.InstrumentTicker == v.Ticker))
                            {
                                var p = new Position() { InstrumentName=v.Name,InstrumentTicker=v.Ticker,CurrentPrice=v.CurrentPrice, Shares = Order.MinOperationShares };


                                if (!TargetObject.PositionList.Any(i => i.InstrumentTicker == v.Ticker))
                                {
                                    PositionList.Add(p);
                                    TargetObject.PositionList.Add(p);
                                }
                            }
                        });
                    }
                });
            }
        }

        public CommonCommand ResetTradeListCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    if (CurrentPosition == null) return;
                    CurrentPosition.RefreshTradeTrace();
                    OnPropertyChanged("CurrentPosition");
                    Refresh();
                });
            }
        }


    }

    public class RowStyleSelector : StyleSelector
    {
        public override Style SelectStyle(object item, DependencyObject container)
        {
            if (((GridViewRow)container).GridViewDataControl.Items.IndexOf(item) == 0)
            {
                Style style = new Style(typeof(GridViewRow)) { BasedOn = (Style)Application.Current.Resources["GridViewRowStyle"] };
                Setter setter = new Setter(GridViewRow.DetailsVisibilityProperty, Visibility.Visible);
                style.Setters.Add(setter);
                return style;
            }

            return new Style(typeof(GridViewRow)) { BasedOn = (Style)Application.Current.Resources["GridViewRowStyle"] };
        }
    }
}
