using System;
using UnityEngine;

public enum TutorialAdvanceType { ClickAnywhere, WaitForSignal }

[Serializable]
public class TutorialStepData
{
    [TextArea(2, 5)] public string description;
    public bool hideDescription;    // true = 이 스텝 동안 설명 패널 숨김 (신호 대기만 할 때)
    public TutorialTargetKey highlightTarget;
    public TutorialAdvanceType advanceType;

    // advanceType == WaitForSignal 일 때만 사용
    public TutorialSignal requiredSignal;

    // 두 신호 중 하나라도 오면 통과 (step 6처럼 Click/Drag 모두 허용할 때)
    public bool hasAlternateSignal;
    public TutorialSignal alternateRequiredSignal;

    // 특정 건물이 관련된 스텝에서만 지정 (null이면 어떤 건물이든 허용)
    public BuildingSO requiredBuilding;
}
