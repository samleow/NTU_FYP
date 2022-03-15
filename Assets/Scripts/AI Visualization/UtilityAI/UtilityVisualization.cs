using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class UtilityVisualization : MonoBehaviour
{
    // all actions
    Action[] _actions;
    // line renderers to plot graphs
    public LineRendererHUD plotGE_t;
    public LineRendererHUD plotGH_t;
    public LineRendererHUD plotPS_t;
    public LineRendererHUD plotGE_p;
    public LineRendererHUD plotGH_p;
    public LineRendererHUD plotPS_p;
    // lines to show current threat/powered up values
    public RectTransform line_t;
    public RectTransform line_p;

    PlayerAI playerAI;

    // grid size for graphs
    Vector2Int gridSize;

    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    void Init()
    {
        playerAI = PlayerAI.Instance;
        _actions = playerAI.actionsAvailable;
        gridSize = new Vector2Int(100, 100);
    }

    public void PlotGraphs()
    {
        Init();

        plotGE_t.color = plotGE_p.color = Color.red;
        plotGH_t.color = plotGH_p.color = Color.green;
        plotPS_t.color = plotPS_p.color = Color.yellow;

        plotGE_t.thickness = plotGE_p.thickness = 2;
        plotGH_t.thickness = plotGH_p.thickness = 2;
        plotPS_t.thickness = plotPS_p.thickness = 2;

        plotGE_t.gridSize = plotGE_p.gridSize = gridSize;
        plotGH_t.gridSize = plotGH_p.gridSize = gridSize;
        plotPS_t.gridSize = plotPS_p.gridSize = gridSize;

        PlotGraph(plotGE_t, GetConsideration(GetAction("Ghost Evading"), "Threat - Ghost Evading"));
        PlotGraph(plotGH_t, GetConsideration(GetAction("Ghost Hunting"), "Threat - Ghost Hunting"));
        PlotGraph(plotPS_t, GetConsideration(GetAction("Pellet Seeking"), "Threat - Pellet Seeking"));
        PlotGraph(plotGE_p, GetConsideration(GetAction("Ghost Evading"), "Powered Up - Ghost Evading"));
        PlotGraph(plotGH_p, GetConsideration(GetAction("Ghost Hunting"), "Powered Up - Ghost Hunting"));
        PlotGraph(plotPS_p, GetConsideration(GetAction("Pellet Seeking"), "Powered Up - Pellet Seeking"));

    }

    Action GetAction(string name)
    {
        return _actions.Where(a => a.Name == name).FirstOrDefault();
    }

    Consideration GetConsideration(Action action, string name)
    {
        return action.considerations.Where(a => a.Name == name).FirstOrDefault();
    }

    public void HighlightGraph()
    {
        plotGE_t.thickness = plotGE_p.thickness = 2;
        plotGH_t.thickness = plotGH_p.thickness = 2;
        plotPS_t.thickness = plotPS_p.thickness = 2;

        if (playerAI.utilityAI.bestAction.Name == "Ghost Evading")
        {
            plotGE_t.thickness = plotGE_p.thickness = 5;
        }
        else if (playerAI.utilityAI.bestAction.Name == "Ghost Hunting")
        {
            plotGH_t.thickness = plotGH_p.thickness = 5;
        }
        else if (playerAI.utilityAI.bestAction.Name == "Pellet Seeking")
        {
            plotPS_t.thickness = plotPS_p.thickness = 5;
        }

        plotGE_t.SetAllDirty();
        plotGE_p.SetAllDirty();
        plotGH_t.SetAllDirty();
        plotGH_p.SetAllDirty();
        plotPS_t.SetAllDirty();
        plotPS_p.SetAllDirty();

        line_t.pivot = new Vector2(playerAI.threatValue, 0.5f);
        line_p.pivot = new Vector2(playerAI.poweredUpValue, 0.5f);
    }

    void PlotGraph(LineRendererHUD lr, Consideration con)
    {
        lr.points = new List<Vector2>();
        for (int i = 0; i < gridSize.x; i++)
        {
            lr.points.Add(new Vector2(i,con.GetAnimationCurve().Evaluate((float)i/gridSize.x)*gridSize.y));
        }

    }

}
