﻿using System.Collections;
using System.Collections.Generic;
using Movement;
using ActionsList;
using Actions;
using Arcs;
using Upgrade;
using Ship;
using Bombs;

namespace Ship
{
    namespace SecondEdition.TIESaBomber
    {
        public class TIESaBomber : FirstEdition.TIEBomber.TIEBomber, TIE
        {
            public TIESaBomber() : base()
            {
                ShipInfo.ShipName = "TIE/sa Bomber";

                ShipInfo.UpgradeIcons.Upgrades.Add(UpgradeType.System);

                ShipInfo.ActionIcons.AddActions(new ActionInfo(typeof(ReloadAction), ActionColor.Red));
                ShipInfo.ActionIcons.AddActions(new ActionInfo(typeof(BarrelRollAction), typeof(TargetLockAction)));

                ShipAbilities.Add(new Abilities.SecondEdition.NimbleBomber());

                IconicPilots[Faction.Imperial] = typeof(ScimitarSquadronPilot);

                DialInfo.AddManeuver(new ManeuverHolder(ManeuverSpeed.Speed3, ManeuverDirection.Forward, ManeuverBearing.KoiogranTurn), MovementComplexity.Complex);
                DialInfo.ChangeManeuverComplexity(new ManeuverHolder(ManeuverSpeed.Speed2, ManeuverDirection.Left, ManeuverBearing.Turn), MovementComplexity.Normal);
                DialInfo.ChangeManeuverComplexity(new ManeuverHolder(ManeuverSpeed.Speed2, ManeuverDirection.Right, ManeuverBearing.Turn), MovementComplexity.Normal);
            }
        }
    }
}

namespace Abilities.SecondEdition
{
    public class NimbleBomber : GenericAbility
    {
        public override void ActivateAbility()
        {
            HostShip.OnGetAvailableBombDropTemplates += AddNimbleBomberTemplates;
        }

        public override void DeactivateAbility()
        {
            HostShip.OnGetAvailableBombDropTemplates -= AddNimbleBomberTemplates;
        }

        private void AddNimbleBomberTemplates(List<BombDropTemplates> availableTemplates)
        {
            if (!availableTemplates.Contains(BombDropTemplates.Bank_1_Left)) availableTemplates.Add(BombDropTemplates.Bank_1_Left);
            if (!availableTemplates.Contains(BombDropTemplates.Bank_1_Right)) availableTemplates.Add(BombDropTemplates.Bank_1_Right);
        }
    }
}
