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
            loadingScenes = new List<AsyncOperation>();
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                SetLoadingScreen(!loadingScreenVisible, animate);
            }

            if (Input.GetMouseButtonDown(1))
            {
                ChangeScene(SceneIndexes.SAMPLE_SCENE);
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

            SetLoadingScreen(true, true);
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
                tipText.text = "Tip: " + newText;
            }

            if (tipCanvasGroup != null)
            {
                tipCanvasGroup.alpha = visible == true ? 1 : 0;
            }
        }

        public void UpdateTipText(int textIndex, bool visible)
        {
            if (tipText != null)
            {
                tipText.text = string.Format("Tip #{0}: {1}", (textIndex + 1).ToString(), tips[textIndex]);
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

                    UpdateLoadingBar(loadProgress);
                    yield return null;
                }
            }
            if (setActiveOnLoad)
            {
                UnityEngine.SceneManagement.SceneManager.SetActiveScene(UnityEngine.SceneManagement.SceneManager.GetSceneByBuildIndex((int)scene));
            }
        }

        public IEnumerator CycleTips() //Responsible for selecting tips and transitioning them
        {
            bool transition = true;
            float timer = 0;
            int targetTextIndex = 0;
            bool firstLoad = true;
            UpdateTipText(Random.Range(0, tips.Count), true);

            while (loadingScreenVisible)
            {
                if (timer >= timeBetweenTips) //If timer has reached its time, get a new target text
                {
                    targetTextIndex = Random.Range(0, tips.Count);
                    transition = true;
                    timer = 0;
                }

                if (firstLoad)
                {
                    firstLoad = false;
                    transition = false;
                }

                if (transition == true && showTips)
                {
                    //First, fade out

                    if (tipCanvasGroup != null)
                    {
                        for (float t = 0; t <= 1; t += 1 / (fadeTime / Time.deltaTime))
                        {
                            tipCanvasGroup.alpha = 1 - fadeCurve.Evaluate(t);
                            yield return new WaitForEndOfFrame();
                        }
                    }

                    //Second, apply new text
                    UpdateTipText(targetTextIndex, false);

                    //Third, fade back in
                    if (tipCanvasGroup != null)
                    {
                        for (float t = 0; t <= 1; t += 1 / (fadeTime / Time.deltaTime))
                        {
                            tipCanvasGroup.alpha = fadeCurve.Evaluate(t);
                            yield return new WaitForEndOfFrame();
                        }
                    }

                    //Lastly, turn off transition
                    transition = false;
                }

                if (showTips == false)
                {
                    Coroutine temp = cycleTipsCoroutine;
                    cycleTipsCoroutine = null;
                    StopCoroutine(temp);
                }

                timer += Time.deltaTime;
                yield return new WaitForEndOfFrame();
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
