using SAP.Net.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Offline.Odata
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 


    public static class Settings
    {
        public static string AppID => "APP_ID";
        public static string ServerHost => "HOST";
        public static bool isHttps => true;
        public static ushort Port => 443;
        public static string User => "SUSER";
        public static string Password => "PASSWORD";
        public static string PassCodeVault => "$S3NH4MU1T0F0D4$";
    }


    public sealed partial class MainPage : Page
    {

        SAP.Logon.Core.RegistrationContext registrationContext;


        public MainPage()
        {
            this.InitializeComponent();
            Start();
        }

        public async void Start()
        {
            await Register();
            if (registrationContext != null)
                await OpenStore();
        }


        public async Task Register(bool register = false)
        {

            try
            {
                var logonCore = await SAP.Logon.Core.LogonCore.InitWithApplicationIdAsync(Settings.AppID);

                if (register)
                {
                    var logonContext = new SAP.Logon.Core.LogonContext
                    {
                        RegistrationContext = new SAP.Logon.Core.RegistrationContext
                        {
                            ApplicationId = Settings.AppID,
                            ServerHost = Settings.ServerHost,
                            IsHttps = Settings.isHttps,
                            ServerPort = Settings.Port,
                            BackendUserName = Settings.User,
                            BackendPassword = Settings.Password
                        }
                    };

                    // registers the device
                    await logonCore.RegisterWithContextAsync(logonContext);
                    // persist locally
                    await logonCore.PersistRegistrationAsync(Settings.PassCodeVault, logonContext);

                    registrationContext = logonContext.RegistrationContext;

                }

                if (logonCore.State.IsRegistered)
                {
                    await logonCore.UnlockSecureStoreAsync(Settings.PassCodeVault);

                    registrationContext = logonCore.LogonContext.RegistrationContext;

                }
                else
                {
                    Register(true);
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }


        public async Task OpenStore()
        {

            try
            {

                /*
                  
                 The line  var store = new SAP.Data.OData.Offline.Store.ODataOfflineStore(); is giving the following exception
                 
                The specified module could not be found.(Exception from HRESULT: 0x8007007E)

                   at System.StubHelpers.StubHelpers.GetWinRTFactoryObject(IntPtr pCPCMD)
                   at lodatawinrt.RequestFailureContext..ctor(RequestFailureDelegate delegate, Object data)
                   at SAP.Data.OData.Offline.Store.ODataOfflineStore..ctor()
                   at Offline.Odata.MainPage.<OpenStore>d__4.MoveNext()

   
                   */

                var store = new SAP.Data.OData.Offline.Store.ODataOfflineStore();
                var options = new SAP.Data.OData.Offline.Store.ODataOfflineStoreOptions();

                HttpClient httpClient = new HttpClient();

                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-SMP-APPCID", registrationContext.CommunicatorId);
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-SUP-APPCID", registrationContext.CommunicatorId);

                httpClient.ShouldHandleXcsrfToken = true;
                options.ConversationManager = httpClient;
                options.Host = Settings.ServerHost;
                options.Port = Settings.Port;
                options.ServiceRoot = Settings.AppID;
                options.EnableHttps = Settings.isHttps;
                options.StoreName = "SFA_MVP_OFFLINE_DEMO";
                options.StoreEncryptionKey = Settings.PassCodeVault;
                options.URLSuffix = "";
                options.EnableRepeatableRequests = true;

                await store.OpenAsync(options);


            }
            catch (Exception ex)
            {
                throw ex;
            }

           

        }

    }
}
