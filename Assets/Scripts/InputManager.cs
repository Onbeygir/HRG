using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class InputManager : MonoBehaviour
{
    private UnityAction _blockedInput;
    private UnityAction _resumeInput;

    
    private bool _inputsBlocked;

    void Awake()
    {
        _blockedInput = new UnityAction(BlockInputs);
        _resumeInput= new UnityAction(ResumeInputs);
        _inputsBlocked = false;
    }

	// Use this for initialization
	void Start ()
	{
	    
	}


    void OnEnable()
    {
        EventManager.StartListening("blockInputs", _blockedInput);
        EventManager.StartListening("resumeInputs", _resumeInput);
    }

    void OnDisable()
    {
        EventManager.StopListening("blockInputs", _blockedInput);
        EventManager.StopListening("resumeInputs", _resumeInput);
    }

    private void BlockInputs()
    {
        _inputsBlocked = true;
    }

    private void ResumeInputs()
    {
        _inputsBlocked = false;
    }
	
	// Update is called once per frame
	void Update () {
        if(_inputsBlocked) return;

	    if (Input.GetMouseButtonDown(0))
	    {
	        EventManager.TriggerEvent("swipeForward");
	    }
	}

}
