using CzyToSmog.net.Interfaces;
using DryIoc;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CzyToSmog.net.UI
{
    public class AppNavigation : IAppNavigation
    {
        private IResolver _resolver;

        private readonly IReadOnlyDictionary<Type, Type> _pages = new Dictionary<Type, Type>
        {
            {typeof(IMainPageViewModel), typeof(MainPage)},
            {typeof(IWeatherPageViewModel), typeof(WeatherPage)}
        };

        public AppNavigation(IResolver resolver)
        {
            _resolver = resolver;
        }

        public void Navigate<T>() where T : class
        {
            var viewModelType = typeof(T);
            var frame = (Frame)Window.Current.Content;
            var viewModel = _resolver.Resolve(viewModelType);

            frame.Navigate(_pages[viewModelType], viewModel);
            ((Page)frame.Content).DataContext = viewModel;

        }

        public void Navigate<T>(object param) where T : class
        {
            var viewModelType = typeof(T);
            var frame = (Frame)Window.Current.Content;
            var viewModel = _resolver.Resolve(viewModelType);

            frame.Navigate(_pages[viewModelType], param);
            ((Page)frame.Content).DataContext = viewModel;
        }
    }
}
