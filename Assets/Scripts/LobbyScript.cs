using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Authentication;
using TMPro;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Networking.Transport.Relay;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Threading.Tasks;
using Unity.Services.Core;



public class LobbyScript : Singleton<LobbyScript>
{
    private Lobby hostLobby;
    private Lobby joinedLobby;
    private string playerName;
    private float lobbyUpdateTimer;

    public TMP_InputField LobbyID;
    public GameObject MatchMakingPanel;
    //public GameObject MainMenyPanel;
    public TextMeshProUGUI ShowTextIdLobby;

    private void Start()
    {
        playerName = "myName" + Random.Range(1, 999);
        Debug.Log("Player name : " + playerName);
        MatchMakingPanel.SetActive(false);
    }

    private void Update()
    {
        HandleLobbyPollForUpdates();
    }

    public async void StartGame()
    {
        //joinedLobby = await CreateLobby();
        joinedLobby = await JoinLobbyByCode(LobbyID.text);
        await UnityServices.InitializeAsync();
    }

    public async void CreateLobbyToStartGame()
    {
        //joinedLobby = await CreateLobby();
        joinedLobby = await CreateLobby();
        await UnityServices.InitializeAsync();
    }

    private async void HandleLobbyPollForUpdates()
    {
        if (joinedLobby != null)
        {
            lobbyUpdateTimer -= Time.deltaTime;
            if (lobbyUpdateTimer < 0f)
            {
                float lobbyUpdateTimerMax = 2.2f;
                lobbyUpdateTimer = lobbyUpdateTimerMax;
                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                joinedLobby = lobby;
            }
        }
    }
    private async Task<Lobby> CreateLobby()
    {
        try
        {
            int maxPlayer = 4;
            string lobbyName = "MyLobby";
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayer);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = new Player
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        {"PlayerName",
                            new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member,playerName)}
                    }
                },
                Data = new Dictionary<string, DataObject>
                {
                    {"JoinCodeKey", new DataObject(DataObject.VisibilityOptions.Public, joinCode) }
                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayer, createLobbyOptions);
            StartCoroutine(HeartBeatLobbyCoroutine(lobby.Id, 15));
            Debug.Log("Created Lobby : " + lobby.Name + " , " + lobby.MaxPlayers + " , " + lobby.Id + " , " + lobby.LobbyCode);

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartHost();

            LobbyScript.Instance.PrintPlayers(lobby);
            MatchMakingPanel.SetActive(false);
            ShowTextIdLobby.text = "ID: "+ lobby.LobbyCode;

            return lobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            return null;
        }
    }

    private static IEnumerator HeartBeatLobbyCoroutine(string lobbyId, float waitTimeSeconds)
    {
        var delay = new WaitForSecondsRealtime(waitTimeSeconds);
        while (true)
        {
            Lobbies.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;
        }
    }

    private async void ListLobbies()
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                Count = 25,
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots,"0",QueryFilter.OpOptions.GT)
                },
                Order = new List<QueryOrder>
                {
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                }
            };
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(queryLobbiesOptions);
            Debug.Log("Lobbies found: " + queryResponse.Results.Count);
            foreach (Lobby lobby in queryResponse.Results)
            {
                Debug.Log(lobby.Name + " , " + lobby.MaxPlayers + " , " + lobby.Data["JoinCodeKey"].Value);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

     public async void JoinLobby()
    {
        try
        {
            Lobby lobby = null;
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync();
            lobby = await LobbyService.Instance.JoinLobbyByIdAsync(LobbyID.text);
            Debug.Log(queryResponse.Results[0].Name + " , " + queryResponse.Results[0].AvailableSlots);

            
            string joinCode = lobby.Id;
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartClient();

        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async Task<Lobby> JoinLobbyByid()
    {
        try
        {
            string LobbyIdInput = LobbyID.text;
            Lobby lobby = await FindRandomLobby(LobbyIdInput);
            if (lobby == null) return null;

            if (lobby.Data["JoinCodeKey"].Value != null)
            {
                string joinCode = lobby.Data["JoinCodeKey"].Value;
                Debug.Log("joincode = " + joinCode);
                JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

                RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
                NetworkManager.Singleton.StartClient();
                return lobby;
            }
            return null;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log("No lobby found");
            return null;
        }
    }

    private async Task<Lobby> FindRandomLobby(string lobbyID)
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots,"1",QueryFilter.OpOptions.GE)
                }
            };
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(queryLobbiesOptions);
            Debug.Log("Lobbies found: " + queryResponse.Results.Count);
            foreach (Lobby lobby in queryResponse.Results)
            {
                string joinCode = lobby.Data["JoinCodeKey"].Value;
                Debug.Log("joincode = " + joinCode);
                if(joinCode == lobbyID)
                {
                    Debug.Log("Joined");
                    return lobby;
                }
                return null;
            }
            return null;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            return null;
        }
    }

    private async Task<Lobby> JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
            {
                Player = new Player
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        {"PlayerName",
                        new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member,playerName)}
                    }
                }
            };
            Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);
            joinedLobby = lobby;
            Debug.Log("lobby with code " + lobbyCode);
            PrintPlayers(joinedLobby);

            string joinCode = lobby.Data["JoinCodeKey"].Value;
            //Debug.Log("joincode = " + joinCode);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartClient();
            ShowTextIdLobby.text = "ID: " + lobby.LobbyCode;
            MatchMakingPanel.SetActive(false);
            return lobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            return null;
        }
    }

    private async void QuickJoinLobby()
    {
        try
        {
            Lobby lobby = await LobbyService.Instance.QuickJoinLobbyAsync();
            Debug.Log(lobby.Name + " , " + lobby.AvailableSlots);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public void PrintPlayers(Lobby lobby)
    {
        Debug.Log("Players in Lobby : " + lobby.Name + lobby.LobbyCode);
        //Debug.Log("Players in Lobby : " + lobby.Name + " : " + lobby.Data["JoinCodeKey"].Value);
        foreach (Player player in lobby.Players)
        {
            Debug.Log(player.Id + " : " + player.Data["PlayerName"].Value);
        }
    }

    private void PrintPlayers()
    {
        PrintPlayers(joinedLobby);
    }

    private async void UpdateLobbyGameMode(string gameMode)
    {
        try
        {
            hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject> {
                    {"GameMode", new DataObject(DataObject.VisibilityOptions.Public, gameMode)}
                }
            });
            joinedLobby = hostLobby;
            PrintPlayers(hostLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void UpdatePlayerName(string newPlayerName)
    {
        try
        {
            playerName = newPlayerName;
            await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id,
                AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
                {
                    Data = new Dictionary<string, PlayerDataObject> {
                    {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member,playerName)}
                }
                });
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void LeaveLobby()
    {
        try
        {
            string playerId = AuthenticationService.Instance.PlayerId;
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void KickPlayer()
    {
        try
        {
            string playerId = joinedLobby.Players[1].Id;
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void MigrateLobbyHost()
    {
        try
        {
            hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                HostId = joinedLobby.Players[1].Id
            });
            joinedLobby = hostLobby;
            PrintPlayers(hostLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void DeleteLobby()
    {
        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
}
