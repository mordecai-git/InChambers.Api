﻿using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using InChambers.Core.Interfaces;
using InChambers.Core.Models.Configurations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;

namespace InChambers.Core.Middlewares;

public class JWTMiddleware
{
    private readonly RequestDelegate _next;
    private ITokenHandler _tokenHandler;

    public JWTMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, IOptions<JwtConfig> jwtConfig, ITokenHandler tokenHandler)
    {
        _tokenHandler = tokenHandler ?? throw new ArgumentNullException(nameof(tokenHandler));

        // continue if action called is anonymous.
        if (IsAnonymous(context))
        {
            await _next(context);
            return;
        }

        // get the token
        string token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        // continue if token is null
        if (token == null)
        {
            await _next(context);
            return;
        }

        // attach the token to the request
        if (await AttachAccountToContext(context, token, jwtConfig.Value))
        {
            await _next(context);
        }
    }

    private static bool IsAnonymous(HttpContext context)
    {
        // Check if the request is handled by an MVC endpoint
        var endpoint = context.GetEndpoint();
        if (endpoint is RouteEndpoint routeEndpoint)
        {
            // Check if the action method is decorated with AllowAnonymous attribute
            var actionDescriptor = routeEndpoint.Metadata.GetMetadata<ControllerActionDescriptor>();

            bool? methodAllowAnonymousAttribute =
                actionDescriptor?.MethodInfo.GetCustomAttributes(inherit: true)
                .OfType<AllowAnonymousAttribute>().Any();

            bool actionIsAnonymous = methodAllowAnonymousAttribute.HasValue && methodAllowAnonymousAttribute.Value;

            return actionIsAnonymous;
        }

        return false;
    }

    private async Task<bool> AttachAccountToContext(HttpContext context, string token, JwtConfig jwtConfig)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            byte[] key = Encoding.ASCII.GetBytes(jwtConfig.Secret);
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = jwtConfig.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtConfig.Audience,
                ValidateLifetime = true,
                RequireExpirationTime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            string id = jwtToken.Claims.First(x => x.Type == "sid").Value;
            string uid = jwtToken.Claims.First(x => x.Type == "uid").Value;

            // get request domain
            string domain = context.Request.Headers["Origin"].ToString();

            //check if token is string in the cache
            bool isValid = await _tokenHandler!.ValidateToken(uid, token, domain);
            if (!isValid)
            {
                context.Items["User"] = null;
                context.User = null;
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return false;
            }
            ;

            // attach account to context on successful jwt validation
            context.Items["User"] = new
            {
                Uid = uid,
                Id = int.Parse(id),
                Roles = jwtToken.Claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value).ToList()
            };

            return true;
        }
        catch (Exception ex)
        {
            // do nothing if jwt validation fails
            // account is not attached to context so request won't have access to secure routes
            Log.Error(ex, "JWT validation failed.");
        }

        return false;
    }
}