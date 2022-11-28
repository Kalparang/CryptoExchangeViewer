using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using System.Windows.Controls;
using System.Windows.Input;
using CryptoExchangeViewer.MVVM;
using CryptoExchangeViewer.UserControls;
using static DBHelper.DBHelper;

namespace CryptoExchangeViewer.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private DBHelper.DBHelper db;
        CancellationTokenSource SelectedToken;
        EventWaitHandle exitHandle =
            new EventWaitHandle(false, EventResetMode.ManualReset);

        //메인뷰에서 암호화폐-암호화폐 Grid의 뷰모델
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

        //메인뷰에서 암호화폐-법정화폐 Grid의 뷰모델
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

        //SymbolUserControl의 Grid 뷰모델
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

        private ObservableCollection<ExchangeViewModel> exchangeList;
        public ObservableCollection<ExchangeViewModel> ExchangeList
        {
            get { return exchangeList; }
            set
            {
                exchangeList = value;
                RaisePropertyChangedEvent("ExchangeList");
            }
        }

        //SymbolUserControl에서 차단된 항목 리스트
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
                SelectedToken.Cancel();

                SelectedToken = new CancellationTokenSource();

                new Task(() =>
                {
                    CancellationTokenSource localToken = SelectedToken;


                    while (localToken.IsCancellationRequested != true)
                    {
                        List<CurrencyExchangeModel> blackModel;
                        
                        if(string.IsNullOrEmpty(SelectedSymbol.MinStandCurrency))
                            blackModel = db.SelectedList(SelectedSymbol.TargetCrypto, SelectedSymbol.MaxStandCurrency);
                        else
                            blackModel = db.SelectedList(SelectedSymbol.TargetCrypto);

                        //if (SelectDetail.Count != blackModel.Count())
                        {
                            SelectDetail = new ObservableCollection<CurrencyViewModel>(blackModel.Select(r => new CurrencyViewModel(
                                r.targetCrypto, r.maxStandCurrency, r.maxMarketNation, r.maxMarketName, r.TargetPrice)).ToList());
                        }

                        Thread.Sleep(3000);
                    }
                }).Start();
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
            db = new DBHelper.DBHelper(exitHandle);
            Application.Current.Exit += Current_Exit;

            CryptoDetail = new ObservableCollection<CurrencyViewModel>();
            CurrencyDetail = new ObservableCollection<CurrencyViewModel>();
            SelectDetail = new ObservableCollection<CurrencyViewModel>();
            BanList = new ObservableCollection<BanViewModel>();

            SelectedToken = new CancellationTokenSource();

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

                CancellationTokenSource BanListToken = new CancellationTokenSource();

                new Task(() =>
                {
                    while (BanListToken.IsCancellationRequested != true)
                    {
                        List<BlackListModel> blackModel = db.SelectBlackList();

                        if (BanList.Count != blackModel.Count())
                        {
                            BanList = new ObservableCollection<BanViewModel>(blackModel.Select(r => new BanViewModel(
                                r.Type, r.Name)).ToList());
                        }

                        Thread.Sleep(3000);
                    }
                }, BanListToken.Token).Start();

                ban.ShowDialog();

                BanListToken.Cancel();
            });

            Task.Run(() => Worker());
        }

        async Task Worker()
        {
            while (true)
            {
                var result = db.select();

                //if (cryptoDetail.Count != result.Count())
                {
                    CryptoDetail = new ObservableCollection<CurrencyViewModel>(result.Select(r => new CurrencyViewModel(
                        r.targetCrypto, r.maxStandCurrency, r.maxMarketNation, r.maxMarketName, r.minMarketNation, r.minMarketName, r.TargetPrice, r.StandPrice,
                        r.percent, r.OneMin, r.FiveMin, r.OneHour, r.OneDay)).ToList());
                }
                //else
                //{
                //    foreach (var data in result)
                //    {
                //        CurrencyViewModel model = null;

                //        foreach (var crypto in cryptoDetail)
                //        {
                //            if (crypto.TargetCrypto == data.targetCrypto && crypto.MaxStandCurrency == data.maxStandCurrency)
                //            {
                //                model = crypto;
                //                crypto.Percent = data.percent;
                //                crypto.TargetPrice = data.TargetPrice;
                //                crypto.MaxStandPrice = data.StandPrice;
                //                crypto.OneMin = data.OneMin;
                //                crypto.FiveMin = data.FiveMin;
                //                crypto.OneHour = data.OneHour;
                //                crypto.OneDay = data.OneDay;
                //                crypto.MaxMarketName = data.maxMarketName;
                //                crypto.MaxMarketNation = data.maxMarketNation;
                //                crypto.MinMarketName = data.minMarketName;
                //                crypto.MinMarketNation = data.minMarketNation;
                //                break;
                //            }
                //        }

                //        //    if (model == null)
                //        //        cryptoDetail.Add(new CryptoViewModel(data.Target, data.Stand, data.MaxMarketName, data.MinMarketName, data.Percent));
                //    }
                //}

                var result2 = db.select2();

                //if (CurrencyDetail.Count != result2.Count())
                {
                    CurrencyDetail = new ObservableCollection<CurrencyViewModel>(result2.Select(r => new CurrencyViewModel(
                        r.targetCrypto, r.maxStandCurrency, r.minStandCurrency, r.maxMarketNation, r.maxMarketName, r.minMarketNation, r.minMarketName, r.TargetPrice, r.StandPrice,
                        r.percent, r.OneMin, r.FiveMin, r.OneHour, r.OneDay)).ToList());
                }
                //else
                //{
                //    foreach (var data in result2)
                //    {
                //        CurrencyViewModel model = null;

                //        foreach (var crypto in CurrencyDetail)
                //        {
                //            if (crypto.TargetCrypto == data.targetCrypto && crypto.MaxStandCurrency == data.maxStandCurrency
                //                && crypto.MinStandCurrency == data.minStandCurrency)
                //            {
                //                model = crypto;
                //                crypto.Percent = data.percent;
                //                crypto.OneMin = data.OneMin;
                //                crypto.FiveMin = data.FiveMin;
                //                crypto.OneHour = data.OneHour;
                //                crypto.OneDay = data.OneDay;
                //                crypto.TargetPrice = data.TargetPrice;
                //                crypto.MaxStandPrice = data.StandPrice;
                //                crypto.MaxMarketNation = data.maxMarketNation;
                //                crypto.MaxMarketName = data.maxMarketName;
                //                crypto.MinMarketNation = data.minMarketNation;
                //                crypto.MinMarketName = data.minMarketName;
                //                break;
                //            }
                //        }

                //        //    if (model == null)
                //        //        cryptoDetail.Add(new CryptoViewModel(data.Target, data.Stand, data.MaxMarketName, data.MinMarketName, data.Percent));
                //    }
                //}

                var result3 = db.SelectExchange();

                var tempList = new ObservableCollection<ExchangeViewModel>();
                decimal[] cny = new decimal[2];
                decimal[] eur = new decimal[2];
                decimal[] hkd = new decimal[2];
                decimal[] jpy = new decimal[2];
                decimal[] usd = new decimal[2];

                foreach(var data in result3)
                {
                    if(data.Target == "KRW")
                    {
                        switch(data.Stand)
                        {
                            case "CNY":
                                cny[0] = data.Price;
                                break;
                            case "EUR":
                                eur[0] = data.Price;
                                break;
                            case "HKD":
                                hkd[0] = data.Price;
                                break;
                            case "JPY":
                                jpy[0] = data.Price;
                                break;
                            case "USD":
                                usd[0] = data.Price;
                                break;
                        }
                    }
                    else
                    {
                        switch (data.Target)
                        {
                            case "CNY":
                                cny[1] = data.Price;
                                break;
                            case "EUR":
                                eur[1] = data.Price;
                                break;
                            case "HKD":
                                hkd[1] = data.Price;
                                break;
                            case "JPY":
                                jpy[1] = data.Price;
                                break;
                            case "USD":
                                usd[1] = data.Price;
                                break;
                        }
                    }
                }

                tempList.Add(new ExchangeViewModel("KRW로 팔때", cny[0], eur[0], hkd[0], jpy[0], usd[0]));
                tempList.Add(new ExchangeViewModel("KRW로 살때", cny[1], eur[1], hkd[1], jpy[1], usd[1]));

                ExchangeList = tempList;

                if (exitHandle.WaitOne(3000) == true)
                    break;
            }
        }

        private void Current_Exit(object sender, ExitEventArgs e)
        {
            exitHandle.Set();
        }
    }
}
