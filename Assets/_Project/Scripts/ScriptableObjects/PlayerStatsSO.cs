using UnityEngine;

[CreateAssetMenu(menuName = "Game/Player Stats")]
public class PlayerStatsSO : ScriptableObject
{
    [Header("Movement")]
    public float MoveSpeed = 5f;
    public float SprintSpeed = 8f;
    public float RotateSpeed = 10f;
    public float JumpHeight = 1.5f;
    public float Gravity = -20f;

    [Header("Health")]
    public int MaxHP = 100;

    [Header("Respawn")]
    public float RespawnDelay = 5f;
}