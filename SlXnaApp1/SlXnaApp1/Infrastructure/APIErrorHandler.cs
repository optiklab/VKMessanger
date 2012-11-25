
using SlXnaApp1.Api;
using SlXnaApp1.Json;
namespace SlXnaApp1.Infrastructure
{
    public class APIErrorHandler
    {
        public string LastErrorMessage
        {
            get;
            private set;
        }

        public int LastMethodErrorCode
        {
            get;
            private set;
        }

        public string LastCaptchaImage
        {
            get;
            private set;
        }

        public string LastCaptchaSid
        {
            get;
            private set;
        }

        public bool IsHandled
        {
            get;
            set;
        }

        public void SaveError(APIError errorCode)
        {
            LastMethodErrorCode = errorCode.ErrorDescription.error_code;
            LastErrorMessage = errorCode.ErrorDescription.error_msg;
            LastCaptchaSid = errorCode.ErrorDescription.captcha_sid;
            LastCaptchaImage = errorCode.ErrorDescription.captcha_img;

            switch (errorCode.ErrorDescription.error_code)
            {
                case 1:
                    //Unknown error occurred.
                    break;
                case 2:
                    //Application is disabled. Enable your application or use test mode.
                    break;
                case 4:
                    //Incorrect signature.
                    break;
                case 5:
                    //User authorization failed.
                    break;
                case 6:
                    //Too many requests per second.
                    break;
                case 7:
                    //Permission to perform this action is denied by user
                    break;
                case 9:
                    //Flood control: message with same guid already sent. 
                    break;
                case 10:
                    //Internal server error.
                    break;
                case 14:
                    //Captcha is needed
                    break;
                case 15:
                    //Access denied: can't add this user
                    break;
                case 100:
                    //One of the parameters specified was missing or invalid 
                    break;
                case 103:
                    //Out of limits
                    break;
                case 113:
                    //Invalid user id.
                    break;
                case 118:
                    //Invalid server.
                    break;
                case 121:
                    //Invalid hash.
                    break;
                case 122:
                    //Invalid photos list.
                    break;
                case 129:
                    //Invalid profile photo.
                    break;
                case 174:
                    //Cannot add yourself to friends.
                    break;
                case 175:
                    //Cannot add this user to friends as they have put you on their blacklist.
                    break;
                case 176:
                    //Cannot add this user to list of friends due to privacy settings.
                    break;
                case 300:
                    //This album is full.
                    break;
                case 1003:
                    //User already invited: message already sent, you can resend message in 300 seconds
                    break;
                case 1004:
                    //This phone used by another user
                    break;
                case 1112:
                    //Processing.. Try later 
                    break;
                case 10007:
                    //Operation denied by user. (В случае использования метода VK.api) 
                    break;
                default:
                    break;
            }
        }
    }
}
