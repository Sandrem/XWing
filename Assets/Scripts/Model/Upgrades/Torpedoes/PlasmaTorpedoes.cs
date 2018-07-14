﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Upgrade;
using Abilities;
using Ship;
using Conditions;
using System.Linq;
using SubPhases;
using ActionsList;
using UpgradesList;

namespace UpgradesList
{
    public class PlasmaTorpedoes : GenericSecondaryWeapon
    {
        public PlasmaTorpedoes() : base()
        {
            

            Types.Add(UpgradeType.Torpedo);

            Name = "Plasma Torpedoes";
            Cost = 3;

            MinRange = 2;
            MaxRange = 3;
            AttackValue = 4;

            RequiresTargetLockOnTargetToShoot = true;
            
            SpendsTargetLockOnTargetToShoot = true;
            IsDiscardedForShot = true;

            UpgradeAbilities.Add(new PlasmaTorpedoAbility());
        }
    }
}

namespace Abilities
{
    public class PlasmaTorpedoAbility : GenericAbility
    {
        public override void ActivateAbility()
        {
            HostShip.OnShotHitAsAttacker += PlanPlasmaTorpedoes;
        }

        public override void DeactivateAbility()
        {
            HostShip.OnCombatDeactivation += DeactivateAbilityPlanned;
        }

        private void DeactivateAbilityPlanned(GenericShip ship)
        {
            HostShip.OnCombatDeactivation -= DeactivateAbilityPlanned;

            HostShip.OnCombatDeactivation -= PlanShieldRemove;

            HostShip.OnShotHitAsAttacker -= PlanPlasmaTorpedoes;
        }

        private void PlanPlasmaTorpedoes()
        {
            if (Combat.ChosenWeapon is PlasmaTorpedoes)
            {
                HostShip.OnAttackFinishAsAttacker += PlanShieldRemove;

                HostShip.OnShotHitAsAttacker -= PlanPlasmaTorpedoes;
            }
        }

        private void PlanShieldRemove(GenericShip hostShip)
        {
            if (Combat.Defender.Shields != 0)
            { ShieldRemove(); }
        }
    
        private void ShieldRemove()
        {
            {
                Combat.Defender.AssignedDamageDiceroll.AddDice(DieSide.Success);
                Messages.ShowInfoToHuman(string.Format("{0} had a Shield removed by Plasma Torpedo", Combat.Defender.PilotName));
                Triggers.RegisterTrigger(
                    new Trigger()
                    {
                        Name = "Damage from Plasma Torpodoes",
                        TriggerType = TriggerTypes.OnDamageIsDealt,
                        TriggerOwner = Combat.Defender.Owner.PlayerNo,
                        EventHandler = Combat.Defender.SufferDamage,
                        EventArgs = new DamageSourceEventArgs()
                        {
                            Source = Combat.Attacker,
                            DamageType = DamageTypes.CardAbility
                        },
                    });
                Triggers.ResolveTriggers(TriggerTypes.OnDamageIsDealt);
            }
        }
    }
}