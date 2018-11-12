﻿using System.Collections;
using System.Collections.Generic;
using Abilities;
using ActionsList;

namespace Ship
{
    namespace SecondEdition.XWing
    {
        public class LeevanTenza : XWing
        {
            public LeevanTenza() : base()
            {
                PilotInfo = new PilotCardInfo(
                    "Leevan Tenza",
                    3,
                    46,
                    limited: 1,
                    abilityType: typeof(LeevanTenzaAbility)
                );

                ModelInfo.SkinName = "Partisan";

                ShipInfo.UpgradeIcons.Upgrades.Add(Upgrade.UpgradeType.Elite);
                ShipInfo.UpgradeIcons.Upgrades.Add(Upgrade.UpgradeType.Illicit);

                SEImageNumber = 8;
            }
        }
    }
}

namespace Abilities
{
    public class LeevanTenzaAbility : GenericAbility
    {
        public override void ActivateAbility()
        {
            HostShip.OnActionIsPerformed += CheckLeevanTenzaAbility;
        }

        public override void DeactivateAbility()
        {
            HostShip.OnActionIsPerformed -= CheckLeevanTenzaAbility;
        }

        private void CheckLeevanTenzaAbility(GenericAction action)
        {
            if (action is BoostAction || action is BarrelRollAction)
            {
                RegisterAbilityTrigger(TriggerTypes.OnActionIsPerformed, AskToUseLeevanTenzaAbility);
            }
        }

        private void AskToUseLeevanTenzaAbility(object sender, System.EventArgs e)
        {
            HostShip.AskPerformFreeAction(new EvadeAction() { IsRed = true }, Triggers.FinishTrigger);
        }
    }
}
