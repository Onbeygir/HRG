using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GraphicEffect : MonoBehaviour
{
    [SerializeField] private bool _fadeIn = true;
    [SerializeField] private float _delayBeforeFade = 3;
    [SerializeField] private float _fadeTime = 1;

    

    [SerializeField] private MaskableGraphic[] _attachedMaskableGraphics;
    private Color[] _initialColors;

    private YieldInstruction _fadeInstruction = new WaitForEndOfFrame();


    void Awake()
    {
        if (_attachedMaskableGraphics == null)
        {
            Debug.LogError("Attach at least 1 text or image");
            return;
        }
        if (_attachedMaskableGraphics.Length == 0)
        {
            Debug.LogError("Attach at least 1 text or image");
            return;
        }
        _initialColors = new Color[_attachedMaskableGraphics.Length];
        int count = _attachedMaskableGraphics.Length;
        Color color;
        for (int i = 0; i < count; i++)
        {
            _initialColors[i] = _attachedMaskableGraphics[i].color;
            color = _initialColors[i];
            color.a = 0;
            _attachedMaskableGraphics[i].color = color;
        }
    }

    // Use this for initialization
    void Start()
    {
        if(_fadeIn) StartCoroutine(FadeIn());
    }


    private IEnumerator FadeIn()
    {
        yield return new WaitForSeconds(_delayBeforeFade);

        Color color;
        float elapsedTime = 0.0f;

        while (elapsedTime < _fadeTime)
        {
            int count = _attachedMaskableGraphics.Length;
            for (int i = 0; i < count; i++)
            {
                color = _attachedMaskableGraphics[i].color;
                color.a = (Mathf.Clamp01(elapsedTime/_fadeTime)*_initialColors[i].a);
                _attachedMaskableGraphics[i].color = color;
            }
            elapsedTime += Time.deltaTime;            
            yield return _fadeInstruction;
        }
    }

}
