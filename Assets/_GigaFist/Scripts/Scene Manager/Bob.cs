using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GigaFist
{
    public class Bob : MonoBehaviour //Will bob objects in list up and down
    {
        public List<BobObject> targets;

        // Start is called before the first frame update
        void Start()
        {
            if (targets != null)
            {
                foreach (BobObject target in targets)
                {
                    if (target.obj != null)
                    {
                        target.origin = target.obj.transform.position;
                    }
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (targets != null)
            {
                foreach (BobObject target in targets)
                {
                    if (target.obj != null && target.origin != null)
                    {
                        if (target.animate)
                        {
                            target.obj.transform.position = target.origin + new Vector3(0, (Mathf.Sin(Time.time * Mathf.PI * target.frequency) * target.amplitude) * 100f, 0);
                        }
                    }
                }
            }
        }
    }
}

[System.Serializable]
public class BobObject
{
    public GameObject obj;
    public bool animate = true;
    [HideInInspector]
    public Vector3 origin;
    [Range(0.01f, 1.5f)]
    public float amplitude = 0.5f;
    [Range(0.01f, 1.5f)]
    public float frequency = 1f;
}