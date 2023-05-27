using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using TMPro;
using Unity.Collections;

public class PlayerName : NetworkBehaviour
{
    public TMP_Text namePrefab;
    private TMP_Text nameLabel;
    public NetworkVariable<int> postX = new NetworkVariable<int>(0,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public NetworkVariable<NetworkString> playerNameA = new NetworkVariable<NetworkString>(
        new NetworkString { info = "Player A" },
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public NetworkVariable<NetworkString> playerNameB = new NetworkVariable<NetworkString>(
        new NetworkString { info = "Player B" },
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public NetworkVariable<NetworkString> playerNameC = new NetworkVariable<NetworkString>(
        new NetworkString { info = "Player C" },
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public NetworkVariable<NetworkString> playerNameD = new NetworkVariable<NetworkString>(
        new NetworkString { info = "Player D" },
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private LoginManager loginManager;

    public struct NetworkString : INetworkSerializable
    {
        public FixedString32Bytes info;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref info);
        }
        public override string ToString()
        {
            return info.ToString();
        }
        public static implicit operator NetworkString(string v) =>
            new NetworkString() { info = new FixedString32Bytes(v) };
    }
    public override void OnNetworkSpawn()
    {
        //base.OnNetworkSpawn();

        GameObject canvas = GameObject.FindWithTag("MainCanvas");
        nameLabel = Instantiate(namePrefab, Vector3.zero, Quaternion.identity) as TMP_Text;
        nameLabel.transform.SetParent(canvas.transform);
        Debug.Log("nameLabel" + nameLabel);

        postX.OnValueChanged += (int previousData, int newValue) =>
        {
            Debug.Log("Owner ID = " + OwnerClientId + " : post x = " + postX.Value);
        };

        playerNameA.OnValueChanged += (NetworkString previousData, NetworkString newValue) =>
        {
            Debug.Log("Owner ID = " + OwnerClientId + " : new data = " + newValue.info);
        };

        playerNameB.OnValueChanged += (NetworkString previousData, NetworkString newValue) =>
        {
            Debug.Log("Owner ID = " + OwnerClientId + " : new data = " + newValue.info);
        };

        playerNameC.OnValueChanged += (NetworkString previousData, NetworkString newValue) =>
        {
            Debug.Log("Owner ID = " + OwnerClientId + " : new data = " + newValue.info);
        };

        playerNameD.OnValueChanged += (NetworkString previousData, NetworkString newValue) =>
        {
            Debug.Log("Owner ID = " + OwnerClientId + " : new data = " + newValue.info);
        };

        if (IsOwner)
        {
            loginManager = GameObject.FindObjectOfType<LoginManager>();
            if (loginManager != null)
            {
                string name = loginManager.userNameInput.text;
                Debug.Log(name);

                if (IsOwnedByServer) { playerNameA.Value = name; }
                else if (OwnerClientId == 1) { playerNameB.Value = name; }
                else if (OwnerClientId == 2) { playerNameC.Value = name; }
                else if (OwnerClientId == 3) { playerNameD.Value = name; }

            }
        }
    }
    public override void OnDestroy()
    {
        if (nameLabel != null)
            Destroy(nameLabel.gameObject);

        base.OnDestroy();
    }
    private void Update()
    {
        updatePlayerInfo();
    }
    private void FixedUpdate()
    {
        Vector3 nameLabelPos = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0, 2.5f, 0));
        nameLabel.text = gameObject.name;
        nameLabel.transform.position = nameLabelPos;
        updatePlayerInfo();
    }
    private void updatePlayerInfo()
    {
        if (IsOwnedByServer)
        {
            nameLabel.text = playerNameA.Value.ToString();
        }
        else
        {
            if (OwnerClientId == 1) { nameLabel.text = playerNameB.Value.ToString(); }
            else if (OwnerClientId == 2) { nameLabel.text = playerNameC.Value.ToString(); }
            else if (OwnerClientId == 3) { nameLabel.text = playerNameD.Value.ToString(); }
        }
    }
    private void OnEnable()
    {
        if (nameLabel != null)
            nameLabel.enabled = true;
    }

    private void OnDisable()
    {
        if (nameLabel != null)
            nameLabel.enabled = false;
    }
}
