﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SubPhases;

namespace MainPhases
{

    public class PlanningPhase : GenericPhase
    {

        public override void StartPhase()
        {
            Name = "Planning Phase";

            Phases.CurrentSubPhase = new RoundStartSubPhase();
            Phases.CurrentSubPhase.Start();
            Phases.CurrentSubPhase.Prepare();
            Phases.CurrentSubPhase.Initialize();
        }

        public override void NextPhase()
        {
            Selection.DeselectAllShips();

            GenericSubPhase subphase = Phases.StartTemporarySubPhaseNew("Notification", typeof(NotificationSubPhase), StartSystemsPhase);
            (subphase as NotificationSubPhase).TextToShow = "Systems";
            subphase.Start();
        }

        private void StartSystemsPhase()
        {
            Phases.CurrentPhase = new SystemsPhase();
            Phases.CurrentPhase.StartPhase();
        }

    }

}