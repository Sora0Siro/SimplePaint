using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public GameObject ToolsInactiveWindow;
    public GameObject ToolsActiveWindow;
    public GameObject goodSaveMessage;
    public Text message;
    public GameObject badSaveMessage;
    float timeForMessage = 2f;
    bool state = false;
    void Awake()
    {
        Action();
        goodSaveMessage.SetActive(false);
        badSaveMessage.SetActive(false);
    }

    public void Action()
    {
        state = !state;
        ToolsInactiveWindow.SetActive(!state);
        ToolsActiveWindow.SetActive(state);
    }

    void HideMessage()
    {
        if (goodSaveMessage.activeSelf)
        {
            goodSaveMessage.SetActive(false);
        }
        else if (badSaveMessage.activeSelf)
        {
            badSaveMessage.SetActive(false);
        }
    }

    public void SaveFailed()
    {
        badSaveMessage.SetActive(true);
        Invoke("HideMessage", timeForMessage);
    }
    public void SaveSuccessful(string location)
    {
        goodSaveMessage.SetActive(true);
        message.text = "Saved in:" + location;
        Invoke("HideMessage", timeForMessage);
    }
}