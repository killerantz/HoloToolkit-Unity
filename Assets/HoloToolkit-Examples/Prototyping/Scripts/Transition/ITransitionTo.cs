using HoloToolkit.Examples.Prototyping;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public interface ITransitionTo {

    /*
    GameObject TargetObject { get; set; }

    LerpTypes LerpType { get; set; }

    float LerpTime { get; set; }

    bool IsRunning { get; set; }

    */

    UnityEvent GetOnCompleteEvent();

    bool GetIsRunning();


    void Run();
    
    void ResetTransform();

    void Reverse();

    void Stop();
}
