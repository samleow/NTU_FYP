using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Consideration : ScriptableObject
{
    public string Name;
    [SerializeField] protected AnimationCurve responseCurve;

    private float _score;
    public float score
    {
        get { return _score; }
        set
        {
            this._score = Mathf.Clamp01(value);
        }
    }

    public virtual void Awake()
    {
        score = 0;
    }

    public virtual AnimationCurve GetAnimationCurve()
    {
        return responseCurve;
    }

    public abstract float ScoreConsideration(PlayerAI playerAI);
}