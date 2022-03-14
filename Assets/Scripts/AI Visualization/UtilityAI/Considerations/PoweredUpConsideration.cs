using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PoweredUpConsideration", menuName = "UtilityAI/Considerations/Powered Up Consideration")]
public class PoweredUpConsideration : Consideration
{
    [SerializeField] private AnimationCurve responseCurve;
    public override float ScoreConsideration(PlayerAI playerAI)
    {
        float poweredUp = playerAI.NumOfScaredGhosts();
        if (playerAI.PoweringDown())
            poweredUp *= 0.5f;
        score = responseCurve.Evaluate(Mathf.Clamp01(poweredUp/4));
        return score;
    }
}
