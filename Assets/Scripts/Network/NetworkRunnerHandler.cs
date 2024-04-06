using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ExitGames.Client.Photon;
using Fusion;
using Fusion.Photon.Realtime;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Utils;

public class NetworkRunnerHandler : MonoBehaviour
{
    public static NetworkRunnerHandler Instance { get; private set; }
    [SerializeField] private NetworkRunner networkRunnerPrefab;
    private NetworkRunner networkRunner;

    private void Awake()
    {
        var networkRunnerInScene = FindObjectOfType<NetworkRunner>();
        if (networkRunnerInScene) networkRunner = networkRunnerInScene;
    }

    private void Start()
    {
        Instance = this;
        if (!networkRunner) networkRunner = Instantiate(networkRunnerPrefab);
        networkRunner.name = "Network Runner";

        var gameMode = GameMode.AutoHostOrClient;
# if UNITY_EDITOR
        gameMode = GameMode.AutoHostOrClient;
        if (ParrelSync.ClonesManager.IsClone()) gameMode = GameMode.Client;
# elif UNITY_SERVER
        gameMode = GameMode.Server;
# endif

        LobbyUI.Instance.SetLoadingText("Connecting to server");
        Debug("Started Network Runner");
        InitializeNetworkRunner(networkRunner, gameMode, "MyGame", NetAddress.Any(),
            SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex));
    }

    protected virtual Task InitializeNetworkRunner(NetworkRunner runner, GameMode mode, string sessionName,
        NetAddress address, SceneRef scene)
    {
        var sceneManager = GetSceneManager(runner);
        runner.ProvideInput = true;
        Success("Starting the game");
        return runner.StartGame(new StartGameArgs
        {
            GameMode = mode,
            Address = address,
            Scene = scene,
            SessionName = sessionName,
            SceneManager = sceneManager
        });
    }

    INetworkSceneManager GetSceneManager(NetworkRunner runner)
    {
        var sceneManager = runner.GetComponents<MonoBehaviour>().OfType<INetworkSceneManager>().FirstOrDefault();

        sceneManager ??= runner.gameObject.AddComponent<NetworkSceneManagerDefault>();
        return sceneManager;
    }
}