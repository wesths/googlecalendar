using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoogleCalendar.Configurations
{
    public static class ConfigurationKeys
    {
        public static string AuthorityServiceName = "Authority";
        public static string OmegaInsuranceServiceName = "HC.RES.OmegaInsurance";
        public static string CorrespondenceServiceName = "HC.RES.Correspondence";
        public static string CustomerServiceName = "HC.ORC.Customer";
        public static string CustomerAccountServiceName = "HC.RES.CustomerAccount";
        public static string InsuranceServiceName = "HC.ORC.Insurance";
        public static string SMSServiceName = "HC.RES.SMS";
        public static string OrderServiceName = "HC.RES.Order";
        public static string EmploymentServiceName = "HC.RES.Employment";

        public static string TriggerKeyFullBenefit = "TriggerKeyFullBenefit";
        public static string TriggerKeyDeathOnly = "TriggerKeyDeathOnly";

        public static string CryptographyKey = "DecryptionKey";
        public static string ServiceSQLUserName = "SQLUserName";
        public static string ServiceSQLPassword = "SQLEncryptedPassword";
    }
}
