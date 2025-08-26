import React, { useState } from "react";
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
  const [selectedDate, setSelectedDate] = useState(null);
  const [eventType, setEventType] = useState("");
  const [description, setDescription] = useState("");
  const [events, setEvents] = useState([]);

  const handleDayClick = (day) => {
    if (!day) return;
    setSelectedDate(new Date(2025, 8, day))
  };

  const handleSubmit = async () => {
    const newEvent = {
      seasonId: 1, // засега фиксирано
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
      setEvents([...events, data]);
      setSelectedDate(null);
      setEventType("");
      setDescription("");
    }
  };


  return (
    <div className="p-6 max-w-4xl mx-auto">
      {/* Header */}
      <div className="flex items-center justify-between mb-6 text-gray-700">
        <button className="p-2 rounded-full hover:bg-gray-600">
          <ChevronLeft size={20} />
        </button>
        <h2 className="text-2xl font-bold">September 2025</h2>
        <button className="p-2 rounded-full hover:bg-gray-600">
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
        {Array.from({ length: 35 }).map((_, idx) => {
          const day = idx + 1 <= 30 ? idx + 1 : null;
          const dayEvents = events.filter(
            (e) => new Date(e.date).getDate() === day
          );

          return (
            <div
              key={idx}
              className="h-20 bg-gray-800 rounded-xl shadow-md flex flex-col items-center justify-center text-gray-200 hover:bg-gray-700 hover:scale-105 transition-transform duration-200 cursor-pointer"
              onClick={() => handleDayClick(day)}
            >
              {day}
              {dayEvents.map((ev, i) => (
                <span key={i} className="text-xs text-green-400">
                  {ev.type}
                </span>
              ))}
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
