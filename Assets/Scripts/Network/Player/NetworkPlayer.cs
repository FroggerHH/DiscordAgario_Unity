using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Fusion.Addons.Physics;
using MyBox;
using UnityEngine;
using UnityEngine.Serialization;
using static Utils;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class NetworkPlayer : NetworkBehaviour, IPlayerLeft
{
    public static NetworkPlayer Local { get; private set; }
    public static List<NetworkPlayer> All = new();
    public static ushort MaxSize => ushort.MaxValue;

    [field: Header("Network properties")]
    [Networked] [field: SerializeField, Fusion.ReadOnly]
    public ushort size { get; set; } = 1;

    [Networked, Capacity(15)] [field: SerializeField, Fusion.ReadOnly]
    public string nickname { get; set; }

    [Networked, SerializeField] [field: SerializeField, Fusion.ReadOnly]
    public Color playerColor { get; set; }

    [Networked, SerializeField] [field: SerializeField, Fusion.ReadOnly]
    public PlayerState playerState { get; set; }

    private PlayerState prevPlayerState;


    [Header("Component references"), Space]
    [Fusion.ReadOnly(InEditMode = false)] public PlayerVisual playerVisual;

    [Fusion.ReadOnly(InEditMode = false)] public MovementHandler movementHandler;
    [Fusion.ReadOnly(InEditMode = false)] public NetworkRigidbody2D networkRigidbody2D;

#if UNITY_EDITOR
    [Header("Editor debug")]
    [SerializeField, Fusion.ReadOnly]
    private ushort debugSize;
#endif

    [ButtonMethod(ButtonMethodDrawOrder.BeforeInspector)]
    private void Awake()
    {
        if (playerVisual == null) playerVisual = GetComponentInChildren<PlayerVisual>();
        if (networkRigidbody2D == null) networkRigidbody2D = GetComponentInChildren<NetworkRigidbody2D>();
    }

    public override void Spawned()
    {
        Info("Player spawned, has input auth: " + Object.InputAuthority);
        All.Add(this);

        if (Object.HasInputAuthority)
        {
            Local = this;
            LobbyUI.Instance.TriggerJoinPanel(true);
            LobbyUI.Instance.TriggerLoadingPanel(false);
            SetPlayerState(PlayerState.Connected);

            DiscordSdk.Instance.OnDiscordUserInfoReady += DiscordUserInfoReady;
            if (DiscordSdk.Instance.IsReady) DiscordUserInfoReady();
        }

        Runner.SetPlayerObject(Object.InputAuthority, Object);
    }

    private void DiscordUserInfoReady() { }

    public PlayerState GetState() => playerState;

    private void OnStateChanged()
    {
        if (prevPlayerState == playerState) return;
        Debug($"{(nickname.Length > 0 ? nickname : Object.Id)} OnStateChanged -> {playerState}");
        if (playerState == PlayerState.Playing)
        {
            if (Object.HasInputAuthority) Utils.Camera.transform.position = transform.position;
            playerVisual.SetVisualActive(true);
            UpdateVisual();
            movementHandler.enabled = true;
        } else
        {
            playerVisual.SetVisualActive(false);
            movementHandler.enabled = false;
        }

        prevPlayerState = playerState;
    }

    public void PlayerLeft(PlayerRef player)
    {
        Debug($"Player left: {player}");
        if (player == Object.InputAuthority) Runner.Despawn(Object);
        All.RemoveAll(x => x?.Id.IsValid != true);
    }

#if UNITY_EDITOR
    private void LateUpdate() { debugSize = size; }
#endif

    private void UpdateVisual()
    {
        name = $"Pl_{Object.Id}_'{nickname}'";
        playerVisual.UpdateVisual();
    }

    public void SetNickname(string nickname) => RPC_SetNickname(nickname);

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SetNickname(string nickname, RpcInfo info = default)
    {
        this.nickname = nickname;
        PlayerValuesChanged();
    }

    public void SetPlayerColor(Color color) => RPC_SetPlayerColor(color);

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SetPlayerColor(Color color, RpcInfo info = default)
    {
        this.playerColor = color;
        PlayerValuesChanged();
    }

    public void SetSize(ushort newSize) => RPC_SetSize(newSize);

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SetSize(ushort newSize, RpcInfo info = default)
    {
        this.size = newSize;
        PlayerValuesChanged();
    }

    public void ResetPlayer() => RPC_ResetPlayer();

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_ResetPlayer(RpcInfo info = default)
    {
        size = 1;
        playerState = PlayerState.Playing;
        playerColor = Random.ColorHSV(0, 1, 0.7f, 1, 0.5f, 1);
        var newPos = GetRandomPos();
        networkRigidbody2D.Teleport(newPos);

        PlayerValuesChanged();
    }

    public void SetPlayerState(PlayerState newState) => RPC_SetPlayerState(newState);

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SetPlayerState(PlayerState newState, RpcInfo info = default)
    {
        playerState = newState;
        PlayerValuesChanged();
    }

    public void PlayerValuesChanged() => RPC_Server_PlayerValuesChanged();

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_Server_PlayerValuesChanged(RpcInfo info = default)
    {
        OnStateChanged();
        UpdateVisual();
    }

    public void StartedPlaying(string nickname) => RPC_StartedPlaying(nickname);

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void RPC_StartedPlaying(string nickname, RpcInfo info = default)
    {
        Success($"{nickname} started playing");
        foreach (var pl in All) pl.UpdateVisual();
    }
}