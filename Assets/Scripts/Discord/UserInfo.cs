using System;
using UnityEngine;

[Serializable]
public struct UserInfoRaw
{
    public string id;
    public string displayName;
    public string username;
    public string avatarUrl;
    
    public string guildId;
    public string guildName;
    public string channelId;
    public string channelName;
}
[Serializable]
public struct UserInfo
{
    public long id;
    public string displayName;
    public string username;
    public Sprite avatar;
    
    public long guildId;
    public string guildName;
    public long channelId;
    public string channelName;

    public string GetNickname() => string.IsNullOrEmpty(displayName) ? username : displayName;
}