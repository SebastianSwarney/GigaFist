using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GigaFist
{
    public class SceneManager : MonoBehaviour
    {
        //LoadScene should simply LOAD the scene
        //ChangeScenes should LOAD if not loaded, and then change to that scene (set as active scene)

        [Header("Scene Management Properties")]
        public static SceneManager instance;
        public bool unloadCurrentOnChange = true;
        public SceneIndexes currentScene = SceneIndexes.SAMPLE_SCENE;

        [Header("Loading Screen Properties")]
        public CanvasGroup loadingScreen;
        public bool animate = true;
        public float fadeTime;
        public AnimationCurve fadeCurve = new AnimationCurve();
        private bool loadingScreenVisible = false;
        private Coroutine transitionAnimation;

        [Space]
        public Slider progressBar;
        [Space]

        [Header("Tips Properties")]
        public bool showTips = true;
        public Text tipText;
        public CanvasGroup tipCanvasGroup;
        public List<string> tips;
        [Space]
        public float timeBetweenTips = 3;
        private Coroutine cycleTipsCoroutine;

        [HideInInspector]
        public List<AsyncOperation> loadingScenes;

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

            SetLoadingScreen(loadingScreenVisible, false);
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                SetLoadingScreen(!loadingScreenVisible, animate);
            }
        }

        public void LoadScene(SceneIndexes scene)
        {
            loadingScenes.Add(UnityEngine.SceneManagement.SceneManager.LoadSceneAsync((int)scene, LoadSceneMode.Additive));
        }

        public void ChangeScene(SceneIndexes scene)
        {
            if (unloadCurrentOnChange)
            {
                loadingScenes.Add(UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync((int)currentScene));
            }

            LoadScene(scene);

            StartCoroutine(TrackLoadProgress(scene, true));
        }

        public void SetLoadingScreen(bool visible, bool animate)
        {
            if (visible)
            {
                if (animate)
                {
                    if (transitionAnimation == null)
                    {
                        transitionAnimation = StartCoroutine(Fade(true, fadeTime));
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    loadingScreen.alpha = 1;
                }
                loadingScreenVisible = true;

                if (showTips)
                {
                    if (cycleTipsCoroutine == null)
                    {
                        cycleTipsCoroutine = StartCoroutine(CycleTips());
                    }
                }
            }
            else
            {
                if (animate)
                {
                    if (transitionAnimation == null)
                    {
                        transitionAnimation = StartCoroutine(Fade(false, fadeTime));
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    loadingScreen.alpha = 0;
                }
                loadingScreenVisible = false;
                StopCoroutine(CycleTips());
                cycleTipsCoroutine = null;
            }
        }

        public void UpdateLoadingBar(float progressValue)
        {
            if (progressBar != null)
            {
                progressBar.value = progressValue;
            }
        }

        public void UpdateTipText(string newText, bool visible)
        {
            if (tipText != null)
            {
                tipText.text = newText;
            }

            if (tipCanvasGroup != null)
            {
                tipCanvasGroup.alpha = visible == true ? 1 : 0;
            }
        }

        public IEnumerator TrackLoadProgress(SceneIndexes scene, bool setActiveOnLoad)
        {
            float loadProgress;
            for (int i = 0; i < loadingScenes.Count; i++)
            {
                while (loadingScenes[i].isDone == false)
                {
                    loadProgress = 0;

                    foreach (AsyncOperation operation in loadingScenes)
                    {
                        loadProgress += operation.progress;
                    }

                    loadProgress = loadProgress / loadingScenes.Count;

                    yield return null;
                }
            }
            UnityEngine.SceneManagement.SceneManager.SetActiveScene(UnityEngine.SceneManagement.SceneManager.GetSceneByBuildIndex((int)scene));
        }

        public IEnumerator CycleTips() //Responsible for selecting tips and transitioning them
        {
            while (loadingScreenVisible)
            {
                int chosenTip = Random.Range(0, tips.Count);

                if (chosenTip < tips.Count)
                {
                    UpdateTipText(tips[chosenTip], showTips);
                }
                else
                {
                    UpdateTipText("No tips!", showTips);
                }

                yield return new WaitForSeconds(timeBetweenTips);
            }
        }

        private IEnumerator Fade(bool fadeIn, float transitionTime)
        {
            //Assign starting alpha based off of desired fade
            float alpha = 0;

            for (float t = 0; t <= 1; t += 1 / (transitionTime / Time.deltaTime))
            {
                //Calculate Alpha value
                alpha = fadeIn == true ? fadeCurve.Evaluate(t) : 1 - fadeCurve.Evaluate(t);
                //Apply alpha value to canvas group
                if (loadingScreen != null)
                {
                    loadingScreen.alpha = alpha;
                }

                yield return new WaitForEndOfFrame();
            }
            yield return new WaitForEndOfFrame();
            transitionAnimation = null;
        }
    }

}
