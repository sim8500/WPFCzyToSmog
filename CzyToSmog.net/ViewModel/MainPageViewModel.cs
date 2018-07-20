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
using CzyToSmog.net.Model;
using CzyToSmog.net.UI;
using ReactiveUI;
using CzyToSmog.net.Interfaces;

namespace CzyToSmog.net.ViewModel
{
    public class MainPageViewModel : ReactiveObject, IMainPageViewModel
    {
        private IAppNavigation _appNav;

        private HttpClient _httpClient;

        private StationInfoModel _currentStation;

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

        public MainPageViewModel(IAppNavigation appNav)
        {
            _appNav = appNav;

            LoadListCommand = ReactiveCommand.CreateFromTask<List<StationInfoModel>>(execute: () => LoadListAsync(), outputScheduler: RxApp.MainThreadScheduler);
            LoadSensorsCommand = ReactiveCommand.CreateFromTask<StationInfoModel, List<SensorInfoModel>>(station => LoadStationSensorsAsync(station), outputScheduler: RxApp.MainThreadScheduler);
            LoadDataCommand = ReactiveCommand.CreateFromTask<SensorInfoModel, SensorDataEntry>(sensor => LoadSensorDataAsync(sensor), outputScheduler: RxApp.MainThreadScheduler);

            _stationsInfoList = LoadListCommand.ToProperty(this, x => x.StationsList, new List<StationInfoModel>());
            _sensorInfoModels = LoadSensorsCommand.ToProperty(this, x => x.SensorsList, new List<SensorInfoModel>());
            _sensorData = LoadDataCommand.ToProperty(this, x => x.SensorData, new SensorDataEntry());

            this.ObservableForProperty(x => x.IsBeingNavigated)
                .Select(e => new Unit())
                .InvokeCommand(LoadListCommand);

            this.ObservableForProperty(x => x.SelectedStation)
                .Select(e => e.Value)
                .InvokeCommand(LoadSensorsCommand);

            this.ObservableForProperty(x => x.SensorsList)
                .Select(e => e.Value)
                .Subscribe(x => SelectedSensor = x.FirstOrDefault() );
                    
            this.ObservableForProperty(x => x.SelectedSensor)
                .Select(e => e.Value)
                .InvokeCommand(LoadDataCommand);


           _filteredStationsInfoList = this.WhenAnyValue( 
                                                         x => x.Filter,
                                                         x => x.StationsList,
                                                         (f,l) => l.CreateDerivedCollection(x => x, 
                                                                                            x => x.StationName.Contains(string.IsNullOrEmpty(f) ? "" : f)
                                                                                           ).ToList()
                                                        )
                                                        .ToProperty(this, x => x.FilteredStationsList, new List<StationInfoModel>());

        }

        ObservableAsPropertyHelper<List<StationInfoModel>> _stationsInfoList;
        ObservableAsPropertyHelper<List<StationInfoModel>> _filteredStationsInfoList;

        ObservableAsPropertyHelper<List<SensorInfoModel>> _sensorInfoModels;

        ObservableAsPropertyHelper<SensorDataEntry> _sensorData;

        public List<StationInfoModel> StationsList { get { return _stationsInfoList.Value;  } }

        public List<StationInfoModel> FilteredStationsList => _filteredStationsInfoList.Value;
               

        public List<SensorInfoModel> SensorsList => _sensorInfoModels.Value;

        public SensorDataEntry SensorData => _sensorData.Value;

        private string _filter;
        public string Filter
        {
            get { return _filter; }
            set { this.RaiseAndSetIfChanged(ref _filter, value); }
        }

        bool _isBeingNavigated = false;

        public bool IsBeingNavigated
        {
            get { return _isBeingNavigated; }
            set { this.RaiseAndSetIfChanged(ref _isBeingNavigated, value); }
        }

        SensorInfoModel _selectedSensor = null;

        public SensorInfoModel SelectedSensor
        {
            get { return _selectedSensor; }
            set { this.RaiseAndSetIfChanged(ref _selectedSensor, value); }
        }

        StationInfoModel _selectedStation = null;

        public StationInfoModel SelectedStation
        {
            get { return _selectedStation; }
            set { this.RaiseAndSetIfChanged(ref _selectedStation, value); }
        }

        #region Commands

        public ReactiveCommand<Unit, List<StationInfoModel>> LoadListCommand
        {
            get; protected set;
        }

        public ReactiveCommand<StationInfoModel, List<SensorInfoModel>> LoadSensorsCommand
        {
            get; protected set;
        }

        public ReactiveCommand<SensorInfoModel, SensorDataEntry> LoadDataCommand
        {
            get; protected set;
        }

        private ReactiveCommand<Unit, Unit> _weatherPageCommand;

        public ICommand WeatherPageCommand
        {
            get
            {
                if (_weatherPageCommand == null)
                {
                    _weatherPageCommand = ReactiveCommand<Unit, Unit>.Create(() => { GoToWeatherPage(); });
                }

                return _weatherPageCommand;
            }
   
        }

        #endregion Commands


        public async Task<List<StationInfoModel>> LoadListAsync()
        {
            var res = await ReqHttpClient.GetAsync("/pjp-api/rest/station/findAll");
            var stream = await res.Content.ReadAsStreamAsync();

            var serializer = new DataContractJsonSerializer(typeof(List<StationInfoModel>));
            var result =  serializer.ReadObject(stream) as List<StationInfoModel>;

            return result;
        }

        public async Task<List<SensorInfoModel>> LoadStationSensorsAsync(StationInfoModel station)
        {
            _currentStation = station;

            var res = await ReqHttpClient.GetAsync($"/pjp-api/rest/station/sensors/{station.Id}/");
            var stream = await res.Content.ReadAsStreamAsync();
            var serializer = new DataContractJsonSerializer(typeof(List<SensorInfoModel>));

            var sensors = serializer.ReadObject(stream) as List<SensorInfoModel>;

            return sensors;
        }

        public async Task<SensorDataEntry> LoadSensorDataAsync(SensorInfoModel sensor)
        {
            if(sensor == null)
            {
                return null;
            }

            var res = await ReqHttpClient.GetAsync($"/pjp-api/rest/data/getData/{sensor.Id}/");
            var stream = await res.Content.ReadAsStreamAsync();
            var serializer = new DataContractJsonSerializer(typeof(SensorDataInfo));

            var model = serializer.ReadObject(stream) as SensorDataInfo;

            return model?.Entries.FirstOrDefault(e => e.Value != null);
        }


        private void GoToWeatherPage()
        {
            if(_currentStation != null)
            {
                _appNav.Navigate<IWeatherPageViewModel>(_currentStation.City.Name);
            }
        }
    }
}
