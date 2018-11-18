using BackTestingCommonLib.OrderProcessor;
using BackTestingCommonLib.RiskControl;
using BackTestingCommonLib.Strategy;
using BackTestingInterface;
using CommonDataSource;
using CommonLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingCommonLib
{
    public class BacktestingResource
    {
        public static string BacktestingProjectFileExt
        {
            get
            {
                return "btproject";
            }
        }
        public static string AnalyseProjectFileExt
        {
            get
            {
                return "alyproject";
            }
        }
        static BacktestingResource _CommonResource;
        public static BacktestingResource CommonResource
        {
            get
            {
                if(_CommonResource==null)
                {
                    _CommonResource = new BacktestingResource();
                    _CommonResource.Initialize();

                }
                return _CommonResource;
            }
        }
        List<IStrategy> _StrategyPrototypeList = new List<IStrategy>();
        public List<IStrategy> StrategyPrototypeList { get { return _StrategyPrototypeList; } }
        List<IDataSource> _DataSourcePrototypeList = new List<IDataSource>();
        public List<IDataSource> DataSourcePrototypeList { get { return _DataSourcePrototypeList; } }
        List<ITradeGate> _TradeGatePrototypeList = new List<ITradeGate>();
        public List<ITradeGate> TradeGatePrototypeList { get { return _TradeGatePrototypeList; } }
        List<ICondition> _ConditionPrototypeList = new List<ICondition>();
        public List<ICondition> ConditionPrototypeList { get { return _ConditionPrototypeList; } }
        List<IRiskControl> _RiskControlPrototypeList = new List<IRiskControl>();
        public List<IRiskControl> RiskControlPrototypeList { get { return _RiskControlPrototypeList; } }

        public string StrategyPath
        {
            get { return System.AppDomain.CurrentDomain.BaseDirectory + "Strategy\\"; }
        }
        public string DataSourcePath
        {
            get { return System.AppDomain.CurrentDomain.BaseDirectory + "DataSource\\"; }
        }
        public string TradeGatePath
        {
            get { return System.AppDomain.CurrentDomain.BaseDirectory + "TradeGate\\"; }
        }

        public string ConditionPath
        {
            get { return System.AppDomain.CurrentDomain.BaseDirectory + "Condition\\"; }
        }
        public string RiskControlPath
        {
            get { return System.AppDomain.CurrentDomain.BaseDirectory + "RiskControl\\"; }
        }

        public void InitPrototype<T>(List<T> list,string path,string nameKey)
        {
            LogSupport.Info("start init "+typeof(T).Name);
            try
            {
                list.Clear();
                if (Directory.Exists(path))
                {
                    var subDir = Directory.GetDirectories(path);
                    foreach (var dir in subDir)
                    {
                        var folder = new DirectoryInfo(dir);
                        foreach (FileInfo tFile in folder.GetFiles())
                        {
                            if ((tFile.Name.EndsWith("dll") || tFile.Name.EndsWith("exe")) && tFile.Name.Contains(nameKey))
                            {
                                Assembly ass = Assembly.LoadFile(tFile.DirectoryName + "\\" + tFile.Name);
                                var tl = ass.GetTypes().ToList();
                                tl.ForEach(v =>
                                {
                                    if (v.GetInterface(typeof(T).Name)!=null)
                                    {
                                        object obj = Activator.CreateInstance(v);
                                        var dp = (T)obj ;
                                        if (dp != null)
                                        {
                                            LogSupport.Info("loading:" + tFile.Name);
                                            //dp.Initialize();

                                            list.Add(dp);
                                        }
                                    }
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogSupport.Error(ex);
            }
            LogSupport.Info("Loaded count:" + list.Count.ToString());
        }

        public void Initialize()
        {
            InitPrototype<IDataSource>(DataSourcePrototypeList, DataSourcePath, "DataSource");
            InitPrototype<ITradeGate>(TradeGatePrototypeList, TradeGatePath, "TradeGate");
            InitPrototype<IStrategy>(StrategyPrototypeList, StrategyPath, "Strategy");
            InitPrototype<ICondition>(ConditionPrototypeList, ConditionPath, "Condition");
            InitPrototype<IRiskControl>(RiskControlPrototypeList, RiskControlPath, "RiskControl");

            DataSourcePrototypeList.Add(new HistoricalDataSource());
            TradeGatePrototypeList.Add(new CommonOrderProcessor());
            StrategyPrototypeList.Add(new StandardStrategy());
            RiskControlPrototypeList.Add(new StandardRiskControl());

        }
    }
}
