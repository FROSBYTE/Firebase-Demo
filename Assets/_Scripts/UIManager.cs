using EasyTransition;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [SerializeField] TransitionSettings _transitionSettings;

    private void Awake()
    {
        instance = this;
    }

    public void SwitchScene()
    {
        TransitionManager.Instance().Transition("Main", _transitionSettings, 0);
    }
}
