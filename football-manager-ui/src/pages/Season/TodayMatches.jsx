// src/pages/TodayMatches.jsx
import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { Trophy, Play, Zap } from "lucide-react";
import TeamLogo from "../../components/TeamLogo";
import { useProcessing } from "../../context/ProcessingContext";
import { useGame } from "../../context/GameContext";

export default function TodayMatches() {
  const { gameSaveId } = useParams();
  const [matches, setMatches] = useState([]);

  const {
    setCurrentGameSave,
    setHasUnplayedMatchesToday,
    setActiveMatch,
    refreshGameStatus,
  } = useGame();

  const { isProcessing, stopProcessing, runSimulateMatches } = useProcessing();

  const normalizeMatch = (m) => ({
    fixtureId: m.fixtureId ?? m.FixtureId,
    competitionName: m.competitionName ?? m.CompetitionName,
    home: m.home ?? m.Home,
    away: m.away ?? m.Away,
    homeGoals: m.homeGoals ?? m.HomeGoals,
    awayGoals: m.awayGoals ?? m.AwayGoals,
    status: Number(m.status ?? m.Status ?? 0),
    isUserTeamMatch:
      typeof m.isUserTeamMatch !== "undefined"
        ? m.isUserTeamMatch
        : m.IsUserTeamMatch ?? false,
    homeLogoFileName: m.homeLogoFileName ?? m.HomeLogoFileName,
    awayLogoFileName: m.awayLogoFileName ?? m.AwayLogoFileName,
    homePenalties: m.homePenalties ?? m.HomePenalties ?? 0,
    awayPenalties: m.awayPenalties ?? m.AwayPenalties ?? 0,
    isElimination: m.isElimination ?? m.IsElimination ?? false,
    winner: m.winner ?? m.Winner ?? null,
  });

  const loadMatches = async () => {
    try {
      const res = await fetch(`/api/matches/today/${gameSaveId}`, {
        credentials: "include",
      });
      if (!res.ok) return console.error("Failed to fetch matches", res.status);
      const data = await res.json();
      const normalized = (data.matches ?? []).map(normalizeMatch);
      setMatches(normalized);
      if (data.activeMatch) setActiveMatch(data.activeMatch);
    } catch (err) {
      console.error("Failed to fetch matches", err);
    }
  };

  useEffect(() => {
    loadMatches();
  }, [gameSaveId]);

  const handleSimulate = async () => {
    try {
      const data = await runSimulateMatches(gameSaveId);
      if (!data) return;

      if (data.matches) {
        const normalized = data.matches.map(normalizeMatch);
        setMatches(normalized);
      } else {
        await loadMatches();
      }

      const status = data.gameStatus ?? data.gameStatus;
      if (status) {
        if (status.gameSave) setCurrentGameSave(status.gameSave);
        setHasUnplayedMatchesToday(Boolean(status.hasUnplayedMatchesToday));
        setActiveMatch(status.activeMatch ?? null);
      } else {
        await refreshGameStatus();
      }
    } catch (err) {
      console.error("Simulation failed:", err);
      alert("Error simulating matches");
    } finally {
      stopProcessing();
    }
  };

  const hasUnplayedMatches = matches.some((m) => m.status === 0);

  const grouped = matches.reduce((acc, m) => {
    if (!acc[m.competitionName]) acc[m.competitionName] = [];
    acc[m.competitionName].push(m);
    return acc;
  }, {});

  const renderStatus = (status) => {
    const styles = {
      0: "bg-gray-200 text-gray-600",
      1: "bg-green-200 text-green-800",
      2: "bg-red-200 text-red-700",
      3: "bg-orange-200 text-orange-700 animate-pulse",
    };
    const labels = ["Scheduled", "Played", "Cancelled", "Live 🔴"];
    return (
      <span
        className={`px-2 py-0.5 text-xs font-semibold rounded-full ${styles[status] || "bg-gray-100 text-gray-400"
          }`}
      >
        {labels[status] || "Unknown"}
      </span>
    );
  };

  return (
    <div className="p-6 sm:p-8 space-y-10 max-w-5xl mx-auto">
      <div className="flex justify-center gap-4 mb-6">
        {hasUnplayedMatches && (
          <button
            onClick={handleSimulate}
            disabled={isProcessing}
            className={`flex items-center gap-2 px-8 py-3 rounded-2xl font-bold shadow-lg transition-all transform
              ${isProcessing
                ? "opacity-60 cursor-not-allowed bg-gray-300 text-gray-600"
                : "bg-gradient-to-r from-sky-600 to-blue-700 hover:from-sky-700 hover:to-blue-800 text-white hover:scale-105"
              }`}
          >
            <Play className="w-5 h-5" />
            {isProcessing ? "Simulating..." : "Simulate Matches"}
          </button>
        )}
      </div>

      {matches.length === 0 && (
        <p className="text-center text-gray-500 italic text-lg animate-pulse">
          No matches today.
        </p>
      )}

      {Object.entries(grouped).map(([competition, compMatches]) => (
        <div
          key={competition}
          className="bg-gradient-to-b from-slate-50 to-slate-100 rounded-3xl shadow-2xl border border-slate-200 p-6 space-y-5 transition-all"
        >
          <h2 className="flex items-center justify-center gap-3 text-2xl font-extrabold text-slate-800 tracking-wide">
            <Trophy className="w-6 h-6 text-amber-500 drop-shadow" />
            <span className="drop-shadow-sm">{competition}</span>
          </h2>

          <div className="space-y-4">
            {compMatches.map((m, idx) => {
              const isWinner = (team) =>
                m.winner && m.winner.toLowerCase() === team.toLowerCase();

              return (
                <div
                  key={idx}
                  className={`flex items-center justify-between px-6 py-4 rounded-2xl border shadow-sm backdrop-blur-sm transition-transform hover:-translate-y-1 hover:shadow-md
                    ${m.isUserTeamMatch
                      ? "bg-sky-50 border-sky-300 ring-2 ring-sky-200"
                      : "bg-white border-slate-200"
                    }`}
                >
                  {/* HOME */}
                  <div className="flex-1 flex items-center justify-end gap-2">
                    <span
                      className={`font-semibold text-right text-lg ${isWinner(m.home)
                        ? "text-amber-500 drop-shadow-sm animate-pulse"
                        : "text-slate-800"
                        }`}
                    >
                      {m.home}
                    </span>
                    <TeamLogo
                      teamName={m.home}
                      logoFileName={m.homeLogoFileName}
                      className={`w-9 h-9 rounded-full shadow ${isWinner(m.home) ? "ring-2 ring-amber-400" : ""
                        }`}
                    />
                  </div>

                  {/* SCORE */}
                  <div className="flex flex-col items-center px-4 text-center">
                    <span className="text-gray-700 font-extrabold text-xl">
                      {m.homeGoals != null && m.awayGoals != null
                        ? `${m.homeGoals} : ${m.awayGoals}`
                        : "vs"}
                      {m.isElimination &&
                        m.homeGoals === m.awayGoals &&
                        (m.homePenalties > 0 || m.awayPenalties > 0) && (
                          <>
                            {" "}
                            <span className="text-sm text-slate-500">
                              ({m.homePenalties}:{m.awayPenalties} pens)
                            </span>
                          </>
                        )}
                    </span>

                    <div className="mt-1">{renderStatus(m.status)}</div>
                  </div>

                  {/* AWAY */}
                  <div className="flex-1 flex items-center justify-start gap-2">
                    <TeamLogo
                      teamName={m.away}
                      logoFileName={m.awayLogoFileName}
                      className={`w-9 h-9 rounded-full shadow ${isWinner(m.away) ? "ring-2 ring-amber-400" : ""
                        }`}
                    />
                    <span
                      className={`font-semibold text-left text-lg ${isWinner(m.away)
                        ? "text-amber-500 drop-shadow-sm animate-pulse"
                        : "text-slate-800"
                        }`}
                    >
                      {m.away}
                    </span>
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      ))}
    </div>
  );
}
