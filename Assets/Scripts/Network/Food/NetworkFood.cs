using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class NetworkFood : NetworkBehaviour
{
    public static List<NetworkFood> networkFoods = new();

    [SerializeField] private float divider;
    [SerializeField] private float adder;

    [Networked] public ushort grouth { get; set; } = 12;

    public override void Spawned() => networkFoods.Add(this);

    public override void Despawned(NetworkRunner runner, bool hasState) => networkFoods.Remove(this);

    public void UpdateSize() =>
        transform.localScale = new Vector3(grouth, grouth, grouth) / divider + adder * Vector3.one;

    public void UpdateSize_AllClients() => Rpc_UpdateSize_AllClients();

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void Rpc_UpdateSize_AllClients() => UpdateSize();
}