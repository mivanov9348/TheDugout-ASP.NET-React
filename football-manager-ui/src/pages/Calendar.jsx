import React, { useState, useEffect } from "react";
import { ChevronLeft, ChevronRight } from "lucide-react";

const eventTypes = [
  "ChampionshipMatch",
  "CupMatch",
  "EuropeanMatch",
  "FriendlyMatch",
  "TransferWindow",
  "Other",
];

const Calendar = () => {
  const daysOfWeek = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];
  const [currentDate, setCurrentDate] = useState(new Date(2025, 8, 1)); 
  const [selectedDate, setSelectedDate] = useState(null);
  const [eventType, setEventType] = useState("");
  const [description, setDescription] = useState("");
  const [events, setEvents] = useState([]);

  const seasonId = 1; 

  // зареждане на евенти от бекенда
  useEffect(() => {
    const fetchEvents = async () => {
      const res = await fetch(`/api/SeasonEvents/${seasonId}`);
      if (res.ok) {
        const data = await res.json();
        setEvents(data);
      }
    };
    fetchEvents();
  }, [seasonId]);

  const handleDayClick = (day) => {
    if (!day) return;
    setSelectedDate(new Date(currentDate.getFullYear(), currentDate.getMonth(), day));
  };

  const handleSubmit = async () => {
    if (!eventType) return alert("Избери тип събитие!");

    const newEvent = {
      seasonId,
      date: selectedDate.toISOString(),
      type: eventType,
      description,
    };

    const res = await fetch("/api/SeasonEvents", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(newEvent),
    });

    if (res.ok) {
      const data = await res.json();
      setEvents([...events, data]); // добавяме новия евент
      setSelectedDate(null);
      setEventType("");
      setDescription("");
    } else {
      const err = await res.text();
      alert("Error: " + err);
    }
  };

  const daysInMonth = new Date(
    currentDate.getFullYear(),
    currentDate.getMonth() + 1,
    0
  ).getDate();

  return (
    <div className="p-6 max-w-4xl mx-auto">
      {/* Header */}
      <div className="flex items-center justify-between mb-6 text-gray-700">
        <button
          className="p-2 rounded-full hover:bg-gray-600"
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
          className="p-2 rounded-full hover:bg-gray-600"
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
  className="h-32 bg-gray-800 rounded-xl shadow-md flex flex-col items-start p-2 text-gray-200 hover:bg-gray-700 hover:scale-105 transition-transform duration-200 cursor-pointer"
  onClick={() => handleDayClick(day)}
>
  {/* Денят горе вдясно */}
  <div className="w-full flex justify-between items-center mb-1">
    <span className="font-bold">{day}</span>
  </div>

  {/* Събития за деня */}
  <div className="flex flex-col gap-1 overflow-hidden text-xs">
    {dayEvents.map((ev, i) => (
      <span
        key={i}
        className="truncate text-green-400"
        title={`${ev.type} - ${ev.description}`}
      >
        {ev.description || ev.type}
      </span>
    ))}
  </div>
</div>

          );
        })}
      </div>

      {/* Modal */}
      {selectedDate && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center">
          <div className="bg-white p-6 rounded-xl shadow-lg w-96">
            <h3 className="text-lg font-bold mb-4">
              Add Event for {selectedDate.toDateString()}
            </h3>

            <select
              className="w-full p-2 border rounded mb-3"
              value={eventType}
              onChange={(e) => setEventType(e.target.value)}
            >
              <option value="">Select event type</option>
              {eventTypes.map((t) => (
                <option key={t} value={t}>
                  {t}
                </option>
              ))}
            </select>

            <textarea
              className="w-full p-2 border rounded mb-3"
              placeholder="Description"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
            />

            <div className="flex justify-end gap-2">
              <button
                onClick={() => setSelectedDate(null)}
                className="px-4 py-2 bg-gray-300 rounded"
              >
                Cancel
              </button>
              <button
                onClick={handleSubmit}
                className="px-4 py-2 bg-blue-600 text-white rounded"
              >
                Save
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default Calendar;
