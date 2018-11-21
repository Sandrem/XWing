﻿using Upgrade;

namespace Ship
{
    namespace SecondEdition.FangFighter
    {
        public class KadSolus : FangFighter
        {
            public KadSolus() : base()
            {
                PilotInfo = new PilotCardInfo(
                    "Kad Solus",
                    4,
                    54,
                    limited: 1,
                    abilityType: typeof(Abilities.SecondEdition.KadSolusAbility)
                );

                ShipInfo.UpgradeIcons.Upgrades.Add(UpgradeType.Elite);

                SEImageNumber = 158;
            }
        }
    }
}

namespace Abilities.SecondEdition
{
    //After you fully execute a red maneuver, gain 2 focus tokens.
    public class KadSolusAbility : Abilities.FirstEdition.KadSolusAbility
    {
        protected override bool CheckAbility()
        {
            if (HostShip.IsBumped) return false;

            return base.CheckAbility();
        }
    }
}