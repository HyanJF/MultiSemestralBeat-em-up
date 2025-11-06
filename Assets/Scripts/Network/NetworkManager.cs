using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FusionLauncher : MonoBehaviour, INetworkRunnerCallbacks
{
    private NetworkRunner _runner;
    public GameObject playerPrefab;

    async void Start()
    {
        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;
        _runner.AddCallbacks(this);

        var sceneRef = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);

        var result = await _runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.AutoHostOrClient,
            SessionName = "SalaDeFusion",
            Scene = sceneRef,  // ✅ Ahora usa SceneRef
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
            PlayerCount = 2
        });

        if (result.Ok)
            Debug.Log("Conectado correctamente a la sesión Fusion.");
        else
            Debug.LogError($"Error al iniciar sesión: {result.ShutdownReason}");
    }

    // =====================================================
    //                   CALLBACKS
    // =====================================================

    public void OnConnectedToServer(NetworkRunner runner)
        => Debug.Log("[Fusion] Conectado al servidor.");

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Jugador conectado: {player}");

        if (runner.IsServer)
        {
            Vector3 spawnPos = new Vector3(UnityEngine.Random.Range(-3, 3), 1, UnityEngine.Random.Range(-3, 3));
            runner.Spawn(playerPrefab, spawnPos, Quaternion.identity, player);
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        => Debug.Log($"Jugador salió: {player}");

    // 👇 Nuevas firmas requeridas por Fusion 2.x
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        => Debug.Log($"Desconectado del servidor: {reason}");

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }

    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { } // para compatibilidad con versiones intermedias
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
}
