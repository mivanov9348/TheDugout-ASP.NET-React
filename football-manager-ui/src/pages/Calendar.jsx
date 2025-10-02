import React, { useState, useEffect } from "react";
import { ChevronLeft, ChevronRight } from "lucide-react";


const Calendar = ({ gameSaveId }) => {
  const { currentGameSave } = useGameSave();
  const [currentDate, setCurrentDate] = useState(new Date(2025, 6, 1));
  const [events, setEvents] = useState([]);

  // Взимаме директно от контекста
  const season = currentGameSave?.seasons?.[0];
  const seasonCurrentDate = season?.currentDate; // 🔥 винаги актуално

  // Зареждане на събитията от бекенда
  useEffect(() => {
    const fetchEvents = async () => {
      if (!gameSaveId) return;
      try {
        const res = await fetch(`/api/calendar?gameSaveId=${gameSaveId}`, {
          credentials: "include",
        });
        if (!res.ok) throw new Error("Грешка при зареждане на календара");

        const seasonData = await res.json();
        setEvents(seasonData.events || []);
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
          <span className="w-3 h-3 bg-gray-600 inline-block rounded"></span>{" "}
          Training
        </span>
      </div>

      {/* Days of week */}
      <div className="grid grid-cols-7 gap-2 text-center font-medium text-gray-700 mb-2">
        {["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"].map((day, idx) => (
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

          const dayEvents = events.filter((e) => e.date === isoDay);
          const isCurrentDay =
            seasonCurrentDate && seasonCurrentDate === isoDay;

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

              {/* Събития */}
              <div className="flex-1 w-full overflow-y-auto space-y-1 text-xs">
                {dayEvents.length > 0 ? (
                  dayEvents.map((ev, i) => {
                    const parts = ev.description
                      .split(",")
                      .map((p) => p.trim());

                    return (
                      <div key={i} className="space-y-0.5">
                        {parts.map((part, j) => (
                          <div
                            key={j}
                            className={`px-1 py-0.5 rounded truncate ${
                              ev.type === "TransferWindow"
                                ? "bg-yellow-500 text-black font-bold"
                                : ev.type === "ChampionshipMatch"
                                ? "bg-blue-600"
                                : ev.type === "EuropeanMatch"
                                ? "bg-purple-600"
                                : ev.type === "CupMatch"
                                ? "bg-green-600"
                                : ev.type === "TrainingDay"
                                ? "bg-gray-600"
                                : "bg-gray-700"
                            }`}
                          >
                            {part}
                          </div>
                        ))}
                      </div>
                    );
                  })
                ) : (
                  <div className="text-gray-400 italic">Training</div>
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
