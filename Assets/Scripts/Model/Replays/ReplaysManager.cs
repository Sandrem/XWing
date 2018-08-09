﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SquadBuilderNS;
using Players;

public static class ReplaysManager
{
    private static readonly string TestSquad1 = "{\"name\":\"X-Wing\",\"faction\":\"rebel\",\"points\":52,\"version\":\"0.3.0\",\"pilots\":[{\"name\":\"wedgeantilles\",\"points\":52,\"ship\":\"xwing\",\"upgrades\":{},\"vendor\":{\"Sandrem.FlyCasual\":{\"skin\":\"Red\"}}}],\"description\":\"Wedge Antilles\"}";
    private static readonly string TestSquad2 = "{\"name\":\"TIE Fighters\",\"faction\":\"imperial\",\"points\":52,\"version\":\"0.3.0\",\"pilots\":[{\"name\":\"blacksquadronace\",\"points\":26,\"ship\":\"tiefighter\",\"upgrades\":{},\"vendor\":{\"Sandrem.FlyCasual\":{\"skin\":\"Gray\"}}},{\"name\":\"blacksquadronace\",\"points\":26,\"ship\":\"tiefighter\",\"upgrades\":{},\"vendor\":{\"Sandrem.FlyCasual\":{\"skin\":\"Gray\"}}}],\"description\":\"Black Squadron Ace\nBlack Squadron Ace\"}";

    public static void StartBattle()
    {
        GameController.Initialize();

        SquadBuilder.SetPlayerSquadFromImportedJson(new JSONObject(TestSquad1), PlayerNo.Player1, delegate { });
        SquadBuilder.SetPlayerSquadFromImportedJson(new JSONObject(TestSquad2), PlayerNo.Player2, delegate { });
        SquadBuilder.SaveSquadConfigurations();
        SquadBuilder.StartLocalGame();

        JSONObject parameters = null;

        // INITIATIVE

        parameters = new JSONObject();
        parameters.AddField("Name", "Me");
        GameController.SendCommand(
            GameCommandTypes.Decision,
            typeof(SubPhases.InitialiveDecisionSubPhase),
            parameters.ToString()
        );

        // OBSTACLES

        parameters = new JSONObject();
        parameters.AddField("Name", "A1");
        parameters.AddField("PositionX", 0f); parameters.AddField("PositionY", 0f); parameters.AddField("PositionZ", 0f);
        parameters.AddField("RotationX", 0f); parameters.AddField("RotationY", 0f); parameters.AddField("RotationZ", 0f);
        GameController.SendCommand(
            GameCommandTypes.ObstaclePlacement,
            typeof(SubPhases.ObstaclesPlacementSubPhase),
            parameters.ToString()
        );

        parameters = new JSONObject();
        parameters.AddField("Name", "A2");
        parameters.AddField("PositionX", 1f); parameters.AddField("PositionY", 0f); parameters.AddField("PositionZ", -1f);
        parameters.AddField("RotationX", 0f); parameters.AddField("RotationY", 0f); parameters.AddField("RotationZ", 0f);
        GameController.SendCommand(
            GameCommandTypes.ObstaclePlacement,
            typeof(SubPhases.ObstaclesPlacementSubPhase),
            parameters.ToString()
        );

        parameters = new JSONObject();
        parameters.AddField("Name", "A3");
        parameters.AddField("PositionX", -2f); parameters.AddField("PositionY", 0f); parameters.AddField("PositionZ", 1f);
        parameters.AddField("RotationX", 0f); parameters.AddField("RotationY", 0f); parameters.AddField("RotationZ", 0f);
        GameController.SendCommand(
            GameCommandTypes.ObstaclePlacement,
            typeof(SubPhases.ObstaclesPlacementSubPhase),
            parameters.ToString()
        );

        parameters = new JSONObject();
        parameters.AddField("Name", "A4");
        parameters.AddField("PositionX", -1f); parameters.AddField("PositionY", 0f); parameters.AddField("PositionZ", 2f);
        parameters.AddField("RotationX", 0f); parameters.AddField("RotationY", 0f); parameters.AddField("RotationZ", 0f);
        GameController.SendCommand(
            GameCommandTypes.ObstaclePlacement,
            typeof(SubPhases.ObstaclesPlacementSubPhase),
            parameters.ToString()
        );

        parameters = new JSONObject();
        parameters.AddField("Name", "A5");
        parameters.AddField("PositionX", -1f); parameters.AddField("PositionY", 0f); parameters.AddField("PositionZ", -1.5f);
        parameters.AddField("RotationX", 0f); parameters.AddField("RotationY", 0f); parameters.AddField("RotationZ", 0f);
        GameController.SendCommand(
            GameCommandTypes.ObstaclePlacement,
            typeof(SubPhases.ObstaclesPlacementSubPhase),
            parameters.ToString()
        );

        parameters = new JSONObject();
        parameters.AddField("Name", "A6");
        parameters.AddField("PositionX", 2f); parameters.AddField("PositionY", 0f); parameters.AddField("PositionZ", 1.5f);
        parameters.AddField("RotationX", 0f); parameters.AddField("RotationY", 0f); parameters.AddField("RotationZ", 0f);
        GameController.SendCommand(
            GameCommandTypes.ObstaclePlacement,
            typeof(SubPhases.ObstaclesPlacementSubPhase),
            parameters.ToString()
        );

        // SHIP SETUP

        parameters = new JSONObject();
        parameters.AddField("Id", 2);
        parameters.AddField("PositionX", -3f); parameters.AddField("PositionY", 0f); parameters.AddField("PositionZ", 4f);
        parameters.AddField("RotationX", 0f); parameters.AddField("RotationY", 180f); parameters.AddField("RotationZ", 0f);
        GameController.SendCommand(
            GameCommandTypes.ShipPlacement,
            typeof(SubPhases.SetupSubPhase),
            parameters.ToString()
        );

        parameters = new JSONObject();
        parameters.AddField("Id", 3);
        parameters.AddField("PositionX", 3f); parameters.AddField("PositionY", 0f); parameters.AddField("PositionZ", 4f);
        parameters.AddField("RotationX", 0f); parameters.AddField("RotationY", 180f); parameters.AddField("RotationZ", 0f);
        GameController.SendCommand(
            GameCommandTypes.ShipPlacement,
            typeof(SubPhases.SetupSubPhase),
            parameters.ToString()
        );

        parameters = new JSONObject();
        parameters.AddField("Id", 1);
        parameters.AddField("PositionX", 0f); parameters.AddField("PositionY", 0f); parameters.AddField("PositionZ", -4f);
        parameters.AddField("RotationX", 0f); parameters.AddField("RotationY", 0f); parameters.AddField("RotationZ", 0f);
        GameController.SendCommand(
            GameCommandTypes.ShipPlacement,
            typeof(SubPhases.SetupSubPhase),
            parameters.ToString()
        );

    }

}

