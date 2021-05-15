using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CryptoExchangeViewer.MVVM;

namespace CryptoExchangeViewer.ViewModels
{
    public class CryptoViewModel : ObservableObject
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

        private string standCrypto;
        public string StandCrypto
        {
            get { return standCrypto; }
            set
            {
                standCrypto = value;
                RaisePropertyChangedEvent("StandCrypto");
            }
        }

        private double price;
        public double Price
        {
            get { return price; }
            set
            {
                price = value;
                RaisePropertyChangedEvent("Price");
            }
        }

        private string market;
        public string Market
        {
            get { return market; }
            set
            {
                market = value;
                RaisePropertyChangedEvent("Market");
            }
        }

        public CryptoViewModel() { }

        public CryptoViewModel(string target, string stand, double price, string market)
        {
            this.targetCrypto = target;
            this.standCrypto = stand;
            this.price = price;
            this.market = market;
        }
    }
}
