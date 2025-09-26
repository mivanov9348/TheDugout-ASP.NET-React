import { Link, useNavigate } from "react-router-dom";
import { useGameSave } from "../context/GameSaveContext";
import { useState, useEffect } from "react";

function Header({ username }) {
  const { currentGameSave, setCurrentGameSave } = useGameSave();
  const [hasUnplayed, setHasUnplayed] = useState(false);
  const [activeMatch, setActiveMatch] = useState(null);

  const navigate = useNavigate();

  // Полинг за активен мач + обновяване на save
  useEffect(() => {
    if (!currentGameSave) return;

    const fetchStatus = async () => {
      try {
        // 1) Проверяваме за активен мач
        const matchRes = await fetch(
          `/api/matches/active/${currentGameSave.id}`,
          { credentials: "include" }
        );
        if (matchRes.ok) {
          const matchData = await matchRes.json();
          setActiveMatch(matchData || null);
        }

        // 2) Обновяваме save, за да дърпаме balance/дата
        const saveRes = await fetch(`/api/games/current`, {
          credentials: "include",
        });
        if (saveRes.ok) {
          const saveData = await saveRes.json();
          setCurrentGameSave(saveData);
          setHasUnplayed(saveData.hasUnplayedMatchesToday);
        }
      } catch (err) {
        console.error("Polling failed:", err);
      }
    };

    fetchStatus(); // първоначално
    const interval = setInterval(fetchStatus, 5000); // на 5 секунди

    return () => clearInterval(interval); // cleanup
  }, [currentGameSave?.id, setCurrentGameSave]);

  if (!currentGameSave) {
    return (
      <header className="px-6 py-3 bg-slate-800 text-white shadow-md">
        <h1 className="font-bold text-lg">No Save Loaded</h1>
      </header>
    );
  }

  const season = currentGameSave.seasons?.[0];
  const team = currentGameSave.userTeam;

  const formatDate = (dateStr) => {
    if (!dateStr) return "";
    return new Date(dateStr).toLocaleDateString("en-GB", {
      day: "numeric",
      month: "short",
      year: "numeric",
    });
  };

  const handleNextDay = async () => {
    if (hasUnplayed || activeMatch) return;

    try {
      const res = await fetch("/api/games/current/next-day", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        credentials: "include",
      });

      if (res.ok) {
        const data = await res.json();
        const updatedSave = data.gameSave;

        setCurrentGameSave(updatedSave);
        setHasUnplayed(data.hasUnplayedMatchesToday);

        if (data.hasMatchesToday) {
          const matchesRes = await fetch(
            `/api/matches/today/${updatedSave.id}`,
            { credentials: "include" }
          );

          if (matchesRes.ok) {
            const matches = await matchesRes.json();
            navigate(`/today-matches/${updatedSave.id}`, {
              state: { matches },
            });
          }
        }
      }
    } catch (err) {
      console.error("Next Day failed:", err);
    }
  };

  const handleGoToMatch = () => {
    if (activeMatch) {
      navigate(`/match/${activeMatch.id}`);
    }
  };

  const buttonLabel = activeMatch
    ? "To Match"
    : hasUnplayed
    ? "Match Day"
    : currentGameSave?.nextDayActionLabel ?? "Next Day →";

  const buttonAction = activeMatch ? handleGoToMatch : handleNextDay;

  return (
    <header className="flex justify-between items-center px-6 py-3 bg-slate-800 text-white shadow-md">
      <div className="flex items-center gap-3">
        <Link
          to="/"
          className="text-xl font-extrabold text-sky-400 hover:text-sky-300 transition"
        >
          Dugout
        </Link>
        <div className="border-l border-slate-600 h-6"></div>
        <div>
          <h1 className="text-lg font-bold">{team?.name ?? "Unknown Team"}</h1>
          <p className="text-sm text-slate-300">
            {team?.leagueName ?? "Unknown League"}
          </p>
        </div>
      </div>

      <div className="flex items-center gap-8 text-sm text-slate-300">
        <span>{season ? formatDate(season.currentDate) : ""}</span>
        <span className="font-semibold">{username}</span>
        <span className="font-semibold text-green-400">
          {team ? `€${team.balance.toLocaleString()}` : "€0"}
        </span>
        <button
          onClick={buttonAction}
          disabled={!activeMatch && hasUnplayed}
          className={`px-4 py-2 rounded-lg font-medium transition ${
            activeMatch
              ? "bg-green-600 hover:bg-green-700"
              : hasUnplayed
              ? "bg-red-600 cursor-not-allowed"
              : "bg-sky-600 hover:bg-sky-700"
          }`}
        >
          {buttonLabel}
        </button>
      </div>
    </header>
  );
}

export default Header;
