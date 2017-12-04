using System;
using UnityEngine;

public class TurnController : MonoBehaviour
{
  #region Constants
  const int actionPointsPerTurn = 3;
  #endregion

  #region Data
  public event Action onActionPointsChange;

  public event Action onTurnChange;

  int _numberOfActionsRemaining = actionPointsPerTurn;

  bool _isCurrentlyFirstPlayersTurn = true;

  /// <summary>
  /// Owned by the scene. Support RPC calls.
  /// </summary>
  PhotonView photonView;
  #endregion

  #region Properties
  /// <summary>
  /// Changing the turn resets action points and fires event.
  /// </summary>
  public bool isCurrentlyFirstPlayersTurn
  {
    get
    {
      return _isCurrentlyFirstPlayersTurn;
    }
    private set
    {
      if (isCurrentlyFirstPlayersTurn == value)
      {
        return;
      }

      _isCurrentlyFirstPlayersTurn = value;
      _numberOfActionsRemaining = actionPointsPerTurn;
      onTurnChange?.Invoke();
    }
  }

  /// <summary>
  /// Setting action points to 0 triggers a turn change via RPC.
  /// </summary>
  public int numberOfActionsRemaining
  {
    get
    {
      return _numberOfActionsRemaining;
    }
    set
    {
      if (numberOfActionsRemaining == value)
      {
        return;
      }

      photonView.RPC("SetActionPoints", PhotonTargets.All, value);
    }
  }
  #endregion

  #region Init
  protected void Awake()
  {
    photonView = GetComponent<PhotonView>();
  }
  #endregion

  #region Private
  /// <summary>
  /// Used by the smart property above.
  /// 
  /// RPC call.
  /// </summary>
  [PunRPC]
  void SetActionPoints(
    int value)
  {
    _numberOfActionsRemaining = value;
    onActionPointsChange?.Invoke();
    if (numberOfActionsRemaining <= 0)
    {
      isCurrentlyFirstPlayersTurn = !isCurrentlyFirstPlayersTurn;
    }
  }
  #endregion
}
