using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PoweredUpConsideration", menuName = "UtilityAI/Considerations/Powered Up Consideration")]
public class PoweredUpConsideration : Consideration
{
    public override float ScoreConsideration(PlayerAI playerAI)
    {
        float poweredUp = playerAI.NumOfScaredGhosts();
        if (playerAI.PoweringDown())
            poweredUp *= 0.5f;
        poweredUp /= 4;
        score = responseCurve.Evaluate(Mathf.Clamp01(poweredUp));
        return score;
    }
}
