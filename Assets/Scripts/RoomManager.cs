using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.VR;

public class RoomManager : MonoBehaviour
{
    
    [Serializable] public enum SceneType
    {
        TimedAndUnskippable,
        //TimedAndSkippable,
        ClickToSkip, //Default
    }

    [Serializable] struct SceneException
    {
        public SceneType Type;
        [Tooltip("Scene indexes start from 0. Do not exceed scene count!")]        
        public int Index;
        [Tooltip("Does not work for clickToSkip scenes!")]
        public float WaitTime;

        public static SceneException CreateSceneException(SceneType t, int i, float w)
        {
            SceneException se = new SceneException();
            se.Type = t;
            se.Index = i;
            se.WaitTime = w;
            return se;
        }
    }
    
    [Tooltip("Add scene exceptions by increasing array size, choosing the exception type and scene index. ClickToSkip is default, not needed to add as an exception. No duplicated indexes allowed")]
    [SerializeField] private SceneException[] _sceneExceptions;

    [SerializeField] private bool _scenesLoop = true;

    private int _sceneCount;
    private int _currentSceneIndex;

    private UnityAction _onSwipedForward;
    private UnityAction _onFadeIn;
    private UnityAction _onFadeOut;

    private AudioSource _audioSource;

    private static RoomManager _roomManager;
    //private bool _journeyFinished = false;

    [SerializeField] private float _renderScale = 1f;
    private Material _fadeMaterial = null;
    [SerializeField] private Shader _shader;

    [SerializeField] private Transform _canvasForFPS;
    [SerializeField] private Text _FPSCounter;
    [SerializeField] private bool _fpsCounterEnabled;

    private const int TargetFps =
#if UNITY_ANDROID // GEARVR
        60;
#else
        75;
#endif
    private const float UpdateInterval = 0.5f;

    private int _framesCount;
    private float _framesTime;

    public static RoomManager Instance
    {
        get
        {
            if (!_roomManager)
            {
                _roomManager = FindObjectOfType(typeof(RoomManager)) as RoomManager;

                if (!_roomManager)
                {
                    Debug.LogError("There needs to be one active EventManger script on a GameObject in your scene.");
                }
            }

            return _roomManager;
        }
    }


    void Awake()
    {
        _roomManager = this;
        DontDestroyOnLoad(this);
        if (_fpsCounterEnabled)
        {
            _canvasForFPS.gameObject.SetActive(true);
            DontDestroyOnLoad(_canvasForFPS);
            StartCoroutine(FPSCounter());
        }

        if(_shader != null) _fadeMaterial = new Material(_shader);
        else Debug.LogError("Assign a shader");


        _audioSource = GetComponent<AudioSource>();

        _sceneCount = SceneManager.sceneCountInBuildSettings;
        _currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        _onSwipedForward = new UnityAction(OnClickAction);
        _onFadeIn = new UnityAction(OnFadeIn);
        _onFadeOut = new UnityAction(OnFadeOut);

        VRSettings.renderScale = _renderScale;

        
        //if (!_journeyFinished)
        
    }

	// Use this for initialization
	void Start ()
	{
        //
    }

    private void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        EventManager.TriggerEvent("fadeIn");
        SceneException se = GetSceneException(_currentSceneIndex);
        switch (se.Type)
        {
            case SceneType.TimedAndUnskippable:
                StartCoroutine(DelayedLogoSceneSkip(se.WaitTime));
                break;
            case SceneType.ClickToSkip:
                StartCoroutine(DelayedResumeInputs());
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    void OnEnable()
    {
        EventManager.StartListening("swipeForward", _onSwipedForward);
        EventManager.StartListening("fadeIn", _onFadeIn);
        EventManager.StartListening("fadeOut", _onFadeOut);
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }

    void OnDisable()
    {
        EventManager.StopListening("swipeForward", _onSwipedForward);
        EventManager.StopListening("fadeIn", _onFadeIn);
        EventManager.StopListening("fadeOut", _onFadeOut);
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }

    private void OnFadeIn()
    {

    }
    
    private void OnFadeOut()
    {

    }

    private void OnClickAction()
    {
        EventManager.TriggerEvent("blockInputs");
        switch (GetExceptionType(_currentSceneIndex))
        {
            case SceneType.TimedAndUnskippable:
                break;
            case SceneType.ClickToSkip:
                _audioSource.Play();
                ChangeToNextRoom();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private SceneType GetExceptionType(int index)
    {
        for (int i = 0; i < _sceneExceptions.Length; i++)
        {
            if (_sceneExceptions[i].Index == index)
            {
                return _sceneExceptions[i].Type;
            }
        }
        return SceneType.ClickToSkip;
    }

    private SceneException GetSceneException(int index)
    {
        for (int i = 0; i < _sceneExceptions.Length; i++)
        {
            if (_sceneExceptions[i].Index == index)
            {
                return _sceneExceptions[i];
            }
        }
        return SceneException.CreateSceneException(SceneType.ClickToSkip, index, 0f);
    }


    private void ChangeToNextRoom()
    {
        if (!_scenesLoop && _currentSceneIndex == _sceneCount - 1) return;
        
        //code used to get scenes loop
        int nextSceneIndex = (_currentSceneIndex+1) % _sceneCount;
        if (nextSceneIndex == 0) nextSceneIndex++; //skip first scene

        EventManager.TriggerEvent("fadeOut");
        _currentSceneIndex = nextSceneIndex;
        StartCoroutine(ChangeSceneTo(nextSceneIndex));

    }

    public Material GetFadeMaterial()
    {
        return _fadeMaterial;
    }

    public Shader GetShader()
    {
        return _shader;
    }

    public int GetCurrentSceneIndex()
    {
        return _currentSceneIndex;
    }

    private IEnumerator ChangeSceneTo(int index)
    {
        yield return new WaitForSeconds(0.12f);
        AsyncOperation changingScene = SceneManager.LoadSceneAsync(index, LoadSceneMode.Single);

        changingScene.allowSceneActivation = false;
        while (!changingScene.isDone)
        {
            //float progress = Mathf.Clamp01(changingScene.progress/0.9f);
            //Debug.Log("Loading progress: " + (progress * 100) + "%");

            // Loading completed
            if (changingScene.progress >= 0.9f) // NECESSARY EVIL
            {
                Debug.Log("loaded!!");
                changingScene.allowSceneActivation = true;
                break;
            }
            yield return null;
        }
    }

    private IEnumerator DelayedLogoSceneSkip(float time)
    {
        yield return new WaitForSeconds(time);
        ChangeToNextRoom();
    }

    private IEnumerator DelayedResumeInputs()
    {
        Debug.Log("delayed resume");
        yield return new WaitForSeconds(2f);
        EventManager.TriggerEvent("resumeInputs");
    }

    private IEnumerator FPSCounter()
    {
        while (_fpsCounterEnabled)
        {
            // monitoring frame counter and the total time
            _framesCount++;
            _framesTime += Time.unscaledDeltaTime;

            // measuring interval ended, so calculate FPS and display on Text
            if (_framesTime > UpdateInterval)
            {
                if (_FPSCounter != null)
                {
                    float fps = _framesCount/_framesTime;
                    _FPSCounter.text = System.String.Format("{0:F2} FPS", fps);
                    _FPSCounter.color = (fps > (TargetFps - 5) ? Color.green : (fps > (TargetFps - 30) ? Color.yellow : Color.red));
                }
                // reset for the next interval to measure
                _framesCount = 0;
                _framesTime = 0;
            }

            yield return new WaitForEndOfFrame();
        }
    }
}
