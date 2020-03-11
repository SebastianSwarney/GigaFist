using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGraphicsController : MonoBehaviour
{
    private PlayerController m_playerController;
    private Animator m_animator;

	public Transform m_visualTilt;
	public float m_maxTiltAngle;

    private void Start()
    {
        m_playerController = GetComponent<PlayerController>();
        m_animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        m_animator.SetBool("IsGrounded", m_playerController.IsGrounded());

        m_animator.SetFloat("AirVelocityY", -m_playerController.m_velocity.y);

        Vector3 reletiveVelocity = transform.InverseTransformDirection(m_playerController.m_velocity);

        m_animator.SetFloat("Forward", reletiveVelocity.z);

		Vector3 reletiveSpeed = transform.InverseTransformDirection(m_playerController.m_velocity / m_playerController.m_baseMovementProperties.m_baseMovementSpeed);

		Quaternion forwardTilt = Quaternion.AngleAxis(m_maxTiltAngle * reletiveSpeed.z, Vector3.right);
		Quaternion sideTilt = Quaternion.AngleAxis(m_maxTiltAngle * -reletiveSpeed.x, Vector3.forward);

		m_visualTilt.transform.localRotation = forwardTilt * sideTilt;


		//m_animator.SetFloat("Strafe", reletiveVelocity.x);
	}
}
