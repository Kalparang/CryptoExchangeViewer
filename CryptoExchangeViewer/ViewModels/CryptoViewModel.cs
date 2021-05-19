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



        public CryptoViewModel() { }

        public CryptoViewModel(string target, string stand, string maxmarket, string minmarket, double percent)
        {
            this.targetCrypto = target;
            this.standCrypto = stand;
            this.maxMarketName = maxmarket;
            this.minMarketName = minmarket;
            this.percent = percent;
        }
    }
}
