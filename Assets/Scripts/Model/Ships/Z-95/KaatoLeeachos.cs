﻿using System;
using System.Collections.Generic;
using Ship;
using SubPhases;
using UnityEngine;
using Upgrade;
using Players;
using System.Linq;
using Tokens;
using RuleSets;
using BoardTools;
using Arcs;

namespace Ship
{
	namespace Z95
	{
		public class KaatoLeeachos : Z95, ISecondEditionPilot
		{
			public KaatoLeeachos() : base()
			{
				PilotName = "Kaa'to Leeachos";
				PilotSkill = 5;
				Cost = 15;

				IsUnique = true;

				PrintedUpgradeIcons.Add(UpgradeType.Elite);
				PrintedUpgradeIcons.Add(UpgradeType.Illicit);

				faction = Faction.Scum;

				PilotAbilities.Add(new Abilities.KaatoLeeachos());
            }

            public void AdaptPilotToSecondEdition()
            {
                PilotSkill = 3;
                Cost = 29;

                PilotAbilities.RemoveAll(ability => ability is Abilities.KaatoLeeachos);
                PilotAbilities.Add(new Abilities.SecondEdition.KaatoLeeachosSE());

                SEImageNumber = 170;
            }
        }
	}
}

namespace Abilities
{
	public class KaatoLeeachos : GenericAbility
	{
		public override void ActivateAbility()
		{
			Phases.Events.OnCombatPhaseStart_Triggers += RegisterAbility;
		}

		public override void DeactivateAbility()
		{
			Phases.Events.OnCombatPhaseStart_Triggers -= RegisterAbility;
		}

		private void RegisterAbility()
		{
			RegisterAbilityTrigger(TriggerTypes.OnCombatPhaseStart, Ability);
		}

		private void Ability(object sender, EventArgs e)
		{
			if (TargetsForAbilityExist (FilterAbilityTarget)) {
				Messages.ShowInfoToHuman ("Kaa'to Leeachos: Select a ship to remove Focus/Evade token from");

				SelectTargetForAbility (
					SelectAbilityTarget,
					FilterAbilityTarget,
					GetAiAbilityPriority,
					HostShip.Owner.PlayerNo,
                    true,
                    null,
                    HostShip.PilotName,
                    "Choose a ship to remove 1 focus or evade token from it and assign this token to yourself.",
                    HostShip.ImageUrl
                );
			} else {
					Triggers.FinishTrigger();
			}
		}

		protected virtual bool FilterAbilityTarget(GenericShip ship)
		{
			return FilterByTargetType(ship, new List<TargetTypes>() { TargetTypes.OtherFriendly }) && FilterTargetsByRange(ship, 1, 2) && FilterTargetWithTokens(ship);
		}

		protected bool FilterTargetWithTokens(GenericShip ship)
		{
			return (ship.Tokens.HasToken(typeof(Tokens.FocusToken)) || ship.Tokens.HasToken(typeof(Tokens.EvadeToken)));
		}

		private int GetAiAbilityPriority(GenericShip ship)
		{
            // This is the same AI logic used on PalobGodalhi and it might be good enough as-is
			int result = 0;

			int shipFocusTokens = ship.Tokens.CountTokensByType(typeof(Tokens.FocusToken));
			int shipEvadeTokens = ship.Tokens.CountTokensByType(typeof(Tokens.EvadeToken));

			result += ship.Cost + ship.UpgradeBar.GetUpgradesOnlyFaceup().Sum(n => n.Cost);
			if (shipFocusTokens > 0)
				result += 50;
			if (shipFocusTokens == 1) 
				result += 100;
			if (shipEvadeTokens > 0)
				result += 25;

			return result;
		}

		private void SelectAbilityTarget()
		{
			GenericShip thisship = TargetShip;
			int numfocustokens = thisship.Tokens.CountTokensByType (typeof(Tokens.FocusToken));
			int numevadetokens = thisship.Tokens.CountTokensByType (typeof(Tokens.EvadeToken));

			if (numfocustokens > 0 && numevadetokens == 0) {
				TakeFocus ();
			} else {
				if (numfocustokens == 0 && numevadetokens > 0) {
					takeEvade ();
				} else {
					if (numfocustokens > 0 && numevadetokens > 0) {
						AskWhichTokenToTake (takeFocusEventHandler, takeEvadeEventHandler);
					} else {
						SelectShipSubPhase.FinishSelection ();
					}
				}
			}
		}	

		private void TakeFocus() {
			TargetShip.Tokens.RemoveToken (
				typeof(FocusToken),
				delegate {
					HostShip.Tokens.AssignToken (
                        typeof(FocusToken),
						delegate {
							SelectShipSubPhase.FinishSelection();
						}
					);
				}
			);
		}

		private void takeEvade() {
			TargetShip.Tokens.RemoveToken (
				typeof(EvadeToken),
				delegate {
					HostShip.Tokens.AssignToken (
                        typeof(EvadeToken),
						delegate {
							SelectShipSubPhase.FinishSelection();
						}
					);
				}
			);
		}

		private void takeFocusEventHandler(object sender, EventArgs e) {
			TargetShip.Tokens.RemoveToken (
				typeof(FocusToken),
				delegate {
					HostShip.Tokens.AssignToken (
                        typeof(FocusToken),
						delegate {
							WhichTokenDecisionSubphase.ConfirmDecisionNoCallback();
							SelectShipSubPhase.FinishSelection();
						}
					);
				}
			);
		}

		private void takeEvadeEventHandler(object sender, EventArgs e) {
			TargetShip.Tokens.RemoveToken (
				typeof(EvadeToken),
				delegate {
					HostShip.Tokens.AssignToken (
                        typeof(EvadeToken),
						delegate {
							WhichTokenDecisionSubphase.ConfirmDecisionNoCallback();
							SelectShipSubPhase.FinishSelection();
						}
					);
				}
			);
		}

		private void AskWhichTokenToTake(EventHandler takeFocusHandler, EventHandler takeEvadeHandler, Action callback = null)
		{
			if (callback == null)
				callback = Triggers.FinishTrigger;

			if (HostShip.Owner.Type == PlayerType.Ai) {
				TakeFocus ();
			} else {

				DecisionSubPhase whichToken = (DecisionSubPhase)Phases.StartTemporarySubPhaseNew (
					                             Name,
					                             typeof(WhichTokenDecisionSubphase),
					                             callback
				                             );

				whichToken.InfoText = "Take which type of Token?";

				whichToken.RequiredPlayer = HostShip.Owner.PlayerNo;

				whichToken.AddDecision ("Focus", takeFocusHandler);
				whichToken.AddDecision ("Evade", takeEvadeHandler);

				whichToken.ShowSkipButton = false;

				whichToken.Start ();
			}
		}

		private class WhichTokenDecisionSubphase : DecisionSubPhase { }

	}
}

namespace Abilities.SecondEdition
{
    public class KaatoLeeachosSE : KaatoLeeachos
    {
        protected override bool FilterAbilityTarget(GenericShip ship)
        {
            return 
                FilterByTargetType(ship, new List<TargetTypes>() { TargetTypes.OtherFriendly }) &&
                FilterTargetsByRange(ship, 1, 2) &&
                FilterTargetWithTokens(ship);
        }
    }
}