using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ThreatConsideration", menuName = "UtilityAI/Considerations/Threat Consideration")]
public class ThreatConsideration : Consideration
{
    [SerializeField] private AnimationCurve responseCurve;
    public override float ScoreConsideration(PlayerAI playerAI)
    {
        score = responseCurve.Evaluate(Mathf.Clamp01(playerAI.threatValue));
        return score;
    }
}
