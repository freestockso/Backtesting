using BackTestingCommonLib.OrderProcessor;
using BackTestingCommonLib.RiskControl;
using BackTestingCommonLib.Strategy;
using BackTestingInterface;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BackTestingCommonLib
{
    public class BacktestingProject :ProjectBase, IBacktestingProject
    {
        bool _IsUnlimited = true;
        public bool IsUnlimited { get { return _IsUnlimited; } set { _IsUnlimited = value; } }

        bool _UseFirstMarketDataInit = true;
        public bool UseFirstMarketDataInit { get { return _UseFirstMarketDataInit; } set { _UseFirstMarketDataInit = value; } }

        List<IMarketData> _MarketDataList = new List<IMarketData>();
        public List<IMarketData> MarketDataList { get { return _MarketDataList; } }

        List<IOrder> _OrderList = new List<IOrder>();
        public List<IOrder> OrderList { get { return _OrderList; } }

        public DateTime TestCurrentTime { get; set; }

        [JsonIgnore]
        public override double FinishPercent
        {
            get
            {
                if ((TestEndTime - TestStartTime).TotalSeconds == 0)
                    return 0;
                return (TestCurrentTime-TestStartTime).TotalSeconds / (TestEndTime - TestStartTime).TotalSeconds;
            }
        }

        DateTime _TestEndTime = DateTime.Now - TimeSpan.FromDays(5);
        public DateTime TestEndTime
        {
            get { return _TestEndTime; }
            set { _TestEndTime = value; }
        }
        DateTime _TestStartTime = DateTime.Now - TimeSpan.FromDays(10);
        public DateTime TestStartTime
        {
            get { return _TestStartTime; } set { _TestStartTime = value; if (_AnalyseStartTime > value) _AnalyseStartTime = value; }
        }

        [JsonIgnore]
        public Money CurrentValue
        {
            get
            {
                return TargetPortfolio.CurrentValue;
            }
        }

        string _Name = "UnNamedProject";
        public override string Name
        {
            get
            {
                return _Name;
            }

            set
            {
                _Name = value;
            }
        }

        public override void LoadOriginalStatus()
        {
            if (Status != ProjectStatus.Stopped)
                Stop();
            CurrentDataSource.LoadOriginalStatus();
            ConditionList.ForEach(v => v.LoadOriginalStatus());
            TargetPortfolio.LoadOriginalStatus();
            TestStrategy.LoadOriginalStatus();
            CurrentTradeGate.LoadOriginalStatus();
            RiskPolicy.LoadOriginalStatus();
        }

        public override void SaveOriginalStatus()
        {
            CurrentDataSource.SaveOriginalStatus();
            ConditionList.ForEach(v => v.SaveOriginalStatus());
            TargetPortfolio.SaveOriginalStatus();
            TestStrategy.SaveOriginalStatus();
            CurrentTradeGate.SaveOriginalStatus();
            RiskPolicy.SaveOriginalStatus();
        }

        DateTime analyseTime;
        void AnalyseStep()
        {
            InstrumentList.ForEach(v => v.Memo = "");
            ConditionList.ForEach(v => v.ClearResult());
            foreach(var inst in InstrumentList)
                foreach (var condition in ConditionList)
                {

                    condition.GenerateResult(inst, AnalyseStartTime, analyseTime,AnalyseGrade);
                }
        }
        void TestStep(DateTime start,DateTime end)
        {
            TestStrategy.CurrentTime = end;
            AddInfo("test step, start:" + start.ToString() + ",end:" + end.ToString());
            var dl =CurrentDataSource.GetDataList(InstrumentList,start,end,Grade);
            AddInfo("got market data ,count is:"+dl.Count.ToString());
            if (dl == null || dl.Count == 0) return;
            TestStrategy.CurrentTime = dl.Max(d => d.Time);
            dl.ForEach(v =>
            {
                var inst = InstrumentList.FirstOrDefault(i => i.Ticker == v.InstrumentTicker);
                if (inst != null)
                {
                    AddInfo("update "+inst.Name+" price, value is:"+v.Close.ToString()+"("+v.Time.ToString()+")");
                    inst.CurrentPrice = v.Close;
                }
            });
            MarketDataList.AddRange(dl);
            AddInfo("portfolio process market data!");
            TargetPortfolio.ProcessMarketData(dl);
            AddInfo("standard portfolio process market data!");
            standardPortfolio.ProcessMarketData(dl);
            AddInfo("strategy process market data!");
            TestStrategy.ProcessMarketData(dl);
            
            if (analyseTime <= end)
            {
                AddInfo("prepare analyse, analyse time is:" + analyseTime.ToString());
                AnalyseStep();
                analyseTime = MarketData.GetNextTime(end, AnalyseGrade);
                var cl = new List<ISignal>();
                foreach (var condition in ConditionList)
                {
                    var rl = condition.GetResult();
                    if (rl!=null&&rl.Count > 0)
                    {
                        cl.AddRange(rl);
                    }
                }
                if (cl.Count > 0)
                {
                    AddInfo("got signal, count is "+cl.Count.ToString());
                    TestStrategy.ProcessSignal(cl);
                }
            }
            AddInfo("strategy process portfolio");
            TestStrategy.ProcessPortfolio();
            var ol=TestStrategy.GetOrderList();
            
            if (ol.Count > 0)
            {
                AddInfo("strategy generate order, count is :" + ol.Count.ToString());
                List<IOrder> col = new List<IOrder>();
                foreach(var o in ol)
                {
                    if (o != null && RiskPolicy.PredictOrder(o, TargetPortfolio))
                        col.Add(o);
                }
                OrderList.AddRange(col);
                AddInfo("trade gate process order");
                CurrentTradeGate.ProcessorOrder(col);
                AddInfo("portfolio info before process order is"+GetPortfolioMemo(TargetPortfolio));
                TargetPortfolio.ProcessOrderList(col);
                AddInfo("portfolio info after process order is" + GetPortfolioMemo(TargetPortfolio));
            }

            if (!IsUnlimited)//adjust risk
            {
                AddInfo("adjust risk");
                ol = RiskPolicy.AdjustRisk(TargetPortfolio);
                if (ol.Count > 0)
                {
                    AddInfo("risk order generate, count is:" + ol.Count.ToString());
                    OrderList.AddRange(ol);
                    List<IOrder> col = ol.Where(v => v != null).ToList();
                    CurrentTradeGate.ProcessorOrder(col);
                    TargetPortfolio.ProcessOrderList(col);
                }
            }
            CurrentValueList.Add(new TimeValueObject() { Time = dl.Max(v=>v.Time), Value = CurrentValue,Memo=GetPortfolioMemo(TargetPortfolio) });
            StandardValueList.Add(new TimeValueObject() { Time = dl.Max(v => v.Time), Value = StandardValue, Memo = GetPortfolioMemo(standardPortfolio) });
            if (_MaxLost.Number > Pnl.Number)
                _MaxLost.Number = Pnl.Number;
            if (TestStepDelayMS>0)
                Thread.Sleep(TestStepDelayMS);
        }
        string GetPortfolioMemo(IPortfolio portfolio)
        {
            var s = "Position Value:";
            s += portfolio.PositionCapital.ToString();
            s += ",Capital:";
            s += portfolio.CurrentCapital.ToString();
            return s;
        }
        Money testStartValue=new Money();
        public override bool CanRun()
        {
            if (InstrumentList.Count == 0) return false;
            if (CurrentDataSource == null) return false;
            if (CurrentTradeGate == null) return false;
            if (RiskPolicy == null) return false;
            if (TestEndTime < TestStartTime || TestEndTime < AnalyseStartTime) return false;
                return true;
        }
        List<string> _TestInfo = new List<string>();
        void AddInfo(string info)
        {
            Console.WriteLine(info);
            _TestInfo.Add(Status.ToString() + "-" + DateTime.Now.ToString() + ":" + info + "\n");
        }
        public override void Start()
        {
            if (!CanRun())
                throw new Exception("Project not ready to run!");
            AddInfo("start running!");
            if (Status == ProjectStatus.Stopped)
            {
                Status = ProjectStatus.Running;
                AddInfo("prepare running!");
                PrepareWork();
                AddInfo("prepared!");
            }
            Status = ProjectStatus.Running;

            ProjectStartTime = DateTime.Now;
            AddInfo("started running, started time is "+ProjectStartTime.ToString());
            for (DateTime time =TestCurrentTime; time <= TestEndTime; time = MarketData.GetNextTime(time, Grade))
            {
                AddInfo("test time is :" + time.ToString()+", prepare into step");
                TestStep(time,MarketData.GetNextTime( time,Grade));
                TestCurrentTime=time  ;
                if (Status != ProjectStatus.Running)
                    return;
            }


            AddInfo("stop running!");
            Stop();
            AddInfo("stoped!");
            if (_TestInfo.Count > 0)
            {
                if (File.Exists(logFilePath))
                    File.Delete(logFilePath);
                File.WriteAllLines(logFilePath, _TestInfo.ToArray());
                File.WriteAllLines(logFilePath, GetResult().Split('\n'));
                if (File.Exists(logFilePath))
                    System.Diagnostics.Process.Start("notepad.exe", logFilePath);
            }
        }
        static string logFilePath = AppDomain.CurrentDomain.BaseDirectory + "BacktestingProjectProcessLog.txt";
        public override void Stop()
        {
            

            FinishWork();
            Thread.Sleep(3000);
            Status = ProjectStatus.Stopped;
        }
        public override void Pause()
        {
            if(Status== ProjectStatus.Running)
                Status = ProjectStatus.Pause;

        }

        [JsonIgnore]
        public ITradeGate CurrentTradeGate
        {
            get;set;
        }

        IPortfolio _TargetPortfolio;
        public IPortfolio TargetPortfolio
        {
            get
            {
                if (_TargetPortfolio == null)
                {
                    _TargetPortfolio = new Portfolio()
                    {
                        GetInstrumentList = () => { return InstrumentList; }
                    };
                }
                return _TargetPortfolio;
            }

            set
            {
                _TargetPortfolio = value;
                _TargetPortfolio.GetInstrumentList = () => { return InstrumentList; };
            }
        }

        IStrategy _TestStrategy;
        [JsonIgnore]
        public IStrategy TestStrategy
        {
            get
            {
                if (_TestStrategy == null)
                {
                    _TestStrategy = new StandardStrategy();
                    _TestStrategy.GetInstrumentList = () => { return InstrumentList; };
                }
                return _TestStrategy;
            }

            set
            {
                _TestStrategy = value;
                if (_TestStrategy != null)
                {
                    _TestStrategy.GetInstrumentList =()=> { return InstrumentList; };
                }
            }
        }

        [JsonIgnore]
        public Money Pnl
        {
            get
            {
                return CurrentValue - testStartValue;
            }
        }

        [JsonIgnore]
        public Money StandardPnl
        {
            get
            {
                if (TargetPortfolio == null) return new Money() { Number = 0 };
                return CurrentValue - StandardValue;
            }
        }
        List<TimeValueObject> _CurrentValueList = new List<TimeValueObject>();
        public List<TimeValueObject> CurrentValueList
        {
            get
            {
                return _CurrentValueList;
            }
        }
        List<TimeValueObject> _StandardValueList = new List<TimeValueObject>();
        public List<TimeValueObject> StandardValueList
        {
            get
            {
                return _StandardValueList;
            }
        }
        IPortfolio standardPortfolio;
        public Money StandardValue
        {
            get
            {
                if(standardPortfolio==null) return new Money() { Number = 0 };
                return standardPortfolio.CurrentValue;
            }
        }

        public override void PrepareWork()
        {
            TargetPortfolio.PrepareWork();
            MarketDataList.Clear();
            CurrentDataSource.DataList.Clear();
            CurrentValueList.Clear();
            StandardValueList.Clear();
            OrderList.Clear();

            DateTime st = TestStartTime;
            if (TestStartTime > AnalyseStartTime)
                st = AnalyseStartTime;
            CurrentDataSource.CacheStartTime = st;
            CurrentDataSource.CacheEndTime = TestEndTime;

            TestCurrentTime = TestStartTime;
            analyseTime = MarketData.GetNextTime(TestCurrentTime,AnalyseGrade);

            _MaxLost = new Money() { FxCode = Pnl.FxCode, Number = 0 };
            if (UseFirstMarketDataInit)
            {
                var fdl = new List<IMarketData>();
                InstrumentList.ForEach(v =>
                {
                    var d = CurrentDataSource.GetFirstData(v, TestStartTime, TestEndTime,Grade);
                    if (d != null)
                        fdl.Add(d);
                });
                TargetPortfolio.ProcessMarketData(fdl);
                TargetPortfolio.PositionList.ForEach(v =>
                {
                    var d = fdl.FirstOrDefault(m => m.InstrumentTicker == v.InstrumentTicker);
                    if (d != null)
                        v.Cost = d.Close;
                });
            }
            if(IsUnlimited)
            {
                TargetPortfolio.IsUnlimited = true;
            }
            standardPortfolio = TargetPortfolio.Clone() as IPortfolio;
            testStartValue = TargetPortfolio.CurrentValue;
            CurrentDataSource.PrepareWork();
            CurrentTradeGate.PrepareWork();         
            foreach(var condition in ConditionList)
            {
                condition.PrepareWork();
                condition.GetInstrumentList = () => { return InstrumentList; };
                condition.AnalyseDataSource = CurrentDataSource;
            }

            TestStrategy.CurrentPortfolio = TargetPortfolio;
            TestStrategy.PrepareWork();
            RiskPolicy.PrepareWork();
        }

        public override void FinishWork()
        {
            TestCurrentTime = TestEndTime;
            TargetPortfolio.FinishWork();
            ConditionList.ForEach(v => v.FinishWork());
            //SensorList.ForEach(v => v.FinishWork());
            CurrentTradeGate.FinishWork();
            CurrentDataSource.FinishWork();
            TestStrategy.FinishWork();
            RiskPolicy.FinishWork();
        }

        public override object Clone()
        {
            var p = new BacktestingProject()
            {
                Name = Name,
                Memo = Memo,
                IsUnlimited=IsUnlimited,
                AnalyseGrade = AnalyseGrade,
                AnalyseStartTime = AnalyseStartTime,
                CurrentDataSource = CurrentDataSource.Clone() as IDataSource,
                CurrentTradeGate = CurrentTradeGate,
                Fine = Fine,
                Grade = Grade,
                ProjectStartTime = ProjectStartTime,
                RiskPolicy = RiskPolicy.Clone() as IRiskControl,
                Status = Status,
                TargetPortfolio = TargetPortfolio.Clone() as IPortfolio,
                TestStrategy = TestStrategy.Clone() as IStrategy,
                TestCurrentTime = TestCurrentTime,
                TestEndTime = TestEndTime,
                TestStartTime = TestStartTime,
                TestStepDelayMS = TestStepDelayMS,

            };

            ConditionList.ForEach(v => p.ConditionList.Add(v.Clone() as ICondition));
            InstrumentList.ForEach(v => p.InstrumentList.Add(v.Clone() as IInstrument));

            return p;
        }

        public SerialInfo TestStrategySerial { get; set; }

        public override void RecoverySerialObject()
        {
            var r = new BacktestingResource();

            if (TestStrategySerial != null)
            {
                var s = TestStrategySerial.CreateInstance(BacktestingResource.CommonResource.StrategyPrototypeList.Cast<ISerialSupport>().ToList());
                if (s != null)
                    TestStrategy = s as IStrategy;
            }
            if (DataSourceSerial != null)
            {
                var s = DataSourceSerial.CreateInstance(BacktestingResource.CommonResource.DataSourcePrototypeList.Cast<ISerialSupport>().ToList());
                if (s != null)
                    CurrentDataSource = s as IDataSource;
            }
            if (TradeGateSerial != null)
            {
                var s = TradeGateSerial.CreateInstance(BacktestingResource.CommonResource.TradeGatePrototypeList.Cast<ISerialSupport>().ToList());
                if (s != null)
                    CurrentTradeGate = s as ITradeGate;
            }
            if (RiskPolicySerial != null)
            {
                var s = RiskPolicySerial.CreateInstance(BacktestingResource.CommonResource.RiskControlPrototypeList.Cast<ISerialSupport>().ToList());
                if (s != null)
                    RiskPolicy = s as IRiskControl;
            }
            ConditionList.Clear();
            ConditionSerialList.ForEach(v =>
            {
                var i = v.CreateInstance(BacktestingResource.CommonResource.ConditionPrototypeList.Cast<ISerialSupport>().ToList());
                if (i != null)
                    ConditionList.Add(i as ICondition);
            });

        }

        public override void SerialObject()
        {
            TestStrategySerial = SerialInfo.GetSerialInfo(TestStrategy);
            RiskPolicySerial = SerialInfo.GetSerialInfo(RiskPolicy);
            DataSourceSerial = SerialInfo.GetSerialInfo(CurrentDataSource);
            TradeGateSerial = SerialInfo.GetSerialInfo(CurrentTradeGate);

            ConditionSerialList.Clear();
            ConditionList.ForEach(v => ConditionSerialList.Add(SerialInfo.GetSerialInfo(v)));
        }

        Money _MaxLost=new Money();
        [JsonIgnore]
        public Money MaxLost { get { return _MaxLost; } }
        [JsonIgnore]
        public double MaxLostPercent { get {
                if (IsUnlimited)
                {
                    if (_MaxCapital != 0) return MaxLost.Number / _MaxCapital;
                }
                if (testStartValue.Number != 0) return MaxLost.Number / testStartValue.Number;
                return 0;
            } }
        [JsonIgnore]
        public double Efficiency { get {
                if (IsUnlimited)
                {
                    if (_MaxCapital != 0) return Pnl.Number / _MaxCapital;
                }
                if (testStartValue.Number != 0) return Pnl.Number / testStartValue.Number;
                return 0;
            } }
        [JsonIgnore]
        public double AverageEfficiency { get {
                var l = GetEfficiencyTrace();
                if(l.Count>0)
                return l.Average(v => v.DoubleValue);
                return 0;
            } }

        double _MaxCapital = 0;
        [JsonIgnore]
        public double MaxCapital { get { return _MaxCapital; } }

        public List<TimeValueObject> GetEfficiencyTrace()
        {
            List<TimeValueObject> l = new List<TimeValueObject>();
            CurrentValueList.ForEach(v =>
            {
                var value = new TimeValueObject() { Time = v.Time };
                if (IsUnlimited)
                {
                    if (_MaxCapital != 0) value.Value = value.DoubleValue/  _MaxCapital;
                }
                else {
                    if (testStartValue.Number != 0)
                        value.Value = (value.DoubleValue - testStartValue.Number) / testStartValue.Number;
                }
                l.Add(value);
            });
            return l;
        }

        MarketDataGrade _AnalyseGrade = MarketDataGrade.FiveMinutes;
        public MarketDataGrade AnalyseGrade { get { return _AnalyseGrade; } set { _AnalyseGrade = value; } }
        DateTime _AnalyseStartTime=DateTime.Now-TimeSpan.FromDays(30);
        public DateTime AnalyseStartTime { get { return _AnalyseStartTime; } set { if (value > _TestStartTime) _AnalyseStartTime = _TestStartTime;else _AnalyseStartTime = value; } }

        IRiskControl _RiskPolicy = new StandardRiskControl();
        [JsonIgnore]
        public IRiskControl RiskPolicy
        {
            get
            {
                return _RiskPolicy;
            }

            set
            {
                _RiskPolicy = value;
            }
        }

        public SerialInfo StrategySerial
        {
            get;set;
        }

        public SerialInfo TradeGateSerial
        {
            get; set;
        }

        public SerialInfo RiskPolicySerial
        {
            get; set;
        }

        public override string GetResult()
        {
            var s = "";

            s += "Backtesting Project " + Name + " from " + TestStartTime.ToString() + " to " + TestEndTime.ToString() + "\n";
            s += "Current Value:" + CurrentValue.ToString()+"\n";
            s += "Pnl:" + Pnl.ToString() + "\n";
            s += "Efficiency:" + Efficiency.ToString() + "\n";
            s += "AverageEfficiency:" + AverageEfficiency.ToString() + "\n";
            s += "Max Lost:" + MaxLost.ToString() + "\n";
            s += "Max Capital:" + MaxCapital.ToString() + "\n";
            
            s += "Max Lost Percent:" + MaxLostPercent.ToString() + "\n";
            return s;
        }
    }
}
