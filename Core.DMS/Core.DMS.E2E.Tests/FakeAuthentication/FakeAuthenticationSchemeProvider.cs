using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DMS.E2E.Tests.FakeAuthentication
{
    public class FakeAuthenticationSchemeProvider : AuthenticationSchemeProvider
    {

        public FakeAuthenticationSchemeProvider(IOptions<AuthenticationOptions> options) : base(options)
        {
        }

        protected FakeAuthenticationSchemeProvider(IOptions<AuthenticationOptions> options, IDictionary<string, AuthenticationScheme> schemes) : base(options, schemes)
        {
        }

        public override Task<AuthenticationScheme> GetSchemeAsync(string name)
        {
            if (name == "bearer")
                return Task.FromResult(new AuthenticationScheme("test", "test", typeof(FakeAuthenticationHandler)));

            return base.GetSchemeAsync(name);
        }
    }
}
