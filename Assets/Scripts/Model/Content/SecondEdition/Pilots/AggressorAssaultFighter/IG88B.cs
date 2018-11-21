﻿using Upgrade;

namespace Ship
{
    namespace SecondEdition.AggressorAssaultFighter
    {
        public class IG88B : AggressorAssaultFighter
        {
            public IG88B() : base()
            {
                PilotInfo = new PilotCardInfo(
                    "IG-88B",
                    4,
                    70,
                    limited: 1,
                    abilityType: typeof(Abilities.FirstEdition.IG88BAbility)
                );

                ShipInfo.UpgradeIcons.Upgrades.Add(UpgradeType.Elite);

                SEImageNumber = 198;
            }
        }
    }
}