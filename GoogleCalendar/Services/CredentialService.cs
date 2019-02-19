using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using GoogleCalendar.Configurations.Contracts;
using GoogleCalendar.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace GoogleCalendar.Services
{
    public class CredentialService : ICredentialService
    {
        private readonly IConfigService _configService;

        public CredentialService(IConfigService configService)
        {
            _configService = configService;
        }

        public ServiceAccountCredential GetServiceAccountCredentials()
        {
            var serviceAccountEmail = _configService.GetServiceAccountEmailAddress();
            var certSecret = _configService.GetCertificateSecret();

            var certificate = new X509Certificate2(@"key.p12", certSecret, X509KeyStorageFlags.Exportable);

            ServiceAccountCredential credential = new ServiceAccountCredential(
               new ServiceAccountCredential.Initializer(serviceAccountEmail)
               {
                   Scopes = new[] { CalendarService.Scope.CalendarReadonly, CalendarService.Scope.CalendarEvents }
               }.FromCertificate(certificate));

            return credential;

        }
    }
}
