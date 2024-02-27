using Core.DMS.Objects.DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Core.DMS.Api.Authentication
{
    public class SSOAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, 
        UrlEncoder encoder, 
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration) 
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var authorizationHeader = httpContextAccessor.HttpContext.Request.Headers.Authorization;

            if (authorizationHeader.Count == 0)
            {
                var guestIdentity = new CoreAuthInfo() { Name = "Guest", AuthenticationType = "bearer", IsAuthenticated = true };

                return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(guestIdentity), "bearer"));
            }

            using var httpClient = new HttpClient { BaseAddress = new Uri(configuration.GetConnectionString("SSO")) };

            string bearerValue = httpContextAccessor.HttpContext.Request.Headers.Authorization.ToString().Split(" ").Last();

            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", bearerValue);

            var result = await httpClient.GetFromJsonAsync<CoreUser>("Core/User/Me()");

            var identity = new CoreAuthInfo() { Name = result.Id, AuthenticationType = "bearer", IsAuthenticated = true };

            return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), "bearer"));
        }
    }
}
