using System.Collections.Generic;

namespace CryptoTicker.Model
{
    class KorbitModel
    {
        public class WebSocketData
        {
            public List<string> channels { get; set; }
        }

        public class WebSocketRequest
        {
            public object accessToken { get; set; }
            public long timestamp { get; set; }
            public string @event { get; set; }
            public KorbitModel.WebSocketData data { get; set; }
        }
    }

    class HuobiModel
    {

    }

    class KukoinModel
    {

    }

    class BitFlyerModel
    {

    }

    class BittrexModel
    {

    }

    class BinanceModel
    {
        public BinanceModel(string symbol, decimal price)
        {
            this.symbol = symbol;
            this.price = price;
        }

        private string symbol;
        public string Symbol
        {
            get { return symbol; }
            set
            {
                symbol = value;
            }
        }

        private decimal price;
        public decimal Price
        {
            get { return price; }
            set
            {
                price = value;
            }
        }

        private decimal priceChangePercent;
        public decimal PriceChangePercent
        {
            get { return priceChangePercent; }
            set
            {
                priceChangePercent = value;
            }
        }

        private decimal highPrice;
        public decimal HighPrice
        {
            get { return highPrice; }
            set
            {
                highPrice = value;
            }
        }

        private decimal lowPrice;
        public decimal LowPrice
        {
            get { return lowPrice; }
            set
            {
                lowPrice = value;
            }
        }

        private decimal volume;
        public decimal Volume
        {
            get { return volume; }
            set
            {
                volume = value;
            }
        }

        private decimal tradeAmount;
        public decimal TradeAmount
        {
            get { return tradeAmount; }
            set
            {
                tradeAmount = value;
            }
        }

        private decimal tradePrice;
        public decimal TradePrice
        {
            get { return tradePrice; }
            set
            {
                tradePrice = value;
            }
        }

    }
}
