// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HoloToolkit.Examples.Prototyping
{
    public interface ITransitionTo
    {

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
}
