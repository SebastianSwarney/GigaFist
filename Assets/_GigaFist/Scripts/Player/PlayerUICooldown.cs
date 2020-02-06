using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUICooldown : MonoBehaviour
{
	private Text m_cooldownText;
	public Image m_cooldownImage;

	private void Start()
	{
		m_cooldownText = GetComponentInChildren<Text>();
	}

	public void DisplayCooldown(float p_cooldownTimer, float p_cooldownTime)
	{
		if (p_cooldownTimer < p_cooldownTime && m_cooldownText.gameObject.activeSelf == false)
		{
			m_cooldownText.gameObject.SetActive(true);
			m_cooldownImage.gameObject.SetActive(true);
		}

		float progress = p_cooldownTimer / p_cooldownTime;

		int displayTime = Mathf.RoundToInt(Mathf.Lerp(p_cooldownTime, 0, progress));
		m_cooldownText.text = displayTime.ToString();

		m_cooldownImage.fillAmount = progress;

		if (p_cooldownTimer == p_cooldownTime)
		{
			m_cooldownText.gameObject.SetActive(false);
			m_cooldownImage.gameObject.SetActive(false);
		}
	}
}
