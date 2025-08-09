using LazyCache;
using InChambers.Core.Constants;
using InChambers.Core.Models.Configurations;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;
using System.Net.Http.Headers;
using System.Text;

namespace InChambers.Core.Middlewares;

public class ApiVideoHttpHandler : DelegatingHandler
{
    private readonly string _apiVideoBaseUrl;
    private readonly IAppCache _cacheService;
    private readonly ILogger _logger;

    public ApiVideoHttpHandler(IOptions<AppConfig> appConfig, IAppCache cacheService, ILogger logger)
    {
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (appConfig == null) throw new ArgumentNullException(nameof(appConfig));
        _apiVideoBaseUrl = appConfig.Value.ApiVideo.BaseUrl;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // get the current url
        string requestedUrl = request.RequestUri.ToString();
        if (!requestedUrl.Contains("api-key"))
        {
            var bearerToken = await CreateBearerToken();
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken ?? "");
        }

        var response = await base.SendAsync(request, cancellationToken);
        return response;
    }

    private async Task<string> CreateBearerToken()
    {
        // get token
        string token = await _cacheService.GetAsync<string>(AuthKeys.ApiVideoToken);
        if (!string.IsNullOrEmpty(token)) return token;

        // TODO: implement a scenario where the refresh token itself fails or is expired
        // get refresh token
        string refreshToken = await _cacheService.GetAsync<string>(AuthKeys.ApiVideoRefreshToken);
        if (string.IsNullOrEmpty(refreshToken)) throw new Exception("Api.Video Refresh Token not found");

        var httpClient = new HttpClient
        {
            BaseAddress = new Uri(_apiVideoBaseUrl)
        };

        var request = new { refreshToken = refreshToken };
        var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync("auth/refresh", content);

        if (response.IsSuccessStatusCode)
        {
            string resStri = await response.Content.ReadAsStringAsync();
            dynamic tokenObj = JsonConvert.DeserializeObject(resStri) ?? "";
            token = tokenObj!.access_token;
            refreshToken = tokenObj.refresh_token;

            DateTime now = DateTime.UtcNow;
            _cacheService.Remove(AuthKeys.ApiVideoToken);
            _cacheService.Remove(AuthKeys.ApiVideoRefreshToken);
            _cacheService.Add(AuthKeys.ApiVideoToken, token, now.AddSeconds(3590));
            _cacheService.Add(AuthKeys.ApiVideoRefreshToken, refreshToken, now.AddYears(20));

            _logger.Information($"--> Api.Video Token refreshed on {now}");
        }

        return token;
    }
}