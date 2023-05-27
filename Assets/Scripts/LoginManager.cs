using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class LoginManager : NetworkBehaviour
{
    public TMP_InputField userNameInput;
    public TMP_InputField userPassInput;
    public GameObject loginPanel;
    public GameObject leaveButton;
    //public GameObject[] SpawnPoint = new GameObject[4];
    public List<GameObject> SpawnPoint_list = new List<GameObject>();
    public List<string> playerNameList = new List<string>();


    [SerializeField]
    public struct ConnectionPayload
    {
        public string PlayerName;
        public string PlayerPass;
    }
    private void Start()
    {
        NetworkManager.Singleton.OnServerStarted += HandleServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
        loginPanel.SetActive(true);
        leaveButton.SetActive(false);
    }
    private void OnDestroy()
    {
        if (NetworkManager.Singleton == null) { return; }
        NetworkManager.Singleton.OnServerStarted -= HandleServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
    }
    private void HandleServerStarted()
    {
        //throw new System.NotImplementedException();
    }

    private void HandleClientConnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            loginPanel.SetActive(false);
            leaveButton.SetActive(true);
        }
    }

    private void HandleClientDisconnect(ulong clientId)
    {
        //throw new System.NotImplementedException();
    }
    public void Leave()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.Shutdown();
            NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.Shutdown();
            loginPanel.SetActive(true);
            leaveButton.SetActive(false);
        }
    }

    public void Host()
    {


        NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;

        NetworkManager.Singleton.StartHost();
    }
    public void Client()
    {
        string userName = userNameInput.GetComponent<TMP_InputField>().text;
        string userPass = userPassInput.GetComponent<TMP_InputField>().text;

        var payload = JsonUtility.ToJson(new ConnectionPayload()
        {
            PlayerName = userName,
            PlayerPass = userPass,
        });
        Debug.Log(payload + " payloaddddddddddddddd");
        byte[] payloadBytes = System.Text.Encoding.ASCII.GetBytes(payload);

        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;

        NetworkManager.Singleton.StartClient();
    }

    private bool approveConnection(string clientData, string serverData, string HostPass, string ClientPass)
    {
        if (HostPass == ClientPass)
        {
            bool isApprove = false;//= System.String.Equals(clientData.Trim(), serverData.Trim()) ? false : true;
            Debug.Log(clientData + "approvecheck");
            Debug.Log("--------------------------");
            Debug.Log(serverData);
            if (NetworkManager.Singleton.IsServer)
                isApprove = true;
            else if (clientData == serverData)
                isApprove = false;
            else
                isApprove = true;


            for (var i = 0; i < playerNameList.Count; i++)
            {
                //Debug.Log(playerNameList[playerNameList.Count - 1]);

                if (clientData == playerNameList[i])
                {
                    isApprove = false;
                }
                Debug.Log("Inloop" + isApprove);
            }
            Debug.Log("Outloop" + isApprove);
            if (playerNameList.Count == 0)
            {
                playerNameList.Add(serverData);
            }
            else if (isApprove)
            {
                playerNameList.Add(clientData);
            }
            return isApprove;
        }
        return false;


    }
    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {

        // The client identifier to be authenticated
        var clientId = request.ClientNetworkId;

        // Additional connection data defined by user code
        var connectionData = request.Payload;


        int byteLength = connectionData.Length;
        bool isApprove = false;
        ;
        if (byteLength > 0 || NetworkManager.Singleton.IsServer)
        {
            string clientName = "";
            string clientPass = "";
            //if (NetworkManager.Singleton.IsServer)
            //{

            //}
            //else if (NetworkManager.Singleton.IsClient)
            //{

            //}

            try
            {
                string payload = System.Text.Encoding.ASCII.GetString(request.Payload);
                var connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload);
                clientName = connectionPayload.PlayerName;
                clientPass = connectionPayload.PlayerPass;
                Debug.Log("Client---------------------------------");
                Debug.Log(clientName);
                Debug.Log(clientPass);
            }
            catch
            {
                clientName = userNameInput.GetComponent<TMP_InputField>().text;
                clientPass = userPassInput.GetComponent<TMP_InputField>().text;
                Debug.Log("Server---------------------------------");
            }




            string clientData = System.Text.Encoding.ASCII.GetString(connectionData, 0, byteLength);

            string hostData = userNameInput.GetComponent<TMP_InputField>().text;
            string HostPass = userPassInput.GetComponent<TMP_InputField>().text;

            isApprove = approveConnection(clientName, hostData, HostPass, clientPass);
        }
        //if(NetworkManager.Singleton.IsHost)
        //    isApprove = true;

        // Your approval logic determines the following values
        response.Approved = isApprove;
        response.CreatePlayerObject = isApprove;

        // The prefab hash value of the NetworkPrefab, if null the default NetworkManager player prefab is used
        response.PlayerPrefabHash = null;

        // Position to spawn the player object (if null it uses default of Vector3.zero)
        response.Position = Vector3.zero;

        // Rotation to spawn the player object (if null it uses the default of Quaternion.identity)
        response.Rotation = Quaternion.identity;
        setSpawnLocation(clientId, response, isApprove);

        // If response.Approved is false, you can provide a message that explains the reason why via ConnectionApprovalResponse.Reason
        // On the client-side, NetworkManager.DisconnectReason will be populated with this message via DisconnectReasonMessage
        response.Reason = "Some reason for not approving the client";


        // If additional approval steps are needed, set this to true until the additional steps are complete
        // once it transitions from true to false the connection approval response will be processed.
        response.Pending = false;
    }
    private void setSpawnLocation(ulong clientId, NetworkManager.ConnectionApprovalResponse response, bool isApprove)
    {
        int randomSpawn = Random.Range(0, SpawnPoint_list.Count);
        Debug.Log("random : " + randomSpawn);
        Vector3 spawnPos = Vector3.zero;
        Quaternion spawnRot = Quaternion.identity;

        switch (randomSpawn)
        {
            case 0:
                spawnPos = SpawnPoint_list[0].transform.position; spawnRot = SpawnPoint_list[0].transform.rotation;
                if (isApprove)
                    SpawnPoint_list.RemoveAt(0);
                break;
            case 1:
                spawnPos = SpawnPoint_list[1].transform.position; spawnRot = SpawnPoint_list[1].transform.rotation;
                if (isApprove)
                    SpawnPoint_list.RemoveAt(1);
                break;
            case 2:
                spawnPos = SpawnPoint_list[2].transform.position; spawnRot = SpawnPoint_list[2].transform.rotation;
                if (isApprove)
                    SpawnPoint_list.RemoveAt(2);
                break;
            case 3:
                spawnPos = SpawnPoint_list[3].transform.position; spawnRot = SpawnPoint_list[3].transform.rotation;
                if (isApprove)
                    SpawnPoint_list.RemoveAt(3);
                break;
        }
        response.Position = spawnPos; response.Rotation = spawnRot;
    }
}
