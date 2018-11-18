using GetMarketDataFromTDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fill5MinutesData
{
    //read 5 minutes market data from tdx files and fill into mysql db
    class Program
    {
        static void Main(string[] args)
        {
            var autoFill = new MarketDataAutoFill();
            autoFill.ProcessorCallBack = RefreshProcess;
            autoFill.DataAutoFill();
            Console.WriteLine(autoFill.Result);
        }

        static void RefreshProcess(int total, int current, double percent, int success, int faild, TimeSpan spend, TimeSpan remain)
        {
            Console.WriteLine("Filling data to database, finish percent:" + percent.ToString("p") +
                    " (" + current.ToString() + "/" + total.ToString() + " ), success " + success.ToString() + ",faild " + faild.ToString() +
                    ", time spend: " + spend.ToString() + ", still need: " + remain.ToString());
        }
    }
}
