using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class PlayerInput : MonoBehaviour
{
	public int m_playerId;

	private PlayerController m_playerController;
	private Player m_playerInputController;

	private bool m_lockLooking;

	private void Start()
	{
		m_playerController = GetComponent<PlayerController>();
		m_playerInputController = ReInput.players.GetPlayer(m_playerId);
	}

	private void Update()
	{
		GetInput();
	}

	public void GetInput()
	{
		Vector2 movementInput = new Vector2(m_playerInputController.GetAxis("MoveHorizontal"), m_playerInputController.GetAxis("MoveVertical"));

		if (Input.GetKeyDown(KeyCode.P))
		{
			m_lockLooking = !m_lockLooking;
		}

		if (!m_lockLooking)
		{
			Vector2 lookInput = new Vector2(m_playerInputController.GetAxis("LookHorizontal"), m_playerInputController.GetAxis("LookVertical"));
		}

		if (m_playerInputController.GetButtonDown("Jump"))
		{

		}

		if (m_playerInputController.GetButtonUp("Jump"))
		{

		}
	}
}
