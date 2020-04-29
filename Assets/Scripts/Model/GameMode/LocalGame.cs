﻿using UnityEngine;
using System;
using System.Collections.Generic;
using SubPhases;
using Players;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using GameCommands;
using Actions;
using Ship;

namespace GameModes
{
    public class LocalGame : GameMode
    {
        public override string Name { get { return "Local"; } }

        public override void ExecuteCommand(GameCommand command)
        {
            GameController.SendCommand(command);
        }

        public override void ExecuteServerCommand(GameCommand command)
        {
            GameController.SendCommand(command);
        }

        public override void RevertSubPhase()
        {
            (Phases.CurrentSubPhase as SelectShipSubPhase).CallRevertSubPhase();
        }

        public override void GiveInitiativeToRandomPlayer()
        {
            if (ReplaysManager.Mode == ReplaysMode.Write)
            {
                int randomPlayer = UnityEngine.Random.Range(1, 3);

                JSONObject parameters = new JSONObject();
                parameters.AddField("player", Tools.IntToPlayer(randomPlayer).ToString());

                GameController.SendCommand(
                    GameCommandTypes.SyncPlayerWithInitiative,
                    null,
                    parameters.ToString()
                );

                Console.Write("Command is executed: " + GameCommandTypes.SyncPlayerWithInitiative, LogTypes.GameCommands, true, "aqua");
                GameController.GetCommand().Execute();
            }
            else if (ReplaysManager.Mode == ReplaysMode.Read)
            {
                GameCommand command = GameController.GetCommand();

                if (command.Type == GameCommandTypes.SyncPlayerWithInitiative)
                {
                    Console.Write("Command is executed: " + command.Type, LogTypes.GameCommands, true, "aqua");
                    command.Execute();
                }
            }
        }

        public override void StartBattle()
        {
            Global.BattleIsReady();
        }

        // BARREL ROLL

        public override void TryConfirmBarrelRollPosition(string templateName, Vector3 shipBasePosition, Vector3 movementTemplatePosition)
        {
            (Phases.CurrentSubPhase as BarrelRollPlanningSubPhase).ConfirmBarrelRollPosition();
        }

        public override void StartBarrelRollExecution()
        {
            (Phases.CurrentSubPhase as BarrelRollPlanningSubPhase).StartRepositionExecution();
        }

        public override void FinishBarrelRoll()
        {
            (Phases.CurrentSubPhase as BarrelRollExecutionSubPhase).FinishBarrelRollAnimation();
        }

        public override void CancelBarrelRoll(List<ActionFailReason> barrelRollProblems)
        {
            (Phases.CurrentSubPhase as BarrelRollPlanningSubPhase).CancelBarrelRoll(barrelRollProblems);
        }

        // DECLOAK

        public override void StartDecloakExecution(Ship.GenericShip ship)
        {
            (Phases.CurrentSubPhase as DecloakPlanningSubPhase).StartRepositionExecution();
        }

        public override void FinishDecloak()
        {
            (Phases.CurrentSubPhase as DecloakBarrelRollExecutionSubPhase).FinishBarrelRollAnimation();
        }

        public override void CancelDecloak(List<ActionFailReason> decloakProblems)
        {
            (Phases.CurrentSubPhase as DecloakPlanningSubPhase).CancelBarrelRoll(decloakProblems);
        }

        // BOOST

        public override void TryConfirmBoostPosition(string selectedBoostHelper)
        {
            (Phases.CurrentSubPhase as BoostPlanningSubPhase).TryConfirmBoostPosition();
        }

        public override void StartBoostExecution(ShipPositionInfo finalPositionInfo)
        {
            (Phases.CurrentSubPhase as BoostPlanningSubPhase).StartBoostExecution(finalPositionInfo);
        }

        public override void FinishBoost()
        {
            Phases.FinishSubPhase(Phases.CurrentSubPhase.GetType());
        }

        public override void CancelBoost(List<ActionFailReason> boostProblems)
        {
            (Phases.CurrentSubPhase as BoostPlanningSubPhase).CancelBoost(boostProblems);
        }

        // Swarm Manager

        public override void SetSwarmManagerManeuver(string maneuverCode)
        {
            SwarmManager.SetManeuver(maneuverCode);
        }

        public override void GenerateDamageDeck(PlayerNo playerNo, int seed)
        {
            SyncDamageDeckSeed(playerNo, seed);
        }

        private void SyncDamageDeckSeed(PlayerNo playerNo, int seed)
        {
            // TODO: Move to player types

            if (ReplaysManager.Mode == ReplaysMode.Write)
            {
                GameController.SendCommand
                (
                    DamageDecks.GenerateDeckShuffleCommand(playerNo, seed)
                );
            }
            else if (ReplaysManager.Mode == ReplaysMode.Read)
            {
                GameCommand command = GameController.GetCommand();

                if (command.Type == GameCommandTypes.DamageDecksSync)
                {
                    Console.Write("Command is executed: " + command.Type, LogTypes.GameCommands, true, "aqua");

                    GameController.ConfirmCommand();
                    command.Execute();
                }
            }
        }

        public override void ReturnToMainMenu()
        {
            Phases.EndGame();
            LoadingScreen.Show();
            SceneManager.LoadScene("MainMenu");
            LoadingScreen.NextSceneIsReady(delegate { });
        }

        public override void QuitToDesktop()
        {
            Application.Quit();
        }
    }
}
