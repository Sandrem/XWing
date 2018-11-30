﻿using Tokens;
using Upgrade;

namespace Ship
{
    namespace SecondEdition.M3AInterceptor
    {
        public class LaetinAshera : M3AInterceptor
        {
            public LaetinAshera() : base()
            {
                PilotInfo = new PilotCardInfo(
                    "Laetin A'shera",
                    3,
                    35,
                    isLimited: true,
                    abilityType: typeof(Abilities.SecondEdition.LaetinAshera),
                    extraUpgradeIcon: UpgradeType.Elite,
                    seImageNumber: 185
                );
            }
        }
    }
}

namespace Abilities.SecondEdition
{
    public class LaetinAshera : GenericAbility
    {
        public override void ActivateAbility()
        {
            HostShip.OnAttackMissedAsAttacker += RegisterTrigger;
            HostShip.OnAttackMissedAsDefender += RegisterTrigger;
        }

        public override void DeactivateAbility()
        {
            HostShip.OnAttackMissedAsAttacker -= RegisterTrigger;
            HostShip.OnAttackMissedAsDefender -= RegisterTrigger;
        }

        private void RegisterTrigger()
        {
            RegisterAbilityTrigger(TriggerTypes.OnAttackMissed, delegate
            {
                HostShip.Tokens.AssignToken(typeof(EvadeToken), Triggers.FinishTrigger);
            });
        }
    }
}