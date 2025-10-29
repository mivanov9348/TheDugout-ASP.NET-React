import React, { useEffect, useState } from "react";
import { ChevronLeft, ChevronRight } from "lucide-react";
import { useActiveSeason } from "../../components/useActiveSeason";

const Calendar = ({ gameSaveId }) => {
  const { season, loading, error } = useActiveSeason(gameSaveId);
  const [currentDate, setCurrentDate] = useState(new Date());
  const [events, setEvents] = useState([]);

  useEffect(() => {
    if (season?.currentDate) {
      const parts = season.currentDate.split("-");
      const [year, month] = parts.map(Number);
      setCurrentDate(new Date(year, month - 1, 1));
    }
  }, [season]);

  useEffect(() => {
    const fetchEvents = async () => {
      if (!gameSaveId) return;
      try {
        const res = await fetch(`/api/calendar?gameSaveId=${gameSaveId}`, {
          credentials: "include",
        });
        if (!res.ok) throw new Error("Грешка при зареждане на календара");
        const data = await res.json();
        setEvents(data.events || []);
      } catch (err) {
        console.error(err);
      }
    };

    fetchEvents();
  }, [gameSaveId]);

  if (loading)
    return <div className="text-center text-gray-400">Зареждане на сезон...</div>;
  if (error)
    return <div className="text-center text-red-400">Грешка: {error}</div>;
  if (!season)
    return <div className="text-center text-gray-400">Няма активен сезон.</div>;

  const daysInMonth = new Date(
    currentDate.getFullYear(),
    currentDate.getMonth() + 1,
    0
  ).getDate();

  const firstDayOfMonth = new Date(
    currentDate.getFullYear(),
    currentDate.getMonth(),
    1
  ).getDay();

  const offset = firstDayOfMonth === 0 ? 6 : firstDayOfMonth - 1;
  const normalizedSeasonDate = season.currentDate?.split("T")[0];

  return (
    <div className="min-h-screen w-full bg-gradient-to-b from-gray-900 via-gray-800 to-gray-900 text-gray-100 p-6">
      {/* Header */}
      <div className="flex items-center justify-between mb-8">
        <button
          className="p-2 rounded-full hover:bg-gray-700 text-gray-100 transition"
          onClick={() =>
            setCurrentDate(
              new Date(currentDate.getFullYear(), currentDate.getMonth() - 1, 1)
            )
          }
        >
          <ChevronLeft size={24} />
        </button>

        <h2 className="text-3xl font-bold tracking-wide text-sky-400">
          {currentDate.toLocaleString("default", { month: "long" })}{" "}
          {currentDate.getFullYear()}
        </h2>

        <button
          className="p-2 rounded-full hover:bg-gray-700 text-gray-100 transition"
          onClick={() =>
            setCurrentDate(
              new Date(currentDate.getFullYear(), currentDate.getMonth() + 1, 1)
            )
          }
        >
          <ChevronRight size={24} />
        </button>
      </div>

      {/* Legend */}
      <div className="flex flex-wrap gap-4 mb-6 text-sm">
        <span className="flex items-center gap-1 text-gray-300">
          <span className="w-3 h-3 bg-yellow-500 rounded"></span> Transfer
        </span>
        <span className="flex items-center gap-1 text-gray-300">
          <span className="w-3 h-3 bg-blue-600 rounded"></span> League
        </span>
        <span className="flex items-center gap-1 text-gray-300">
          <span className="w-3 h-3 bg-purple-600 rounded"></span> Europe
        </span>
        <span className="flex items-center gap-1 text-gray-300">
          <span className="w-3 h-3 bg-green-600 rounded"></span> Cup
        </span>
        <span className="flex items-center gap-1 text-gray-300">
          <span className="w-3 h-3 bg-gray-600 rounded"></span> Training
        </span>
      </div>

      {/* Days of week */}
      <div className="grid grid-cols-7 gap-2 text-center font-semibold text-gray-300 mb-2">
        {["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"].map((day) => (
          <div key={day}>{day}</div>
        ))}
      </div>

      {/* Calendar grid */}
      <div className="grid grid-cols-7 gap-3 auto-rows-[160px]">
        {Array.from({ length: offset }).map((_, idx) => (
          <div key={`empty-${idx}`} />
        ))}

        {Array.from({ length: daysInMonth }).map((_, idx) => {
          const day = idx + 1;
          const isoDay = `${currentDate.getFullYear()}-${String(
            currentDate.getMonth() + 1
          ).padStart(2, "0")}-${String(day).padStart(2, "0")}`;
          const dayEvents = events.filter((e) => e.date === isoDay);
          const isCurrentDay = normalizedSeasonDate === isoDay;

          return (
            <div
              key={idx}
              className={`rounded-xl shadow-md flex flex-col items-start p-2 transition-all duration-200
                ${dayEvents.length > 0 ? "bg-gray-700 hover:bg-gray-600" : "bg-gray-800 hover:bg-gray-700"}
                ${isCurrentDay ? "border-4 border-red-500 shadow-[0_0_12px_rgba(239,68,68,0.8)]" : ""}
              `}
            >
              <div className="w-full flex justify-between items-center mb-1">
                <span className="font-bold text-gray-100">{day}</span>
              </div>

              <div className="flex-1 w-full overflow-y-auto space-y-1 text-xs">
                {dayEvents.length > 0 ? (
                  dayEvents.map((ev, i) => {
                    const parts = ev.description.split(",").map((p) => p.trim());
                    return (
                      <div key={i} className="space-y-0.5">
                        {parts.map((part, j) => (
                          <div
                            key={j}
                            className={`px-1 py-0.5 rounded truncate ${
                              ev.type === "TransferWindow"
                                ? "bg-yellow-500 text-gray-900 font-bold"
                                : ev.type === "ChampionshipMatch"
                                ? "bg-blue-600 text-gray-100"
                                : ev.type === "EuropeanMatch"
                                ? "bg-purple-600 text-gray-100"
                                : ev.type === "CupMatch"
                                ? "bg-green-600 text-gray-100"
                                : ev.type === "TrainingDay"
                                ? "bg-gray-600 text-gray-100"
                                : "bg-gray-700 text-gray-100"
                            }`}
                          >
                            {part}
                          </div>
                        ))}
                      </div>
                    );
                  })
                ) : (
                  <div className="text-gray-500 italic">Training</div>
                )}
              </div>

              {dayEvents.some((ev) => ev.type === "TransferWindow") && (
                <button
                  className="mt-1 px-2 py-1 bg-yellow-500 text-gray-900 text-[10px] font-bold rounded hover:bg-yellow-400 transition w-full"
                  onClick={() =>
                    alert(`Assign Friendly for ${day}/${currentDate.getMonth() + 1}`)
                  }
                >
                  Assign Friendly
                </button>
              )}
            </div>
          );
        })}
      </div>
    </div>
  );
};

export default Calendar;
