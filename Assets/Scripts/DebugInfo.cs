using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugInfo : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        var mapSize = Utils.GetMapSize();
        Gizmos.color = Color.black;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(mapSize, mapSize, 0));
    }
}