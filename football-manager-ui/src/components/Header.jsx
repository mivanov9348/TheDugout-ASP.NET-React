import { Link, useNavigate } from "react-router-dom";
import { useGameSave } from "../context/GameSaveContext";
import { useState } from "react";

function Header({ username }) {
  const { currentGameSave, setCurrentGameSave } = useGameSave();
  const [hasUnplayed, setHasUnplayed] = useState(false);

  const navigate = useNavigate();

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
    if (hasUnplayed) return; // 🚫 защитна проверка — не минаваш напред

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

  const nextDayLabel = hasUnplayed
    ? "Match Day"
    : currentGameSave?.nextDayActionLabel ?? "Next Day →";

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
          onClick={handleNextDay}
          disabled={hasUnplayed} // 🚫 блокираме бутона
          className={`px-4 py-2 rounded-lg font-medium transition ${
            hasUnplayed
              ? "bg-red-600 cursor-not-allowed"
              : "bg-sky-600 hover:bg-sky-700"
          }`}
        >
          {nextDayLabel}
        </button>
      </div>
    </header>
  );
}

export default Header;
