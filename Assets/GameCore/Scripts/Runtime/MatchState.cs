using System;
using UnityEngine;
using SlotsTavern.Data;

namespace SlotsTavern.Runtime
{
    public enum MatchPhase
    {
        None = 0,
        BuildPhase = 10,
        RoundStart = 20,
        TurnStart = 30,
        PreSpinItemPhase = 40,
        SpinPhase = 50,
        ResolvePhase = 60,
        RoundEnd = 70,
        MatchEnd = 80
    }

    [Serializable]
    public class MatchState
    {
        public MatchConfig matchConfig;

        public MatchPhase currentPhase = MatchPhase.None;
        public int currentRoundIndex = 1;

        public string activePlayerId;

        public PlayerMatchState playerA;
        public PlayerMatchState playerB;

        public PlayerRoundState playerARound;
        public PlayerRoundState playerBRound;

        public bool isMatchEnded;

        public MatchState(MatchConfig matchConfig, PlayerMatchState playerA, PlayerMatchState playerB)
        {
            this.matchConfig = matchConfig;
            this.playerA = playerA;
            this.playerB = playerB;

            currentPhase = MatchPhase.BuildPhase;
            currentRoundIndex = 1;
            activePlayerId = string.Empty;
            isMatchEnded = false;
        }

        public PlayerMatchState GetPlayerMatchState(string playerId)
        {
            if (playerA != null && playerA.playerId == playerId)
                return playerA;

            if (playerB != null && playerB.playerId == playerId)
                return playerB;

            return null;
        }

        public PlayerRoundState GetPlayerRoundState(string playerId)
        {
            if (playerARound != null && playerARound.playerId == playerId)
                return playerARound;

            if (playerBRound != null && playerBRound.playerId == playerId)
                return playerBRound;

            return null;
        }

        public string GetOpponentPlayerId(string playerId)
        {
            if (playerA != null && playerA.playerId == playerId && playerB != null)
                return playerB.playerId;

            if (playerB != null && playerB.playerId == playerId && playerA != null)
                return playerA.playerId;

            return string.Empty;
        }

        public bool IsPlayerActive(string playerId)
        {
            return activePlayerId == playerId;
        }

        public bool HasPlayerWonMatch(string playerId)
        {
            if (matchConfig == null)
                return false;

            PlayerMatchState player = GetPlayerMatchState(playerId);
            if (player == null)
                return false;

            return player.roundWins >= matchConfig.RequiredWins;
        }

        public RoundConfig GetCurrentRoundConfig()
        {
            if (matchConfig == null)
                return null;

            return matchConfig.GetRoundConfig(currentRoundIndex);
        }

        public void SetPhase(MatchPhase phase)
        {
            currentPhase = phase;
        }

        public void SetActivePlayer(string playerId)
        {
            activePlayerId = playerId;
        }

        public void EndMatch()
        {
            isMatchEnded = true;
            currentPhase = MatchPhase.MatchEnd;
        }
    }
}