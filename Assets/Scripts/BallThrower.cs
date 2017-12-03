﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallThrower : MonoBehaviour
{
  #region Data
  public delegate void OnPlayerSpawn(PhotonView photonView);
  public static event OnPlayerSpawn onPlayerSpawn;

  [SerializeField]
  float throwStrength;

  // Could probably be automatic somehow
  [Header("Screen positions")]
  [SerializeField]
  float minX;

  [SerializeField]
  float maxX;

  [SerializeField]
  float minY;

  [SerializeField]
  float maxY;

  // Ball
  GameObject ball;

  Rigidbody ballBody;

  bool holdingBall;

  Vector3 originalBallPosition;

  readonly LinkedList<Vector3> positionList = new LinkedList<Vector3>();

  [SerializeField]
  GameObject wand;

  float? whenBallWasReleased;

  [SerializeField]
  float timeTillBallReset = 6;

  PhotonView photonView;

  ParticleSystem ballParticleSystem;

  TurnController turnController;

  Player player;
  #endregion

  #region Init
  protected void Awake()
  {
    player = GetComponentInParent<Player>();
    turnController = GameObject.FindObjectOfType<TurnController>();
    photonView = GetComponent<PhotonView>();

    if (photonView.isMine)
    {
      ball = PhotonNetwork.Instantiate(GameManager.instance.currentBallPrefab.name, transform.position, transform.rotation, 0);
      PhotonView ballsView = ball.GetComponent<PhotonView>();
      ballsView.RequestOwnership();
      ball.GetComponent<Rigidbody>().useGravity = false;
      ballBody = ball.GetComponent<Rigidbody>();
      ballParticleSystem = ball.GetComponent<ParticleSystem>();
    }
    onPlayerSpawn?.Invoke(photonView);

    if (wand != null)
    {
      //ballBody.gameObject.SetActive(false);
    }
  }

  protected void Start()
  {
    if (photonView.isMine)
    {
      if (wand != null)
      {
        ballBody.transform.position = wand.transform.position;
      }
    }

    originalBallPosition = ball != null ? ball.transform.position : Vector3.zero;
  }
  #endregion

  #region Events
  protected void Update()
  {
    if (photonView.isMine == false
      || player.isMyTurn == false)
    {
      return;
    }
    ConsiderReloading();

    if (whenBallWasReleased == null)
    {
      //UIController.instance.arcMeter.UpdateArc();
    }

    Aim();

    Shoot();
  }
  #endregion

  void Aim()
  {
    if (Input.GetMouseButton(0))
    {
      if (!holdingBall)
      {
        holdingBall = true;

        ballBody.velocity = Vector3.zero;
        ballBody.useGravity = false;
      }
      else
      {
        SetBallPosition(Input.mousePosition);
        positionList.AddFirst(ball.transform.position);

        // Only store the last 10 positions - play with this value for results
        if (positionList.Count > 10)
        {
          positionList.RemoveLast();
        }
      }
    }
  }

  void Shoot()
  {
    if (Input.GetMouseButtonUp(0))
    {
      if (holdingBall)
      {
        Vector3 direction;
        if (GetThrowMagnitude(out direction) == false)
        {
          ball.transform.position = originalBallPosition;
          // NOT THROWN STUFF
        }
        else
        {
          ThrowBall(direction);
        }

        holdingBall = false;
        positionList.Clear();
      }
    }
  }

  void ConsiderReloading()
  {
    return; // TODO
    if (whenBallWasReleased == null)
    {
      return;
    }

    if (Time.timeSinceLevelLoad - whenBallWasReleased > timeTillBallReset
      || Input.GetMouseButtonDown(1))
    {
      whenBallWasReleased = null;
      ballBody.transform.position = originalBallPosition;
      ballBody.useGravity = false;
      ballBody.velocity = Vector3.zero;
      if (wand != null)
      {
        //ballBody.gameObject.SetActive(false);
        if (ballParticleSystem)
        {
          ballParticleSystem.Simulate(1, true, true);
          ballParticleSystem.Play();
        }
      }
      UIController.instance.EnableThrowUI();
    }
  }

  void ThrowBall(
    Vector3 direction)
  {
    Debug.Assert(ballBody != null);

    direction += transform.forward * direction.magnitude;

    direction = (direction + transform.forward * direction.magnitude) * throwStrength;

    if (player.isPlayer0 == false)
    {
      direction.z = -direction.z;
    }

    Debug.DrawRay(ball.transform.position, direction);

    ballBody.useGravity = true;
    ballBody.AddForce(direction, ForceMode.Impulse);

    turnController.numberOfActionsRemaining--;
  }

  bool GetThrowMagnitude(
    out Vector3 direction)
  {
    direction = Vector3.zero;

    // Need to hold for at least two frames
    if (positionList.Count < 2)
      return false;

    direction = positionList.First.Value - positionList.Last.Value;

    // Minimum strength of throw - Play with this value for results
    if (direction.magnitude < .25)
      return false;

    Debug.Log(direction.magnitude);

    return true;
  }

  void SetBallPosition(
    Vector2 mousePosition)
  {
    float maxX = this.maxX;
    float minX = this.minX;

    if (player.isPlayer0 == false)
    {
      maxX = this.minX;
      minX = this.maxX;
    }

    float percentX = mousePosition.x / Screen.width;
    float percentY = mousePosition.y / Screen.height;

    Vector3 ballPosition = new Vector3(minX + percentX * (maxX - minX), minY + percentY * (maxY - minY), originalBallPosition.z);
    ball.transform.position = ballPosition;
  }
}
