using CzyToSmog.net.Interfaces;
using CzyToSmog.net.Model;
using ReactiveUI;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive;
using System.Windows.Input;

namespace CzyToSmog.net.ViewModel
{
    class WeatherPageViewModel : ReactiveObject, IWeatherPageViewModel
    {
        private HttpClient _httpClient = null;
        private HttpClient ReqHttpClient
        {
            get
            {
                if (_httpClient == null)
                {
                    _httpClient = new HttpClient
                    {
                        BaseAddress = new Uri("http://api.openweathermap.org/")
                    };

                    _httpClient.DefaultRequestHeaders.Accept.Clear();
                    _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                }
                return _httpClient;
            }
        }

        ObservableAsPropertyHelper<WeatherDataModel> _weatherData;

        public WeatherDataModel WeatherData => _weatherData.Value;

        public ReactiveCommand<string, WeatherDataModel> LoadWeatherCommand
        {
            get; protected set;
        }

        private ReactiveCommand<Unit, Unit> _goBackCommand;

        public ICommand GoBackCommand
        {
            get
            {
                if (_goBackCommand == null)
                {
                    _goBackCommand = ReactiveCommand<Unit, Unit>.Create(() => { _appNav.Navigate<IMainPageViewModel>(); });
                }

                return _goBackCommand;
            }

        }

        Uri _weatherIconUrl = null;
        public Uri WeatherIconUrl
        {
            get => _weatherIconUrl;

            set { this.RaiseAndSetIfChanged(ref _weatherIconUrl, value); }
        }

        IAppNavigation _appNav;

        public WeatherPageViewModel(IAppNavigation appNav)
        {
            _appNav = appNav;

            LoadWeatherCommand = ReactiveCommand.CreateFromTask<string, WeatherDataModel>(city => LoadWeatherData(city), outputScheduler: RxApp.MainThreadScheduler);
            _weatherData = LoadWeatherCommand.ToProperty(this, x => x.WeatherData, new WeatherDataModel());

            this.ObservableForProperty(x => x.WeatherData)
                .Select(e => e.Value)
                .Subscribe(x =>
                {
                    if (x != null && x.WeatherEntries != null && x.WeatherEntries.Any())
                    {
                        var icon = x.WeatherEntries.First().Icon;
                        WeatherIconUrl = new Uri($"http://openweathermap.org/img/w/{icon}.png");
                    }
                });
        }

        public async Task<WeatherDataModel> LoadWeatherData(string city)
        {
            var res = await ReqHttpClient.GetAsync($"data/2.5/weather?q={city}&appid=fe26c1493a338c073be30186a4b8d59e&units=metric");
            var stream = await res.Content.ReadAsStreamAsync();
            var serializer = new DataContractJsonSerializer(typeof(WeatherDataModel));

            var result = serializer.ReadObject(stream) as WeatherDataModel;
            return result;
        }
    }
}
