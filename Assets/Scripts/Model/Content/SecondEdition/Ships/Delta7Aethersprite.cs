﻿using System;
using System.Collections;
using System.Collections.Generic;
using Actions;
using ActionsList;
using Arcs;
using BoardTools;
using Movement;
using Ship;
using UnityEngine;
using Upgrade;

namespace Ship.SecondEdition.Delta7Aethersprite
{
    public class Delta7Aethersprite : GenericShip
    {
        public Delta7Aethersprite() : base()
        {
            ShipInfo = new ShipCardInfo
            (
                "Delta-7 Aethersprite",
                BaseSize.Small,
                Faction.Republic,
                new ShipArcsInfo(ArcType.Front, 2), 3, 3, 1,
                new ShipActionsInfo(
                    new ActionInfo(typeof(FocusAction)),
                    new ActionInfo(typeof(EvadeAction), ActionColor.Purple),
                    new ActionInfo(typeof(TargetLockAction)),
                    new ActionInfo(typeof(BarrelRollAction)),
                    new ActionInfo(typeof(BoostAction))
                ),
                new ShipUpgradesInfo(
                    UpgradeType.Title,
                    UpgradeType.Modification,
                    UpgradeType.Configuration,
                    UpgradeType.Astromech
                )
            );

            ShipAbilities.Add(new Abilities.SecondEdition.FineTunedControlsAbility());

            IconicPilots = new Dictionary<Faction, System.Type> {
                { Faction.Republic, typeof(ObiWanKenobi) }
            };

            ModelInfo = new ShipModelInfo(
                "Delta-7 Aethersprite",
                "Red",
                new Vector3(-3.75f, 7.85f, 5.55f),
                0.6f
            );

            DialInfo = new ShipDialInfo(
                new ManeuverInfo(ManeuverSpeed.Speed1, ManeuverDirection.Left, ManeuverBearing.Turn, MovementComplexity.Normal),
                new ManeuverInfo(ManeuverSpeed.Speed1, ManeuverDirection.Left, ManeuverBearing.Bank, MovementComplexity.Easy),
                new ManeuverInfo(ManeuverSpeed.Speed1, ManeuverDirection.Right, ManeuverBearing.Bank, MovementComplexity.Easy),
                new ManeuverInfo(ManeuverSpeed.Speed1, ManeuverDirection.Right, ManeuverBearing.Turn, MovementComplexity.Normal),

                new ManeuverInfo(ManeuverSpeed.Speed2, ManeuverDirection.Left, ManeuverBearing.Turn, MovementComplexity.Normal),
                new ManeuverInfo(ManeuverSpeed.Speed2, ManeuverDirection.Left, ManeuverBearing.Bank, MovementComplexity.Easy),
                new ManeuverInfo(ManeuverSpeed.Speed2, ManeuverDirection.Forward, ManeuverBearing.Straight, MovementComplexity.Easy),
                new ManeuverInfo(ManeuverSpeed.Speed2, ManeuverDirection.Right, ManeuverBearing.Bank, MovementComplexity.Easy),
                new ManeuverInfo(ManeuverSpeed.Speed2, ManeuverDirection.Right, ManeuverBearing.Turn, MovementComplexity.Normal),
                new ManeuverInfo(ManeuverSpeed.Speed2, ManeuverDirection.Left, ManeuverBearing.SegnorsLoop, MovementComplexity.Complex),
                new ManeuverInfo(ManeuverSpeed.Speed2, ManeuverDirection.Right, ManeuverBearing.SegnorsLoop, MovementComplexity.Complex),

                new ManeuverInfo(ManeuverSpeed.Speed3, ManeuverDirection.Left, ManeuverBearing.Bank, MovementComplexity.Normal),
                new ManeuverInfo(ManeuverSpeed.Speed3, ManeuverDirection.Forward, ManeuverBearing.Straight, MovementComplexity.Easy),
                new ManeuverInfo(ManeuverSpeed.Speed3, ManeuverDirection.Right, ManeuverBearing.Bank, MovementComplexity.Normal),

                new ManeuverInfo(ManeuverSpeed.Speed4, ManeuverDirection.Forward, ManeuverBearing.Straight, MovementComplexity.Normal),
                new ManeuverInfo(ManeuverSpeed.Speed4, ManeuverDirection.Forward, ManeuverBearing.KoiogranTurn, MovementComplexity.Complex),

                new ManeuverInfo(ManeuverSpeed.Speed5, ManeuverDirection.Forward, ManeuverBearing.Straight, MovementComplexity.Normal),
                new ManeuverInfo(ManeuverSpeed.Speed5, ManeuverDirection.Forward, ManeuverBearing.KoiogranTurn, MovementComplexity.Complex)
            );

            SoundInfo = new ShipSoundInfo(
                new List<string>()
                {
                    "XWing-Fly1",
                    "XWing-Fly2",
                    "XWing-Fly3"
                },
                "XWing-Laser", 2
            );

            // TODO: AI
            HotacManeuverTable = new AI.EscapeCraftTable();

            ManeuversImageUrl = "https://vignette.wikia.nocookie.net/xwing-miniatures-second-edition/images/f/f3/Maneuver_delta-7_aethersprite.png";
        }
    }
}


namespace Abilities.SecondEdition
{
    //After you fully execute a maneuver, you may spend 1 force to perform a barrel roll or boost action.
    public class FineTunedControlsAbility : GenericAbility
    {
        public override string Name { get { return "Fine-Tuned Controls"; } }

        public override void ActivateAbility()
        {
            HostShip.OnMovementFinish += RegisterTrigger;
        }

        public override void DeactivateAbility()
        {
            HostShip.OnMovementFinish -= RegisterTrigger;
        }

        private void RegisterTrigger(GenericShip ship)
        {
            if (HostShip.State.Force > 0 && !(HostShip.IsStressed || Board.IsOffTheBoard(HostShip) || HostShip.IsBumped))
            {
                RegisterAbilityTrigger(TriggerTypes.OnMovementFinish, AskPerformRepositionAction);
            }
        }
        
        private void AskPerformRepositionAction(object sender, System.EventArgs e)
        {
            if (HostShip.State.Force > 0)
            {
                Messages.ShowInfoToHuman("Fine-Tuned Controls: You may spend 1 force to perform a barrel roll or boost action");
                HostShip.BeforeActionIsPerformed += PayForceCost;

                HostShip.AskPerformFreeAction(
                    new List<GenericAction>()
                    {
                    new BoostAction(),
                    new BarrelRollAction()
                    },
                    CleanUp
                );
            }
            else
            {
                Triggers.FinishTrigger();
            }
        }


        private void PayForceCost(GenericAction action, ref bool isFreeAction)
        {
            HostShip.State.Force--;
            HostShip.BeforeActionIsPerformed -= PayForceCost;
        }

        private void CleanUp()
        {
            HostShip.BeforeActionIsPerformed -= PayForceCost;
            Triggers.FinishTrigger();
        }
    }
}
