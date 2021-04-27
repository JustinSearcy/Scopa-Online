using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;

public class GameIntro : MonoBehaviour
{
    [SerializeField] private List<TextMeshProUGUI> playerNameTexts = new List<TextMeshProUGUI>(2);

    void Start()
    {
        PlayerManager[] playerManagers = FindObjectsOfType<PlayerManager>();
        for (int i = 0; i < playerManagers.Length; i++)
        {
            playerNameTexts[i].text = playerManagers[i].DisplayName;
            playerManagers[i].GetComponent<PlayerManager>().GetObjects();
        }
    }
}
