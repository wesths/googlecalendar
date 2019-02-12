using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoogleCalendar.Configurations.Contracts
{
    public interface IConfigService
    {
        string GetServiceAccountEmailAddress();
        string GetCertificateSecret();
        string GetCalenderId();
        
    }
}
