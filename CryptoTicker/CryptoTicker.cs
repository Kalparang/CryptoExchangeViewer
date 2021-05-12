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

namespace CryptoTicker
{
    class Korbit
    {
        private WebSocket ws;

        Dictionary<string, Dictionary<string, string>> GetAllSymbols()
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

        void Socket(Action<dynamic> func)
        {
            bool result = false;

            ws = new WebSocket("wss://ws.korbit.co.kr/v1/user/push");

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

    class Huobi
    {
        WebCallResult<HuobiSymbolTicks> GetAllSymbols()
        {
            WebCallResult<HuobiSymbolTicks> marketDetails = null;

            using (var client = new HuobiClient())
            {
                marketDetails = client.GetTickers();
            }

            return marketDetails;
        }

        void Socket(Action<HuobiSymbolDatas> func)
        {
            var socketClient = new HuobiSocketClient();
            socketClient.SubscribeToTickerUpdates(func);
        }
    }

    class Kukoin
    {
        WebCallResult<KucoinTicks> GetAllSymbols()
        {
            WebCallResult<KucoinTicks> marketDetails = null;

            using (var client = new KucoinClient())
            {
                marketDetails = client.GetTickers();
            }

            return marketDetails;
        }

        void Socket(Action<KucoinStreamTick> func)
        {
            var socketClient = new KucoinSocketClient();
            socketClient.SubscribeToAllTickerUpdates(func);
        }
    }

    class BitFlyer
    {
        Action func = null;

        void Socket(Action func)
        {
            this.func = func;

            var client = new RealtimeApi();

            client.Subscribe<Ticker>("lightning_ticker_ETH_JPY", null, func, OnError);
            client.Subscribe<Ticker>("lightning_ticker_BTC_JPY", null, func, OnError);
        }

        void OnError(string message, Exception ex)
        {
            //if(func != null) Socket(func);
        }
    }

    class Bittrex
    {
        WebCallResult<IEnumerable<BittrexTick>> GetAllSymbols()
        {
            WebCallResult<IEnumerable<BittrexTick>> tickers = null;

            using (var client = new BittrexClient())
            {
                tickers = client.GetTickers();
            }

            return tickers;
        }

        void Socket(Action<BittrexTickersUpdate> func)
        {
            var socketClient = new BittrexSocketClient();
            var subscription = socketClient.SubscribeToSymbolTickerUpdatesAsync(func);
        }
    }

    class Binance
    {
        WebCallResult<IEnumerable<BinancePrice>> GetAllSymbols()
        {
            WebCallResult<IEnumerable<BinancePrice>> result = null;

            using (var client = new BinanceClient())
            {
                result = client.Spot.Market.GetPrices();
            }

            return result;
        }

        void Socket(Action<IEnumerable<IBinanceTick>> func)
        {
            var socketClient = new BinanceSocketClient();
            socketClient.Spot.SubscribeToAllSymbolTickerUpdates(func);
        }
    }
}
