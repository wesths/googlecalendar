using Google.Apis.Auth.OAuth2;

namespace GoogleCalendar.Interfaces
{
    public interface ICredentialService
    {
        ServiceAccountCredential GetServiceAccountCredentials();
    }
}