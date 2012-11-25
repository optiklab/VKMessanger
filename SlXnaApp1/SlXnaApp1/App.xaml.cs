using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SlXnaApp1.Api;
using SlXnaApp1.Infrastructure;
using SlXnaApp1.Services;
using Microsoft.Phone.Tasks;
using System.Threading;
using System.Diagnostics;

namespace SlXnaApp1
{
    public partial class App : Application
    {
        #region Constructor

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            // Global handler for uncaught exceptions. 
            UnhandledException += Application_UnhandledException;

            // Standard Silverlight initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();

            PhotoGallery = new PhotoChooserTask();
            PhotoGallery.ShowCamera = true;

            // !!!!!!!!! FOR TESTING CONTACTS.
            //SavePhoneNumberTask savePhoneNumberTask = new SavePhoneNumberTask();
            //savePhoneNumberTask.PhoneNumber = "89289541260";
            //savePhoneNumberTask.Show();

            _settings = new Settings(new ProtectDataAdapter());

            if (!_settings.IsPushOn)
            {
                // If timer has gone, so we can turn on Push notifications.
                if (DateTime.Now >= _settings.PushTurnOnTime)
                {
                    _settings.IsPushOn = true;
                    App.Current.PushNotificationsSwitchTimer.Dispose();
                }
                else
                {
                    // Turn on push notifications when time occurs.
                    TimeSpan temp = _settings.PushTurnOnTime - DateTime.Now;

                    PushNotificationsSwitchTimer = new Timer(state =>
                    {
                        _settings.IsPushOn = true;
                        App.Current.PushNotificationsSwitchTimer.Dispose();
                    }, null, Convert.ToInt32(temp.TotalMilliseconds), -1);
                }
            }
        }

        #endregion

        #region SDK Public properties

        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public PhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>
        /// Provides access to a ContentManager for the application.
        /// </summary>
        public ContentManager Content { get; private set; }

        /// <summary>
        /// Provides access to a GameTimer that is set up to pump the FrameworkDispatcher.
        /// </summary>
        public GameTimer FrameworkDispatcherTimer { get; private set; }

        /// <summary>
        /// Provides access to the AppServiceProvider for the application.
        /// </summary>
        public AppServiceProvider Services { get; private set; }

        #endregion

        #region App public properties

        public new static App Current
        {
            get { return (App)Application.Current; }
        }

        public PushNotifications PushNotifications { get; set; }

        public LongPollService LongPollService { get; set; }

        public EntityService EntityService { get; set; }

        public APIErrorHandler APIErrorHandler { get; set; }

        public UpdatesService UpdatesService { get; set; }

        public PhotoChooserTask PhotoGallery
        {
            private set;
            get;
        }

        #endregion

        #region Settings public properties

        public bool IsFirstRun
        {
            get { return _settings.IsFirstRun; }
            set { _settings.IsFirstRun = value; }
        }

        public bool IsSoundOn
        {
            get { return _settings.IsSoundOn; }
            set { _settings.IsSoundOn = value; }
        }

        public bool IsVibrationOn
        {
            get { return _settings.IsVibrationOn; }
            set { _settings.IsVibrationOn = value; }
        }

        public bool IsToastOn
        {
            get { return _settings.IsToastOn; }
            set { _settings.IsToastOn = value; }
        }

        public bool IsPushOn
        {
            get { return _settings.IsPushOn; }
            set { _settings.IsPushOn = value; }
        }

        public DateTime PushTurnOnTime
        {
            get { return _settings.PushTurnOnTime; }
            set { _settings.PushTurnOnTime = value; }
        }

        public DateTime LastContactsSync
        {
            get { return _settings.LastContactsSync; }
            set { _settings.LastContactsSync = value; }
        }

        #endregion

        #region Additional public properties

        public SolidColorBrush BlueBrush
        {
            get;
            private set;
        }

        public SolidColorBrush GrayBrush
        {
            get
            {
                return GRAY_BRUSH;
            }
        }

        public SolidColorBrush BlackBrush
        {
            get
            {
                return BLACK_BRUSH;
            }
        }

        public SolidColorBrush WhiteBrush
        {
            get
            {
                return WHITE_BRUSH;
            }
        }

        public SolidColorBrush AntiPhoneBackgroundBrush
        {
            get
            {
                Visibility darkBackgroundVisibility = (Visibility)Application.Current.Resources["PhoneDarkThemeVisibility"];

                if (darkBackgroundVisibility == Visibility.Visible)
                    return WHITE_BRUSH;
                else
                    return BLACK_BRUSH;
            }
        }

        public Timer PushNotificationsSwitchTimer { get; set; }

        #endregion

        #region Event handlers

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
            BlueBrush = (SolidColorBrush)Application.Current.Resources["PhoneAccentBrush"]; //BlueColorBrush"];

            APIErrorHandler = new APIErrorHandler();
            LongPollService = new LongPollService();
            PushNotifications = new PushNotifications();
            UpdatesService = new UpdatesService();
            EntityService = new EntityService();
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            if (LongPollService == null)
                LongPollService = new LongPollService();

            if (PushNotifications == null)
                PushNotifications = new PushNotifications();

            if (APIErrorHandler == null)
                APIErrorHandler = new APIErrorHandler();

            if (UpdatesService == null)
                UpdatesService = new UpdatesService();

            if (EntityService == null)
                EntityService = new EntityService();

            if (!_settings.IsPushOn)
            {
                // If timer has gone, so we can turn on Push notifications.
                if (DateTime.Now >= _settings.PushTurnOnTime)
                {
                    _settings.IsPushOn = true;
                    App.Current.PushNotificationsSwitchTimer.Dispose();
                }
                else
                {
                    // Turn on push notifications when time occurs.
                    TimeSpan temp = _settings.PushTurnOnTime - DateTime.Now;

                    PushNotificationsSwitchTimer = new Timer(state =>
                    {
                        _settings.IsPushOn = true;
                        App.Current.PushNotificationsSwitchTimer.Dispose();
                    }, null, Convert.ToInt32(temp.TotalMilliseconds), -1);
                }
            }
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        #endregion

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new PhoneApplicationFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;

            GlobalIndicator.Instance.Initialize(RootFrame);
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        #endregion

        #region Private fields

        private Settings _settings = null;
        private EmailComposeTask _emailComposeTask = new EmailComposeTask();
        private static SolidColorBrush GRAY_BRUSH = new SolidColorBrush(Colors.Gray);
        private static SolidColorBrush BLACK_BRUSH = new SolidColorBrush(Colors.Black);
        private static SolidColorBrush WHITE_BRUSH = new SolidColorBrush(Colors.White);

        #endregion
    }
}