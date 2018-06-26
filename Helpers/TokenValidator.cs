using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace MjIot.Client.WebApi.Helpers
{
    public class TokenValidator : DelegatingHandler
    {
        public TokenValidator()
        {
            _certificateLoader = new LocalCertificateLoader();
        }

        private ICertificateLoader _certificateLoader;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            //PREFLIGHT CALL
            if (request.Method.Method == "OPTIONS")
                return base.SendAsync(request, cancellationToken);


            //determine whether a jwt exists or not
            string token;
            if (!TryRetrieveToken(request, out token))
            {
                return Task<HttpResponseMessage>.Factory.StartNew(() => new HttpResponseMessage(HttpStatusCode.Unauthorized) { });
                //allow requests with no token - whether a action method needs an authentication can be set with the claimsauthorization attribute
                //return base.SendAsync(request, cancellationToken);
            }

            //try
            //{
            //    const string sec = "401b09eab3c013d4ca54922bb802bec8fd5318192b0a75f201d8b3727429090fb337591abd3e44453b954555b7a0812e1081c39b740293f765eae731f5a65ed1";
            //    var now = DateTime.UtcNow;
            //    var securityKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.Default.GetBytes(sec));


            //    SecurityToken securityToken;
            //    JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            //    TokenValidationParameters validationParameters = new TokenValidationParameters()
            //    {
            //        ValidAudience = "http://localhost:50191",
            //        ValidIssuer = "http://localhost:50191",
            //        ValidateLifetime = true,
            //        ValidateIssuerSigningKey = true,
            //        LifetimeValidator = this.LifetimeValidator,
            //        IssuerSigningKey = securityKey
            //    };
            //    //extract and assign the user of the jwt
            //    Thread.CurrentPrincipal = handler.ValidateToken(token, validationParameters, out securityToken);
            //    HttpContext.Current.User = handler.ValidateToken(token, validationParameters, out securityToken);

            //    return base.SendAsync(request, cancellationToken);
            //}
            //catch (SecurityTokenValidationException e)
            //{
            //    statusCode = HttpStatusCode.Unauthorized;
            //}
            //catch (Exception ex)
            //{
            //    statusCode = HttpStatusCode.InternalServerError;
            //}

            //request.Properties.Add("userId", 1);

            var publicKey = _certificateLoader.LoadCertificate().PublicKey.Key as RSACryptoServiceProvider;

            try
            {
                string jsonString = Jose.JWT.Decode(token, publicKey);
                var json = JObject.Parse(jsonString);
                var userId = json["sub"];
                request.Properties.Add("userId", userId);
            }
            catch (Exception e)
            {
                return Task<HttpResponseMessage>.Factory.StartNew(() => new HttpResponseMessage(HttpStatusCode.Unauthorized) { });
            }

            //statusCode = HttpStatusCode.OK;

            return base.SendAsync(request, cancellationToken);
        }

        private static bool TryRetrieveToken(HttpRequestMessage request, out string token)
        {
            token = null;
            IEnumerable<string> authzHeaders;
            if (!request.Headers.TryGetValues("Authorization", out authzHeaders) || authzHeaders.Count() > 1)
            {
                return false;
            }
            var bearerToken = authzHeaders.ElementAt(0);
            token = bearerToken.StartsWith("Bearer ") ? bearerToken.Substring(7) : bearerToken;
            return true;
        }
    }

    public interface ICertificateLoader
    {
        X509Certificate2 LoadCertificate();
    }

    public class LocalCertificateLoader : ICertificateLoader
    {
        private string _certificatePath;

        public LocalCertificateLoader()
        {
            _certificatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Certificates", "public.crt");
        }

        public X509Certificate2 LoadCertificate()
        {
            var cert = new X509Certificate2(_certificatePath);
            return cert;
        }
    }
}