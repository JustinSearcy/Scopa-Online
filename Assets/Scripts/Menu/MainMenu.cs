using UnityEngine;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private NetworkManagerScopa networkManager = null;

    [Header("UI")]
    [SerializeField] private GameObject landingPagePanel = null;

    public void HostLobby()
    {
        networkManager = GameObject.Find("NetworkManager").gameObject.GetComponent<NetworkManagerScopa>();

        networkManager.StartHost();

        landingPagePanel.SetActive(false);
    }

    public void CloseLobby()
    {
        networkManager.StopHost();

        landingPagePanel.SetActive(true);
    }
}
