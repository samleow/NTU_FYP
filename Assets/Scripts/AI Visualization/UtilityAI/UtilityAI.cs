using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UtilityAI : MonoBehaviour
{
    public bool finishedDeciding { get; set; }
    public Action bestAction { get; set; }

    private PlayerAI playerAI;

    // Start is called before the first frame update
    void Start()
    {
        playerAI = PlayerAI.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        // Utility AI always runs in the background,
        // even when it is not selected !
        // TODO:
        if (bestAction == null)
            DecideBestAction(playerAI.actionsAvailable);
    }

    // Loop through all the available actions 
    // Give me the highest scoring action
    public void DecideBestAction(Action[] actionsAvailable)
    {
        float score = 0f;
        int nextBestActionIndex = 0;
        for (int i = 0; i < actionsAvailable.Length; i++)
        {
            if (ScoreAction(actionsAvailable[i]) > score)
            {
                nextBestActionIndex = i;
                score = actionsAvailable[i].score;
            }
        }

        bestAction = actionsAvailable[nextBestActionIndex];
        finishedDeciding = true;
        //playerAI.bestAction = bestAction;
    }

    // Loop through all the considerations of the action
    // Score all the considerations
    // Average the consideration scores ==> overall action score
    public float ScoreAction(Action action)
    {
        float score = 1f;
        for (int i = 0; i < action.considerations.Length; i++)
        {
            float considerationScore = action.considerations[i].ScoreConsideration(playerAI);
            score *= considerationScore;

            if (score == 0)
            {
                action.score = 0;
                return action.score; // No point computing further
            }
        }

        // Averaging scheme of overall score
        float originalScore = score;
        float modFactor = 1 - (1 / action.considerations.Length);
        float makeupValue = (1 - originalScore) * modFactor;
        action.score = originalScore + (makeupValue * originalScore);

        return action.score;
    }

}
