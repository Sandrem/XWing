using Upgrade;
using System.Linq;
using System.Collections.Generic;
using ActionsList;
using System;

namespace UpgradesList.SecondEdition
{
    public class R5Astromech : GenericUpgrade
    {
        public R5Astromech() : base()
        {
            UpgradeInfo = new UpgradeCardInfo(
                "R5 Astromech",
                UpgradeType.Astromech,
                cost: 4,
                abilityType: typeof(Abilities.SecondEdition.R5AstromechAbility),
                charges: 2,
                seImageNumber: 56
            );
        }
    }
}

namespace Abilities.SecondEdition
{
    //Action: Spend 1 charge to repair 1 facedown damage card.
    //Action: Repair 1 faceup Ship damage card.
    public class R5AstromechAbility : GenericAbility
    {
        public override void ActivateAbility()
        {
            HostShip.OnGenerateActions += AddAction;
        }

        public override void DeactivateAbility()
        {
            HostShip.OnGenerateActions -= AddAction;
        }

        private void AddAction(Ship.GenericShip ship)
        {
            if (ship.Damage.GetFacedownCards().Any() && HostUpgrade.State.Charges > 0)
            {
                ship.AddAvailableAction(new RepairAction(RepairAction.CardFace.FaceDown)
                {
                    ImageUrl = HostUpgrade.ImageUrl,
                    HostShip = HostShip,
                    PayRepairCost = () =>
                    {
                        var result = false;
                        if (HostUpgrade.State.Charges > 0)
                        {
                            HostUpgrade.State.SpendCharge();
                            result = true;
                        }
                        return result;
                    }
                });
            }
            if (ship.Damage.GetFaceupCrits(CriticalCardType.Ship).Any())
            {
                ship.AddAvailableAction(new RepairAction(RepairAction.CardFace.FaceUp, CriticalCardType.Ship)
                {
                    ImageUrl = HostUpgrade.ImageUrl,
                    HostShip = HostShip,
                    Source = this.HostUpgrade
                });
            }
        }
    }
}

namespace ActionsList
{
    public class RepairAction : GenericAction
    {
        public enum CardFace
        {
            FaceDown,
            FaceUp
        }

        public Func<bool> PayRepairCost = () => true;

        private readonly CardFace damageCardFace;
        private readonly CriticalCardType? criticalCardType;

        public RepairAction(CardFace face, CriticalCardType? type = null)
        {
            damageCardFace = face;
            criticalCardType = type;

            DiceModificationName = Name = "Repair 1 " + face.ToString().ToLower() + (type != null ? " " + type.ToString() : "") + " damage";
        }

        public override GenericAction Clone()
        {
            return new RepairAction(damageCardFace, criticalCardType);
        }

        public override void ActionTake()
        {
            if (PayRepairCost())
            {
                if (damageCardFace == CardFace.FaceDown)
                {
                    if (HostShip.Damage.DiscardRandomFacedownCard())
                    {
                        Sounds.PlayShipSound("R2D2-Proud");
                        Messages.ShowInfoToHuman("Facedown Damage card is discarded");
                    }
                    Phases.CurrentSubPhase.CallBack();
                }
                else if (damageCardFace == CardFace.FaceUp)
                {
                    List<GenericDamageCard> shipCritsList = HostShip.Damage.GetFaceupCrits(criticalCardType);

                    if (shipCritsList.Count == 1)
                    {
                        HostShip.Damage.FlipFaceupCritFacedown(shipCritsList.First());
                        Sounds.PlayShipSound("R2D2-Proud");
                        Phases.CurrentSubPhase.CallBack();
                    }
                    else if (shipCritsList.Count > 1)
                    {
                        Phases.StartTemporarySubPhaseOld(
                            Source.UpgradeInfo.Name + ": Select faceup ship Crit",
                            typeof(SubPhases.R5AstromechDecisionSubPhase),
                            Phases.CurrentSubPhase.CallBack
                        );
                    }
                }
            }
        }
    }
}