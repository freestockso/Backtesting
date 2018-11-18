using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PineRiver.Core.Config.Implementations;
using PineRiver.Core.MarketData;
using PineRiver.Core.MarketData.BPipe;
using PineRiver.Core.MarketData.BPipe.Authentication;
using PineRiver.Core.MarketData.BPipe.Wrappers;
using PineRiver.Core.MarketData.Data;
using PineRiver.Core.MarketData.Responses;
using PineRiver.Core.MarketData.Enums;
using System.IO;
using System.Text.RegularExpressions;


namespace ReportCommonLib
{
    public class BloombergDataProvider : IDisposable
    {
        public string GetClearTicker(string ticker)//return a clean ticker
        {
            var s=Regex.Replace(ticker,"\\s+","");
            if (!s.EndsWith("Equity"))
                s += "Equity";
            return s;
        }
        List<Exception> _BloombergExceptionList = new List<Exception>();
        public List<Exception> BloombergExceptionList
        {
            get
            {
                return _BloombergExceptionList;
            }
        }

        private static readonly Lazy<BloombergDataProvider> BloombergDataProviderLazy;

        private readonly MarketDataProvider _marketDataProvider = new MarketDataProvider();
        private bool _isDisposed;

        static BloombergDataProvider()
        {
            BloombergDataProviderLazy =
                new Lazy<BloombergDataProvider>(() => new BloombergDataProvider());
        }
        private BloombergDataProvider()
        {

        }

        ~ BloombergDataProvider()
        {
            this.Dispose(false);
        }

        public static BloombergDataProvider Instance
        {
            get { return BloombergDataProviderLazy.Value; }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected void Dispose(bool isDisposing)
        {
            if (!_isDisposed)
            {
                try
                {
                    if (_marketDataProvider != null)
                    {
                        _marketDataProvider.Dispose();
                    }
                }
                catch
                {

                }
                if (isDisposing)
                {
                    GC.SuppressFinalize(this);
                }
                _isDisposed = true;
            }
        }

        public void Request(string securityId, string[] fields, Action<Dictionary<string,object>> parseResultTableAction, List<Override> overrideList)
        {
            if (string.IsNullOrWhiteSpace(securityId) || fields == null || !fields.Any())
            {
                return;
            }

            var response =
                _marketDataProvider.GetData(
                    new StaticDataRequest(SecurityIdentifier.CreateBatchFromBloombergTickers(securityId),
                        Field.CreateBatch(fields), overrideList, allowFallbackToDelayedData: true));

            if (response == null
                || response.Result == null
                || !response.Result.Rows.Any())
            {
                throw new InvalidDataException(
                    string.Format("Failed to get data from Bloomberg for '{0}'. The result is empty.",
                        securityId));
            }

            if (response.Status == ResponseStatus.Failed)
            {
                throw new BPipeException("Response failed.", response.Exception);
            }

            var dataRow = response.Result.Rows.FirstOrDefault();

            parseResultTableAction(GetResult(dataRow,fields.ToList()));
        }

        public void Request(string[] securityIds, string[] fields, Action<IReadOnlyList<Dictionary<string, object>>> parseResultTableAction)
        {
            var response =
                _marketDataProvider.GetData(
                    new StaticDataRequest(SecurityIdentifier.CreateBatchFromBloombergTickers(securityIds),
                        Field.CreateBatch(fields),
                        allowFallbackToDelayedData: true));

            if (response == null
                || response.Result == null
                || !response.Result.Rows.Any())
            {
                throw new InvalidDataException(
                    string.Format("Failed to get data from Bloomberg for '{0}'. The result dataset is empty.",
                                  string.Join(",", securityIds)));
            }

            if (response.Status == ResponseStatus.Failed)
            {
                throw new BPipeException("Response failed.", response.Exception);
            }
            parseResultTableAction(GetResultList(response.Result.Rows,fields.ToList()));
        }

        public void RequestHist(string securityId,
            string[] fields,
            DateTime requestDate,
            Action<Dictionary<string, object>> parseResultTableAction)
        {
            var action =
                new Action<IReadOnlyList<Dictionary<string, object>>>(
                    rowList =>
                    {
                        var row = rowList.FirstOrDefault();
                        if (row != null)
                        {
                            parseResultTableAction(row);
                        }
                    });

            RequestHist(new[] { securityId }, fields, requestDate, requestDate, action);
        }

        public void RequestHist(string[] securityIds,
            string[] fields,
            DateTime requestStartDate,
            DateTime requestEndDate,
            Action<IReadOnlyList<Dictionary<string, object>>> parseResultTableAction)
        {
            var response =
                _marketDataProvider.GetData(
                    new HistoryStaticDataRequest(SecurityIdentifier.CreateBatchFromBloombergTickers(securityIds),
                        Field.CreateBatch(fields),
                        allowFallbackToDelayedData: true,
                        optionalParameters:
                            new[]
                            {
                                BloombergOptionalParameters.PeriodicitySelectionParameter(PeriodicitySelection.Daily),
                                BloombergOptionalParameters.PeriodicityAdjustmentParameter(PeriodicityAdjustment.Actual),
                                BloombergOptionalParameters.StartDateParameter(requestStartDate),
                                BloombergOptionalParameters.EndDateParameter(requestEndDate)
                            }
                        ));

            if (response == null
                || response.Result == null
                || !response.Result.Rows.Any())
            {
                throw new InvalidDataException(
                    string.Format("Failed to get data from Bloomberg for '{0}'. The result dataset is empty.",
                        string.Join(",", securityIds)));
            }

            if (response.Status == ResponseStatus.Failed)
            {
                throw new BPipeException("Response failed.", response.Exception);
            }
            var dataList = GetResultList(response.Result.Rows, fields.ToList());
            parseResultTableAction(dataList);
        }

        List<Dictionary<string, object>> GetResultList(IReadOnlyList<IDataRow> dataRowList,List<string> fieldList)
        {
            var list = new List<Dictionary<string, object>>();
            foreach (var data in dataRowList)
            {
                try
                {
                    list.Add(GetResult(data, fieldList));
                }
                catch (Exception ex)
                {
                    BloombergExceptionList.Add(ex);
                }
            }
            return list;
        }
        Dictionary<string, object> GetResult(IDataRow dataRow, List<string> fieldList)
        {

                var d = new Dictionary<string, object>();
                foreach (var field in fieldList)
                {
                    d.Add(field, dataRow.GetValue(field));
                }
                d.Add(QueryFieldName, dataRow.GetValue(0).ToString());
                return d;
        }
        public static string QueryFieldName = "QueryField";
        //public static Frequency ConvertToFrequency(int bloombergFrequencyId, Frequency defaultValue = Frequency.None)
        //{
        //    switch (bloombergFrequencyId)
        //    {
        //        case 1:
        //            return Frequency.Annual;
        //        case 2:
        //            return Frequency.Semiannual;
        //        case 4:
        //            return Frequency.Quarterly;
        //        case 6:
        //            return Frequency.Bimonthly;
        //        case 12:
        //            return Frequency.Monthly;
        //        default:
        //            return defaultValue;
        //    }
        //}

        //public static int ConvertFrequencyToInt(Frequency frequency, int defaultValue = 0)
        //{
        //    switch (frequency)
        //    {
        //        case Frequency.Monthly:
        //            return 12;
        //        case Frequency.Bimonthly:
        //            return 6;
        //        case Frequency.Quarterly:
        //            return 4;
        //        case Frequency.Semiannual:
        //            return 2;
        //        case Frequency.Annual:
        //            return 1;
        //        default:
        //            return defaultValue;
        //    }
        //}

        public static bool IsBloombergIdEndWith(string bloombergId, params string[] postfixes)
        {
            if (string.IsNullOrWhiteSpace(bloombergId))
            {
                return false;
            }
            return
                postfixes.Any(
                    postfix => bloombergId.Trim().EndsWith(postfix, StringComparison.InvariantCultureIgnoreCase));
        }

        public static string GetBloombergIdWithOutSector(string bloombergId, params string[] postfixes)
        {
            if (string.IsNullOrWhiteSpace(bloombergId) || !postfixes.Any())
            {
                return bloombergId;
            }

            foreach (var postfix in postfixes)
            {
                var index = bloombergId.IndexOf(postfix, StringComparison.InvariantCultureIgnoreCase);
                if (index > 0)
                {
                    return bloombergId.Substring(0, index);
                }
            }
            return bloombergId;
        }
    }
    public class MarketDataProvider : IDataProvider<IBloombergStaticDataRequest, IBloombergSubscriptionRequest>
    {
        private static readonly IAuthenticationStrategy AuthenticationStrategy;

        private static readonly StaticConfiguration Config;

        private readonly Subject<BPipeConnectionInfo> _statusSubject = new Subject<BPipeConnectionInfo>();
        private BloombergApiAdaptor _adaptor;
        private bool _initialized;
        private object _syncRoot = new object();
        private BPipeConnectionStatus[] _connectionStatus;
        private CompositeDisposable _allDisposables;

        static MarketDataProvider()
        {
            MultiAppAuthenticationStrategy.AuthType authType;
            if (!Enum.TryParse(ConfigurationManager.AppSettings["BPipe.Authentication.Mode"], out authType))
            {
                authType = MultiAppAuthenticationStrategy.AuthType.AppOnly;
            }
            AuthenticationStrategy = new MultiAppAuthenticationStrategy(
                ConfigurationManager.AppSettings["BPipe.Authentication.ApplicationName"],
                authType);
            Config = new StaticConfiguration
            {
                AppSettings = { { "LocationSearchOrder", ConfigurationManager.AppSettings["BPipe.LocationSearchOrder"] ?? "DA,NY,HK" } }
            };
        }

        private BloombergApiAdaptor GetAdaptor()
        {
            return LazyInitializer.EnsureInitialized(ref _adaptor, ref _initialized, ref _syncRoot, CreateBloombergApiAdaptor);
        }

        private BloombergApiAdaptor CreateBloombergApiAdaptor()
        {
            var sharedContext = new MarketDataContext(authenticationStrategy: AuthenticationStrategy,
                staticConfigurationLayer: Config);

            var statusList = sharedContext.GetConnectionStatus()
                .Where(si => si.ServerGroup.ServerType != ServerType.Platform)
                .ToArray();

            _connectionStatus = statusList
                .Select(si => BPipeConnectionStatus.NotStarted)
                .ToArray();

            _allDisposables = new CompositeDisposable();
            var count = 0;
            foreach (var ci in statusList)
            {
                int idx = count++;
                _allDisposables.Add(
                    ci.Subscribe(c =>
                    {
                        _connectionStatus[idx] = c;
                        if (_connectionStatus.All(s => s == BPipeConnectionStatus.Disconnected))
                        {
                            _initialized = false;
                            Dispose();
                        }
                    })
                );
                _statusSubject.OnNext(ci);
            }
            var adaptor = sharedContext.CreateBloombergApiAdaptor();
            _allDisposables.Add(adaptor);
            _allDisposables.Add(sharedContext);

            return adaptor;
        }

        private void CheckIfConnectionFailed(IMarketDataResponse response)
        {
            if (response.Status == ResponseStatus.Succeeded
                && response.Result.Rows.Count > 0)
            {
                var lastColumnIdx = response.Result.Columns.Count - 1;
                if (response.Result.Rows[0].HasError(lastColumnIdx))
                {
                    var error = response.Result.Rows[0].GetErrorMessage(lastColumnIdx);
                    var index = error.IndexOf("ConnectionFailed", StringComparison.InvariantCultureIgnoreCase);
                    if (index > -1)
                        throw new Exception(error.Substring(index, error.Length - index));
                }
            }
        }

        public IMarketDataResponse GetData(IBloombergStaticDataRequest dataRequest)
        {
            var adaptor = GetAdaptor();
            var r = adaptor.GetData(dataRequest);
            CheckIfConnectionFailed(r);
            return r;
        }

        public IDataSubscription CreateDataSubscription(IBloombergSubscriptionRequest dataRequest)
        {
            var dataAdaptor = GetAdaptor();
            return dataAdaptor.CreateDataSubscription(dataRequest);
        }

        public IObservable<BPipeConnectionInfo> ConnectionStatus
        {
            get { return _statusSubject.AsObservable(); }
        }

        public void Dispose()
        {
            try
            {
                lock (_syncRoot)
                    if (_allDisposables != null && !_allDisposables.IsDisposed)
                    {
                        _allDisposables.Dispose();
                    }
            }
            catch
            { }
        }

        public Task<IMarketDataResponse> GetDataAsync(IBloombergStaticDataRequest dataRequest)
        {
            var dataAdaptor = GetAdaptor();
            return dataAdaptor.GetDataAsync(dataRequest)
                .ContinueWith(t =>
                {
                    CheckIfConnectionFailed(t.Result);
                    return t.Result;
                }, TaskContinuationOptions.OnlyOnRanToCompletion);
        }
    }

}
