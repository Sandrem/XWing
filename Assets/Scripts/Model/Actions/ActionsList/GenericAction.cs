using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ship;
using Upgrade;
using System;
using SubPhases;
using Actions;

public enum DiceModificationTimingType
{
    AfterRolled,
    Opposite,
    Normal,
    CompareResults
}

namespace ActionsList
{
    public class GenericAction
    {
        public virtual string Name { get; set; }
        public virtual string DiceModificationName { get; set; }
        public virtual string ImageUrl { get; set; }

        public bool IsRealAction = true;
        public bool IsCoordinatedAction = false;

        public bool IsRed { get { return Color == ActionColor.Red; } }

        public bool IsPurple { get { return Color == ActionColor.Purple; } }

        public ActionColor Color;

        public bool IsCritCancelAction;

        public List<System.Type> TokensSpend = new List<System.Type>();

        public bool IsReroll;

        public bool IsTurnsOneFocusIntoSuccess;
        public bool IsTurnsAllFocusIntoSuccess;
        public bool IsForced;

        // Allow to use even if dice modifications are forbidden
        public bool IsNotRealDiceModification;

        public bool CanBeUsedFewTimes;

        public DiceModificationTimingType DiceModificationTiming = DiceModificationTimingType.Normal;

        public bool CanBePerformedWhileStressed;
        public virtual bool CanBePerformedAsAFreeAction { get { return true; } }

        //Ship that owns this dice modification (for aura-like abilities - source of aura-like ability)
        public GenericShip HostShip { get; set; }

        //Ship that uses this dice modification (for aura-like abilities - user of aura-like ability)
        public GenericShip DiceModificationShip { get; set; }

        public GenericUpgrade Source { get; set; }

        public GenericAction AsRedAction
        {
            get
            {
                var redAction = (GenericAction)MemberwiseClone();
                redAction.Color = ActionColor.Red;
                return redAction;
            }
        }

        public GenericAction AsCoordinatedAction
        {
            get
            {
                var coordinatedAction = (GenericAction)MemberwiseClone();
                coordinatedAction.IsCoordinatedAction = true;
                return coordinatedAction;
            }
        }

        public bool IsInActionBar;

        public Action DoAction = delegate { };
        public Action<Action> DoDiceModification = delegate { };
        public Func<bool> CheckDiceModificationAvailable = delegate { return true; };
        public Func<int> GenerateDiceModificationAiPriority = delegate { return 0; };

        public virtual void ActionEffect(Action callBack)
        {
            DoDiceModification(callBack);
        }

        public virtual bool IsDiceModificationAvailable()
        {
            return CheckDiceModificationAvailable();
        }

        public virtual int GetDiceModificationPriority()
        {
            int result = GenerateDiceModificationAiPriority();

            /* ATTACK
            * 110 - Free add dice with value
            * 100 - Free change limited by side if 1
            * 95 - Free rerolls limited by side
            * 90 - Free rerolls
            * 85 - Reroll of all rerollable dice if bad result
            * 80 - Rerolls TL
            * 70 - Free focus to crit
            * 60 - Free focus to crit, another to hits
            * 55 - Free side to hit
            * 53 - Spend die with eye result
            * 50 - Regular Focus to hits if 1+
            * 45 - Force to change eye to evade if 1
            * 41 - Calculate
            * 40 - Regular Focus to hits if 1
            * 35 - Force to change eye to evade if 1+
            * 33 - Eye to hit by dealing damage to teammate
            * 30 - Reroll of focus is focus token is available
            * 20 - Hits to crits
            */

            /* DEFENCE
            * 110 - Free add dice with value
            * 100 - Free change limited by side if 1
            * 95 - Free rerolls limited by side
            * 90 - Free rerolls
            * 89 - Not-free add dice with value
            * 87 - Not-Free rerolls limited by side if 1
            * 85 - Not-Free rerolls
            * 80 - Free focus to evades
            * 70 - Regular Evade if 1 uncancelled
            * 65 - Not free Evade if 1 uncancelled
            * 50 - Regular Focus to evades if 1+
            * 45 - Force to change eye to evade if 1
            * 40 - Regular Focus to evades if 1
            * 35 - Force to change eye to evade if 1+
            * 20 - Regular Evade if >1 uncancelled
            * 15 - Not free Evade if >1 uncancelled
            */

            return result;
        }

        public virtual void ActionTake()
        {
            DoAction();
        }

        public virtual bool IsActionAvailable()
        {
            return true;
        }

        public virtual int GetActionPriority()
        {
            int result = 0;

            /*
            * 100 - Rotate arc to get a shot in no enemies in arc
            * 90 - Cancel crit
            * 80 - Evade action is 1-hull
            * 50 - Focus action if has target
            * 25 - Reinforce action if there are no enemies
            * 20 - Focus action if no target
            * 10 - Focus action if Expertise is installed
            */

            return result;
        }

        public void DoOnlyEffect(Action callback)
        {
            HostShip = Selection.ThisShip;
            Phases.StartTemporarySubPhaseNew<ActionEffectSubPhase>(
                "Action effect",
                delegate
                {
                    Phases.FinishSubPhase(Phases.CurrentSubPhase.GetType());
                    callback();
                }
            );
            Phases.CurrentSubPhase.RequiredPlayer = HostShip.Owner.PlayerNo;
            ActionTake();
        }

        public virtual void RevertActionOnFail(bool hasSecondChance = false) {}

        //override this for actions that require further initialization
        public virtual GenericAction Clone()
        {
            return (GenericAction)Activator.CreateInstance(GetType());
        }
    }

}

namespace SubPhases
{
    public class ActionEffectSubPhase : GenericSubPhase
    {
        public override void Next()
        {
            Phases.CurrentSubPhase = PreviousSubPhase;
        }
    }
}
