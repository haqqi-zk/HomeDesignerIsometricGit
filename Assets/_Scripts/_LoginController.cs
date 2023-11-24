using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class _LoginController : MonoBehaviour
{
    public Button LoginButton;
    public Button RegisterButton;

    [Header("Login UI")]
    public TMP_InputField LoginEmail;
    public TMP_InputField LoginPassword;

    [Header("Register UI")]
    public TMP_InputField Username;
    public TMP_InputField Email;
    public TMP_InputField ConfirmEmail;
    public TMP_InputField Password;
    public TMP_InputField ConfirmPassword;

    [Header("Error UI")]
    public GameObject ErrorPanel;
    public TMP_Text ErrorText;
    public async void Login()
    {
        if (LoginEmail.text != string.Empty && LoginPassword.text != string.Empty)
        {
            await _DatabaseController.SignInUser(LoginEmail.text, LoginPassword.text);
            SceneManager.LoadScene("HomeHQ");
        }
        else
        {
            PopupErrorMessage("Please fill the required fields");
        }
    }
    public async void Register()
    {
        if (Email.text != string.Empty && Password.text != string.Empty && ConfirmEmail.text != null && ConfirmPassword.text != null && Username.text != null)
        {
            bool emailPassword = ConfirmEmailPassword();
            if (emailPassword)
            {
                bool usernameExist = await _DatabaseController.Instance.CheckPlayerExist(Username.text);
                if (!usernameExist)
                {
                    await _DatabaseController.SignUpUser(Username.text, Email.text, Password.text);
                    SceneManager.LoadScene("HomeHQ");
                }
                else
                {
                    PopupErrorMessage("This username is already been used");
                }
            }
        }
        else
        {
            PopupErrorMessage("Please fill the required fields");
        }
    }
    public bool ConfirmEmailPassword()
    {
        if(Email.text != ConfirmEmail.text)
        {
            PopupErrorMessage("Email Doesn't Match"); return false;
        }
        if(Password.text != ConfirmPassword.text)
        {
            PopupErrorMessage("Password Doesn't Match"); return false;
        }
;       return true;
    }
    public void ResetInputFields()
    {
        LoginEmail.text = string.Empty;
        LoginPassword.text = string.Empty;

        Username.text = string.Empty;
        Email.text = string.Empty;
        ConfirmEmail.text = string.Empty;
        Password.text = string.Empty;
        ConfirmPassword.text = string.Empty;
    }
    public void PopupErrorMessage(string errorMessage)
    {
        ErrorPanel.SetActive(true);
        ErrorText.text = errorMessage;
        ResetInputFields();
    }
}
