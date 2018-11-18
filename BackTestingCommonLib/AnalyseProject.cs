using BackTestingInterface;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BackTestingCommonLib
{
    public class AnalyseProject : ProjectBase, IAnalyseProject
    {
        DateTime _AnalyseStartTime = DateTime.Now-TimeSpan.FromDays(7);
        public DateTime AnalyseStartTime
        {
            get
            {
                return _AnalyseStartTime;
            }

            set
            {
                _AnalyseStartTime = value;
                IsChanged = true;
            }
        }

        DateTime _AnalyseEndTime = DateTime.Now;
        public DateTime AnalyseEndTime
        {
            get
            {
                return _AnalyseEndTime;
            }

            set
            {
                _AnalyseEndTime = value;
                IsChanged = true;
            }
        }

        int _DefaultFilerNumber = 20;
        public int DefaultFilterNumber
        {
            get { return _DefaultFilerNumber; }
            set { _DefaultFilerNumber = value; IsChanged = true; }
        }

        List<ICondition> _PredicateList = new List<ICondition>();
        [JsonIgnore]
        public List<ICondition> PredicateList
        {
            get
            {
                return _PredicateList;
            }
        }

        List<SerialInfo> _PredicateSerialList = new List<SerialInfo>();
        public List<SerialInfo> PredicateSerialList
        {
            get
            {
                return _PredicateSerialList;
            }
        }

        List<IInstrument> _ResultList = new List<IInstrument>();
        public List<IInstrument> ResultList
        {
            get
            {
                return _ResultList;
            }
        }

        List<IInstrument> _BlockList = new List<IInstrument>();
        public List<IInstrument> BlockList
        {
            get
            {
                return _BlockList;
            }
        }

        string _Name = "Unnamed analyse project";
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

        public override void SerialObject()
        {
            ConditionSerialList.Clear();
            ConditionList.ForEach(v => ConditionSerialList.Add(SerialInfo.GetSerialInfo(v)));

            PredicateSerialList.Clear();
            PredicateList.ForEach(v => PredicateSerialList.Add(SerialInfo.GetSerialInfo(v)));
        }
        public override void RecoverySerialObject()
        {
            ConditionList.Clear();
            ConditionSerialList.ForEach(v =>
            {
                var i = v.CreateInstance(BacktestingResource.CommonResource.ConditionPrototypeList.Cast<ISerialSupport>().ToList());
                if (i != null)
                    ConditionList.Add(i as ICondition);
            });

            PredicateList.Clear();
            PredicateSerialList.ForEach(v =>
            {
                var i = v.CreateInstance(BacktestingResource.CommonResource.ConditionPrototypeList.Cast<ISerialSupport>().ToList());
                if (i != null)
                    PredicateList.Add(i as ICondition);
            });
        }

        int total = 0;
        int current = 0;

        void Process(List<IInstrument> sl)
        {
            for (var i=current ; i < total; i++)
            {
                foreach(var condition in ConditionList)
                {
                    condition.GetInstrumentList = () => { return sl; };
                    condition.AnalyseDataSource = CurrentDataSource;
                    if(condition.MaxResultCount==int.MaxValue)
                        condition.MaxResultCount = DefaultFilterNumber;
                    condition.GenerateResult(sl[i],AnalyseStartTime,AnalyseEndTime,null);
                }
                foreach (var predicate in PredicateList)
                {
                    predicate.GetInstrumentList = () => { return sl; };
                    predicate.AnalyseDataSource = CurrentDataSource;
                    if (predicate.MaxResultCount==int.MaxValue)
                    predicate.MaxResultCount = DefaultFilterNumber;
                    predicate.GenerateResult(sl[i], AnalyseStartTime, AnalyseEndTime,null);
                }

                current++;
                if (TestStepDelayMS > 0)
                    Thread.Sleep(TestStepDelayMS);
                if (Status != ProjectStatus.Running)
                {
                    RefreshResult();
                    break;
                }
            }
            RefreshResult();
        }
        void RefreshResult()
        {
            ResultList.Clear();
            foreach (var condition in ConditionList)
            {
                var l = condition.GetResult(null, AnalyseEndTime);
                foreach (var r in l)
                {
                    var inst = InstrumentList.FirstOrDefault(v => v.Ticker == r.Ticker);
                    if(inst!=null)
                        if (!ResultList.Any(v => v.Ticker == inst.Ticker))
                            ResultList.Add(inst);
                }
                    
            }
                
            var rl = new List<IInstrument>();
            foreach (var preicate in PredicateList)
            {
                rl.Clear();
                var l = preicate.GetResult(null, AnalyseEndTime);
                foreach (var r in l)
                {
                    var inst = InstrumentList.FirstOrDefault(v => v.Ticker == r.Ticker);
                    rl.Add(inst);
                }
                ResultList.RemoveAll(v => !rl.Any(r => r.Ticker == v.Ticker));
            }
            
        }

        public void GenerateResult()
        {
            var sl = InstrumentList.Except(BlockList).ToList();
            if (sl.Count == 0) return;
            total = sl.Count;
            Process(sl);
        }

        public override object Clone()
        {
            var p = new AnalyseProject()
            {
                AnalyseEndTime = AnalyseEndTime,
                AnalyseStartTime = AnalyseStartTime,
                ProjectStartTime = ProjectStartTime,
                CurrentDataSource = CurrentDataSource.Clone() as IDataSource,
                DefaultFilterNumber = DefaultFilterNumber,
                Fine = Fine,
                Grade = Grade,
                Memo = Memo,
                Name = Name,
                TestStepDelayMS = TestStepDelayMS,
                Status = Status
            };

            InstrumentList.ForEach(v => p.InstrumentList.Add(v.Clone() as IInstrument));
            ResultList.ForEach(v => p.ResultList.Add(v.Clone() as IInstrument));
            BlockList.ForEach(v => p.BlockList.Add(v.Clone() as IInstrument));
            ConditionList.ForEach(v => p.ConditionList.Add(v.Clone() as ICondition));
            PredicateList.ForEach(v => p.PredicateList.Add(v.Clone() as ICondition));

            return p;
        }
        public override void LoadOriginalStatus()
        {
            if (Status != ProjectStatus.Stopped)
                Stop();
            CurrentDataSource.LoadOriginalStatus();
            ConditionList.ForEach(v => v.LoadOriginalStatus());
            PredicateList.ForEach(v => v.LoadOriginalStatus());
        }

        public override void SaveOriginalStatus()
        {
            CurrentDataSource.SaveOriginalStatus();
            ConditionList.ForEach(v => v.SaveOriginalStatus());
            PredicateList.ForEach(v => v.SaveOriginalStatus());

        }
        [JsonIgnore]
        public override double FinishPercent
        {
            get
            {
                if (total == 0) return 0;
                return Convert.ToDouble(current) / total;
            }
        }
        public override bool CanRun()
        {
            if ((ConditionList.Count == 0&&PredicateList.Count==0) || InstrumentList.Count == 0) return false;
            if (AnalyseEndTime <= AnalyseStartTime) return false;
            if (CurrentDataSource == null) return false;
            return true;
        }

        public override void Start()
        {
            if (!CanRun())
                throw new Exception("Project not ready to run!");

                
            if(Status== ProjectStatus.Stopped)
            {
                Status = ProjectStatus.Running;
                PrepareWork();

            }
            Status = ProjectStatus.Running;
            GenerateResult();
            Stop();
        }

        public override void Pause()
        {
            if(Status== ProjectStatus.Running)
                Status = ProjectStatus.Pause;
        }

        public override void Stop()
        {
            Status = ProjectStatus.Stopped;
            FinishWork();
        }

        public override void PrepareWork()
        {
            CurrentDataSource.PrepareWork();
            ProjectStartTime = DateTime.Now;
            ResultList.Clear();
            ConditionList.ForEach(v => v.PrepareWork());
            PredicateList.ForEach(v => v.PrepareWork());
            current = 0;
            CurrentDataSource.CacheSize = 5;
        }

        public override void FinishWork()
        {
            CurrentDataSource.FinishWork();
            ConditionList.ForEach(v => v.FinishWork());
            PredicateList.ForEach(v => v.FinishWork());
        }

        public override string GetResult()
        {
            var s = "";

            s += "Analyse Project " + Name + " from " + AnalyseStartTime.ToString() + " to " + AnalyseEndTime.ToString() + "\n";
            foreach(var i in ResultList)
            {
                s += i.Name + "(" + i.Ticker + ")\n";
            }


            return s;
        }
    }

    
}
