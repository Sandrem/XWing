﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class FinishDiceRollStep : IDiceRollStep
{
    public bool IsExecuted { get; set; }
    public Type SubphaseType { get; private set; }

    public FinishDiceRollStep(Type subphaseType)
    {
        SubphaseType = subphaseType;
    }

    public void Start()
    {
        IsExecuted = true;

        Phases.FinishSubPhase(SubphaseType);
    }

    public void Finish()
    {

    }
}
