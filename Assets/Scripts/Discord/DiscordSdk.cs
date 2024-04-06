using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using static Utils;

public class DiscordSdk : MonoBehaviour
{
    public static DiscordSdk Instance { get; private set; }
    private UserInfo user;
    public UserInfo GetUser() => user;
    public bool IsReady => !user.Equals(default);

    private void Awake() { Instance = this; }

    [DllImport("__Internal")]
    private static extern void InitDiscord();

    public Action OnDiscordUserInfoReady;

    public void SetUserInfo(string userInfoJson)
    {
        UserInfoRaw raw = JsonUtility.FromJson<UserInfoRaw>(userInfoJson);
        user = new UserInfo()
        {
            id = long.Parse(raw.id),
            displayName = raw.displayName,
            username = raw.username,
            guildId = long.Parse(raw.guildId),
            guildName = raw.guildName,
            channelId = long.Parse(raw.channelId),
            channelName = raw.channelName,
        };

        LoadImageFromWEB(raw.avatarUrl, (image) => user.avatar = image);
        Success($"User info loaded");
        OnDiscordUserInfoReady?.Invoke();
    }

    public void LogError(string error) => Error(error);
    public void LogInfo(string msg) => Info(msg);
    public void LogSuccess(string msg) => Success(msg);
    public void LogFail(string msg) => Success(msg);
    public void LogDebug(string msg) => Success(msg);

    private void Start()
    {
#if !UNITY_EDITOR
        InitDiscord();
#else
        UserInfoRaw testUserInfo = new UserInfoRaw()
        {
            id = "825413508031971399",
            displayName = "JustAFrogger",
            username = "justafrogger",
            avatarUrl =
                "https://cdn.discordapp.com/avatars/825413508031971399/1e1469bc30bc7c47dd6527913e637f9c.png?size=1024&format=webp&quality=lossless&width=0&height=256",
            channelId = "1047090734026739763",
            channelName = "First one",
            guildId = "981889916080381992",
            guildName = "JF-PLANET"
        };
        SetUserInfo(JsonUtility.ToJson(testUserInfo));
#endif
    }
}