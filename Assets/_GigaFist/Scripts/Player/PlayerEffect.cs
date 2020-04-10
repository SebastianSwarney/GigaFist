using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEffect : MonoBehaviour
{

    public GameObject onHitEffectPrefab;
    public GameObject onDamageEffectPrefab;


    public void SpawnHitEffect(int id)
    {
        if (onHitEffectPrefab != null)
        {
            Instantiate(onHitEffectPrefab, transform.position, Quaternion.identity);
        }
    }

    public void SpawnDamageEffect(int id)
    {
        if (onDamageEffectPrefab != null)
        {
            Instantiate(onDamageEffectPrefab, transform.position, Quaternion.identity);
        }
    }
}
