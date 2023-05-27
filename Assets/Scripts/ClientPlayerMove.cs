using System;
using Cinemachine;
using System.Collections;
using StarterAssets;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Assumes client authority
/// </summary>
[DefaultExecutionOrder(1)] // after server component
public class ClientPlayerMove : NetworkBehaviour
{

    [SerializeField]
    CharacterController m_CharacterController;

    [SerializeField]
    ThirdPersonController m_ThirdPersonController;

    [SerializeField]
    CapsuleCollider m_CapsuleCollider;

    [SerializeField]
    Transform m_CameraFollow;

    [SerializeField]
    PlayerInput m_PlayerInput;


    void Awake()
    {

        m_ThirdPersonController.enabled = false;
        m_CapsuleCollider.enabled = false;
        m_CharacterController.enabled = false;

        //Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        enabled = IsClient;
        if (!IsOwner)
        {
            enabled = false;
            m_CharacterController.enabled = true;
            m_ThirdPersonController.enabled = true;

            m_CapsuleCollider.enabled = true;
            return;
        }

        // player input is only enabled on owning players
        m_PlayerInput.enabled = true;
        m_ThirdPersonController.enabled = true;

        m_CharacterController.enabled = true;
        m_CapsuleCollider.enabled = true;

        var cinemachineVirtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
        cinemachineVirtualCamera.Follow = m_CameraFollow;

    }
    private void Update()
    {
        Screen.lockCursor = false;

    }

    
}
