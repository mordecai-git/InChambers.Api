using System.Net;
using InChambers.Core.Models.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace InChambers.Api.Controllers;

[ApiController]
public class BaseController : ControllerBase
{
    /// <summary>
    /// Returns the appropriate HTTP Response.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <returns></returns>
    protected IActionResult ProcessResponse(Result result)
    {
        if (result.Success)
        {
            if (result.Status == StatusCodes.Status201Created)
            {
                return StatusCode(StatusCodes.Status201Created, result);
            }

            return Ok(result);
        }
        else if (result.Status == StatusCodes.Status401Unauthorized)
        {
            return Unauthorized(result);
        }
        else if (result.Status == StatusCodes.Status403Forbidden)
        {
            return StatusCode((int)HttpStatusCode.Forbidden, result);
        }
        else if (result.Status == StatusCodes.Status404NotFound)
        {
            return NotFound(result);
        }
        else if (result.Status == StatusCodes.Status500InternalServerError)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, result);
        }
        else
        {
            return BadRequest(result);
        }
    }
}