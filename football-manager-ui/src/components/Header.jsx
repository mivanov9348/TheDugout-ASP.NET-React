// src/components/Header.jsx
import { Link, useNavigate, useLocation } from "react-router-dom";
import { useEffect } from "react";
import { useProcessing } from "../context/ProcessingContext";
import { useGame } from "../context/GameContext";
import Swal from "sweetalert2";

function Header({ username }) {
  const { startProcessing, stopProcessing } = useProcessing();
  const {
    currentGameSave,
    hasUnplayedMatchesToday: hasUnplayed,
    setHasUnplayedMatchesToday,
    isLoading,
    refreshGameStatus,
    setCurrentGameSave,
  } = useGame();

  const navigate = useNavigate();
  const location = useLocation();

  // 🔄 Проверка при смяна на страница дали има неизиграни мачове
  useEffect(() => {
    const checkStatus = async () => {
      try {
        const res = await fetch("/api/games/current/status", {
          credentials: "include",
        });
        if (!res.ok) return;
        const status = await res.json();

        setHasUnplayedMatchesToday(Boolean(status.hasUnplayedMatchesToday));

        // ако има неизиграни мачове, прехвърли към TodayMatches
        if (
          status.hasUnplayedMatchesToday &&
          !location.pathname.includes("/today-matches")
        ) {
          const gid = status.gameSave?.id ?? currentGameSave?.id;
          navigate(`/today-matches/${gid}`);
        }
      } catch (err) {
        console.error("Status refresh failed:", err);
      }
    };

    checkStatus();
  }, [location.pathname]);

  // 🕒 Бутон "Next Day"
  const handleNextDay = async () => {
    if (hasUnplayed) {
      await Swal.fire({
        title: "⚽ Match Day!",
        text: "There are unplayed matches for today!",
        icon: "info",
        confirmButtonText: "To Matches",
        confirmButtonColor: "#f59e0b",
        background: "#1e293b",
        color: "#fff",
      });
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
        const errorData = await res.json().catch(() => ({}));
        const message = errorData?.message ?? "Unknown error";

        if (message.includes("unplayed matches")) {
          await Swal.fire({
            title: "⚽ Match Day!",
            text: "There are unplayed matches for today!",
            icon: "info",
            confirmButtonText: "To Matches",
            confirmButtonColor: "#f59e0b",
            background: "#1e293b",
            color: "#fff",
          });
          navigate(`/today-matches/${currentGameSave.id}`);
          return;
        }

        console.error("Next day API failed:", message);
        const fallback = await refreshGameStatus();
        if (fallback?.hasUnplayedMatchesToday) {
          navigate(`/today-matches/${fallback.gameSave?.id ?? currentGameSave.id}`);
        }
        return;
      }

      const payload = await res.json();
      const status = payload.gameStatus ?? payload;

      if (status) {
        if (status.gameSave) setCurrentGameSave(status.gameSave);
        setHasUnplayedMatchesToday(Boolean(status.hasUnplayedMatchesToday));

        if (status.hasUnplayedMatchesToday) {
          const gid = status.gameSave?.id ?? currentGameSave.id;
          navigate(`/today-matches/${gid}`);
          return;
        }
      } else {
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

  // 🧩 UI helper-и
  const formatDate = (dateStr) =>
    dateStr
      ? new Date(dateStr).toLocaleDateString("en-GB", {
          day: "numeric",
          month: "short",
          year: "numeric",
        })
      : "";

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

  const buttonLabel = hasUnplayed ? "Match Day" : "Next Day →";
  const buttonColor = hasUnplayed
    ? "bg-amber-600 hover:bg-amber-700"
    : "bg-sky-600 hover:bg-sky-700";

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
          className={`px-4 py-2 rounded-lg font-medium transition ${buttonColor}`}
          title={hasUnplayed ? "Go to today's matches" : "Advance to next day"}
        >
          {buttonLabel}
        </button>
      </div>
    </header>
  );
}

export default Header;
