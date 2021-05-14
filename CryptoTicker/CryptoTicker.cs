using System;
using System.Net;
using System.IO;
using System.Collections.Generic;

using Newtonsoft.Json;
using WebSocketSharp;

using CryptoTicker.Model;

using CryptoExchange.Net.Objects;
using Binance.Net;
using Binance.Net.Interfaces;
using Binance.Net.Objects.Spot.MarketData;
using Huobi.Net;
using Huobi.Net.Objects;
using Kucoin.Net;
using Kucoin.Net.Objects;
using Kucoin.Net.Objects.Sockets;
using BitFlyer.Apis;
using Bittrex.Net;
using Bittrex.Net.Objects;
using Bittrex.Net.Sockets;
using CryptoExchange.Net.Sockets;
using System.Threading.Tasks;

namespace CryptoTicker
{
    public class Korbit
    {
        private WebSocket ws;

        public Dictionary<string, Dictionary<string, string>> GetAllSymbols()
        {
            Dictionary<string, Dictionary<string, string>> result = null;

            string responseFromServer = string.Empty;

            WebRequest request = WebRequest.Create(@"https://api.korbit.co.kr/v1/ticker/detailed/all");
            request.Method = "GET";
            request.ContentType = "application/json";

            using (WebResponse response = request.GetResponse())
            using (Stream dataStream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(dataStream))
            {
                responseFromServer = reader.ReadToEnd();
            }

            Dictionary<string, Dictionary<string, string>> Symbols =
                JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(responseFromServer);

            //foreach (var Symbol in Symbols)
            //{
            //    var symbol = Symbol.Key; ///***_&&&
            //    foreach (var data in Symbol.Value)
            //    {
            //        switch (data.Key)
            //        {
            //            case "timestamp":   //문제 있음 확인 해야됨
            //                var timestamp = new DateTime(new DateTime(long.Parse(data.Value)).Ticks + new DateTime(1970, 1, 1, 0, 0, 0).Ticks);
            //                break;
            //            case "last":
            //                var price = data.Value;
            //                break;
            //            case "open":
            //                var open = data.Value;
            //                break;
            //            case "bid":
            //                var bid = data.Value;
            //                break;
            //            case "ask":
            //                var ask = data.Value;
            //                break;
            //            case "low":
            //                var low = data.Value;
            //                break;
            //            case "high":
            //                var high = data.Value;
            //                break;
            //            case "volume":
            //                var volume = data.Value;
            //                break;
            //            case "change":
            //                var change = data.Value;
            //                break;
            //            case "changePercent":
            //                var changePercent = data.Value;
            //                break;
            //            default:
            //                var de = data.Value;
            //                break;
            //        }
            //    }
            //}

            result = Symbols;

            return result;
        }

        public void Socket(Action<dynamic> func)
        {
            ws = new WebSocket("wss://ws.korbit.co.kr/v1/user/push");
            {
                ws.OnOpen += (sender, e) =>
                {
                    var t = ws.ReadyState.ToString();
                    KorbitModel.WebSocketRequest korbit = new KorbitModel.WebSocketRequest();
                    korbit.accessToken = null;
                    korbit.timestamp = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).Ticks;
                    korbit.@event = "korbit:subscribe";
                    korbit.data = new KorbitModel.WebSocketData();
                    korbit.data.channels = new List<string>();
                    korbit.data.channels.Add("ticker:btc_krw,eth_krw");

                    string js = JsonConvert.SerializeObject(korbit);
                    ws.Send(js);
                };

                ws.OnClose += (sender, e) =>
                {
                    var t = e.Code;
                };

                ws.OnMessage += (sender, e) =>
                {
                    var t = e.Data;

                    dynamic Tickers = JsonConvert.DeserializeObject<dynamic>(e.Data);
                    if (Tickers["event"] == "korbit:push-ticker")
                    {
                        func(Tickers);
                        var timestamp = Tickers["timestamp"];
                        var symbol = Tickers["data"]["currency_pair"];    //***_&&&
                        var price = long.Parse(((string)Tickers["data"]["last"]).Trim('{').Trim('}'));
                    }
                };

                ws.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                ws.Connect();
            }
        }
    }

    public class Huobi
    {
        HuobiSocketClient socketClient = null;

        public WebCallResult<HuobiSymbolTicks> GetAllSymbols()
        {
            WebCallResult<HuobiSymbolTicks> marketDetails = null;

            using (var client = new HuobiClient())
            {
                marketDetails = client.GetTickers();
            }

            return marketDetails;
        }

        public void Socket(Action<HuobiSymbolDatas> func)
        {
            socketClient = new HuobiSocketClient();
            socketClient.SubscribeToTickerUpdates(func);
        }
    }

    public class Kukoin
    {
        KucoinSocketClient socketClient = null;

        public WebCallResult<KucoinTicks> GetAllSymbols()
        {
            WebCallResult<KucoinTicks> marketDetails = null;

            using (var client = new KucoinClient())
            {
                marketDetails = client.GetTickers();
            }

            return marketDetails;
        }

        public void Socket(Action<KucoinStreamTick> func)
        {
            socketClient = new KucoinSocketClient();
            socketClient.SubscribeToAllTickerUpdates(func);
        }
    }

    public class BitFlyer
    {
        Action<Ticker> func = null;
        RealtimeApi client = null;
        RealtimeApi client2 = null;

        public void Socket(Action<Ticker> func)
        {
            this.func = func;

            client = new RealtimeApi();
            client2 = new RealtimeApi();

            client.Subscribe<Ticker>("lightning_ticker_ETH_JPY", func, onConnect, OnError);
            client2.Subscribe<Ticker>("lightning_ticker_BTC_JPY", func, onConnect, OnError);
        }

        void onConnect()
        {

        }

        void OnError(string message, Exception ex)
        {
            if(func != null) Socket(func);
        }
    }

    public class Bittrex
    {
        BittrexSocketClient socketClient = null;
        Task<CallResult<UpdateSubscription>> subscription = null;

        public WebCallResult<IEnumerable<BittrexTick>> GetAllSymbols()
        {
            WebCallResult<IEnumerable<BittrexTick>> tickers = null;

            using (var client = new BittrexClient())
            {
                tickers = client.GetTickers();
            }

            return tickers;
        }

        public void Socket(Action<BittrexTickersUpdate> func)
        {
            socketClient = new BittrexSocketClient();
            subscription = socketClient.SubscribeToSymbolTickerUpdatesAsync(func);
        }
    }

    public class Binance
    {
        BinanceSocketClient socketClient = null;

        public WebCallResult<IEnumerable<BinancePrice>> GetAllSymbols()
        {
            WebCallResult<IEnumerable<BinancePrice>> result = null;

            using (var client = new BinanceClient())
            {
                result = client.Spot.Market.GetPrices();
            }

            return result;
        }

        public void Socket(Action<IEnumerable<IBinanceTick>> func)
        {
            socketClient = new BinanceSocketClient();
            socketClient.Spot.SubscribeToAllSymbolTickerUpdates(func);
        }
    }

    public class Currency
    {
        public static List<CurrencyExchangeModel> Exchange(
            out DateTime date,
            string[] CurrencyList
            )
        {
            List<CurrencyExchangeModel> Models = new List<CurrencyExchangeModel>();
            date = new DateTime();

            string url = "https://earthquake.kr:23490/query/";

            foreach(string currency in CurrencyList)
            {
                url += currency + "KRW" + ",";
                url += "KRW" + currency + ",";
            }

            url = url.Substring(0, url.Length - 1);

            string responseText = string.Empty;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.Timeout = 5 * 1000;

            using (HttpWebResponse resp = (HttpWebResponse)request.GetResponse())
            {
                HttpStatusCode status = resp.StatusCode;
                Console.WriteLine(status);

                Stream respStream = resp.GetResponseStream();
                using (StreamReader sr = new StreamReader(respStream))
                {
                    responseText = sr.ReadToEnd();
                }
            }

            dynamic cu = JsonConvert.DeserializeObject<dynamic>(responseText);

            foreach (dynamic c in cu)
            {
                string name = c.Name;

                if (name == "update")
                {
                    long TimeStamp = c.Value;

                    date = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddMilliseconds(TimeStamp);
                }
                else
                {
                    string Target = name.Substring(0, 3);
                    string Stand = name.Substring(3);
                    double Price = c.Value[0];
                    double High = c.Value[5];
                    double Low = c.Value[6];

                    Models.Add(new CurrencyExchangeModel(Target, Stand, Price, High, Low));
                }
            }

            return Models;
        }
    }
}
