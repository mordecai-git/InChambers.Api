using InChambers.Core.Interfaces;
using InChambers.Core.Models.Utilities;
using InChambers.Core.Models.View.Config;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace InChambers.Api.Controllers;

[ApiController]
[Route("api/v1/configs")]
[Authorize]
public class ConfigsController : BaseController
{
    private readonly IConfigService _configService;

    public ConfigsController(IConfigService configService) =>
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));

    /// <summary>
    /// List all pricing durations
    /// </summary>
    /// <returns></returns>
    [HttpGet("durations")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResult<List<DurationView>>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
    public async Task<IActionResult> ListDurations()
    {
        var res = await _configService.ListDurations();
        return ProcessResponse(res);
    }

    [HttpPost("notify-me")]
    [AllowAnonymous]
    public IActionResult NotifyMeOfLunch(NotifyMeOfLunchModel model)
    {
        try
        {
            // get the file
            string path = Path.Combine(Directory.GetCurrentDirectory(), "notification-emails.txt");

            // Append the email to the file
            using (StreamWriter sw = System.IO.File.AppendText(path))
            {
                sw.WriteLine(model.Email);
            }

            var res = new
            {
                success = true,
                message = "Thank you for subscribing, we will notify you once we're up and running"
            };

            var result = new SuccessResult(res);
            return ProcessResponse(result);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unable to add email for {@Model}", model);
            var result = new ErrorResult(ex.Message);
            return ProcessResponse(result);
        }
    }
}

public class NotifyMeOfLunchModel
{
    public string Email { get; set; }
}
