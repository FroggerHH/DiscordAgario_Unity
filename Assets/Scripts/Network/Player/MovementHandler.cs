using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.Serialization;
using static Utils;
using Random = UnityEngine.Random;

[RequireComponent(typeof(NetworkPlayer))]
public class MovementHandler : NetworkBehaviour
{
    private Vector2 inputDirection = Vector2.zero;
    [SerializeField, ReadOnly(InEditMode = false)] private Rigidbody2D rb;
    [SerializeField, ReadOnly(InEditMode = false)] private CircleCollider2D collider;
    [SerializeField] private float basicMoveSpeed = 150;
    [SerializeField, ReadOnly(InEditMode = false)] private NetworkPlayer networkPlayer;
# if UNITY_EDITOR
    [SerializeField, ReadOnly] private float debugSpeed;
# endif

    private void Awake()
    {
        if (!rb) rb = gameObject.AddComponent<Rigidbody2D>();
        if (!networkPlayer) networkPlayer = GetComponent<NetworkPlayer>();
        if (!collider) collider = gameObject.GetComponentInChildren<CircleCollider2D>();

        networkPlayer.movementHandler = this;
    }

    public void LateUpdate() { UpdateCamera(); }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData input)) inputDirection = input.movementInput;

        DoMovement();
    }

    private void DoMovement()
    {
        if (Object.HasStateAuthority)
        {
            var movementDir = inputDirection;
            movementDir.Normalize();

            var mapSize_modif = GetMapSize() / 2;
            var transformLocalScale = networkPlayer.playerVisual.GetBodyVisual().transform.localScale
                                      + new Vector3(0.5f, 0.5f, 0.5f);
            var transformPosition = transform.position;
            if (transformPosition.x < -mapSize_modif + transformLocalScale.x / 2 && movementDir.x < 0 ||
                transformPosition.x > mapSize_modif - transformLocalScale.x / 2 && movementDir.x > 0)
            {
                movementDir.x = 0;
                rb.velocity = new(0, rb.velocity.y);
            }

            if (transformPosition.y + collider.radius < -mapSize_modif + transformLocalScale.y / 2 && movementDir.y < 0
                ||
                transformPosition.y > mapSize_modif - transformLocalScale.y / 2 && movementDir.y > 0)
            {
                movementDir.y = 0;
                rb.velocity = new(rb.velocity.x, 0);
            }

            var moveSpeed = (networkPlayer.size / Mathf.Pow(networkPlayer.size, 1.1f)) * basicMoveSpeed;

            rb.velocity = movementDir.normalized * moveSpeed * Runner.DeltaTime;
            CollisionCheck();

#if UNITY_EDITOR
            debugSpeed = moveSpeed;
#endif
        }
    }

    private void UpdateCamera()
    {
        if (Object.HasInputAuthority)
        {
            var camera = Utils.Camera;
            var visualTransform = networkPlayer.playerVisual.GetBodyVisual().transform;
            var aspectRatio = camera.aspect;
            var ortoSize = (visualTransform.localScale.x + 7) / aspectRatio;

            camera.transform.position = Vector3.Slerp(camera.transform.position,
                new Vector3(visualTransform.position.x, visualTransform.position.y, 0), Runner.DeltaTime);
            camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, ortoSize, Runner.DeltaTime * 0.1f);

            if (Input.GetKeyDown(KeyCode.Space)) networkPlayer.PlayerValuesChanged();
        }
    }

    private void CollisionCheck()
    {
        collider.enabled = false;
        var visual = networkPlayer.playerVisual.GetVisual().transform;
        var hitcollider = Runner.GetPhysicsScene2D().OverlapCircle(visual.position, (visual.localScale.x / 2) * 0.8f);
        collider.enabled = true;
        if (!hitcollider) return;

        if (hitcollider.CompareTag("Food") && IsFullInside(hitcollider))
        {
            var food = hitcollider.GetComponent<NetworkFood>();
            networkPlayer.size = (ushort)Mathf.Clamp(networkPlayer.size + food.grouth, networkPlayer.size,
                NetworkPlayer.MaxSize);
            food.transform.position = GetRandomPos();
            food.grouth = NetworkSpawner.Instance.GetRandomFoodGrouth();
            food.UpdateSize_AllClients();
            networkPlayer.PlayerValuesChanged();
        } else if (hitcollider.CompareTag("Player") && IsFullInside(hitcollider))
        {
            var otherPlayer = hitcollider.GetComponentInParent<NetworkPlayer>();

            if (otherPlayer.size < networkPlayer.size)
            {
                networkPlayer.size = (ushort)Mathf.Clamp(networkPlayer.size + otherPlayer.size * 0.4f,
                    networkPlayer.size + 20, NetworkPlayer.MaxSize);

                Kill(otherPlayer);
            } else
            {
                otherPlayer.size = (ushort)Mathf.Clamp(otherPlayer.size + networkPlayer.size * 0.4f,
                    otherPlayer.size + 20, NetworkPlayer.MaxSize);

                Kill(networkPlayer);
            }
        }
    }

    private bool IsFullInside(Collider2D other) =>
        collider.bounds.Contains(other.bounds.max) && collider.bounds.Contains(other.bounds.min);


    private void Kill(NetworkPlayer player)
    {
        player.size = 0;
        player.playerState = PlayerState.Dead;
        player.PlayerValuesChanged();
    }
}