using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class _PlayerInfo : MonoBehaviour
{
    public TMP_Text PlayerName;
    private void Start()
    {
        SetPlayerName();
    }
    private void SetPlayerName()
    {
        PlayerName.text = _DatabaseController.PlayerNameDatabase;
    }
    public void Logout()
    {
        SceneManager.LoadScene("LoginScene");
    }
}
