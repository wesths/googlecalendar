using GoogleCalendar.Configurations.ConfigModels;
using GoogleCalendar.Configurations.Contracts;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoogleCalendar.Configurations
{
    public class ConfigService : IConfigService
    {
        protected AppSettings _appSettings { get; }
        protected IConfiguration _config { get; }
        public ConfigService(IConfiguration config,
          AppSettings appSettings)
        {
            _appSettings = appSettings;
            _config = config;
        }

        public string GetServiceAccountEmailAddress()
        {
            return _appSettings.ServiceAccountEmailAddress;
        }

        public string GetCertificateSecret()
        {
            return _appSettings.CertificateSecret;
        }

        public string GetCalenderId()
        {
            return _appSettings.CalendarId;
        }

        public int GetIntervalMinutes()
        {
            return _appSettings.IntervalMinutes;
        }
    }
}
