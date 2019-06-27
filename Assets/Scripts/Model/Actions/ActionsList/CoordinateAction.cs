﻿using Actions;
using ActionsList;
using BoardTools;
using RulesList;
using Ship;
using SubPhases;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tokens;
using UnityEngine;

namespace Actions
{
    public class CoordinateActionData : EventArgs
    {
        public int MaxTargets { get; set; }
        public bool SameShipTypeLimit { get; set; }
        public bool SameActionLimit { get; set; }
        public bool OnlyNonLimited { get; set; }
        public GenericShip CoordinateProvider { get; private set; }

        public CoordinateActionData(GenericShip coordinateProvider)
        {
            CoordinateProvider = coordinateProvider;
            MaxTargets = 1;
        }
    }
}

namespace ActionsList
{

    public class CoordinateAction : GenericAction
    {
        public CoordinateAction()
        {
            Name = DiceModificationName = "Coordinate";
        }

        public override void ActionTake()
        {
            CoordinateActionData coordinateActionData = HostShip.CallCheckCoordinateModeModification();

            if (coordinateActionData.MaxTargets == 1)
            {
                CoordinateTargetSubPhase subphase = Phases.StartTemporarySubPhaseNew<CoordinateTargetSubPhase>(
                    "Select target for Coordinate",
                    Phases.CurrentSubPhase.CallBack
                );
                subphase.HostAction = this;
                subphase.Start();
            }
            else
            {
                CoordinateMultiTargetSubPhase subphase = Phases.StartTemporarySubPhaseNew<CoordinateMultiTargetSubPhase>(
                    "Select targets for Coordinate",
                    Phases.CurrentSubPhase.CallBack
                );
                subphase.HostAction = this;

                subphase.RequiredPlayer = HostShip.Owner.PlayerNo;

                subphase.Filter = FilterCoordinateTargets;
                subphase.MaxToSelect = coordinateActionData.MaxTargets;
                subphase.WhenDone = CoordinateTargets;
                subphase.CoordinateActionData = coordinateActionData;

                subphase.AbilityName = "Coordinate Action";
                subphase.Description = "Select another ships.\nThey perform free action.";

                subphase.Start();
            }
        }

        private void CoordinateTargets(Action callback)
        {
            Phases.CurrentSubPhase.Pause();

            foreach (GenericShip ship in Selection.MultiSelectedShips)
            {
                Triggers.RegisterTrigger(
                    new Trigger()
                    {
                        Name = string.Format("Coordinate {0}: {1}", ship.ShipId, ship.PilotInfo.PilotName),
                        TriggerType = TriggerTypes.OnCoordinateMultiTargetsAreSelected,
                        TriggerOwner = HostShip.Owner.PlayerNo,
                        EventHandler = delegate { CoordinateShipForMultiSelection(ship); }
                    }
                );
            }

            Triggers.ResolveTriggers(TriggerTypes.OnCoordinateMultiTargetsAreSelected, callback);
        }

        private void CoordinateShipForMultiSelection(GenericShip targetShip)
        {
            Selection.ThisShip.CallCoordinateTargetIsSelected(
                targetShip,
                delegate { PerformMultiCoordinateEffect(targetShip); }
            );
        }

        private void PerformMultiCoordinateEffect(GenericShip targetShip)
        {
            var coordinatingShip = Selection.ThisShip;
            Selection.ChangeActiveShip(targetShip);
            GenericAction currentAction = ActionsHolder.CurrentAction;

            Triggers.RegisterTrigger(
                new Trigger()
                {
                    Name = "Coordinate",
                    TriggerOwner = Selection.ThisShip.Owner.PlayerNo,
                    TriggerType = TriggerTypes.OnFreeActionPlanned,
                    EventHandler = delegate { PerformFreeAction(targetShip); }
                }
            );

            MovementTemplates.ReturnRangeRuler();

            Triggers.ResolveTriggers(TriggerTypes.OnFreeActionPlanned, (System.Action)delegate {
                Selection.ThisShip = coordinatingShip;
                ActionsHolder.CurrentAction = currentAction;
                Phases.FinishSubPhase(typeof(CoordinateTargetSubPhase));
                Triggers.FinishTrigger();
            });
        }

        protected virtual void PerformFreeAction(GenericShip targetShip)
        {
            targetShip.AskPerformFreeAction(
                GetPossibleActions(), 
                delegate {
                    Selection.ChangeActiveShip((Phases.CurrentSubPhase as CoordinateMultiTargetSubPhase).CoordinateActionData.CoordinateProvider);
                    Triggers.FinishTrigger();
                }
            );
        }

        protected virtual List<GenericAction> GetPossibleActions()
        {
            return Selection.ThisShip.GetAvailableActions();
        }

        private bool FilterCoordinateTargets(GenericShip ship)
        {
            return ship.Owner.PlayerNo == Selection.ThisShip.Owner.PlayerNo
                && Board.CheckInRange(Selection.ThisShip, ship, 1, 2, RangeCheckReason.CoordinateAction)
                && ship.CanBeCoordinated;
        }

        public override void RevertActionOnFail(bool hasSecondChance = false)
        {
            if (hasSecondChance)
            {
                UI.ShowSkipButton();
                UI.HighlightSkipButton();
            }
            else
            {
                Phases.GoBack();
            }
        }
    }

}

namespace SubPhases
{

    public class CoordinateTargetSubPhase : SelectShipSubPhase
    {
        public override void Prepare()
        {
            PrepareByParameters(
                SelectCoordinateTarget,
                FilterCoordinateTargets,
                GetAiCoordinatePriority,
                Selection.ThisShip.Owner.PlayerNo,
                false,
                "Coordinate Action",
                "Select another ship.\nIt performs free action."
            );
        }

        protected override void CancelShipSelection()
        {
            Rules.Actions.ActionIsFailed(TheShip, HostAction, ActionFailReason.WrongRange, true);
        }

        public override void SkipButton()
        {
            Rules.Actions.ActionIsFailed(TheShip, HostAction, ActionFailReason.WrongRange, false);
        }

        private int GetAiCoordinatePriority(GenericShip ship)
        {
            int result = 0;

            result += NeedTokenPriority(ship);
            result += ship.PilotInfo.Cost + ship.UpgradeBar.GetUpgradesOnlyFaceup().Sum(n => n.UpgradeInfo.Cost);

            return result;
        }

        private int NeedTokenPriority(GenericShip ship)
        {
            if (!ship.Tokens.HasToken(typeof(FocusToken))) return 100;
            if (ship.ActionBar.HasAction(typeof(EvadeAction)) && !ship.Tokens.HasToken(typeof(EvadeToken))) return 50;
            if (ship.ActionBar.HasAction(typeof(TargetLockAction)) && !ship.Tokens.HasToken(typeof(BlueTargetLockToken), '*')) return 50;
            return 0;
        }

        private bool FilterCoordinateTargets(GenericShip ship)
        {
            return ship.Owner.PlayerNo == Selection.ThisShip.Owner.PlayerNo
                && Board.CheckInRange(Selection.ThisShip, ship, 1, 2, RangeCheckReason.CoordinateAction)
                && ship.CanBeCoordinated;
        }

        private void SelectCoordinateTarget()
        {
            Selection.ThisShip.CallCoordinateTargetIsSelected(TargetShip, PerformCoordinateEffect);
        }

        private void PerformCoordinateEffect()
        {
            var coordinatingShip = Selection.ThisShip;
            Selection.ThisShip = TargetShip;
            GenericAction currentAction = ActionsHolder.CurrentAction;

            Triggers.RegisterTrigger(
                new Trigger()
                {
                    Name = "Coordinate",
                    TriggerOwner = Selection.ThisShip.Owner.PlayerNo,
                    TriggerType = TriggerTypes.OnFreeActionPlanned,
                    EventHandler = PerformFreeAction
                }
            );

            MovementTemplates.ReturnRangeRuler();

            Triggers.ResolveTriggers(TriggerTypes.OnFreeActionPlanned, (System.Action)delegate {
                Selection.ThisShip = coordinatingShip;
                ActionsHolder.CurrentAction = currentAction;
                Phases.FinishSubPhase(typeof(CoordinateTargetSubPhase));
                CallBack();
            });
        }

        public override void RevertSubPhase() { }

        protected virtual List<GenericAction> GetPossibleActions()
        {
            return Selection.ThisShip.GetAvailableActions();
        }

        protected virtual void PerformFreeAction(object sender, System.EventArgs e)
        {
            TargetShip.AskPerformFreeAction(GetPossibleActions(), Triggers.FinishTrigger);
        }

    }

    public class CoordinateMultiTargetSubPhase : MultiSelectionSubphase
    {
        public CoordinateActionData CoordinateActionData;
    }

}
