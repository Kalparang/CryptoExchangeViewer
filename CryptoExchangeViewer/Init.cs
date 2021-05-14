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
        private Kukoin kukoin = null;
        private CryptoTicker.BitFlyer bitFlyer = null;
        private CryptoTicker.Bittrex bittrex = null;
        private CryptoTicker.Binance binance = null;

        TextBox label1, label2, label3, label4, label5, label6;

        public Init(TextBox label1, TextBox label2, TextBox label3, TextBox label4, TextBox label5, TextBox label6)
        {
            this.label1 = label1;
            this.label2 = label2;
            this.label3 = label3;
            this.label4 = label4;
            this.label5 = label5;
            this.label6 = label6;

            new Thread(KorbitInit).Start();
            new Thread(HuobiInit).Start();
            new Thread(KukoinInit).Start();
            new Thread(BitFlyerInit).Start();
            new Thread(BittrexInit).Start();
            new Thread(BinanceInit).Start();
            new Thread(CurrencyExchangeInit).Start();
        }

        private void DBInit()
        {

        }

        private void KorbitSocketFunc(dynamic Tickers)
        {
            label1.Dispatcher.Invoke(new Action(() =>
            {
                label1.Text = Convert.ToString(Tickers);
            }));
            
        }

        private void KorbitInit()
        {
            korbit = new Korbit();

            Dictionary<string, Dictionary<string, string>> KorbitMarketDetails =
                korbit.GetAllSymbols();

            foreach (var Symbol in KorbitMarketDetails)
            {
                var symbol = Symbol.Key; ///***_&&&
                foreach (var data in Symbol.Value)
                {
                    switch (data.Key)
                    {
                        case "timestamp":
                            var timestamp =
                                new DateTime(1970, 1, 1, 0, 0, 0).AddMilliseconds(long.Parse(data.Value));
                            break;
                        case "last":
                            var price = double.Parse(data.Value);
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
            }

            korbit.Socket(KorbitSocketFunc);
        }

        private void HuobiSocketFunc(HuobiSymbolDatas Datas)
        {
            label2.Dispatcher.Invoke(new Action(() =>
            {
                label2.Text = Convert.ToString(Datas.Timestamp);
            }));
        }

        private void HuobiInit()
        {
            huobi = new CryptoTicker.Huobi();
            WebCallResult<HuobiSymbolTicks> HuobiMarketDetails =
                huobi.GetAllSymbols();

            huobi.Socket(HuobiSocketFunc);
        }

        private void KukoinSocketFunc(KucoinStreamTick Tick)
        {
            label3.Dispatcher.Invoke(new Action(() =>
            {
                label3.Text = Convert.ToString(Tick.Timestamp);
            }));
        }

        private void KukoinInit()
        {
            kukoin = new Kukoin();

            WebCallResult<KucoinTicks> KukoinMarketDetails =
                kukoin.GetAllSymbols();

            kukoin.Socket(KukoinSocketFunc);
        }

        private void BitFlyerSocketFunc(Ticker ticker)
        {
            label4.Dispatcher.Invoke(new Action(() =>
            {
                label4.Text = Convert.ToString(ticker);
            }));
        }

        private void BitFlyerInit()
        {
            bitFlyer = new CryptoTicker.BitFlyer();

            bitFlyer.Socket(BitFlyerSocketFunc);
        }

        private void BittrexSocketFunc(BittrexTickersUpdate Tickers)
        {
            label5.Dispatcher.Invoke(new Action(() =>
            {
                label5.Text = Convert.ToString(Tickers.Sequence);
            }));
        }

        private void BittrexInit()
        {
            bittrex = new CryptoTicker.Bittrex();

            WebCallResult<IEnumerable<BittrexTick>> BittrexMarketDetails =
                bittrex.GetAllSymbols();

            bittrex.Socket(BittrexSocketFunc);
        }

        private void BinanceSocketFunc(IEnumerable<IBinanceTick> Tickers)
        {
            label6.Dispatcher.Invoke(new Action(() =>
            {
                label6.Text = Convert.ToString(Tickers.ToArray()[0].LastPrice);
            }));
        }

            private void BinanceInit()
        {
            binance = new CryptoTicker.Binance();

            WebCallResult<IEnumerable<BinancePrice>> BittrexMarketDetails =
                binance.GetAllSymbols();

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

                foreach (var model in models)
                {

                }

                System.Threading.Thread.Sleep(60 * 1000);
            }
        }
    }
}
