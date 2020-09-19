﻿using UnityEngine;
using Mirror;

public class GameManager : NetworkBehaviour {

    [SerializeField] private int maxRound = 3;
    [SerializeField] private float maxRoundTime = 60f;
    [SerializeField] private float maxPrepareTime = 15f;

    [SyncVar] public bool isPlaying = false;
    [SyncVar] public int currentRound = 0;
    [SyncVar] public float currentRoundTime;
    [SyncVar] public float currentPrepareTime;

    [SerializeField] private RoundManager roundManager;

    public int MaxRound { get; }
    public int MaxRoundTime { get; }
    public int MaxPrepareTime { get; }

    public override void OnStartServer() {
        base.OnStartServer();
        this.currentPrepareTime = this.maxPrepareTime;
        this.currentRoundTime = this.maxRoundTime;
        roundManager.StartCoroutine("PrepareRound");
        Debug.Log("GameManager.OnStartServer()");
    }

    public void OnGUI() {

    }
}
