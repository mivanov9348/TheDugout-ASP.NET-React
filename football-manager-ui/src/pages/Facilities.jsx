import { useState } from "react";
import { ArrowUpCircle } from "lucide-react";

export default function Facilities() {
  // ⚠️ Example data only — this is just a skeleton, not final implementation
  const [facilities, setFacilities] = useState({
    stadium: { name: "🏟️ Stadium", level: 3, capacity: 25000 },
    training: { name: "💪 Training Facilities", level: 2, capacity: "Medium" },
    youth: { name: "👶 Youth Academy", level: 1, capacity: "Small" },
  });

  const handleUpgrade = (key) => {
    setFacilities((prev) => ({
      ...prev,
      [key]: { ...prev[key], level: prev[key].level + 1 },
    }));
  };

  const renderFacility = (key, facility) => {
    return (
      <div className="bg-white/80 backdrop-blur-sm rounded-2xl shadow-lg p-6 flex flex-col hover:scale-[1.02] transition-transform duration-200">
        <h3 className="text-xl font-semibold mb-2 flex items-center gap-2">
          {facility.name}
        </h3>
        <div className="mb-4">
          <div className="text-sm text-gray-600">
            Level: <span className="font-bold">{facility.level}</span>
          </div>
          {facility.capacity && (
            <div className="text-sm text-gray-600">
              Capacity: <span className="font-bold">{facility.capacity}</span>
            </div>
          )}
          <div className="w-full bg-gray-200 rounded-full h-3 mt-3">
            <div
              className="bg-blue-500 h-3 rounded-full"
              style={{ width: `${facility.level * 20}%` }}
            ></div>
          </div>
        </div>
        <button
          onClick={() => handleUpgrade(key)}
          className="mt-auto flex items-center justify-center gap-2 bg-blue-600 hover:bg-blue-700 text-white font-semibold py-2 px-4 rounded-xl shadow transition"
        >
          <ArrowUpCircle size={18} />
          Upgrade
        </button>
      </div>
    );
  };

  return (
    <div className="p-6">
      <h2 className="text-2xl font-bold mb-2 text-center">🏗️ Facilities</h2>
      <p className="text-center text-sm text-gray-500 mb-6">
        Example UI — values and logic are placeholders, not final.
      </p>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        {Object.entries(facilities).map(([key, facility]) =>
          renderFacility(key, facility)
        )}
      </div>
    </div>
  );
}
