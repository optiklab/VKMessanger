
namespace SlXnaApp1.Infrastructure
{
    public class LocalizedStrings
    {
        public AppResources LocalizedResources
        {
            get
            {
                return _localizedResources;
            }
        }

        private static AppResources _localizedResources = new AppResources();
    }
}
