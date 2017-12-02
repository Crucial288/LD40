﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkController : MonoBehaviour
{
  [SerializeField]
  float zPositionPlayer1;

  [SerializeField]
  float zPositionPlayer2;

  RoomOptions roomOptions;

  protected void Awake()
  {
    roomOptions = new RoomOptions()
    {
      MaxPlayers = 2
    };
    PhotonNetwork.ConnectUsingSettings("0.1");
  }

  protected void OnJoinedLobby()
  {
    PhotonNetwork.JoinRandomRoom(roomOptions.CustomRoomProperties, roomOptions.MaxPlayers);
  }

  protected void OnPhotonRandomJoinFailed()
  {
    PhotonNetwork.CreateRoom(null, roomOptions, TypedLobby.Default);
  }

  protected void OnJoinedRoom()
  {
    PhotonPlayer[] playerList = PhotonNetwork.playerList;

    print($"Welcome!  There are a total of {playerList.Length} players in the room");

    Vector3 position = ChangeBall.instance.currentBallPrefab.transform.position;
    if (playerList.Length == 1)
    {
      position.z = zPositionPlayer1;
    }
    else
    {
      return;
      position.z = zPositionPlayer2;
    }

    GameObject ball = PhotonNetwork.Instantiate(ChangeBall.instance.currentBallPrefab.name, position, transform.rotation, 0);
    PhotonView view = ball.GetComponent<PhotonView>();
    view.RequestOwnership();
    Debug.Assert(view.isMine);
  }
}
