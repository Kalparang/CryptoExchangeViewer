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
                var result = await db.SelectTest();

                CryptoDetail = new ObservableCollection<CryptoViewModel>(result.Select(r => new CryptoViewModel(r.Target, r.Stand, r.Price, r.Market)).ToList());

                System.Threading.Thread.Sleep(3000);
            }
        }
    }
}
