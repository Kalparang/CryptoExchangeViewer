using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;
using Binance.Net.Interfaces;
using Binance.Net.Objects.Spot.MarketData;
using BitFlyer.Apis;
using Bittrex.Net.Objects;
using Bittrex.Net.Sockets;
using CryptoExchange.Net.Objects;
using CryptoTicker;
using CryptoTicker.Model;
using DBHelper;
using Huobi.Net.Objects;
using Kucoin.Net.Objects;
using Kucoin.Net.Objects.Sockets;

namespace CryptoExchangeViewer.Init
{
    class Init
    {
        private Korbit korbit = null;
        private CryptoTicker.Huobi huobi = null;
        private CryptoTicker.Kucoin kucoin = null;
        private CryptoTicker.BitFlyer bitFlyer = null;
        private CryptoTicker.Bittrex bittrex = null;
        private CryptoTicker.Binance binance = null;
        private DBHelper.DBHelper db = null;

        TextBox label1, label2, label3, label4, label5, label6;

        public Init(TextBox label1, TextBox label2, TextBox label3, TextBox label4, TextBox label5, TextBox label6)
        {
            this.label1 = label1;
            this.label2 = label2;
            this.label3 = label3;
            this.label4 = label4;
            this.label5 = label5;
            this.label6 = label6;

            DBInit();

            new Thread(KorbitInit).Start();
            new Thread(HuobiInit).Start();
            new Thread(KucoinInit).Start();
            new Thread(BitFlyerInit).Start();
            new Thread(BittrexInit).Start();
            new Thread(BinanceInit).Start();
            new Thread(CurrencyExchangeInit).Start();
        }

        private void DBInit()
        {
            db = new DBHelper.DBHelper();
        }

        private void KorbitSocketFunc(dynamic Tickers)
        {
            label1.Dispatcher.Invoke(new Action(() =>
            {
                label1.Text = Convert.ToString(Tickers);
            }));

            long Timestamp = Tickers["timestamp"];
            string symbol = Tickers["data"]["currency_pair"];    //***_&&&
            double price = double.Parse(((string)Tickers["data"]["last"]).Trim('{').Trim('}'));

            DBHelper.DBHelper.CryptoInputModel model = new DBHelper.DBHelper.CryptoInputModel();
            model.Market = korbit.Market;

            model.Date = new DateTime(1970, 1, 1, 0, 0, 0).AddMilliseconds(Timestamp);
            
            model.Target = symbol.Split('_')[0];
            model.Stand = symbol.Split('_')[1];
            model.Price = price;

            db.InputCryptoData(model);
        }

        private void KorbitInit()
        {
            korbit = new Korbit();

            db.InputMarketData(new DBHelper.DBHelper.MarketInputModel(korbit.Market, korbit.Nation));

            Dictionary<string, Dictionary<string, string>> KorbitMarketDetails =
                korbit.GetAllSymbols();

            foreach (var Symbol in KorbitMarketDetails)
            {
                var symbol = Symbol.Key; ///***_&&&

                DBHelper.DBHelper.CryptoInputModel model = new DBHelper.DBHelper.CryptoInputModel();

                model.Market = korbit.Market;
                model.Target = symbol.Split('_')[0];
                model.Stand = symbol.Split('_')[1];

                foreach (var data in Symbol.Value)
                {
                    switch (data.Key)
                    {
                        case "timestamp":
                            model.Date =
                                new DateTime(1970, 1, 1, 0, 0, 0).AddMilliseconds(long.Parse(data.Value));
                            break;
                        case "last":
                            model.Price = double.Parse(data.Value);
                            break;
                        case "low":
                            var low = double.Parse(data.Value);
                            break;
                        case "high":
                            var high = double.Parse(data.Value);
                            break;
                        default:
                            var de = data.Value;
                            break;
                    }
                }

                db.InputCryptoData(model);
            }

            korbit.Socket(KorbitSocketFunc);
        }

        private void HuobiSocketFunc(HuobiSymbolDatas Datas)
        {
            label2.Dispatcher.Invoke(new Action(() =>
            {
                label2.Text = Convert.ToString(Datas.Timestamp);
            }));

            DateTime date = Datas.Timestamp;

            foreach (HuobiSymbolTicker tick in Datas.Ticks)
            {
                string Target;
                string Stand;

                int mid = tick.Symbol.Length / 2;

                Target = tick.Symbol.Substring(0, mid);
                Stand = tick.Symbol.Substring(mid);

                double Price = (double)tick.Close;
                double High = (double)tick.High;
                double Low = (double)tick.Low;

                DBHelper.DBHelper.CryptoInputModel model =
                    new DBHelper.DBHelper.CryptoInputModel(Target, Stand, Price, huobi.Market, date);

                db.InputCryptoData(model);
            }

        }

        private void HuobiInit()
        {
            huobi = new CryptoTicker.Huobi();

            db.InputMarketData(new DBHelper.DBHelper.MarketInputModel(huobi.Market, huobi.Nation));

            WebCallResult<HuobiSymbolTicks> HuobiMarketDetails =
                huobi.GetAllSymbols();

            DateTime date = HuobiMarketDetails.Data.Timestamp;

            foreach(HuobiSymbolTick tick in HuobiMarketDetails.Data.Ticks)
            {
                string Target;
                string Stand;

                int mid = tick.Symbol.Length / 2;

                Target = tick.Symbol.Substring(0, mid);
                Stand = tick.Symbol.Substring(mid);

                double Price = (double)tick.Ask;
                double High = (double)tick.High;
                double Low = (double)tick.Low;

                DBHelper.DBHelper.CryptoInputModel model =
                    new DBHelper.DBHelper.CryptoInputModel(Target, Stand, Price, huobi.Market, date);

                db.InputCryptoData(model);
            }

            huobi.Socket(HuobiSocketFunc);
        }

        private void KucoinSocketFunc(KucoinStreamTick Tick)
        {
            label3.Dispatcher.Invoke(new Action(() =>
            {
                label3.Text = Convert.ToString(Tick.Timestamp);
            }));

            string Target = Tick.Symbol.Split('-')[0];
            string Stand = Tick.Symbol.Split('-')[1];
            double Price = (double)Tick.LastTradePrice;
            DateTime Date = Tick.Timestamp;

            DBHelper.DBHelper.CryptoInputModel model =
                    new DBHelper.DBHelper.CryptoInputModel(Target, Stand, Price, kucoin.Market, Date);

            db.InputCryptoData(model);
        }

        private void KucoinInit()
        {
            kucoin = new CryptoTicker.Kucoin();

            db.InputMarketData(new DBHelper.DBHelper.MarketInputModel(kucoin.Market, kucoin.Nation));

            WebCallResult<KucoinTicks> KukoinMarketDetails =
                kucoin.GetAllSymbols();

            DateTime date = KukoinMarketDetails.Data.Timestamp;

            foreach(KucoinAllTick data in KukoinMarketDetails.Data.Data)
            {
                string Symbol = data.Symbol;
                double Price = 0;
                double High = 0;
                double Low = 0;

                Symbol = data.Symbol;
                if(data.Last != null)
                    Price = (double)data.Last;
                if (data.High != null)
                    High = (double)data.High;
                if (data.Low != null)
                    Low = (double)data.Low;

                string Target = Symbol.Split('-')[0];
                string Stand = Symbol.Split('-')[1];

                DBHelper.DBHelper.CryptoInputModel model =
                    new DBHelper.DBHelper.CryptoInputModel(Target, Stand, Price, kucoin.Market, date);

                db.InputCryptoData(model);
            }

            kucoin.Socket(KucoinSocketFunc);
        }

        private void BitFlyerSocketFunc(Ticker ticker)
        {
            label4.Dispatcher.Invoke(new Action(() =>
            {
                label4.Text = Convert.ToString(ticker);
            }));

            string Target = ticker.ProductCode.Split('_')[0];
            string Stand = ticker.ProductCode.Split('_')[1];
            double Price = ticker.LatestPrice;
            DateTime Date = ticker.Timestamp;

            DBHelper.DBHelper.CryptoInputModel model =
                    new DBHelper.DBHelper.CryptoInputModel(Target, Stand, Price, bitFlyer.Market, Date);

            db.InputCryptoData(model);
        }

        private void BitFlyerInit()
        {
            bitFlyer = new CryptoTicker.BitFlyer();

            db.InputMarketData(new DBHelper.DBHelper.MarketInputModel(bitFlyer.Market, bitFlyer.Nation));

            bitFlyer.Socket(BitFlyerSocketFunc);
        }

        private void BittrexSocketFunc(BittrexTickersUpdate Tickers)
        {
            label5.Dispatcher.Invoke(new Action(() =>
            {
                label5.Text = Convert.ToString(Tickers.Sequence);
            }));

            DateTime Date = bittrex.GetServerTime();

            foreach (BittrexTick tick in Tickers.Deltas)
            {
                string Target = tick.Symbol.Split('-')[0];
                string Stand = tick.Symbol.Split('-')[1];
                double Price = (double)tick.LastTradeRate;

                DBHelper.DBHelper.CryptoInputModel model =
                    new DBHelper.DBHelper.CryptoInputModel(Target, Stand, Price, bittrex.Market, Date);

                db.InputCryptoData(model);
            }
        }

        private void BittrexInit()
        {
            bittrex = new CryptoTicker.Bittrex();

            db.InputMarketData(new DBHelper.DBHelper.MarketInputModel(bittrex.Market, bittrex.Nation));

            WebCallResult<IEnumerable<BittrexTick>> BittrexMarketDetails =
                bittrex.GetAllSymbols();

            DateTime Date = DateTime.UtcNow;

            foreach(var d in BittrexMarketDetails.ResponseHeaders)
            {
                if (d.Key == "Date")
                {
                    Date = Convert.ToDateTime(d.Value.First()).ToUniversalTime();
                    break;
                }
            }

            foreach (BittrexTick tick in BittrexMarketDetails.Data)
            {
                string Target = tick.Symbol.Split('-')[0];
                string Stand = tick.Symbol.Split('-')[1];
                double Price = (double)tick.LastTradeRate;

                DBHelper.DBHelper.CryptoInputModel model =
                    new DBHelper.DBHelper.CryptoInputModel(Target, Stand, Price, bittrex.Market, Date);

                db.InputCryptoData(model);
            }

            bittrex.Socket(BittrexSocketFunc);
        }

        private void BinanceSocketFunc(IEnumerable<IBinanceTick> Tickers)
        {
            label6.Dispatcher.Invoke(new Action(() =>
            {
                label6.Text = Convert.ToString(Tickers.ToArray()[0].LastPrice);
            }));

            DateTime Date = binance.GetServerTime();

            foreach (var tick in Tickers)
            {
                int mid = tick.Symbol.Length / 2;
                string Target = tick.Symbol.Substring(0, mid);
                string Stand = tick.Symbol.Substring(mid);
                double Price = (double)tick.LastPrice;

                DBHelper.DBHelper.CryptoInputModel model =
                    new DBHelper.DBHelper.CryptoInputModel(Target, Stand, Price, binance.Market, Date);

                db.InputCryptoData(model);
            }
        }

        private void BinanceInit()
        {
            binance = new CryptoTicker.Binance();

            db.InputMarketData(new DBHelper.DBHelper.MarketInputModel(binance.Market, binance.Nation));

            WebCallResult<IEnumerable<BinancePrice>> BinanceMarketDetail =
                binance.GetAllSymbols();

            DateTime Date = binance.GetServerTime();

            foreach (var data in BinanceMarketDetail.Data)
            {
                int mid = data.Symbol.Length / 2;

                string Target = data.Symbol.Substring(0, mid);
                string Stand = data.Symbol.Substring(mid);
                double Price = (double)data.Price;

                DBHelper.DBHelper.CryptoInputModel model =
                    new DBHelper.DBHelper.CryptoInputModel(Target, Stand, Price, binance.Market, Date);

                db.InputCryptoData(model);
            }

            binance.Socket(BinanceSocketFunc);
        }

        private void CurrencyExchangeInit()
        {
            DateTime date;
            string[] CurrencyList = { "USD", "EUR", "JPY", "CNY", "HKD" };

            while (true)
            {
                List<CurrencyExchangeModel> models =
                    Currency.Exchange(out date, CurrencyList);

                foreach (CurrencyExchangeModel model in models)
                {
                    DBHelper.DBHelper.ExchangeInputModel exmodel =
                        new DBHelper.DBHelper.ExchangeInputModel(model.Date, model.Target, model.Stand, model.Price);

                    db.InputExchangeData(exmodel);
                }

                System.Threading.Thread.Sleep(60 * 1000);
            }
        }
    }
}
