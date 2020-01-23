using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GigaFist
{
    public class SceneManager : MonoBehaviour
    {
        public static SceneManager instance;
        public CanvasGroup canvasGroup;
        public float fadeTime;
        public AnimationCurve fadeCurve = new AnimationCurve();

        // Start is called before the first frame update
        void Start()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void ChangeScene(SceneIndexes scene)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene((int)scene, LoadSceneMode.Additive);
        }

        private IEnumerator Fade(bool fadeOut, float transitionTime)
        {            
            //Assign starting alpha based off of desired fade
            float alpha = 0;

            for (float t = 0; t <= 1; t += 1 / (transitionTime / Time.deltaTime))
            {
                //Calculate Alpha value
                if (fadeOut)
                {
                    alpha = fadeCurve.Evaluate(t);
                }
                else
                {
                    alpha = 1 - fadeCurve.Evaluate(t);
                }
                
                //Apply alpha value to canvas group
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = alpha;
                }

                yield return new WaitForEndOfFrame();
            }
            yield return new WaitForEndOfFrame();
        }
    }

}
