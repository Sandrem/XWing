﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DamageDeckCardSE
{

    public class DirectHit : GenericDamageCard
    {
        public DirectHit()
        {
            Name = "Direct Hit";
            Type = CriticalCardType.Ship;
            AiAvoids = true;
            ImageUrl = "https://i.imgur.com/aOzGkNK.jpg";
        }

        public override void ApplyEffect(object sender, EventArgs e)
        {
            SufferAdditionalDamage();
        }

        private void SufferAdditionalDamage()
        {
            Messages.ShowInfo("Direct Hit: Suffer 1 additional damage");

            DamageSourceEventArgs directhitDamage = new DamageSourceEventArgs()
            {
                Source = this,
                SourceDescription = "Critical hit card",
                DamageType = DamageTypes.CriticalHitCard
            };

            Selection.ActiveShip.Damage.TryResolveDamage(1, directhitDamage, RepairDirectHit);
        }

        public void RepairDirectHit()
        {
            DiscardEffect();
            Triggers.FinishTrigger();
        }

        public override void DiscardEffect()
        {
            base.DiscardEffect();

            // Do nothing;
        }
    }

}