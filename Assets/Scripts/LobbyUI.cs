using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    public static LobbyUI Instance { get; private set; }

    [SerializeField] private Button joinButton;
    [SerializeField] private GameObject joinPanel;
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private TMP_Text loadingText;
    [SerializeField] private TMP_Text logoText;
    [SerializeField] private TMP_Dropdown inputModeDropdown;

    [SerializeField] private float dotsAnimationDuration = 0.15f;
    [SerializeField] public InputMode inputMode;

    private void Awake()
    {
        Instance = this;
        joinButton.onClick.AddListener(JoinGameUI);
        DiscordSdk.Instance.OnDiscordUserInfoReady += OnDiscordUserInfoReady;
        if (DiscordSdk.Instance.IsReady) OnDiscordUserInfoReady();
        joinButton.interactable = false;
        inputModeDropdown.onValueChanged.AddListener(SetInputMode);
        TriggerLogo(true);
        TriggerJoinPanel(false);
        TriggerLoadingPanel(true);

        SetLoadingText("Loading");
        StartCoroutine(AnimateLoadingText());
    }

    private void SetInputMode(int _) => inputMode = (InputMode)inputModeDropdown.value;


    private IEnumerator AnimateLoadingText()
    {
        yield return new WaitForSeconds(dotsAnimationDuration);
        loadingText.text = loadingText.text.Replace(".", "");
        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForSeconds(dotsAnimationDuration);
            loadingText.text = loadingText.text + ".";
        }

        StartCoroutine(AnimateLoadingText());
    }

    private void OnDiscordUserInfoReady() { joinButton.interactable = true; }

    private void JoinGameUI()
    {
        if (!DiscordSdk.Instance.IsReady) return;
        OnJoinGame(DiscordSdk.Instance.GetUser().GetNickname());
    }

    public void OnJoinGame(string nickname)
    {
        NetworkPlayer.Local.SetNickname(nickname);
        NetworkPlayer.Local.ResetPlayer();
        NetworkPlayer.Local.StartedPlaying(nickname);

        TriggerJoinPanel(false);
        TriggerLoadingPanel(false);
        TriggerLogo(false);
    }

    public void TriggerLoadingPanel(bool active) => loadingPanel.SetActive(active);

    public void TriggerJoinPanel(bool active)
    {
        joinPanel.SetActive(active);
    }

    public void TriggerLogo(bool active) => logoText.gameObject.SetActive(active);
    public void SetLoadingText(string msg) => loadingText.text = msg;
}