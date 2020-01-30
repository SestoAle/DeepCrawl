using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class RogueAcademy : Academy
{
    public override void AcademyReset()
    {
        if (BoardManagerSystem.instance.isTraning)
        {
            BoardManagerSystem.instance.resetTraining();
            if(BoardManagerSystem.instance.doubleAgent)
            {
                List<Brain> brains = Academy.GetBrains(gameObject);

                foreach(Brain br in brains)
                {
                    if(br.brainType == BrainType.Internal)
                    {
                        ((CoreBrainInternal)br.coreBrain).updateGraphModel();
                    }
                }
            }
        }
    }
}
