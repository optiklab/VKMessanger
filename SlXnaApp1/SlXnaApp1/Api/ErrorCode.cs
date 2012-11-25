
namespace SlXnaApp1.Api
{
    internal enum ErrorCode
    {
        AUTH_SUCCESS = 0,
        AUTH_ERROR_NETWORK = 1,
        AUTH_ERROR_INCORRECT_PASSWORD = 2,
        AUTH_CANCELED = 3,
        INCORRECT_RESPONSE = 4,
        REAUTH_INVALID_CLIENT = 5,
        NEED_CAPTCHA = 6,
        UNKNOWN_ERROR = 7
    };
}
