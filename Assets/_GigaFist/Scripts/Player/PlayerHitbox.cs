using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHitbox : MonoBehaviour
{
	public bool m_hitboxActive;

	public delegate void ColliderHitEvent(GameObject p_collidingObject);
	public event ColliderHitEvent m_colliderHitEvent;

	private void OnTriggerEnter(Collider other)
	{
		//Debug.Log(other.gameObject.name);

		m_colliderHitEvent(other.gameObject);
	}
}
