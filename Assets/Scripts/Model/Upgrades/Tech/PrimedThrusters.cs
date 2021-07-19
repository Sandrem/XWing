using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Upgrade;
using Abilities;
using Ship;
using Tokens;
using ActionsList;
using Movement;
using System.Linq;


namespace UpgradesList
{

	public class PrimedThrusters : GenericUpgrade
    {
		public PrimedThrusters() : base()
        {
            Types.Add(UpgradeType.Tech);
			Name = "Primed Thrusters";
            Cost = 1;

			UpgradeAbilities.Add(new PrimedThrustersAbility());
        }
		public override bool IsAllowedForShip(GenericShip ship)
		{
			return ship.ShipBaseSize == BaseSize.Small;         
		}
    }

}

namespace Abilities
{
	public class PrimedThrustersAbility : GenericAbility
    {
		
		//Stress Tokens do not prevent you from performing boost or barrel roll actions 
		//unless you have 3 or more stress tokens.
		public override void ActivateAbility()
        {
			HostShip.OnActionSubPhaseStart += PrimedThrustersBonus;
        }

		public override void DeactivateAbility()
		{
			HostShip.OnActionSubPhaseStart -= PrimedThrustersBonus;
		}

		private void PrimedThrustersBonus(GenericShip ship)
		{
			//Workaround for Red manuevers skipping actions because of stress
			if (ship.AssignedManeuver.ColorComplexity == Movement.ManeuverColor.Red && ship.Tokens.CountTokensByType (typeof(Tokens.StressToken)) < 3)
			{
				RegisterAbilityTrigger(TriggerTypes.OnShipMovementExecuted, PerformFreeAction);
			}

			//Allow to perform boost or barrel roll actions even while stressed
			ship.OnTryAddAvailableAction -= Rules.Stress.CanPerformActions;
			ship.OnTryAddAvailableAction += CanPerformActions;
		}

		private void PerformFreeAction(object sender, System.EventArgs e)
		{

			List<GenericAction> freeActions = new List<GenericAction> ();
			
			if (HostShip.GetAvailablePrintedActionsList().Any (n => n.GetType () == typeof(ActionsList.BoostAction)) || HostShip.GetAvailableFreeActionsList().Any( n => n.GetType () == typeof(ActionsList.BarrelRollAction)) ) {
				freeActions.Add (new BoostAction ());
			}			
			if (HostShip.GetAvailablePrintedActionsList().Any (n => n.GetType () == typeof(ActionsList.BarrelRollAction)) || HostShip.GetAvailableFreeActionsList().Any( n => n.GetType () == typeof(ActionsList.BarrelRollAction)) ) {
				freeActions.Add (new BarrelRollAction ());			
			}

			HostShip.AskPerformFreeAction(freeActions, Triggers.FinishTrigger);
		}

		public void CanPerformActions (GenericAction action, ref bool result)
		{

			if (action.Name == "Barrel Roll" || action.Name == "Boost") {
				if (HostShip.Tokens.CountTokensByType (typeof(Tokens.StressToken)) < 3) {
					result = true;
				} else {
					Messages.ShowError ("Primed Thrusters: Can't have 3 or more Stress tokens");
					result = Selection.ThisShip.CanPerformActionsWhileStressed || action.CanBePerformedWhileStressed;
				}
			} else {
				if (HostShip.Tokens.GetToken (typeof(StressToken)) != null) {					
					result = Selection.ThisShip.CanPerformActionsWhileStressed || action.CanBePerformedWhileStressed;
				}
			}
		}

	}
    
}
