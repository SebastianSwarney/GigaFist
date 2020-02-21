using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGraphicsController : MonoBehaviour
{
    private PlayerController m_playerController;
    private Animator m_animator;

    private void Start()
    {
        m_playerController = GetComponent<PlayerController>();
        m_animator = GetComponent<Animator>();
    }

    void Update()
    {
        m_animator.SetBool("IsGrounded", m_playerController.IsGrounded());

        m_animator.SetFloat("AirVelocityY", -m_playerController.m_velocity.y);

        Vector3 reletiveVelocity = transform.InverseTransformDirection(m_playerController.m_velocity);

        m_animator.SetFloat("Forward", reletiveVelocity.z);
        m_animator.SetFloat("Strafe", reletiveVelocity.x);
    }
}
