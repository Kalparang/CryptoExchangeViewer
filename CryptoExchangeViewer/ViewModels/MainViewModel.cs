using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CryptoExchangeViewer.MVVM;

namespace CryptoExchangeViewer.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private ObservableCollection<CurrencyViewModel> cryptoDetail;
        public ObservableCollection<CurrencyViewModel> CryptoDetail
        {
            get { return cryptoDetail; }
            set
            {
                cryptoDetail = value;
                RaisePropertyChangedEvent("CryptoDetail");
            }
        }

        private ObservableCollection<CurrencyViewModel> currencyDetail;
        public ObservableCollection<CurrencyViewModel> CurrencyDetail
        {
            get { return currencyDetail; }
            set
            {
                currencyDetail = value;
                RaisePropertyChangedEvent("CurrencyDetail");
            }
        }

        private ObservableCollection<CurrencyViewModel> selectDetail;
        public ObservableCollection<CurrencyViewModel> SelectDetail
        {
            get { return selectDetail; }
            set
            {
                selectDetail = value;
                RaisePropertyChangedEvent("SelectDetail");
            }
        }

        private CurrencyViewModel selectedSymbol;
        public CurrencyViewModel SelectedSymbol
        {
            get { return selectedSymbol; }
            set
            {
                if (value == null)
                    return;

                selectedSymbol = value;
                RaisePropertyChangedEvent("SymbolIsSelected");
                RaisePropertyChangedEvent("SelectedSymbol");
                ChangeSymbol();
            }
        }

        private void ChangeSymbol()
        {
            if (SelectedSymbol != null)
            {

            }
        }

        public bool SymbolIsSelected
        {
            get { return SelectedSymbol != null; }
        }

        public MainViewModel()
        {
            CryptoDetail = new ObservableCollection<CurrencyViewModel>();
            CurrencyDetail = new ObservableCollection<CurrencyViewModel>();

            Task.Run(() => Worker());
        }

        async Task Worker()
        {
            DBHelper.DBHelper db = new DBHelper.DBHelper();

            while (true)
            {
                var result = db.select();

                if (cryptoDetail.Count != result.Count())
                {
                    CryptoDetail = new ObservableCollection<CurrencyViewModel>(result.Select(r => new CurrencyViewModel(
                        r.targetCrypto, r.maxStandCurrency, r.maxMarketNation, r.maxMarketName, r.minMarketNation, r.minMarketName, r.percent)).ToList());
                }
                else
                {
                    foreach (var data in result)
                    {
                        CurrencyViewModel model = null;

                        foreach (var crypto in cryptoDetail)
                        {
                            if (crypto.TargetCrypto == data.targetCrypto && crypto.MaxStandCurrency == data.maxStandCurrency)
                            {
                                model = crypto;
                                crypto.Percent = data.percent;
                                break;
                            }
                        }

                        //    if (model == null)
                        //        cryptoDetail.Add(new CryptoViewModel(data.Target, data.Stand, data.MaxMarketName, data.MinMarketName, data.Percent));
                    }
                }

                var result2 = db.select2();

                if (CurrencyDetail.Count != result2.Count())
                {
                    CurrencyDetail = new ObservableCollection<CurrencyViewModel>(result2.Select(r => new CurrencyViewModel(
                        r.targetCrypto, r.maxStandCurrency, r.minStandCurrency, r.maxMarketName, r.maxMarketName, r.minMarketNation, r.minMarketName, r.percent)).ToList());
                }
                else
                {
                    foreach (var data in result2)
                    {
                        CurrencyViewModel model = null;

                        foreach (var crypto in CurrencyDetail)
                        {
                            if (crypto.TargetCrypto == data.targetCrypto && crypto.MaxStandCurrency == data.maxStandCurrency && crypto.MinStandCurrency == data.minStandCurrency)
                            {
                                model = crypto;
                                crypto.Percent = data.percent;
                                crypto.MaxMarketNation = data.maxMarketNation;
                                crypto.MaxMarketName = data.maxMarketName;
                                crypto.MinMarketNation = data.minMarketNation;
                                crypto.MinMarketName = data.minMarketName;
                                break;
                            }
                        }

                        //    if (model == null)
                        //        cryptoDetail.Add(new CryptoViewModel(data.Target, data.Stand, data.MaxMarketName, data.MinMarketName, data.Percent));
                    }
                }


                System.Threading.Thread.Sleep(3000);
            }
        }
    }
}
