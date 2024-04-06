using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using TMPro;
using UnityEditor;
using UnityEngine.Serialization;
using UnityEngine.UI;

[RequireComponent(typeof(NetworkPlayer))]
public class PlayerVisual : NetworkBehaviour
{
    [field: SerializeField, ReadOnly] [Networked]
    private Color32 nicknameColor { get; set; } = Color.white;

    [Header("Components")]
    [SerializeField, ReadOnly(InEditMode = false)] private GameObject visual;

    [SerializeField, ReadOnly(InEditMode = false)] private GameObject bodyVisual;
    [SerializeField, ReadOnly(InEditMode = false)] private GameObject nicknameVisual;
    [SerializeField, ReadOnly(InEditMode = false)] private Image avatarImage;
    [SerializeField, ReadOnly(InEditMode = false)] private TMP_Text nicknameText;
    [SerializeField, ReadOnly(InEditMode = false)] private NetworkPlayer networkPlayer;

    [Space]
    [SerializeField, ReadOnly(InEditMode = false)] private float maxNicknameVisualSize = 2.3f;

    public override void Spawned()
    {
        UpdateVisual();

        DiscordSdk.Instance.OnDiscordUserInfoReady += DiscordUserInfoReady;
        if (DiscordSdk.Instance.IsReady) DiscordUserInfoReady();
    }

    private void DiscordUserInfoReady() { UpdateVisual(); }

    public void UpdateVisual()
    {
        nicknameText.text = networkPlayer.nickname;
        nicknameText.color = nicknameColor;

        avatarImage.sprite = DiscordSdk.Instance.GetUser().avatar;
        if (HasInputAuthority && networkPlayer.playerColor != Color.white && avatarImage.sprite != null)
            networkPlayer.SetPlayerColor(Color.white);
        bodyVisual.transform.localScale = Vector3.one + Vector3.one * (networkPlayer.size / 10_00f);
        var clamp = Mathf.Clamp(bodyVisual.transform.localScale.x, 1f, maxNicknameVisualSize);
        nicknameVisual.transform.localScale = new(clamp, clamp, clamp);
        var nickScale = nicknameVisual.transform.localScale.magnitude;
        var bodyScale = bodyVisual.transform.localScale.magnitude;
        // if (nickScale < bodyScale)
        // {
        //     var tmpText = NetworkSpawner.Instance.playerPrefab.playerVisual.nicknameText;
        //     nicknameText.fontSizeMin = Mathf.Clamp((bodyScale - nickScale) * 0.5f, tmpText.fontSizeMin, tmpText.fontSizeMin);
        // }

        if (networkPlayer.GetState() == PlayerState.Playing) SetVisualActive(true);
        else SetVisualActive(false);
    }

    public void SetVisualActive(bool flag) => visual.SetActive(flag);

    public GameObject GetVisual() => visual;
    public GameObject GetBodyVisual() => bodyVisual;
    public GameObject GetNicknameVisual() => nicknameVisual;
}