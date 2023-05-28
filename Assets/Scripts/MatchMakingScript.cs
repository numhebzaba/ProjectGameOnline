using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Netcode;
using System.Threading.Tasks;
using ParrelSync;
using Unity.Services;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using TMPro;

public class MatchMakingScript : MonoBehaviour
{
   public TMP_InputField playerNameInput;
    public GameObject startButton;
    public GameObject MatchMakingPanel;
    private string PlayerName;
    string lobbyname = "Mylobby";
    private Lobby joinedLobby;

    public async void StartGame()
    {
        startButton.SetActive(false);
        MatchMakingPanel.SetActive(false);
        PlayerName = playerNameInput.GetComponent<TMP_InputField>().text;
        //joinedLobby = await CreateLobby();
        joinedLobby = await JoinLobby() ?? await CreateLobby();
        if(joinedLobby == null)
        {
            startButton.SetActive(true);
            MatchMakingPanel.SetActive(true);
        }
        await UnityServices.InitializeAsync();
    }

    private async Task<Lobby> CreateLobby()
    {
        try
        {
            int maxPlayer = 4;
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
                            new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member,PlayerName)}
                    }
                },
                Data = new Dictionary<string, DataObject>
                {
                    {"JoinCodeKey", new DataObject(DataObject.VisibilityOptions.Public, joinCode) }
                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyname, maxPlayer, createLobbyOptions);
            StartCoroutine(HeartBeatLobbyCoroutine(lobby.Id, 15));
            Debug.Log("Created Lobby : " + lobby.Name + " , " + lobby.MaxPlayers + " , " + lobby.Id + " , " + lobby.LobbyCode);

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartHost();

            LobbyScript.Instance.PrintPlayers(lobby);
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

    private async Task<Lobby> JoinLobby()
    {
        try
        {
            Lobby lobby = await FindRandomLobby();
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

    private async Task<Lobby> FindRandomLobby()
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
                return lobby;
            }
            return null;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            return null;
        }
    }
}
