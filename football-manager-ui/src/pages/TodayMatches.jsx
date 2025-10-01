import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { Trophy, Play } from "lucide-react";
import TeamLogo from "../components/TeamLogo";
import { useGameSave } from "../context/GameSaveContext"; // 👈 ИМПОРТИРАЙ КОНТЕКСТА

export default function TodayMatches() {
  const { gameSaveId } = useParams();
  const [matches, setMatches] = useState([]);
  const [userFixtureId, setUserFixtureId] = useState(null);
  const [hasUnplayed, setHasUnplayed] = useState(false);
  const [activeMatch, setActiveMatch] = useState(null);
  
  // 👇 ДОБАВИ ТОВА ЗА ДОСТЪП ДО ФУНКЦИИТЕ ОТ ХЕДЪРА
  const { setCurrentGameSave } = useGameSave();

  const navigate = useNavigate();

  // зареждане на мачовете
  const loadMatches = async () => {
    try {
      const res = await fetch(`/api/matches/today/${gameSaveId}`, {
        credentials: "include",
      });
      const data = await res.json();
      setMatches(data.matches);
      if (data.activeMatch) setActiveMatch(data.activeMatch);

      // намери потребителския мач
      const userMatch = data.matches.find((m) => m.isUserTeamMatch);
      setUserFixtureId(userMatch ? userMatch.fixtureId : null);
      
      // 👇 ОБНОВИ hasUnplayed ВЪЗ ОСНОВА НА МАЧОВЕТЕ
      const hasUnplayedMatchesToday = data.matches.some(m => m.status === 0);
      setHasUnplayed(hasUnplayedMatchesToday);
    } catch (err) {
      console.error("Failed to fetch matches", err);
    }
  };

  useEffect(() => {
    loadMatches();
  }, [gameSaveId]);

  const handleToMatch = () => {
    if (userFixtureId) {
      navigate(`/live-match/${userFixtureId}`);
    }
  };

  // 👇 ПРОМЕНЕНА ФУНКЦИЯ ЗА СИМУЛИРАНЕ
  const handleSimulate = async () => {
    try {
      const res = await fetch(`/api/matches/simulate/${gameSaveId}`, {
        method: "POST",
        credentials: "include",
      });

      if (!res.ok) {
        alert("Failed to simulate matches");
        return;
      }

      const data = await res.json();

      // 👇 ОБНОВИ ВСИЧКО ВЕДНАГА
      if (data.matches) setMatches(data.matches);
      
      // 👇 АКО БЕКЕНДЪТ ВРЪЩА gameStatus (според предишния ми съвет)
      if (data.gameStatus) {
        setCurrentGameSave(data.gameStatus.gameSave);
        setHasUnplayed(data.gameStatus.hasUnplayedMatchesToday);
        setActiveMatch(data.gameStatus.activeMatch);
      } 
      // 👇 АКО БЕКЕНДЪТ ВРЪЩА САМО hasUnplayedMatchesToday и activeMatch
      else {
        if (typeof data.hasUnplayedMatchesToday === "boolean") {
          setHasUnplayed(data.hasUnplayedMatchesToday);
        }
        if (data.activeMatch) {
          setActiveMatch(data.activeMatch);
        } else {
          setActiveMatch(null);
        }
      }

      // 👇 ОБНОВИ userFixtureId СЛЕД СИМУЛИРАНЕ
      const userMatch = data.matches?.find((m) => m.isUserTeamMatch);
      setUserFixtureId(userMatch ? userMatch.fixtureId : null);

    } catch (err) {
      console.error("Simulation failed:", err);
      alert("Error simulating matches");
    }
  };

  // 👇 ПРОМЕНЕНА ПРОВЕРКА - ползва локалното състояние
  const hasUnplayedMatches = matches.some((m) => m.status === 0);

  // групиране по състезание
  const grouped = matches.reduce((acc, m) => {
    if (!acc[m.competitionName]) acc[m.competitionName] = [];
    acc[m.competitionName].push(m);
    return acc;
  }, {});

  const renderStatus = (status) => {
    switch (status) {
      case 0:
        return (
          <span className="px-2 py-0.5 text-xs rounded-full bg-gray-200 text-gray-600">
            Scheduled
          </span>
        );
      case 1:
        return (
          <span className="px-2 py-0.5 text-xs rounded-full bg-green-200 text-green-700">
            Played
          </span>
        );
      case 2:
        return (
          <span className="px-2 py-0.5 text-xs rounded-full bg-red-200 text-red-700">
            Cancelled
          </span>
        );
      case 3:
        return (
          <span className="px-2 py-0.5 text-xs rounded-full bg-orange-200 text-orange-700 animate-pulse">
            Live 🔴
          </span>
        );
      default:
        return (
          <span className="px-2 py-0.5 text-xs rounded-full bg-gray-100 text-gray-400">
            Unknown
          </span>
        );
    }
  };

  return (
    <div className="p-6 sm:p-8 space-y-10 max-w-5xl mx-auto">
      {/* Buttons */}
      <div className="flex justify-center gap-4 mb-6">
        {/* To Match */}
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

        {/* Simulate */}
        {hasUnplayedMatches && (
          <button
            onClick={handleSimulate}
            className="flex items-center gap-2 px-6 py-3 rounded-2xl shadow-md font-semibold transition transform
              bg-gradient-to-r from-sky-600 to-blue-700 hover:from-sky-700 hover:to-blue-800 text-white hover:scale-105"
          >
            <Play className="w-5 h-5" />
            Simulate Matches
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
          {/* Competition header */}
          <h2 className="flex items-center justify-center gap-2 text-2xl font-bold text-slate-700 border-b pb-2">
            <Trophy className="w-6 h-6 text-amber-500" />
            {competition}
          </h2>

          {/* Match list */}
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
                {/* Home team */}
                <div className="flex-1 flex items-center justify-end gap-2">
                  <span className="font-semibold text-slate-800">{m.home}</span>
                  <TeamLogo
                    teamName={m.home}
                    logoFileName={m.homeLogoFileName}
                    className="w-8 h-8 rounded-full shadow"
                  />
                </div>

                {/* Result + Status */}
                <div className="flex flex-col items-center px-4">
                  <span className="text-gray-700 font-bold">
                    {m.homeGoals != null && m.awayGoals != null
                      ? `${m.homeGoals} : ${m.awayGoals}`
                      : "vs"}
                  </span>
                  {renderStatus(m.status)}
                </div>

                {/* Away team */}
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