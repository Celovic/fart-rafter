using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;

namespace FartRafters
{
    public static class OnlineServices
    {
        public enum Status
        {
            Idle,
            Connecting,
            Ready,
            NotConfigured,
            Offline
        }

        static Task<bool> pending;

        public static Status State { get; private set; } = Status.Idle;
        public static string Message { get; private set; } = "";
        public static bool Ready => State == Status.Ready;
        public static string PlayerId => Ready ? AuthenticationService.Instance.PlayerId : "";
        public static event Action StateChanged;

        public static bool Configured => !string.IsNullOrEmpty(Application.cloudProjectId);

        public static Task<bool> EnsureReady()
        {
            if (Ready)
            {
                return Task.FromResult(true);
            }
            if (pending == null || pending.IsCompleted)
            {
                pending = Connect();
            }
            return pending;
        }

        static async Task<bool> Connect()
        {
            if (!Configured)
            {
                SetState(Status.NotConfigured, "Hesap ayarı bekleniyor: oyun henüz Unity Cloud projesine bağlanmadı.");
                return false;
            }
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                SetState(Status.Offline, "İnternet bağlantısı yok.");
                return false;
            }
            SetState(Status.Connecting, "Bağlanıyor…");
            try
            {
                if (UnityServices.State != ServicesInitializationState.Initialized)
                {
                    var options = new InitializationOptions().SetEnvironmentName(OnlineConfig.Instance.environment);
                    string[] args = Environment.GetCommandLineArgs();
                    for (int i = 0; i < args.Length - 1; i++)
                    {
                        if (args[i] == "-netprofile")
                        {
                            options.SetProfile(args[i + 1]);
                        }
                    }
                    await UnityServices.InitializeAsync(options);
                }
                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                }
                await SyncName();
                SetState(Status.Ready, "");
                ScoreSync.Flush();
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[FartRafters] Online services: " + exception.Message);
                SetState(Status.Offline, "Bağlantı kurulamadı. Biraz sonra tekrar dene.");
                return false;
            }
        }

        public static async Task SyncName()
        {
            if (!AuthenticationService.Instance.IsSignedIn || !PlayerProfile.HasName)
            {
                return;
            }
            string wanted = NameGenerator.ToServiceName(PlayerProfile.DisplayName);
            string current = AuthenticationService.Instance.PlayerName ?? "";
            int hash = current.LastIndexOf('#');
            string body = hash > 0 ? current.Substring(0, hash) : current;
            if (string.IsNullOrEmpty(wanted) || body == wanted)
            {
                return;
            }
            try
            {
                await AuthenticationService.Instance.UpdatePlayerNameAsync(wanted);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[FartRafters] Player name: " + exception.Message);
            }
        }

        public static string ServiceName => Ready ? AuthenticationService.Instance.PlayerName ?? "" : "";

        public static async Task<bool> DeleteAccount()
        {
            bool ok = true;
            if (Ready)
            {
                try
                {
                    await FriendsClient.Shutdown();
                    await AuthenticationService.Instance.DeleteAccountAsync();
                }
                catch (Exception exception)
                {
                    Debug.LogWarning("[FartRafters] Delete account: " + exception.Message);
                    ok = false;
                }
            }
            if (ok)
            {
                ProgressStore.ClearAll();
                SetState(Status.Idle, "");
            }
            return ok;
        }

        static void SetState(Status state, string message)
        {
            State = state;
            Message = message;
            StateChanged?.Invoke();
        }
    }
}
