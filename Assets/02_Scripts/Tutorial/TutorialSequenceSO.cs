using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TutorialSequence", menuName = "Babel/Tutorial Sequence")]
public class TutorialSequenceSO : ScriptableObject
{
    public List<TutorialStepData> steps = new();
}
