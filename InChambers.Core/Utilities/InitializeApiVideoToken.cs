using LazyCache;
using InChambers.Core.Constants;
using InChambers.Core.Models.Configurations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;
using System.Text;

namespace InChambers.Core.Utilities;

public static class InitializeApiVideoToken
{
    public static async Task InitializeToken(IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices.CreateScope();
        var cacheService = serviceScope.ServiceProvider.GetService<IAppCache>();
        if (cacheService == null) throw new ArgumentNullException(nameof(cacheService));

        var httpClientFactory = serviceScope.ServiceProvider.GetService<IHttpClientFactory>();
        if (httpClientFactory == null) throw new ArgumentNullException(nameof(httpClientFactory));

        var httpClient = httpClientFactory.CreateClient(HttpClientKeys.ApiVideo);

        var appConfig = serviceScope.ServiceProvider.GetService<IOptions<AppConfig>>();
        string apiVideoKey = appConfig!.Value.ApiVideo.Key;

        var request = new { apiKey = apiVideoKey };
        var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync("auth/api-key", content);

        if (!response.IsSuccessStatusCode)
        {
            string stringResponse = await response.Content.ReadAsStringAsync();
            object error = JsonConvert.DeserializeObject<object>(stringResponse);
            Log.Error("--> Could not get Api.Video Token: {@Error}", error ?? "Unknown error");
            return;
        }

        string resString = await response.Content.ReadAsStringAsync();
        dynamic tokenObj = JsonConvert.DeserializeObject<dynamic>(resString);
        string token = tokenObj!.access_token;
        string refreshToken = tokenObj.refresh_token;

        cacheService.Remove(AuthKeys.ApiVideoToken);
        cacheService.Remove(AuthKeys.ApiVideoRefreshToken);
        cacheService.Add(AuthKeys.ApiVideoToken, token, DateTime.UtcNow.AddSeconds(3590));
        cacheService.Add(AuthKeys.ApiVideoRefreshToken, refreshToken, DateTime.UtcNow.AddYears(20));

        Log.Information("--> Api.Video Token initialized");
    }
}