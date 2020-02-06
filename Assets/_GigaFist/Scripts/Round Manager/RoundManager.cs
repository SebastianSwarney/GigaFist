﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundManager : MonoBehaviour
{
	public static RoundManager Instance;

	public int m_roundStartCountdownTime;

	public int m_numberOfPlayers;

	public Vector3[] m_spawnPositions;
	public GameObject m_playerPrefab;

	private List<PlayerController> m_players = new List<PlayerController>();

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Destroy(this);
		}
	}

	private void Start()
	{
		StartCoroutine(RunRoundStartUp());
	}

	private IEnumerator RunRoundStartUp()
	{
		SpawnPlayers();
		FreezePlayers();

		yield return new WaitForSeconds(m_roundStartCountdownTime);

		UnFreezePlayers();
	}

	private void SpawnPlayers()
	{
		for (int i = 0; i < m_numberOfPlayers; i++)
		{
			m_players.Add(Instantiate(m_playerPrefab, m_spawnPositions[i], Quaternion.identity).GetComponent<PlayerController>());
			m_players[i].RunRoundSetup(i, m_numberOfPlayers);
		}
	}

	private void FreezePlayers()
	{
		foreach (PlayerController player in m_players)
		{
			player.FreezeSelf();
		}
	}

	private void UnFreezePlayers()
	{
		foreach (PlayerController player in m_players)
		{
			player.UnFreezeSelf();
		}
	}

	public void OnPlayerDeath()
	{
		CheckPlayers();
	}

	private void CheckPlayers()
	{
		int deadPlayers = 0;
		int alivePlayerIndex = 0;

		if (m_numberOfPlayers == 1)
		{
			Debug.Log("Shit no one won LMAO");

			return;
		}

		for (int i = 0; i < m_players.Count; i++)
		{
			if (m_players[i].m_states.m_aliveState == PlayerController.AliveState.IsDead)
			{
				deadPlayers++;
			}
			else
			{
				alivePlayerIndex = i;
			}
		}

		if (deadPlayers == m_numberOfPlayers - 1)
		{
			PlayerWon(alivePlayerIndex);
		}
	}

	private void PlayerWon(int p_winningPlayer)
	{
		Debug.Log("Player " + p_winningPlayer + " wins!!!!!!");
	}

	private void OnDrawGizmos()
	{
		foreach (Vector3 position in m_spawnPositions)
		{
			Gizmos.DrawWireSphere(position, 1f);
		}
	}
}
