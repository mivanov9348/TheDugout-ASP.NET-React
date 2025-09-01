import React, { useState, useEffect } from "react";
import { ChevronLeft, ChevronRight } from "lucide-react";

const Calendar = ({ gameSaveId }) => {
  const daysOfWeek = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];
  const [currentDate, setCurrentDate] = useState(new Date(2025, 8, 1));
  const [events, setEvents] = useState([]);

  // 🟢 Зареждаме евентите от бекенда
  useEffect(() => {
    const fetchEvents = async () => {
      if (!gameSaveId) return;
      try {
        const res = await fetch(`/api/calendar?gameSaveId=${gameSaveId}`, {
          credentials: "include",
        });
        if (!res.ok) throw new Error("Грешка при зареждане на календара");
        const seasons = await res.json();

        if (seasons.length > 0) {
          setEvents(seasons[0].events || []);
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

  return (
    <div className="p-6 max-w-5xl mx-auto">
      {/* Header */}
      <div className="flex items-center justify-between mb-6 text-gray-700">
        <button
          className="p-2 rounded-full hover:bg-gray-300"
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
          className="p-2 rounded-full hover:bg-gray-300"
          onClick={() =>
            setCurrentDate(
              new Date(currentDate.getFullYear(), currentDate.getMonth() + 1, 1)
            )
          }
        >
          <ChevronRight size={20} />
        </button>
      </div>

      {/* Days of week */}
      <div className="grid grid-cols-7 gap-2 text-center font-medium text-gray-700 mb-2">
        {daysOfWeek.map((day, idx) => (
          <div key={idx}>{day}</div>
        ))}
      </div>

      {/* Calendar grid */}
      <div className="grid grid-cols-7 gap-2">
        {Array.from({ length: daysInMonth }).map((_, idx) => {
          const day = idx + 1;
          const dayEvents = events.filter(
            (e) =>
              new Date(e.date).getDate() === day &&
              new Date(e.date).getMonth() === currentDate.getMonth() &&
              new Date(e.date).getFullYear() === currentDate.getFullYear()
          );

          return (
            <div
              key={idx}
              className="h-32 bg-gray-800 rounded-xl shadow-md flex flex-col items-start p-2 text-gray-200"
            >
              {/* Денят */}
              <div className="w-full flex justify-between items-center mb-1">
                <span className="font-bold">{day}</span>
              </div>

              {/* Събитията за деня */}
              <div className="flex-1 w-full overflow-y-auto space-y-1 text-xs">
                {dayEvents.map((ev, i) => (
                  <div
                    key={i}
                    className="px-1 py-0.5 rounded bg-gray-700 text-gray-100 truncate"
                  >
                    {ev.description}
                  </div>
                ))}
              </div>

              {/* Бутон само за TransferWindow */}
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
