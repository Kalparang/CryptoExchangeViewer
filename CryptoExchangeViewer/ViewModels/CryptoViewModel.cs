using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CryptoExchangeViewer.MVVM;

namespace CryptoExchangeViewer.ViewModels
{
    //public class CryptoViewModel : ObservableObject
    //{
    //    private string targetCrypto;
    //    public string TargetCrypto
    //    {
    //        get { return targetCrypto; }
    //        set
    //        {
    //            targetCrypto = value;
    //            RaisePropertyChangedEvent("TargetCrypto");
    //        }
    //    }

    //    private string standCrypto;
    //    public string StandCrypto
    //    {
    //        get { return standCrypto; }
    //        set
    //        {
    //            standCrypto = value;
    //            RaisePropertyChangedEvent("StandCrypto");
    //        }
    //    }

    //    private string maxMarketName;
    //    public string MaxMarketName
    //    {
    //        get { return maxMarketName; }
    //        set
    //        {
    //            maxMarketName = value;
    //            RaisePropertyChangedEvent("MaxMarketName");
    //        }
    //    }

    //    private string minMarketName;
    //    public string MinMarketName
    //    {
    //        get { return minMarketName; }
    //        set
    //        {
    //            minMarketName = value;
    //            RaisePropertyChangedEvent("MinMarketName");
    //        }
    //    }

    //    private double percent;
    //    public double Percent
    //    {
    //        get { return percent; }
    //        set
    //        {
    //            percent = value;
    //            RaisePropertyChangedEvent("Percent");
    //        }
    //    }



    //    public CryptoViewModel() { }

    //    public CryptoViewModel(string target, string stand, string maxmarket, string minmarket, double percent)
    //    {
    //        this.targetCrypto = target;
    //        this.standCrypto = stand;
    //        this.maxMarketName = maxmarket;
    //        this.minMarketName = minmarket;
    //        this.percent = percent;
    //    }
    //}

    public class BanViewModel : ObservableObject
    {
        private string type;
        public string Type
        {
            get { return type; }
            set
            {
                type = value;
                RaisePropertyChangedEvent("Type");
            }
        }

        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                RaisePropertyChangedEvent("Name");
            }
        }

        public BanViewModel(int Type, string Name)
        {
            string type;

            switch (Type)
            {
                case 1:
                    type = "거래소";
                    break;
                case 2:
                    type = "나라";
                    break;
                default:
                    type = "";
                    break;
            }

            this.type = type;
            this.name = Name;
        }

        public BanViewModel(string Type, string Name)
        {
            this.type = Type;
            this.name = Name;
        }
    }

    public class ExchangeViewModel : ObservableObject
    {
        public ExchangeViewModel(string target, decimal cny, decimal eur, decimal hkd, decimal jpy, decimal usd)
        {
            this.target = target;
            this.cny = cny;
            this.eur = eur;
            this.hkd = hkd;
            this.jpy = jpy;
            this.usd = usd;
        }
        
        private string target;
        public string Target
        {
            get { return target; }
            set
            {
                target = value;
                RaisePropertyChangedEvent("Target");
            }
        }

        private decimal cny;
        public decimal CNY
        {
            get { return cny; }
            set
            {
                cny = value;
                RaisePropertyChangedEvent("CNY");
            }
        }

        private decimal eur;
        public decimal EUR
        {
            get { return eur; }
            set
            {
                eur = value;
                RaisePropertyChangedEvent("EUR");
            }
        }

        private decimal hkd;
        public decimal HKD
        {
            get { return hkd; }
            set
            {
                hkd = value;
                RaisePropertyChangedEvent("HKD");
            }
        }

        private decimal jpy;
        public decimal JPY
        {
            get { return jpy; }
            set
            {
                jpy = value;
                RaisePropertyChangedEvent("JPY");
            }
        }

        private decimal usd;
        public decimal USD
        {
            get { return usd; }
            set
            {
                usd = value;
                RaisePropertyChangedEvent("USD");
            }
        }
    }


    public class CurrencyViewModel : ObservableObject
    {
        private string targetCrypto;
        public string TargetCrypto
        {
            get { return targetCrypto; }
            set
            {
                targetCrypto = value;
                RaisePropertyChangedEvent("TargetCrypto");
            }
        }

        private string maxStandCurrency;
        public string MaxStandCurrency
        {
            get { return maxStandCurrency; }
            set
            {
                maxStandCurrency = value;
                RaisePropertyChangedEvent("MaxStandCurrency");
            }
        }

        private string minStandCurrency;
        public string MinStandCurrency
        {
            get { return minStandCurrency; }
            set
            {
                minStandCurrency = value;
                RaisePropertyChangedEvent("MinStandCurrency");
            }
        }

        private string maxMarketNation;
        public string MaxMarketNation
        {
            get { return maxMarketNation; }
            set
            {
                maxMarketNation = value;
                RaisePropertyChangedEvent("MaxMarketNation");
            }
        }

        private string maxMarketName;
        public string MaxMarketName
        {
            get { return maxMarketName; }
            set
            {
                maxMarketName = value;
                RaisePropertyChangedEvent("MaxMarketName");
            }
        }

        private string minMarketNation;
        public string MinMarketNation
        {
            get { return minMarketNation; }
            set
            {
                minMarketNation = value;
                RaisePropertyChangedEvent("MinMarketNation");
            }
        }

        private string minMarketName;
        public string MinMarketName
        {
            get { return minMarketName; }
            set
            {
                minMarketName = value;
                RaisePropertyChangedEvent("MinMarketName");
            }
        }

        private decimal targetPrice;
        public decimal TargetPrice
        {
            get { return targetPrice; }
            set
            {
                targetPrice = value;
                RaisePropertyChangedEvent("TargetPrice");
            }
        }

        private decimal maxStandPrice;
        public decimal MaxStandPrice
        {
            get { return maxStandPrice; }
            set
            {
                maxStandPrice = value;
                RaisePropertyChangedEvent("MaxStandPrice");
            }
        }

        private decimal minStandPrice;
        public decimal MinStandPrice
        {
            get { return minStandPrice; }
            set
            {
                minStandPrice = value;
                RaisePropertyChangedEvent("MinStandPrice");
            }
        }

        private double percent;
        public double Percent
        {
            get { return percent; }
            set
            {
                percent = value;
                RaisePropertyChangedEvent("Percent");
            }
        }

        private double oneMin;
        public double OneMin
        {
            get { return oneMin; }
            set
            {
                oneMin = value;
                RaisePropertyChangedEvent("OneMin");
            }
        }

        private double fiveMin;
        public double FiveMin
        {
            get { return fiveMin; }
            set
            {
                fiveMin = value;
                RaisePropertyChangedEvent("FiveMin");
            }
        }

        private double oneHour;
        public double OneHour
        {
            get { return oneHour; }
            set
            {
                oneHour = value;
                RaisePropertyChangedEvent("OneHour");
            }
        }

        private double oneDay;
        public double OneDay
        {
            get { return oneDay; }
            set
            {
                oneDay = value;
                RaisePropertyChangedEvent("OneDay");
            }
        }

        public CurrencyViewModel(string target, string Stand, string nation, string market, decimal Price)
        {
            this.targetCrypto = target;
            this.maxStandCurrency = Stand;
            this.minStandCurrency = "";
            this.maxMarketNation = nation;
            this.maxMarketName = market;
            this.minMarketNation = "";
            this.minMarketName = "";
            this.targetPrice = Price;
            this.maxStandPrice = 0;
            this.percent = 0;
        }

        public CurrencyViewModel(string target, string maxStand, string maxnation, string maxmarket, string minnation, string minmarket, decimal TargetPrice, decimal StandPrice)
        {
            this.targetCrypto = target;
            this.maxStandCurrency = maxStand;
            this.minStandCurrency = "";
            this.maxMarketNation = maxnation;
            this.maxMarketName = maxmarket;
            this.minMarketNation = minnation;
            this.minMarketName = minmarket;
            this.targetPrice = TargetPrice;
            this.maxStandPrice = StandPrice;
            this.percent = 0;
        }

        public CurrencyViewModel(string target, string maxStand, string maxnation, string maxmarket, string minnation, string minmarket, decimal TargetPrice, decimal StandPrice,
            double percent, double oneMin, double fiveMin, double oneHour, double oneDay)
        {
            this.targetCrypto = target;
            this.maxStandCurrency = maxStand;
            this.minStandCurrency = "";
            this.maxMarketNation = maxnation;
            this.maxMarketName = maxmarket;
            this.minMarketNation = minnation;
            this.minMarketName = minmarket;
            this.targetPrice = TargetPrice;
            this.maxStandPrice = StandPrice;
            this.percent = percent;
            this.oneMin = oneMin;
            this.fiveMin = fiveMin;
            this.oneHour = oneHour;
            this.oneDay = oneDay;
        }

        public CurrencyViewModel(string target, string maxStand, string minStand, string maxnation, string maxmarket, string minnation, string minmarket, decimal TargetPrice, decimal StandPrice,
            double percent, double oneMin, double fiveMin, double oneHour, double oneDay)
        {
            this.targetCrypto = target;
            this.maxStandCurrency = maxStand;
            this.minStandCurrency = minStand;
            this.maxMarketNation = maxnation;
            this.maxMarketName = maxmarket;
            this.minMarketNation = minnation;
            this.minMarketName = minmarket;
            this.targetPrice = TargetPrice;
            this.maxStandPrice = StandPrice;
            this.percent = percent;
            this.oneMin = oneMin;
            this.fiveMin = fiveMin;
            this.oneHour = oneHour;
            this.oneDay = oneDay;
        }
    }
}
