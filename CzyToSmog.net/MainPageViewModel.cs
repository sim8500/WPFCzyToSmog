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
using Windows.UI.Core;

namespace CzyToSmog.net
{
    class MainPageViewModel : INotifyPropertyChanged
    {
        private List<StationInfoModel> _stationsInfoList;

        private List<SensorInfoModel> _sensorInfoModels;

        private SensorDataEntry _sensorInfo;
 
        private RelayCommand _listCommand;

        private RelayCommand _sensorsCommand;

        private RelayCommand _dataCommand;

        private HttpClient _httpClient;

        private CoreDispatcher _dispatcher;

        private SensorInfoModel _selectedSensor = null;

        private HttpClient ReqHttpClient
        {
            get {
                if (_httpClient == null)
                {
                    _httpClient = new HttpClient
                    {
                        BaseAddress = new Uri("http://api.gios.gov.pl/")
                    };

                    _httpClient.DefaultRequestHeaders.Accept.Clear();
                    _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                }
                return _httpClient;
            }
        }

        public MainPageViewModel(CoreDispatcher d)
        {
            _dispatcher = d;
        }

        public List<StationInfoModel> StationsList
        {
            get { return _stationsInfoList; }
            set { SetProperty(ref _stationsInfoList, value); }
        }

        public List<SensorInfoModel> SensorsList
        {
           get { return _sensorInfoModels; }
           set { SetProperty(ref _sensorInfoModels, value); }
        }

        public SensorDataEntry SensorInfo
        {
            get { return _sensorInfo; }
            set { SetProperty(ref _sensorInfo, value); }
        }

        public SensorInfoModel SelectedSensor
        {
            get { return _selectedSensor; }
            set { SetProperty(ref _selectedSensor, value); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #region Commands

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
                if (_sensorsCommand == null)
                {
                    _sensorsCommand = new RelayCommand((station) => LoadStationSensors(station as StationInfoModel));
                }

                return _sensorsCommand;
            }
        }

        public ICommand LoadDataCommand
        {
            get
            {
                if(_dataCommand == null)
                {
                    _dataCommand = new RelayCommand((s) => LoadSensorData(s as SensorInfoModel));
                }

                return _dataCommand;
            }
        }
    
        #endregion Commands

   
        public void LoadListAsync(Page page)
        {

            var serializer = new DataContractJsonSerializer(typeof(List<StationInfoModel>));
            var listObservable = ReqHttpClient.GetAsync("/pjp-api/rest/station/findAll").ToObservable();

            listObservable.Select(async r => await r.Content.ReadAsStreamAsync())
                            .Select(l => serializer.ReadObject(l.Result) as List<StationInfoModel>)
                            .ObserveOn(page.Content.Dispatcher)
                            .Subscribe(sl => { StationsList = sl;  });

        }

        public void LoadStationSensors(StationInfoModel station)
        {
            if(station == null)
            {
                return;
            }

            var serializer = new DataContractJsonSerializer(typeof(List<SensorInfoModel>));
 
            var listObservable = ReqHttpClient.GetAsync($"/pjp-api/rest/station/sensors/{station.Id}/").ToObservable();

            listObservable.Select(async r => await r.Content.ReadAsStreamAsync())
                            .Select(l => serializer.ReadObject(l.Result) as List<SensorInfoModel>)
                            .ObserveOn(_dispatcher)
                            .Subscribe(sl => {
                                                SensorsList = sl;
                                                if(SensorsList != null && SensorsList.Count > 0)
                                                {
                                                    SelectedSensor = SensorsList.First();
                                                }
                                             });
        }

        public void LoadSensorData(SensorInfoModel sensor)
        {
            if (sensor == null)
                return;

            var serializer = new DataContractJsonSerializer(typeof(SensorDataInfo));

            var dataObservable = ReqHttpClient.GetAsync($"/pjp-api/rest/data/getData/{sensor.Id}/").ToObservable();

            dataObservable.Select(async r => await r.Content.ReadAsStreamAsync())
                .Select(l => serializer.ReadObject(l.Result) as SensorDataInfo)
                .ObserveOn(_dispatcher)
                .Subscribe(sdi => { SensorInfo = sdi.Entries.First((x) => x.Value != null); });
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
