import { Link } from "react-router-dom";
import { useGameSave } from "../context/GameSaveContext";

function Header({ username }) {
  const { currentGameSave, setCurrentGameSave } = useGameSave();

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
    try {
      const res = await fetch("/api/games/current/next-day", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        credentials: "include",
      });

      if (res.ok) {
        const updatedSave = await res.json();
        setCurrentGameSave(updatedSave);
      }
    } catch (err) {
      console.error("Next Day failed:", err);
    }
  };

  // 🔹 използваме това, което идва от бекенда
  const nextDayLabel = currentGameSave?.nextDayActionLabel ?? "Next Day →";

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
          className="bg-sky-600 hover:bg-sky-700 px-4 py-2 rounded-lg font-medium"
        >
          {nextDayLabel}
        </button>
      </div>
    </header>
  );
}

export default Header;
