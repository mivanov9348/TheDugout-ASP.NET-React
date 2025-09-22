import React, { useState, useEffect } from "react";
import { ChevronLeft, ChevronRight } from "lucide-react";

const Calendar = ({ gameSaveId }) => {
  const daysOfWeek = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];
  const [currentDate, setCurrentDate] = useState(new Date(2025, 6, 1));
  const [events, setEvents] = useState([]);
  const [seasonCurrentDate, setSeasonCurrentDate] = useState(null);

  // helper за нормализация на датите
  const normalize = (dateStr) => {
    const d = new Date(dateStr);
    return d.toISOString().slice(0, 10); // винаги YYYY-MM-DD
  };

  // Зареждане от бекенда
useEffect(() => {
  const fetchEvents = async () => {
    if (!gameSaveId) return;
    try {
      const res = await fetch(`/api/calendar?gameSaveId=${gameSaveId}`, {
        credentials: "include",
      });
      if (!res.ok) throw new Error("Грешка при зареждане на календара");
      const seasons = await res.json();

      console.log("📅 API response:", seasons); // 👉 виж какво идва

      if (seasons.length > 0) {
        console.log("📅 Events loaded:", seasons[0].events);
        setEvents(seasons[0].events || []);
        setSeasonCurrentDate(seasons[0].currentDate);
      }
    } catch (err) {
      console.error(err);
    }
  };

  fetchEvents();
}, [gameSaveId]);


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

  // shift така че понеделник да е първи
  const offset = firstDayOfMonth === 0 ? 6 : firstDayOfMonth - 1;

  return (
    <div className="p-6 max-w-6xl mx-auto">
      {/* Header */}
      <div className="flex items-center justify-between mb-6 text-gray-700">
        <button
          className="p-2 rounded-full hover:bg-gray-200"
          onClick={() =>
            setCurrentDate(
              new Date(currentDate.getFullYear(), currentDate.getMonth() - 1, 1)
            )
          }
        >
          <ChevronLeft size={20} />
        </button>
        <h2 className="text-2xl font-bold">
          {currentDate.toLocaleString("default", { month: "long" })}{" "}
          {currentDate.getFullYear()}
        </h2>
        <button
          className="p-2 rounded-full hover:bg-gray-200"
          onClick={() =>
            setCurrentDate(
              new Date(currentDate.getFullYear(), currentDate.getMonth() + 1, 1)
            )
          }
        >
          <ChevronRight size={20} />
        </button>
      </div>

      {/* Legend */}
      <div className="flex gap-4 mb-4 text-sm flex-wrap text-gray-700">
        <span className="flex items-center gap-1">
          <span className="w-3 h-3 bg-yellow-500 inline-block rounded"></span>{" "}
          Transfer
        </span>
        <span className="flex items-center gap-1">
          <span className="w-3 h-3 bg-blue-600 inline-block rounded"></span>{" "}
          League
        </span>
        <span className="flex items-center gap-1">
          <span className="w-3 h-3 bg-purple-600 inline-block rounded"></span>{" "}
          Europe
        </span>
        <span className="flex items-center gap-1">
          <span className="w-3 h-3 bg-green-600 inline-block rounded"></span>{" "}
          Cup
        </span>
        <span className="flex items-center gap-1">
          <span className="w-3 h-3 bg-gray-500 inline-block rounded"></span>{" "}
          Free day
        </span>
      </div>

      {/* Days of week */}
      <div className="grid grid-cols-7 gap-2 text-center font-medium text-gray-700 mb-2">
        {daysOfWeek.map((day, idx) => (
          <div key={idx}>{day}</div>
        ))}
      </div>

      {/* Calendar grid */}
      <div className="grid grid-cols-7 gap-2">
        {Array.from({ length: offset }).map((_, idx) => (
          <div key={`empty-${idx}`} />
        ))}

        {Array.from({ length: daysInMonth }).map((_, idx) => {
          const day = idx + 1;
          const isoDay = `${currentDate.getFullYear()}-${String(
            currentDate.getMonth() + 1
          ).padStart(2, "0")}-${String(day).padStart(2, "0")}`;

          const dayEvents = events.filter(
            (e) => normalize(e.date) === isoDay
          );

          const isCurrentDay =
            seasonCurrentDate && normalize(seasonCurrentDate) === isoDay;

          return (
            <div
              key={idx}
              className={`h-32 rounded-xl shadow-md flex flex-col items-start p-2 text-gray-200
                ${dayEvents.length > 0 ? "bg-gray-800" : "bg-gray-700"}
                ${isCurrentDay ? "border-4 border-red-500" : ""}
              `}
            >
              {/* Денят */}
              <div className="w-full flex justify-between items-center mb-1">
                <span className="font-bold">{day}</span>
              </div>

              {/* Събитията за деня */}
              <div className="flex-1 w-full overflow-y-auto space-y-1 text-xs">
                {dayEvents.length > 0 ? (
                  dayEvents.map((ev, i) => (
                    <div
                      key={i}
                      className={`px-1 py-0.5 rounded truncate ${
                        ev.type === "TransferWindow"
                          ? "bg-yellow-500 text-black font-bold"
                          : ev.type === "ChampionshipMatch"
                          ? "bg-blue-600"
                          : ev.type === "EuropeanMatch"
                          ? "bg-purple-600"
                          : ev.type === "CupMatch"
                          ? "bg-green-600"
                          : "bg-gray-600"
                      }`}
                    >
                      {ev.description}
                    </div>
                  ))
                ) : (
                  <div className="text-gray-400 italic">Free day</div>
                )}
              </div>

              {/* Бутон за TransferWindow */}
              {dayEvents.some((ev) => ev.type === "TransferWindow") && (
                <button
                  className="mt-1 px-2 py-1 bg-yellow-500 text-black text-[10px] font-bold rounded hover:bg-yellow-400 transition w-full"
                  onClick={() =>
                    alert(
                      `Assign Friendly for ${day}/${currentDate.getMonth() + 1}`
                    )
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
