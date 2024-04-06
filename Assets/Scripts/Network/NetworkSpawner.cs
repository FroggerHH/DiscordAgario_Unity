using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using static Utils;

public class NetworkSpawner : SimulationBehaviour, INetworkRunnerCallbacks
{
    public static NetworkSpawner Instance { get; private set; }

    [Header("Player")] public NetworkPlayer playerPrefab;

    [Header("Food")]
    [SerializeField] NetworkFood foodPrefab;

    InputHandler inputHandler;

    private bool isFoodSpawned = false;
    [SerializeField] private int foodCount = 300;
    public RandomDistribution foodRandomDistribution;

    private void Start()
    {
        Instance = this;
        Debug($"NetworkSpawner Started");
    }

    async void TrySpawnFood()
    {
        if (isFoodSpawned) return;
        LobbyUI.Instance.SetLoadingText($"Spawning some food");
        await Task.Delay(1000);
        for (int i = 0; i < foodCount; i++)
        {
            var spawnedFood = Runner.Spawn(foodPrefab, Vector3.zero, Quaternion.identity);
            spawnedFood.transform.position = GetRandomPos();
            spawnedFood.grouth = GetRandomFoodGrouth();
            spawnedFood.UpdateSize();
        }

        isFoodSpawned = true;
    }

    public async void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Info($"Player joined: {player}");
        LobbyUI.Instance.SetLoadingText($"Final clean up");
        await Task.Delay(500);

        if (runner.IsServer)
        {
            runner.Spawn(playerPrefab, Vector3.zero, Quaternion.identity, player);
            TrySpawnFood();
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        if (!inputHandler && NetworkPlayer.Local) inputHandler = NetworkPlayer.Local.GetComponent<InputHandler>();

        if (inputHandler) input.Set(inputHandler.GetNetworkInput());
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        Debug($"NetworkSpawner - OnObjectExitAOI");
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        Debug($"NetworkSpawner - OnObjectEnterAOI");
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { Debug($"NetworkSpawner - OnPlayerLeft"); }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        Debug($"NetworkSpawner - OnInputMissing");
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug($"NetworkSpawner - OnShutdown");
    }

    public void OnConnectedToServer(NetworkRunner runner) { Debug($"NetworkSpawner - OnConnectedToServer"); }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        Debug($"NetworkSpawner - OnDisconnectedFromServer, reason: {reason}");
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        Debug($"NetworkSpawner - OnConnectRequest, runner: {runner}, request: {request}, token: {token}");
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        Debug($"NetworkSpawner - OnConnectFailed, runner: {runner}, remoteAddress: {remoteAddress}, reason: {reason}");
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        Debug($"NetworkSpawner - OnUserSimulationMessage, runner: {runner}, message: {message}");
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        Debug($"NetworkSpawner - OnSessionListUpdated, runner: {runner}, sessionList: {sessionList}");
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        Debug($"NetworkSpawner - OnCustomAuthenticationResponse, runner: {runner}, data: {data}");
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        Debug($"NetworkSpawner - OnHostMigration, runner: {runner}, hostMigrationToken: {hostMigrationToken}");
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
        Debug($"NetworkSpawner - OnReliableDataReceived, runner: {runner}, player: {player}, key: {key}, data: {data}");
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
        Debug(
            $"NetworkSpawner - OnReliableDataProgress, runner: {runner}, player: {player}, key: {key}, progress: {progress}");
    }

    public void OnSceneLoadDone(NetworkRunner runner) { Debug($"NetworkSpawner - OnSceneLoadDone, runner: {runner}"); }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        Debug($"NetworkSpawner - OnSceneLoadStart, runner: {runner}");
    }

    public ushort GetRandomFoodGrouth() => (ushort)(foodRandomDistribution.RandomInt() / 7);
}