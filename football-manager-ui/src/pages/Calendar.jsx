import React from "react";
import { ChevronLeft, ChevronRight } from "lucide-react";

const Calendar = () => {
  const daysOfWeek = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];

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
        {Array.from({ length: 35 }).map((_, idx) => (
          <div
            key={idx}
            className="h-20 bg-gray-800 rounded-xl shadow-md flex items-center justify-center text-gray-200 hover:bg-gray-700 hover:scale-105 transition-transform duration-200"
          >
            {idx + 1 <= 30 ? idx + 1 : ""}
          </div>
        ))}
      </div>
    </div>
  );
};

export default Calendar;
