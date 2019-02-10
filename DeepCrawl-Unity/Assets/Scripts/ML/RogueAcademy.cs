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
    }
  }
}
