﻿using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class JoinScopaMenu : MonoBehaviour
{
    [SerializeField] private NetworkManagerScopa networkManager = null;

    [Header("UI")]
    [SerializeField] private GameObject landingPagePanel = null;
    [SerializeField] private TMP_InputField ipAddressInputField = null;
    [SerializeField] private Button joinButton = null;

    private void OnEnable()
    {
        NetworkManagerScopa.OnClientConnected += HandleClientConnected;
        NetworkManagerScopa.OnClientDisconnected += HandleClientDisconnected;
    }
    
    private void OnDisable()
    {
        NetworkManagerScopa.OnClientConnected -= HandleClientConnected;
        NetworkManagerScopa.OnClientDisconnected -= HandleClientDisconnected;
    }

    public void JoinLobby()
    {
        networkManager = GameObject.Find("NetworkManager").gameObject.GetComponent<NetworkManagerScopa>();

        string ipAddress = ipAddressInputField.text;

        networkManager.networkAddress = ipAddress;
        networkManager.StartClient();

        joinButton.interactable = false;
    }

    private void HandleClientConnected()
    {
        joinButton.interactable = true;

        gameObject.SetActive(false);
        landingPagePanel.SetActive(false);
    }

    private void HandleClientDisconnected()
    {
        joinButton.interactable = true;
    }
}
