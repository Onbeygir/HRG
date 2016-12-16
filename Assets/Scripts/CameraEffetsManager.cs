using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>
/// Must be attached to the camera
/// </summary>
public class CameraEffetsManager : MonoBehaviour
{

    private bool _isFading;
    private bool _faded;

    private Material _fadeMat;

    /// <summary>
	/// How long it takes to fade.
	/// </summary>
	[SerializeField]
    private float _fadeTime = 2.0f;

    /// <summary>
    /// The initial screen color.
    /// </summary>
    [SerializeField]
    private Color _fadeColor = new Color(0.01f, 0.01f, 0.01f, 1.0f);

    private YieldInstruction _fadeInstruction = new WaitForEndOfFrame();

    void Awake()
    {
        if (RoomManager.Instance != null)
            _fadeMat = RoomManager.Instance.GetFadeMaterial();
        else Debug.LogError("RoomManager is missing in the scene");

        if (!_fadeMat)
        {
            Debug.LogError("No fade material found");
        }
    }

    void OnEnable()
    {
        EventManager.StartListening("fadeIn", OnFadeIn);
        EventManager.StartListening("fadeOut", OnFadeOut);
    }

    void OnDisable()
    {
        EventManager.StopListening("fadeIn", OnFadeIn);
        EventManager.StopListening("fadeOut", OnFadeOut);
    }

    public void OnFadeIn()
    {
        //Debug.Log("start fade in");
        StartCoroutine(FadeIn());
    }

    public void OnFadeOut()
    {
        //Debug.Log("start fade out");
        StartCoroutine(FadeIn(true));
    }

    /// <summary>
    /// Fades alpha from 1.0 to 0.0 
    /// </summary>
    IEnumerator FadeIn(bool reverse = false)
    {
        _fadeMat.color = _fadeColor;
        Color color = _fadeColor;
        if (!reverse) color.a = 1;
        else color.a = 0;
        _isFading = true;
        _faded = reverse;
        float elapsedTime = 0.0f;
        while (elapsedTime < _fadeTime)
        {
            
            elapsedTime += Time.deltaTime;
            if (!reverse) color.a = 1 - Mathf.Clamp01(elapsedTime/_fadeTime);
            else color.a = Mathf.Clamp01(elapsedTime / _fadeTime);
            //Debug.Log(color.a);
            _fadeMat.color = color;
            yield return _fadeInstruction;
        }
        _isFading = false;
    }

    void OnPostRender()
    {
        if (_isFading || _faded)
        {            
            _fadeMat.SetPass(0);
            GL.Color(_fadeMat.color);
            GL.PushMatrix();
            GL.LoadOrtho();

            GL.Begin(GL.QUADS);
            GL.Vertex3(0f, 0f, -12f);
            GL.Vertex3(0f, 1f, -12f);
            GL.Vertex3(1f, 1f, -12f);
            GL.Vertex3(1f, 0f, -12f);
            GL.End();

            GL.PopMatrix();
        }
    }

    
}
