using System.Text;
using System.IO.IsolatedStorage;
using System.Diagnostics;
using System;

namespace SlXnaApp1.Infrastructure
{
    public class Settings
    {
        #region Constructor

        public Settings(IProtectData protectDataAdapter)
        {
            this.protectDataAdapter = protectDataAdapter;
            isolatedStore = IsolatedStorageSettings.ApplicationSettings;
            encoding = new UTF8Encoding();
        }

        #endregion

        #region Public properties

        public bool IsFirstRun
        {
            get
            {
                // Default is TRUE, so when you first time get this flag - it is true...
                // until you write some value explicitly
                return GetValueOrDefault(IsFirstRunKeyName, true);
            }
            set
            {
                AddOrUpdateValue(IsFirstRunKeyName, value);
            }
        }

        public bool IsSoundOn
        {
            get { return GetValueOrDefault(IsSoundOnKeyName, true); }
            set { AddOrUpdateValue(IsSoundOnKeyName, value); }
        }

        public bool IsVibrationOn
        {
            get { return GetValueOrDefault(IsVibrationOnKeyName, true); }
            set { AddOrUpdateValue(IsVibrationOnKeyName, value); }
        }

        public bool IsToastOn
        {
            get { return GetValueOrDefault(IsToastOnKeyName, true); }
            set { AddOrUpdateValue(IsToastOnKeyName, value); }
        }

        public bool IsPushOn
        {
            get { return GetValueOrDefault(IsPushOnKeyName, true); }
            set { AddOrUpdateValue(IsPushOnKeyName, value); }
        }

        public DateTime PushTurnOnTime
        {
            get
            {
                return GetValueOrDefault(PushTurnOnTimeKeyName, DateTime.MinValue);
            }
            set
            {
                AddOrUpdateValue(PushTurnOnTimeKeyName, value);
            }
        }

        public DateTime LastContactsSync
        {
            get
            {
                return GetValueOrDefault(LastContactsSyncKeyName, DateTime.MinValue);
            }
            set
            {
                AddOrUpdateValue(LastContactsSyncKeyName, value);
            }
        }

        public string Password
        {
            get
            {
                return PasswordByteArray.Length == 0 ? string.Empty : 
                    encoding.GetString(PasswordByteArray, 0, PasswordByteArray.Length);
            }
            set
            {
                PasswordByteArray = encoding.GetBytes(value);
            }
        }

        public string UserName
        {
            get
            {
                return GetValueOrDefault(UserNameKeyName, string.Empty);
            }
            set
            {
                AddOrUpdateValue(UserNameKeyName, value);
            }
        }

        public string UserId
        {
            get
            {
                return GetValueOrDefault(UserIdKeyName, string.Empty);
            }
            set
            {
                AddOrUpdateValue(UserIdKeyName, value);
            }
        }

        public string AccessToken
        {
            get
            {
                return GetValueOrDefault(AccessTokenKeyName, string.Empty);
            }
            set
            {
                AddOrUpdateValue(AccessTokenKeyName, value);
            }
        }

        public string Secret
        {
            get
            {
                return GetValueOrDefault(SecretKeyName, string.Empty);
            }
            set
            {
                AddOrUpdateValue(SecretKeyName, value);
            }
        }

        public int Ts
        {
            get
            {
                return GetValueOrDefault(TsKeyName, -1);
            }
            set
            {
                AddOrUpdateValue(TsKeyName, value);
            }
        }

        public string LongPollServerKey
        {
            get
            {
                return GetValueOrDefault(ServerKeyName, string.Empty);
            }
            set
            {
                AddOrUpdateValue(ServerKeyName, value);
            }
        }

        public string LongPollServerUri
        {
            get
            {
                return GetValueOrDefault(ServerUriName, string.Empty);
            }
            set
            {
                AddOrUpdateValue(ServerUriName, value);
            }
        }

        #endregion

        #region Private properties

        private byte[] PasswordByteArray
        {
            get
            {
                byte[] encryptedValue = GetValueOrDefault(PasswordSettingKeyName, new byte[0]);

                if (encryptedValue.Length == 0)
                    return new byte[0];
                else
                    return protectDataAdapter.Unprotect(encryptedValue, null);
            }
            set
            {
                byte[] encryptedValue = protectDataAdapter.Protect(value, null);
                AddOrUpdateValue(PasswordSettingKeyName, encryptedValue);
            }
        }

        #endregion

        #region Private methods

        private void AddOrUpdateValue(string key, object value)
        {
            try
            {
                if (this.isolatedStore.Contains(key))
                {
                    // If the new value is different, set the new value.
                    if (this.isolatedStore[key] != value)
                    {
                        this.isolatedStore[key] = value;
                        isolatedStore.Save();
                    }
                }
                else
                {
                    this.isolatedStore.Add(key, value);
                    isolatedStore.Save();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("AddOrUpdateValue failed:" + ex.Message);
            }
        }

        private T GetValueOrDefault<T>(string key, T defaultValue)
        {
            T value = default(T);

            try
            {
                if (this.isolatedStore.Contains(key))
                    value = (T)this.isolatedStore[key];
                else
                    value = defaultValue;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetValueOrDefault failed:" + ex.Message);
            }

            return value;
        }

        #endregion

        #region Private constants

        private const string IsPushOnKeyName = "IsPushOn";
        private const string IsVibrationOnKeyName = "IsVibrationOn";
        private const string IsToastOnKeyName = "IsToastOn";
        private const string IsSoundOnKeyName = "IsSoundOn";
        private const string PushTurnOnTimeKeyName = "PushTurnOnTime";
        private const string LastContactsSyncKeyName = "LastContactsSync";
        private const string UserNameKeyName = "UserName";
        private const string IsFirstRunKeyName = "IsFirstRun";
        private const string PasswordSettingKeyName = "Password";
        private const string UserIdKeyName = "UserId";
        private const string AccessTokenKeyName = "AccessToken";
        private const string SecretKeyName = "Secret";
        private const string TsKeyName = "Ts";
        private const string ServerKeyName = "Key";
        private const string ServerUriName = "Server";

        #endregion

        #region Private fields

        private readonly IProtectData protectDataAdapter;
        private readonly IsolatedStorageSettings isolatedStore;
        private UTF8Encoding encoding;

        #endregion
    }
}
