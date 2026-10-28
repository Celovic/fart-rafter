using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace FartRafters
{
    public class OnlineRoom : MonoBehaviour
    {
        const string CodeKey = "code";

        public static OnlineRoom Current { get; private set; }
        public static bool Active => Current != null && Current.connected;

        NetworkManager network;
        ISession session;
        bool connected;
        bool closing;

        public RoomState State { get; private set; } = new RoomState();
        public StartInfo Started { get; private set; }
        public bool IsHost { get; private set; }
        public ulong LocalClientId => network != null ? network.LocalClientId : 0;
        public bool InGame => Started != null;
        public string LastError { get; private set; } = "";

        public event Action RoomChanged;
        public event Action<string> Closed;
        public event Action<EmojiInfo> EmojiReceived;
        public event Action<string, ulong, FastBufferReader> GameMessage;

        public static OnlineRoom Ensure()
        {
            if (Current != null)
            {
                return Current;
            }
            var go = new GameObject("OnlineRoom");
            DontDestroyOnLoad(go);
            Current = go.AddComponent<OnlineRoom>();
            return Current;
        }

        void EnsureNetwork()
        {
            if (network == null)
            {
                network = NetworkManager.Singleton;
            }
            if (network == null)
            {
                var go = new GameObject("NetworkManager");
                go.SetActive(false);
                var transport = go.AddComponent<UnityTransport>();
                network = go.AddComponent<NetworkManager>();
                network.NetworkConfig = new NetworkConfig
                {
                    NetworkTransport = transport,
                    EnableSceneManagement = false,
                    ConnectionApproval = false,
                    TickRate = 30
                };
                go.SetActive(true);
            }
            network.OnClientConnectedCallback -= OnClientConnected;
            network.OnClientDisconnectCallback -= OnClientDisconnected;
            network.OnClientConnectedCallback += OnClientConnected;
            network.OnClientDisconnectCallback += OnClientDisconnected;
        }

        public async Task<bool> Host(bool direct, DeviceInfo self, RoomState settings)
        {
            EnsureNetwork();
            IsHost = true;
            State = settings;
            State.direct = direct;
            try
            {
                if (direct)
                {
                    var transport = (UnityTransport)network.NetworkConfig.NetworkTransport;
                    transport.SetConnectionData("0.0.0.0", OnlineConfig.Instance.directPort, "0.0.0.0");
                    if (!network.StartHost())
                    {
                        return Fail("Oda kurulamadı.");
                    }
                    State.code = LocalAddress();
                }
                else
                {
                    if (!await OnlineServices.EnsureReady())
                    {
                        return Fail(OnlineServices.Message);
                    }
                    string code = await UniqueCode();
                    var options = new SessionOptions
                    {
                        MaxPlayers = OnlineConfig.Instance.maxDevices,
                        IsPrivate = false,
                        Name = "Fart Rafter " + code,
                        SessionProperties = new Dictionary<string, SessionProperty>
                        {
                            { CodeKey, new SessionProperty(code, VisibilityPropertyOptions.Public, PropertyIndex.String1) }
                        }
                    }.WithRelayNetwork();
                    session = await MultiplayerService.Instance.CreateSessionAsync(options);
                    State.code = code;
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[FartRafters] Host: " + exception.Message);
                return Fail("Oda kurulamadı. İnternet bağlantını kontrol et.");
            }
            connected = true;
            RegisterHandlers();
            self.clientId = network.LocalClientId;
            State.devices.Clear();
            State.devices.Add(self);
            Replan();
            _ = FriendsClient.SetActivity("odada", State.code);
            return true;
        }

        public async Task<bool> Join(bool direct, string code, DeviceInfo self)
        {
            EnsureNetwork();
            IsHost = false;
            pendingHello = self;
            try
            {
                if (direct)
                {
                    var transport = (UnityTransport)network.NetworkConfig.NetworkTransport;
                    transport.SetConnectionData(code, OnlineConfig.Instance.directPort);
                    if (!network.StartClient())
                    {
                        return Fail("Odaya bağlanılamadı.");
                    }
                }
                else
                {
                    if (!await OnlineServices.EnsureReady())
                    {
                        return Fail(OnlineServices.Message);
                    }
                    var query = new QuerySessionsOptions
                    {
                        Count = 1,
                        FilterOptions = new List<FilterOption> { new FilterOption(FilterField.StringIndex1, code, FilterOperation.Equal) }
                    };
                    QuerySessionsResults results = await MultiplayerService.Instance.QuerySessionsAsync(query);
                    if (results.Sessions == null || results.Sessions.Count == 0)
                    {
                        return Fail("Bu kodla bir oda bulunamadı.");
                    }
                    session = await MultiplayerService.Instance.JoinSessionByIdAsync(results.Sessions[0].Id);
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[FartRafters] Join: " + exception.Message);
                return Fail("Odaya katılınamadı. Kodu kontrol et.");
            }
            connected = true;
            RegisterHandlers();
            if (network.IsConnectedClient)
            {
                SendHello();
            }
            return true;
        }

        DeviceInfo pendingHello;

        async Task<string> UniqueCode()
        {
            for (int attempt = 0; attempt < 6; attempt++)
            {
                string code = Random.Range(100000, 1000000).ToString();
                var query = new QuerySessionsOptions
                {
                    Count = 1,
                    FilterOptions = new List<FilterOption> { new FilterOption(FilterField.StringIndex1, code, FilterOperation.Equal) }
                };
                QuerySessionsResults results = await MultiplayerService.Instance.QuerySessionsAsync(query);
                if (results.Sessions == null || results.Sessions.Count == 0)
                {
                    return code;
                }
            }
            return Random.Range(100000, 1000000).ToString();
        }

        bool Fail(string message)
        {
            LastError = message;
            Shutdown();
            return false;
        }

        static string LocalAddress()
        {
            try
            {
                foreach (System.Net.IPAddress address in System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList)
                {
                    if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        return address.ToString();
                    }
                }
            }
            catch (Exception)
            {
                return "127.0.0.1";
            }
            return "127.0.0.1";
        }

        void RegisterHandlers()
        {
            CustomMessagingManager messaging = network.CustomMessagingManager;
            if (messaging == null)
            {
                return;
            }
            messaging.RegisterNamedMessageHandler(RoomMessages.Hello, OnHello);
            messaging.RegisterNamedMessageHandler(RoomMessages.Room, OnRoom);
            messaging.RegisterNamedMessageHandler(RoomMessages.Ready, OnReady);
            messaging.RegisterNamedMessageHandler(RoomMessages.Emoji, OnEmoji);
            messaging.RegisterNamedMessageHandler(RoomMessages.Kick, OnKick);
            messaging.RegisterNamedMessageHandler(RoomMessages.Start, OnStart);
            messaging.RegisterNamedMessageHandler(RoomMessages.Lobby, OnLobby);
            messaging.RegisterNamedMessageHandler(RoomMessages.Rematch, OnRematch);
            foreach (string name in new[] { RoomMessages.Input, RoomMessages.Snapshot, RoomMessages.Event, RoomMessages.Report })
            {
                string captured = name;
                messaging.RegisterNamedMessageHandler(captured, (sender, reader) => GameMessage?.Invoke(captured, sender, reader));
            }
        }

        void OnClientConnected(ulong clientId)
        {
            if (!IsHost && clientId == network.LocalClientId)
            {
                SendHello();
            }
        }

        void OnClientDisconnected(ulong clientId)
        {
            if (closing)
            {
                return;
            }
            if (!IsHost)
            {
                if (clientId == network.LocalClientId || clientId == NetworkManager.ServerClientId)
                {
                    Close(State.devices.Count == 0 ? "Odaya bağlanılamadı. Kodu ya da bağlantını kontrol et." : "Oda kapandı.");
                }
                return;
            }
            DeviceInfo device = State.Device(clientId);
            if (device != null)
            {
                State.devices.Remove(device);
                if (!InGame)
                {
                    Replan();
                }
                else
                {
                    DeviceLeft?.Invoke(clientId);
                }
            }
        }

        public event Action<ulong> DeviceLeft;

        void SendHello()
        {
            if (pendingHello == null)
            {
                return;
            }
            SendJson(RoomMessages.Hello, NetworkManager.ServerClientId, pendingHello);
        }

        void OnHello(ulong sender, FastBufferReader reader)
        {
            if (!IsHost)
            {
                return;
            }
            DeviceInfo device = ReadJson<DeviceInfo>(reader);
            if (device == null)
            {
                return;
            }
            device.clientId = sender;
            device.locals = Mathf.Clamp(device.locals, 1, 3);
            device.ready = false;
            if (string.IsNullOrWhiteSpace(device.name) || device.name.Length > 32 || !ProfanityFilter.IsClean(device.name))
            {
                device.name = "Oyuncu " + (State.devices.Count + 1);
            }
            State.devices.RemoveAll(existing => existing.clientId == sender);
            State.devices.Add(device);
            Replan();
        }

        void OnRoom(ulong sender, FastBufferReader reader)
        {
            if (IsHost)
            {
                return;
            }
            RoomState state = ReadJson<RoomState>(reader);
            if (state != null)
            {
                State = state;
                RoomChanged?.Invoke();
            }
        }

        void OnReady(ulong sender, FastBufferReader reader)
        {
            if (!IsHost)
            {
                return;
            }
            DeviceInfo update = ReadJson<DeviceInfo>(reader);
            DeviceInfo device = State.Device(sender);
            if (update == null || device == null)
            {
                return;
            }
            device.ready = update.ready;
            device.locals = Mathf.Clamp(update.locals, 1, 3);
            device.avatars = update.avatars;
            Replan();
        }

        void OnEmoji(ulong sender, FastBufferReader reader)
        {
            EmojiInfo emoji = ReadJson<EmojiInfo>(reader);
            if (emoji == null)
            {
                return;
            }
            if (IsHost)
            {
                emoji.clientId = sender;
                DeviceInfo device = State.Device(sender);
                emoji.name = device != null ? device.name : "?";
                Broadcast(RoomMessages.Emoji, emoji, true);
            }
            EmojiReceived?.Invoke(emoji);
        }

        void OnKick(ulong sender, FastBufferReader reader)
        {
            if (!IsHost)
            {
                Close("Oda kurucusu seni odadan çıkardı.");
            }
        }

        void OnStart(ulong sender, FastBufferReader reader)
        {
            if (IsHost)
            {
                return;
            }
            StartInfo start = ReadJson<StartInfo>(reader);
            if (start != null)
            {
                Started = start;
                SceneManager.LoadScene(GameSelection.GameScene);
            }
        }

        void OnLobby(ulong sender, FastBufferReader reader)
        {
            if (IsHost)
            {
                return;
            }
            Started = null;
            SceneManager.LoadScene(GameSelection.MenuScene);
        }

        public void Replan()
        {
            if (!IsHost)
            {
                return;
            }
            SeatPlanner.Assign(State, OnlineConfig.Instance.maxSeats);
            Broadcast(RoomMessages.Room, State, false);
            RoomChanged?.Invoke();
        }

        public void SetSettings(bool adventure, int level, int difficulty, int bots, int botLevel)
        {
            if (!IsHost || InGame)
            {
                return;
            }
            State.adventure = adventure;
            State.level = level;
            State.difficulty = difficulty;
            State.bots = bots;
            State.botLevel = botLevel;
            Replan();
        }

        public void SetBiome(int choice)
        {
            if (!IsHost || InGame)
            {
                return;
            }
            State.biome = Mathf.Clamp(choice, 0, Biomes.RandomChoice);
            Replan();
        }

        public void SetWeather(int choice)
        {
            if (!IsHost || InGame)
            {
                return;
            }
            State.weather = Mathf.Clamp(choice, 0, Weathers.Count - 1);
            Replan();
        }

        public void SetRaft(int index)
        {
            if (!IsHost || InGame)
            {
                return;
            }
            State.raft = Mathf.Max(0, index);
            Replan();
        }

        public void SetLocal(int locals, int[] avatars, bool ready)
        {
            if (IsHost)
            {
                DeviceInfo self = State.devices.Count > 0 ? State.devices[0] : null;
                if (self != null)
                {
                    self.locals = Mathf.Clamp(locals, 1, 3);
                    self.avatars = avatars;
                    self.ready = true;
                }
                Replan();
                return;
            }
            SendJson(RoomMessages.Ready, NetworkManager.ServerClientId, new DeviceInfo { locals = locals, avatars = avatars, ready = ready });
        }

        public void SendEmoji(int index)
        {
            DeviceInfo self = State.Device(LocalClientId);
            var emoji = new EmojiInfo { clientId = LocalClientId, name = self != null ? self.name : PlayerProfile.DisplayName, emoji = index };
            if (IsHost)
            {
                Broadcast(RoomMessages.Emoji, emoji, true);
                EmojiReceived?.Invoke(emoji);
            }
            else
            {
                SendJson(RoomMessages.Emoji, NetworkManager.ServerClientId, emoji);
            }
        }

        public async void Kick(ulong clientId)
        {
            if (!IsHost || clientId == LocalClientId)
            {
                return;
            }
            SendJson(RoomMessages.Kick, clientId, new EmojiInfo());
            DeviceInfo device = State.Device(clientId);
            if (session != null && device != null && !string.IsNullOrEmpty(device.playerId))
            {
                try
                {
                    await session.AsHost().RemovePlayerAsync(device.playerId);
                }
                catch (Exception exception)
                {
                    Debug.LogWarning("[FartRafters] Kick: " + exception.Message);
                }
            }
            network.DisconnectClient(clientId);
        }

        public bool AllReady
        {
            get
            {
                for (int i = 1; i < State.devices.Count; i++)
                {
                    if (!State.devices[i].ready)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public void StartGame()
        {
            if (!IsHost || InGame)
            {
                return;
            }
            Replan();
            int seed = Environment.TickCount;
            Started = new StartInfo
            {
                seed = seed,
                biome = (int)Biomes.Resolve(State.biome, seed),
                raft = State.raft,
                weather = State.weather,
                adventure = State.adventure,
                level = State.level,
                difficulty = State.difficulty,
                botLevel = State.botLevel,
                seats = new List<SeatInfo>(State.seats)
            };
            if (session != null)
            {
                try
                {
                    IHostSession host = session.AsHost();
                    host.IsLocked = true;
                    _ = host.SavePropertiesAsync();
                }
                catch (Exception exception)
                {
                    Debug.LogWarning("[FartRafters] Lock: " + exception.Message);
                }
            }
            Broadcast(RoomMessages.Start, Started, false);
            _ = FriendsClient.SetActivity("oyunda", "");
            SceneManager.LoadScene(GameSelection.GameScene);
        }

        public void RequestRematch()
        {
            if (IsHost)
            {
                BackToLobby();
                return;
            }
            SendJson(RoomMessages.Rematch, NetworkManager.ServerClientId, new EmojiInfo());
        }

        void OnRematch(ulong sender, FastBufferReader reader)
        {
            if (IsHost && InGame)
            {
                BackToLobby();
            }
        }

        public void BackToLobby()
        {
            if (!IsHost)
            {
                return;
            }
            Started = null;
            foreach (DeviceInfo device in State.devices)
            {
                device.ready = device.clientId == LocalClientId;
            }
            if (session != null)
            {
                try
                {
                    IHostSession host = session.AsHost();
                    host.IsLocked = false;
                    _ = host.SavePropertiesAsync();
                }
                catch (Exception exception)
                {
                    Debug.LogWarning("[FartRafters] Unlock: " + exception.Message);
                }
            }
            Broadcast(RoomMessages.Lobby, new EmojiInfo(), false);
            _ = FriendsClient.SetActivity("odada", State.code);
            SceneManager.LoadScene(GameSelection.MenuScene);
        }

        public async void Leave()
        {
            closing = true;
            if (session != null)
            {
                try
                {
                    if (IsHost)
                    {
                        await session.AsHost().DeleteAsync();
                    }
                    else
                    {
                        await session.LeaveAsync();
                    }
                }
                catch (Exception exception)
                {
                    Debug.LogWarning("[FartRafters] Leave: " + exception.Message);
                }
            }
            Close("");
        }

        void Close(string reason)
        {
            closing = true;
            bool inGame = InGame;
            Shutdown();
            Closed?.Invoke(reason);
            _ = FriendsClient.SetActivity("menü", "");
            if (inGame && !string.IsNullOrEmpty(reason))
            {
                PendingNotice = reason;
                SceneManager.LoadScene(GameSelection.MenuScene);
            }
            else if (!string.IsNullOrEmpty(reason))
            {
                PendingNotice = reason;
            }
            Destroy(gameObject);
        }

        public static string PendingNotice { get; set; } = "";

        void Shutdown()
        {
            connected = false;
            Started = null;
            if (network != null)
            {
                network.OnClientConnectedCallback -= OnClientConnected;
                network.OnClientDisconnectCallback -= OnClientDisconnected;
                if (network.IsListening)
                {
                    network.Shutdown();
                }
            }
            session = null;
        }

        void OnDestroy()
        {
            if (Current == this)
            {
                Current = null;
            }
        }

        public void Broadcast<T>(string message, T payload, bool includeEmojiSender)
        {
            if (!IsHost || network == null || !network.IsListening)
            {
                return;
            }
            string json = JsonUtility.ToJson(payload);
            using (var writer = new FastBufferWriter(json.Length * 2 + 16, Allocator.Temp, 1 << 18))
            {
                writer.WriteValueSafe(json);
                foreach (ulong client in network.ConnectedClientsIds)
                {
                    if (client != network.LocalClientId)
                    {
                        Send(message, client, writer, NetworkDelivery.ReliableFragmentedSequenced);
                    }
                }
            }
        }

        public void SendJson<T>(string message, ulong target, T payload)
        {
            if (!CanSend)
            {
                return;
            }
            string json = JsonUtility.ToJson(payload);
            using (var writer = new FastBufferWriter(json.Length * 2 + 16, Allocator.Temp, 1 << 18))
            {
                writer.WriteValueSafe(json);
                Send(message, target, writer, NetworkDelivery.ReliableFragmentedSequenced);
            }
        }

        public void SendRaw(string message, ulong target, FastBufferWriter writer, NetworkDelivery delivery)
        {
            if (CanSend)
            {
                Send(message, target, writer, delivery);
            }
        }

        bool CanSend => network != null && network.IsListening && (network.IsServer || network.IsConnectedClient) && network.CustomMessagingManager != null;

        void Send(string message, ulong target, FastBufferWriter writer, NetworkDelivery delivery)
        {
            try
            {
                network.CustomMessagingManager.SendNamedMessage(message, target, writer, delivery);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[FartRafters] Send " + message + ": " + exception.Message);
            }
        }

        public IEnumerable<ulong> RemoteClients()
        {
            if (network == null || !network.IsListening || !IsHost)
            {
                yield break;
            }
            foreach (ulong client in network.ConnectedClientsIds)
            {
                if (client != network.LocalClientId)
                {
                    yield return client;
                }
            }
        }

        public static T ReadJson<T>(FastBufferReader reader) where T : class
        {
            try
            {
                reader.ReadValueSafe(out string json);
                return JsonUtility.FromJson<T>(json);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[FartRafters] Message: " + exception.Message);
                return null;
            }
        }
    }
}
