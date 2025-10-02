import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { Trophy, Play } from "lucide-react";
import TeamLogo from "../components/TeamLogo";
import { useProcessing } from "../context/ProcessingContext";

export default function TodayMatches() {
  const { gameSaveId } = useParams();
  const [matches, setMatches] = useState([]);
  const [userFixtureId, setUserFixtureId] = useState(null);
  const [activeMatch, setActiveMatch] = useState(null);
  const { setCurrentGameSave } = useGameSave();
  const { runSimulateMatches, isProcessing } = useProcessing();

  const navigate = useNavigate();

  const normalizeMatch = (m) => ({
    fixtureId: m.fixtureId ?? m.FixtureId,
    competitionName: m.competitionName ?? m.CompetitionName,
    home: m.home ?? m.Home,
    away: m.away ?? m.Away,
    homeGoals: m.homeGoals ?? m.HomeGoals,
    awayGoals: m.awayGoals ?? m.AwayGoals,
    status: m.status ?? m.Status ?? 0,
    isUserTeamMatch:
      typeof m.isUserTeamMatch !== "undefined"
        ? m.isUserTeamMatch
        : m.IsUserTeamMatch ?? false,
    homeLogoFileName: m.homeLogoFileName ?? m.HomeLogoFileName,
    awayLogoFileName: m.awayLogoFileName ?? m.AwayLogoFileName,
  });

  // зареждане на мачовете
  const loadMatches = async () => {
    try {
      const res = await fetch(`/api/matches/today/${gameSaveId}`, {
        credentials: "include",
      });
      const data = await res.json();
      const normalized = data.matches.map(normalizeMatch);
      setMatches(normalized);
      if (data.activeMatch) setActiveMatch(data.activeMatch);
      const userMatch = normalized.find((m) => m.isUserTeamMatch);
      setUserFixtureId(userMatch?.fixtureId ?? null);
    } catch (err) {
      console.error("Failed to fetch matches", err);
    }
  };

  useEffect(() => {
    loadMatches();
  }, [gameSaveId]);

  const handleToMatch = () => {
    if (userFixtureId) navigate(`/live-match/${userFixtureId}`);
  };

  const handleSimulate = async () => {
    if (isProcessing) return;
    try {
      const data = await runSimulateMatches(gameSaveId, { stepDelay: 400 });
      if (!data) return;

      if (data.gameStatus?.gameSave) {
        setCurrentGameSave(data.gameStatus.gameSave);
      }
      if (Array.isArray(data.matches)) {
        const normalized = data.matches.map(normalizeMatch);
        setMatches(normalized);
        const userMatch = normalized.find((m) => m.isUserTeamMatch);
        setUserFixtureId(userMatch?.fixtureId ?? null);
      } else {
        await loadMatches();
      }

      // dispatch global event за Header
      window.dispatchEvent(
        new CustomEvent("gameStatusUpdated", { detail: data.gameStatus })
      );
    } catch (err) {
      console.error("Simulation failed:", err);
      alert("Error simulating matches");
    }
  };

  const hasUnplayedMatches = matches.some((m) => m.status === 0);

  const grouped = matches.reduce((acc, m) => {
    if (!acc[m.competitionName]) acc[m.competitionName] = [];
    acc[m.competitionName].push(m);
    return acc;
  }, {});

  const renderStatus = (status) => {
    switch (status) {
      case 0:
        return <span className="px-2 py-0.5 text-xs rounded-full bg-gray-200 text-gray-600">Scheduled</span>;
      case 1:
        return <span className="px-2 py-0.5 text-xs rounded-full bg-green-200 text-green-700">Played</span>;
      case 2:
        return <span className="px-2 py-0.5 text-xs rounded-full bg-red-200 text-red-700">Cancelled</span>;
      case 3:
        return <span className="px-2 py-0.5 text-xs rounded-full bg-orange-200 text-orange-700 animate-pulse">Live 🔴</span>;
      default:
        return <span className="px-2 py-0.5 text-xs rounded-full bg-gray-100 text-gray-400">Unknown</span>;
    }
  };

  return (
    <div className="p-6 sm:p-8 space-y-10 max-w-5xl mx-auto">
      {/* Buttons */}
      <div className="flex justify-center gap-4 mb-6">
        <button
          onClick={handleToMatch}
          disabled={!userFixtureId}
          className={`flex items-center gap-2 px-6 py-3 rounded-2xl shadow-md font-semibold transition transform
            ${
              userFixtureId
                ? "bg-gradient-to-r from-green-600 to-emerald-700 hover:from-green-700 hover:to-emerald-800 text-white hover:scale-105"
                : "bg-gray-300 text-gray-500 cursor-not-allowed"
            }`}
        >
          <Play className="w-5 h-5" />
          To Match
        </button>

        {hasUnplayedMatches && (
          <button
            onClick={handleSimulate}
            disabled={isProcessing}
            className={`flex items-center gap-2 px-6 py-3 rounded-2xl shadow-md font-semibold transition transform
              ${
                isProcessing
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
        <p className="text-center text-gray-500 italic text-lg">
          No matches today.
        </p>
      )}

      {Object.entries(grouped).map(([competition, compMatches]) => (
        <div
          key={competition}
          className="bg-gradient-to-b from-white to-slate-50 rounded-2xl shadow-lg border border-slate-200 p-6 space-y-5"
        >
          <h2 className="flex items-center justify-center gap-2 text-2xl font-bold text-slate-700 border-b pb-2">
            <Trophy className="w-6 h-6 text-amber-500" />
            {competition}
          </h2>

          <div className="space-y-4">
            {compMatches.map((m, idx) => (
              <div
                key={idx}
                className={`flex items-center justify-between px-6 py-4 rounded-xl border transition shadow-sm hover:shadow-md
                  ${
                    m.isUserTeamMatch
                      ? "bg-sky-50 border-sky-300 animate-pulse"
                      : "bg-white border-slate-100"
                  }`}
              >
                <div className="flex-1 flex items-center justify-end gap-2">
                  <span className="font-semibold text-slate-800">{m.home}</span>
                  <TeamLogo
                    teamName={m.home}
                    logoFileName={m.homeLogoFileName}
                    className="w-8 h-8 rounded-full shadow"
                  />
                </div>
                <div className="flex flex-col items-center px-4">
                  <span className="text-gray-700 font-bold">
                    {m.homeGoals != null && m.awayGoals != null
                      ? `${m.homeGoals} : ${m.awayGoals}`
                      : "vs"}
                  </span>
                  {renderStatus(m.status)}
                </div>
                <div className="flex-1 flex items-center justify-start gap-2">
                  <TeamLogo
                    teamName={m.away}
                    logoFileName={m.awayLogoFileName}
                    className="w-8 h-8 rounded-full shadow"
                  />
                  <span className="font-semibold text-slate-800">{m.away}</span>
                </div>
              </div>
            ))}
          </div>
        </div>
      ))}
    </div>
  );
}
