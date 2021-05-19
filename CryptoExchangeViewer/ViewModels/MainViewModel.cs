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
        private ObservableCollection<CryptoViewModel> cryptoDetail;
        public ObservableCollection<CryptoViewModel> CryptoDetail
        {
            get { return cryptoDetail; }
            set
            {
                cryptoDetail = value;
                RaisePropertyChangedEvent("CryptoDetail");
            }
        }

        public MainViewModel()
        {
            CryptoDetail = new ObservableCollection<CryptoViewModel>();

            Task.Run(() => Worker());
        }

        async Task Worker()
        {
            DBHelper.DBHelper db = new DBHelper.DBHelper();

            while (true)
            {
                //var result = await db.SelectTest2();
                var result = db.select();

                var ccount = cryptoDetail.Count;
                var acount = result.Count();

                if (cryptoDetail.Count < result.Count())
                {
                    CryptoDetail = new ObservableCollection<CryptoViewModel>(result.Select(r => new CryptoViewModel(
                        r.Target, r.Stand, r.MaxMarketName, r.MinMarketName, r.Percent)).ToList());
                }
                else
                {
                    foreach (var data in result)
                    {
                        CryptoViewModel model = null;

                        foreach (var crypto in cryptoDetail)
                        {
                            if (crypto.TargetCrypto == data.Target && crypto.StandCrypto == data.Stand)
                            {
                                model = crypto;
                                crypto.Percent = data.Percent;
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
