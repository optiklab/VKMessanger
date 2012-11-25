using System;
using System.Diagnostics;
using System.IO.IsolatedStorage;

namespace SlXnaApp1.Cache
{
    public static class CacheHelpers
    {
        // Increase Isolated Storage Quota
        public static bool GetMoreSpace(Int64 spaceReq)
        {
            bool result = false;

            try
            {
                IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
                Int64 spaceAvail = store.AvailableFreeSpace;

                if (spaceReq > spaceAvail)
                {
                    result = store.IncreaseQuotaTo(store.Quota + spaceReq);
                }

                result = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetMoreSpace error: " + ex.Message);
            }

            return result;
        }
    }
}
