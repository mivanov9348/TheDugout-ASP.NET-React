import { Link, useNavigate, useLocation } from "react-router-dom";
import { useEffect, useState } from "react";
import { useProcessing } from "../../context/ProcessingContext";
import { useGame } from "../../context/GameContext";
import Swal from "sweetalert2";
import { RotateCcw } from "lucide-react";

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

  const [isRefreshing, setIsRefreshing] = useState(false);
  const navigate = useNavigate();
  const location = useLocation();

  // 🔄 Проверка при смяна на страница (веднъж, без полинг)
  useEffect(() => {
    const updateStatus = async () => {
      const status = await refreshGameStatus();

      // просто ъпдейтваме флага, без navigate()
      if (status?.hasUnplayedMatchesToday !== undefined) {
        setHasUnplayedMatchesToday(status.hasUnplayedMatchesToday);
      }
    };

    updateStatus();
  }, [location.pathname]);


  // 🧩 Ръчно опресняване чрез бутон 🔃
  const handleRefresh = async () => {
    setIsRefreshing(true);
    try {
      const status = await refreshGameStatus();
      if (status?.hasUnplayedMatchesToday !== undefined) {
        setHasUnplayedMatchesToday(status.hasUnplayedMatchesToday);
      }
      // ❌ без navigate()
    } catch (err) {
      console.error("Refresh failed:", err);
    } finally {
      setIsRefreshing(false);
    }
  };

  const getEventDisplay = (type) => {
  switch (type) {
    case "StartSeason":
      return { label: "Season Kickoff", icon: "🚀" };
    case "ChampionshipMatch":
      return { label: "League Match Day", icon: "🏆" };
    case "CupMatch":
      return { label: "Cup Match Day", icon: "🥇" };
    case "EuropeanMatch":
      return { label: "European Night", icon: "🌍" };
    case "FriendlyMatch":
      return { label: "Friendly Match", icon: "🤝" };
    case "TransferWindow":
      return { label: "Transfer Window", icon: "💰" };
    case "TrainingDay":
      return { label: "Training Day", icon: "💪" };
    case "EndOfSeason":
      return { label: "End of Season", icon: "🎉" };
    default:
      return { label: "Regular Day", icon: "📅" };
  }
};


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
      const res = await fetch("/api/season/current/next-day", {
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

  // 🏁 Бутон "End Season"
const handleEndSeason = async () => {
  const confirm = await Swal.fire({
    title: "🏁 End of Season",
    text: "Are you sure you want to end the season?",
    icon: "warning",
    showCancelButton: true,
    confirmButtonText: "Yes, end it",
    cancelButtonText: "Cancel",
    confirmButtonColor: "#e11d48",
    cancelButtonColor: "#334155",
    background: "#1e293b",
    color: "#fff",
  });

  if (!confirm.isConfirmed) return;

  const seasonId = currentGameSave?.activeSeason.id;
  if (!seasonId) {
    Swal.fire({
      title: "❌ Error",
      text: "No active season found.",
      icon: "error",
      background: "#1e293b",
      color: "#fff",
    });
    return;
  }

  startProcessing("Ending current season...");
  try {
    // 🔹 1. Само приключваме сезона
    const res = await fetch(`/api/season/season/${seasonId}/end`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      credentials: "include",
    });

    const data = await res.json();

    if (!res.ok) {
      await Swal.fire({
        title: "⚠️ Cannot End Season",
        text: data.message || "Unknown error",
        icon: "error",
        background: "#1e293b",
        color: "#fff",
      });
      return;
    }

    await Swal.fire({
      title: "✅ Season Ended!",
      text: data.message || "Season has been successfully completed.",
      icon: "success",
      background: "#1e293b",
      color: "#fff",
    });

    // 🔹 2. Без start-new-season, директно пращаме към overview
    navigate(`/season/${seasonId}/overview`);

  } catch (err) {
    console.error("End season failed:", err);
    Swal.fire({
      title: "❌ Error",
      text: "Unexpected error while ending season.",
      icon: "error",
      background: "#1e293b",
      color: "#fff",
    });
  } finally {
    stopProcessing();
  }
};


  // 🧩 Формат на дата
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

  const season = currentGameSave.activeSeason;
  const team = currentGameSave.userTeam;

  // Проверяваме дали е последният ден от сезона
  const isLastDay =
    season && new Date(season.currentDate).toDateString() ===
    new Date(season.endDate).toDateString();

  const buttonLabel = isLastDay
    ? "🏁 End Season"
    : hasUnplayed
      ? "Match Day"
      : "Next Day →";

  const buttonColor = isLastDay
    ? "bg-rose-600 hover:bg-rose-700"
    : hasUnplayed
      ? "bg-amber-600 hover:bg-amber-700"
      : "bg-sky-600 hover:bg-sky-700";

  const buttonHandler = isLastDay ? handleEndSeason : handleNextDay;

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

      <div className="flex items-center gap-6 text-sm text-slate-300">
        <button
          onClick={handleRefresh}
          disabled={isRefreshing}
          className="text-sky-400 hover:text-sky-300 transition"
          title="Refresh game status"
        >
          <RotateCcw
            className={`w-5 h-5 ${isRefreshing ? "animate-spin text-sky-300" : ""}`}
          />
        </button>
{season?.currentEventType && (
  <div className="flex items-center gap-2 text-sm text-sky-300 font-semibold">
    <span>{getEventDisplay(season.currentEventType).icon}</span>
    <span>{getEventDisplay(season.currentEventType).label}</span>
  </div>
)}

        <span>{season ? formatDate(season.currentDate) : ""}</span>
        <span className="font-semibold">{username}</span>
        <span className="font-semibold text-green-400">
          {team ? `€${team.balance.toLocaleString()}` : "€0"}
        </span>

        <button
          onClick={buttonHandler}
          className={`px-4 py-2 rounded-lg font-medium transition ${buttonColor}`}
        >
          {buttonLabel}
        </button>
      </div>
    </header>
  );
}

export default Header;
