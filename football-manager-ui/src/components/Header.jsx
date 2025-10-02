// src/components/Header.jsx
import { Link, useNavigate } from "react-router-dom";
import { useProcessing } from "../context/ProcessingContext";
import { useGame } from "../context/GameContext";

function Header({ username }) {
  const { startProcessing, stopProcessing } = useProcessing();
  const {
    currentGameSave,
    hasUnplayedMatchesToday: hasUnplayed,
    setHasUnplayedMatchesToday,
    activeMatch,
    setActiveMatch,
    isLoading,
    refreshGameStatus,
    setCurrentGameSave,
  } = useGame();

  const navigate = useNavigate();

  if (isLoading && !currentGameSave) {
    return (
      <header className="px-6 py-3 bg-slate-800 text-white shadow-md">
        <h1 className="font-bold text-lg">Loading...</h1>
      </header>
    );
  }

  if (!currentGameSave) {
    return (
      <header className="px-6 py-3 bg-slate-800 text-white shadow-md">
        <h1 className="font-bold text-lg">No Save Loaded</h1>
      </header>
    );
  }

  const season = currentGameSave.seasons?.[0];
  const team = currentGameSave.userTeam;

  const formatDate = (dateStr) =>
    dateStr
      ? new Date(dateStr).toLocaleDateString("en-GB", {
          day: "numeric",
          month: "short",
          year: "numeric",
        })
      : "";

  const handleNextDay = async () => {
    // ако вече има неизиграни — отиди в today-matches
    if (hasUnplayed) {
      navigate(`/today-matches/${currentGameSave.id}`);
      return;
    }

    startProcessing("Advancing to next day...");
    try {
      const res = await fetch("/api/games/current/next-day", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        credentials: "include",
      });

      if (!res.ok) {
        console.error("Next day API failed with status", res.status);
        // опитай да опресниш статуса директно
        const fallback = await refreshGameStatus();
        if (fallback?.hasUnplayedMatchesToday) {
          navigate(`/today-matches/${fallback.gameSave?.id ?? currentGameSave.id}`);
        }
        return;
      }

      const payload = await res.json();
      const status = payload.gameStatus ?? payload;

      // Ако backend е включил новия status — обнови контекста
      if (status) {
        if (status.gameSave) setCurrentGameSave(status.gameSave);
        setHasUnplayedMatchesToday(Boolean(status.hasUnplayedMatchesToday));
        setActiveMatch(status.activeMatch ?? null);

        // веднага навигирай при наличие на неизиграни
        if (status.hasUnplayedMatchesToday) {
          const gid = status.gameSave?.id ?? currentGameSave.id;
          navigate(`/today-matches/${gid}`);
          return;
        }
      } else {
        // ако няма status в тялото — опресни и провери
        const fallback = await refreshGameStatus();
        if (fallback?.hasUnplayedMatchesToday) {
          navigate(`/today-matches/${fallback.gameSave?.id ?? currentGameSave.id}`);
          return;
        }
      }
    } catch (err) {
      console.error("Next Day failed:", err);
    } finally {
      stopProcessing();
    }
  };

  const handleGoToMatch = () => activeMatch && navigate(`/match/${activeMatch.id}`);
  const handleGoToTodayMatches = () =>
    currentGameSave && navigate(`/today-matches/${currentGameSave.id}`);

  let buttonLabel = "Next Day →";
  let buttonAction = handleNextDay;
  let buttonTitle = "Continue to next day";

  if (activeMatch) {
    buttonLabel = "To Match";
    buttonAction = handleGoToMatch;
    buttonTitle = "Continue active match";
  } else if (hasUnplayed) {
    buttonLabel = "Match Day";
    buttonAction = handleGoToTodayMatches;
    buttonTitle = "You have unplayed matches today";
  }

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
          title={buttonTitle}
          className={`px-4 py-2 rounded-lg font-medium transition ${
            activeMatch
              ? "bg-green-600 hover:bg-green-700"
              : hasUnplayed
              ? "bg-amber-600 hover:bg-amber-700"
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
