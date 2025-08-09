/* 
 * TODOs:
 * Implement a background service using Quartz to expire user's courses.
 * Implement a background service using Quartz to expire user's series.
 * Implement a background service using Quartz to send email reminders to users.
 * Implement a background service using Quartz to find stale videos and delete them from api.video
 * Implement a background service using Quartz to find stale documents and delete them from the file system
 */


using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using FluentValidation;
using Mapster;
using InChambers.Core.Constants;
using InChambers.Core.Interfaces;
using InChambers.Core.Middlewares;
using InChambers.Core.Models.App;
using InChambers.Core.Models.Input.AnnotatedAgreements;
using InChambers.Core.Models.Input.Auth;
using InChambers.Core.Models.Input.Courses;
using InChambers.Core.Models.Input.Series;
using InChambers.Core.Models.Input.SmeHub;
using InChambers.Core.Models.Utilities;
using InChambers.Core.Models.View;
using InChambers.Core.Models.View.Courses;
using InChambers.Core.Models.View.Questions;
using InChambers.Core.Models.View.Series;
using InChambers.Core.Models.View.SmeHub;
using InChambers.Core.Models.View.Users;
using InChambers.Core.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;

namespace InChambers.Core.Extensions;

/// <summary>
/// Extension methods for configuring services in the application.
/// </summary>
public static class ServiceExtensions
{
    /// <summary>
    /// Configures services for the application, including database, validation, authentication, authorization, HTTP context, caching, and various services.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configuration">The configuration for the application.</param>
    /// <param name="isProduction">A flag indicating whether the application is running in a production environment.</param>
    /// <returns>The modified <see cref="IServiceCollection"/> for method chaining.</returns>
    public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration, bool isProduction)
    {
        // set up database
        string connectionString = configuration.GetConnectionString("InChambers")!;
        services.AddDbContext<InChambersContext>(
            (sp, options) => options
                .UseSqlServer(connectionString, b => b.MigrationsAssembly("InChambers.Api"))
                .AddInterceptors(
                    sp.GetRequiredService<SoftDeleteInterceptor>())
                .LogTo(Console.WriteLine, LogLevel.Information));

        // Add fluent validation.
        services.AddValidatorsFromAssembly(Assembly.Load("InChambers.Core"));
        services.AddFluentValidationAutoValidation(fluentConfig =>
        {
            // Disable the built-in .NET model (data annotations) validation.
            fluentConfig.DisableBuiltInModelValidation = true;

            // Enable validation for parameters bound from `BindingSource.Form` binding sources.
            fluentConfig.EnableFormBindingSourceAutomaticValidation = true;

            // Enable validation for parameters bound from `BindingSource.Path` binding sources.
            fluentConfig.EnablePathBindingSourceAutomaticValidation = true;

            // Enable validation for parameters bound from 'BindingSource.Custom' binding sources.
            fluentConfig.EnableCustomBindingSourceAutomaticValidation = true;

            // Replace the default result factory with a custom implementation.
            fluentConfig.OverrideDefaultResultFactoryWith<CustomResultFactory>();
        });

        services.AddHttpContextAccessor();

        services.AddLazyCache();

        services.AddAuthentication(option =>
        {
            option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["JwtConfig:Issuer"],
                ValidAudience = configuration["JwtConfig:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtConfig:Secret"]!)),
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .Build();
        });

        // Add HTTP clients
        services.AddHttpClient(HttpClientKeys.ApiVideo, client =>
        {
            string baseAddress = configuration["AppConfig:ApiVideo:BaseUrl"]!;

            client.BaseAddress = new Uri(baseAddress);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        })
        .AddHttpMessageHandler<ApiVideoHttpHandler>()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
            {
                AllowAutoRedirect = false,
                UseDefaultCredentials = true
            });

        // Set up Paystack HttpClient
        string paystackHttpClientName = configuration["Paystack:HttpClientName"]!;
        ArgumentException.ThrowIfNullOrEmpty(paystackHttpClientName);

        string paystackKey = configuration["Paystack:Key"]!;
        ArgumentException.ThrowIfNullOrEmpty(paystackKey);

        // Configure Paystack HttpClient
        services.AddHttpClient(
            paystackHttpClientName,
            client =>
            {
                // Set the base address of the named client.
                client.BaseAddress = new Uri("https://api.paystack.co/");

                // Add a user-agent default request header.
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", paystackKey);
            });

        // Mapster global Setting. This can also be overwritten per transform
        TypeAdapterConfig.GlobalSettings.Default
            .NameMatchingStrategy(NameMatchingStrategy.IgnoreCase)
            .IgnoreNullValues(true)
            .AddDestinationTransform((string x) => x.Trim())
            .AddDestinationTransform((string x) => x ?? "")
            .AddDestinationTransform(DestinationTransform.EmptyCollectionIfNull);

        // map user models
        TypeAdapterConfig<User, UserProfileView>
            .NewConfig()
            .Map(dest => dest.LastLoginDate, src => src.Login.CreatedAtUtc)
            .Map(dest => dest.Roles, src => src.UserRoles != null ? src.UserRoles.Select(ur => ur.Role!.Name) : new List<string>());

        // map courses models
        TypeAdapterConfig<CourseModel, Course>
            .NewConfig()
            .Map(dest => dest.Tags, src => string.Join(",", src.Tags));

        TypeAdapterConfig<Course, CourseView>
            .NewConfig()
            .Map(dest => dest.ThumbnailUrl, src => src.Video != null ? src.Video.ThumbnailUrl : "")
            .Map(dest => dest.Duration, src => src.Video == null ? "" : TimeSpan.FromSeconds(src.Video.VideoDuration).ToString("hh\\:mm\\:ss"));

        TypeAdapterConfig<Course, CourseDetailView>
            .NewConfig()
            .Map(dest => dest.Tags, src => src.Tags.Split(",", StringSplitOptions.None).ToList())
            .Map(dest => dest.VideoIsUploaded, src => src.Video != null ? src.Video.IsUploaded : false)
            .Map(dest => dest.ThumbnailUrl, src => src.Video != null ? src.Video.ThumbnailUrl : "")
            .Map(dest => dest.PreviewVideoId, src => src.Video != null ? src.Video.PreviewVideoId : null)
            .Map(dest => dest.Duration, src => src.Video == null ? "" : TimeSpan.FromSeconds(src.Video.VideoDuration).ToString("hh\\:mm\\:ss"))
            .Ignore(dest => dest.Resources);

        TypeAdapterConfig<CoursePrice, PriceView>
            .NewConfig()
            .Map(dest => dest.Name, src => src.Duration!.Name);

        TypeAdapterConfig<CourseDocument, DocumentView>
            .NewConfig()
            .Map(dest => dest.Name, src => src.Document!.Name)
            .Map(dest => dest.Type, src => src.Document!.Type)
            .Map(dest => dest.Url, src => src.Document!.Url)
            .Map(dest => dest.ThumbnailUrl, src => src.Document!.ThumbnailUrl);

        // map series models
        TypeAdapterConfig<SeriesModel, Series>
            .NewConfig()
            .Map(dest => dest.Tags, src => string.Join(",", src.Tags));

        TypeAdapterConfig<SeriesPrice, PriceView>
           .NewConfig()
           .Map(dest => dest.Name, src => src.Duration!.Name);

        TypeAdapterConfig<Series, SeriesDetailView>
            .NewConfig()
            .Map(dest => dest.Tags, src => src!.Tags.Split(",", StringSplitOptions.None).ToList())
            .Map(dest => dest.Duration, src => Utilities.Extensions.FormatDuration(TimeSpan.FromSeconds(src.Courses
                        .Where(c => !c.IsDeleted)
                        .Select(c => c.Course!.Video != null ? c.Course.Video.VideoDuration : 0)
                        .Sum())));

        TypeAdapterConfig<SeriesCourse, SeriesCouresView>
            .NewConfig()
            .Map(dest => dest.Id, src => src.CourseId)
            .Map(dest => dest.Uid, src => src.Course!.Uid)
            .Map(dest => dest.Title, src => src.Course!.Title)
            .Map(dest => dest.Summary, src => src.Course!.Summary);

        TypeAdapterConfig<SeriesQuestion, SeriesQuestionView>
            .NewConfig()
            .Map(dest => dest.Options, src => src.Options.Where(opt => !opt.IsDeleted).Adapt<List<QuestionOptionView>>());

        // map sme hubs
        TypeAdapterConfig<SmeHubModel, SmeHub>
            .NewConfig()
            .Map(dest => dest.Tags, src => string.Join(",", src.Tags));


        TypeAdapterConfig<SmeHub, SmeHubDetailView>
            .NewConfig()
            .Map(dest => dest.Tags, src => src.Tags.Split(",", StringSplitOptions.None).ToList());

        // map sme hubs
        TypeAdapterConfig<AnnotatedAgreementModel, AnnotatedAgreement>
            .NewConfig()
            .Map(dest => dest.Tags, src => string.Join(",", src.Tags));


        TypeAdapterConfig<AnnotatedAgreement, AnnotatedAgreementDetailView>
            .NewConfig()
            .Map(dest => dest.Tags, src => src.Tags.Split(",", StringSplitOptions.None).ToList());

        services.AddSingleton<ICacheService, CacheService>();

        services.TryAddScoped<SoftDeleteInterceptor>();
        services.TryAddScoped<UserSession>();
        services.TryAddScoped<ITokenHandler, Services.TokenHandler>();
        services.TryAddScoped<IFileService, FileService>();
        services.TryAddScoped<IEmailService, EmailService>();

        services.TryAddTransient<IAuthService, AuthService>();
        services.TryAddTransient<ICourseService, CourseService>();
        services.TryAddTransient<IConfigService, ConfigService>();
        services.TryAddTransient<ApiVideoHttpHandler>();
        services.TryAddTransient<IVideoService, VideoService>();
        services.TryAddTransient<IQuestionService, QuestionService>();
        services.TryAddTransient<ISeriesService, SeriesService>();
        services.TryAddTransient<ISmeHubService, SmeHubService>();
        services.TryAddTransient<IOrderService, OrderService>();
        services.TryAddTransient<IUserService, UserService>();
        services.TryAddTransient<IAnnotatedAgreementService, AnnotatedAgreementService>();

        return services;
    }
}