using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Reactive;
using System.Reactive.Threading.Tasks;
using System.Reactive.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Windows.Input;

namespace CzyToSmog.net
{
    class MainPageViewModel : INotifyPropertyChanged
    {
        private List<StationInfoModel> _stationsInfoList;

        private List<SensorInfoModel> _sensorInfoModels;
 
        private RelayCommand _listCommand;

        private RelayCommand _detailsCommand;

        private int _selectedIndex = 0;

        public List<StationInfoModel> StationsList {
                                                        get { return _stationsInfoList; }
                                                        set { SetProperty(ref _stationsInfoList, value); }
                                                   }

        public List<SensorInfoModel> SensorsList {
                                                    get { return _sensorInfoModels; }
                                                    set { SetProperty(ref _sensorInfoModels, value); }
                                                 }

        public int SelectedIndex {
                                    get { return _selectedIndex; }
                                    set { SetProperty(ref _selectedIndex, value); }
                                 }

        public event PropertyChangedEventHandler PropertyChanged;

        public MainPageViewModel()
        {
        }

        public ICommand LoadListCommand
        {
            get
            {
                if(_listCommand == null)
                {
                    _listCommand = new RelayCommand((page) => LoadListAsync(page as Page));
                }

                return _listCommand;
            }
        }

        public ICommand LoadSensorsCommand
        {
            get
            {
                if (_detailsCommand == null)
                {
                    _detailsCommand = new RelayCommand((page) => LoadStationSensors(page as Page));
                }

                return _detailsCommand;
            }
        }


        /* public void LoadStationsAsync(Page page)
         {
             var client = new HttpClient
             {
                 BaseAddress = new Uri("http://api.gios.gov.pl/")
             };

             client.DefaultRequestHeaders.Accept.Clear();
             client.DefaultRequestHeaders.Accept.Add(
                 new MediaTypeWithQualityHeaderValue("application/json"));

             var katoUrl = "/pjp-api/rest/aqindex/getIndex/814";
             var glcUrl = "/pjp-api/rest/aqindex/getIndex/809";
             var krkUrl = "/pjp-api/rest/aqindex/getIndex/400";
             var serializer = new DataContractJsonSerializer(typeof(StationOverviewModel));

             var queryObservable = System.Reactive.Linq.Observable.Merge(client.GetAsync(katoUrl).ToObservable(),
                                                                         client.GetAsync(glcUrl).ToObservable(),
                                                                         client.GetAsync(krkUrl).ToObservable());

             Stations = new List<StationOverviewModel>();

             queryObservable.Select(async r => await r.Content.ReadAsStreamAsync())
                             .Select(w => serializer.ReadObject(w.Result) as StationOverviewModel)
                             .Select(s => { return SetStationName(s); })
                             .Subscribe(async t => {
                                 Stations.Add(t); },
                                 async () => await page.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                                                                 () => { NotifyPropertyChanged("Stations"); })
                             );

         }*/

        public void LoadListAsync(Page page)
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri("http://api.gios.gov.pl/")
            };

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var serializer = new DataContractJsonSerializer(typeof(List<StationInfoModel>));
            var listObservable = client.GetAsync("/pjp-api/rest/station/findAll").ToObservable();

            listObservable.Select(async r => await r.Content.ReadAsStreamAsync())
                            .Select(l => serializer.ReadObject(l.Result) as List<StationInfoModel>)
                            .ObserveOn(page.Content.Dispatcher)
                            .Subscribe(sl => { StationsList = sl;  });

        }

        public void LoadStationSensors(Page page)
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri("http://api.gios.gov.pl/")
            };

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var serializer = new DataContractJsonSerializer(typeof(List<SensorInfoModel>));
            var item = _stationsInfoList.ElementAt(_selectedIndex);
 
            var listObservable = client.GetAsync($"/pjp-api/rest/station/sensors/{item.Id}/").ToObservable();

            listObservable.Select(async r => await r.Content.ReadAsStreamAsync())
                            .Select(l => serializer.ReadObject(l.Result) as List<SensorInfoModel>)
                            .ObserveOn(page.Content.Dispatcher)
                            .Subscribe(sl => { SensorsList = sl; },
                                        e => {

                                            SensorsList = new List<SensorInfoModel>() {
                                                                                        new SensorInfoModel { Id = $"error returned: {e.Message}" }
                                                                                    };
                                            } );
        }


        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (object.Equals(storage, value))
                return false;
            storage = value;
            this.NotifyPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Notifies listeners that a property value has changed.
        /// </summary>
        /// <param name="propertyName">Name of the property used to notify listeners.  This
        /// value is optional and can be provided automatically when invoked from compilers
        /// that support <see cref="CallerMemberNameAttribute"/>.</param>
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private StationOverviewModel SetStationName(StationOverviewModel model)
        {
            switch(model.StationId)
            {
                case 814:
                    model.StationName = "Kato";
                    break;
                case 809:
                    model.StationName = "Gliwice";
                    break;
                case 400:
                    model.StationName = "Kraków";
                    break;
                default:
                    model.StationName = "Radom";
                    break;
            }

            return model;
        }
    }
}
