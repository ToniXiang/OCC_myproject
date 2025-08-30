using MauiApp1;
using Microsoft.Extensions.Logging;
using 簡易的行控中心.ViewModels;

namespace 簡易的行控中心
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
            builder.Services.AddTransient<HomePage>();
            builder.Services.AddSingleton<HomePageViewModel>();
            builder.Services.AddSingleton<TrafficDataService>();
            builder.Services.AddTransient<global::簡易的行控中心.EventPage>();
            builder.Services.AddTransient<global::簡易的行控中心.ViewModels.EventPageViewModel>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
