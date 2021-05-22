using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CryptoExchangeViewer.MVVM;
using CryptoExchangeViewer.UserControls;

namespace CryptoExchangeViewer.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private DBHelper.DBHelper db;

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

        private ObservableCollection<BanViewModel> banList;
        public ObservableCollection<BanViewModel> BanList
        {
            get { return banList; }
            set
            {
                banList = value;
                RaisePropertyChangedEvent("BanList");
            }
        }

        private BanViewModel selectedBan;
        public BanViewModel SelectedBan
        {
            get { return selectedBan; }
            set
            {
                selectedBan = value;
                RaisePropertyChangedEvent("BanIsSelected");
                RaisePropertyChangedEvent("SelectedBan");
                SelectBan();
            }
        }

        private void SelectBan()
        {
            if(SelectedBan != null)
            {

            }
        }

        public bool BanIsSelected
        {
            get { return SelectedBan != null; }
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

        public ICommand BanHighestNation { get; set; }
        public ICommand BanHighestMarket { get; set; }
        public ICommand BanLowestNation { get; set; }
        public ICommand BanLowestMarket { get; set; }
        public ICommand UnblockCommand { get; set; }
        public ICommand BanListCommand { get; set; }
        private BanListWindow ban;

        public MainViewModel()
        {
            db = new DBHelper.DBHelper();

            CryptoDetail = new ObservableCollection<CurrencyViewModel>();
            CurrencyDetail = new ObservableCollection<CurrencyViewModel>();

            BanHighestNation = new DelegateCommand((obejct) =>
            {
                string BanName = SelectedSymbol.MaxMarketNation;
                int Type = 2;

                db.AddBan(Type, BanName);
            });

            BanHighestMarket = new DelegateCommand((obejct) =>
            {
                string BanName = SelectedSymbol.MaxMarketName;
                int Type = 1;

                db.AddBan(Type, BanName);
            });

            BanLowestNation = new DelegateCommand((obejct) =>
            {
                string BanName = SelectedSymbol.MinMarketNation;
                int Type = 2;

                db.AddBan(Type, BanName);
            });

            BanLowestMarket = new DelegateCommand((obejct) =>
            {
                string BanName = SelectedSymbol.MinMarketName;
                int Type = 1;

                db.AddBan(Type, BanName);
            });

            UnblockCommand = new DelegateCommand((obejct) =>
            {
                int type = 1;
                string Name = SelectedBan.Name;

                switch (SelectedBan.Type)
                {
                    case "거래소":
                        type = 1;
                        break;
                    case "나라":
                        type = 2;
                        break;
                    default:
                        return;
                }

                db.SubBan(type, Name);
            });

            BanListCommand = new DelegateCommand((obejct) =>
            {
                ban = new BanListWindow(this);
                ban.ShowDialog();

            });

            Task.Run(() => Worker());
        }

        async Task Worker()
        {
            while (true)
            {
                var result = db.select();

                if (cryptoDetail.Count != result.Count())
                {
                    CryptoDetail = new ObservableCollection<CurrencyViewModel>(result.Select(r => new CurrencyViewModel(
                        r.targetCrypto, r.maxStandCurrency, r.maxMarketNation, r.maxMarketName, r.minMarketNation, r.minMarketName, r.TargetPrice, r.StandPrice, r.percent)).ToList());
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
                        r.targetCrypto, r.maxStandCurrency, r.minStandCurrency, r.maxMarketName, r.maxMarketName, r.minMarketNation, r.minMarketName, r.TargetPrice, r.StandPrice, r.percent)).ToList());
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
