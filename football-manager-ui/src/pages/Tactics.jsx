import React, { useState, useEffect } from "react";

const Tactics = ({ gameSaveId }) => {
  const [formations, setFormations] = useState([]);
  const [players, setPlayers] = useState({
    Goalkeeper: [],
    Defender: [],
    Midfielder: [],
    Forward: [],
  });
  const [selectedFormation, setSelectedFormation] = useState("");
  const [lineup, setLineup] = useState({}); // { "DEF-1": playerId, ... }
  const [loading, setLoading] = useState(true);

  // Зареждане на формации
  useEffect(() => {
    const fetchFormations = async () => {
      try {
        const res = await fetch(`/api/tactics/${gameSaveId}/formations`, {
          credentials: "include",
        });
        if (!res.ok) throw new Error("Грешка при зареждане на формации");
        const data = await res.json();
        setFormations(data); // [{ id, name }, ...]
      } catch (err) {
        console.error(err);
      }
    };
    if (gameSaveId) fetchFormations();
  }, [gameSaveId]);

  // Зареждане на играчи
  useEffect(() => {
    const fetchPlayers = async () => {
      try {
        const res = await fetch(`/api/tactics/${gameSaveId}/players`, {
          credentials: "include",
        });
        if (!res.ok) throw new Error("Грешка при зареждане на играчи");
        const data = await res.json();
        setPlayers(data);
      } catch (err) {
        console.error(err);
      } finally {
        setLoading(false);
      }
    };
    if (gameSaveId) fetchPlayers();
  }, [gameSaveId]);

  // Изчисляване на слотовете по формация
  const getPositionSlots = () => {
    if (!selectedFormation) return { GK: 1, DEF: 0, MID: 0, FWD: 0 };
    const parts = selectedFormation.split("-").map(Number);
    let def = parts[0];
    let mid = parts[1];
    let fwd = parts[2];
    if (parts.length === 4) {
      mid += parts[2];
      fwd = parts[3];
    }
    return { GK: 1, DEF: def, MID: mid, FWD: fwd };
  };
  const slots = getPositionSlots();

  // Запис на тактиката
  const handleSave = async () => {
    try {
      const res = await fetch(`/api/tactics/${gameSaveId}/save`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        credentials: "include",
        body: JSON.stringify({
          formation: selectedFormation,
          lineup,
        }),
      });
      if (!res.ok) throw new Error("Грешка при запис на тактиката");
      alert("Тактиката е записана успешно!");
    } catch (err) {
      console.error(err);
      alert(err.message);
    }
  };

  // Рендер на dropdown за дадена позиция
  const renderSlots = (position, count, availablePlayers) => {
    const slotsArray = [];
    for (let i = 1; i <= count; i++) {
      const slotKey = `${position}-${i}`;
      slotsArray.push(
        <tr key={slotKey}>
          <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
            {`${position} ${i}`}
          </td>
          <td className="px-6 py-4 whitespace-nowrap">
            <select
              className="block w-full px-3 py-2 border border-gray-300 bg-white rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm"
              value={lineup[slotKey] || ""}
              onChange={(e) =>
                setLineup((prev) => ({ ...prev, [slotKey]: e.target.value }))
              }
            >
              <option value="">Select Player</option>
              {availablePlayers.map((player) => (
                <option key={player.id} value={player.id}>
                  {player.name}
                </option>
              ))}
            </select>
          </td>
        </tr>
      );
    }
    return slotsArray;
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center h-screen text-gray-700">
        Зареждане...
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-100 p-6">
      <div className="max-w-6xl mx-auto">
        <h1 className="text-3xl font-bold text-center mb-8 text-gray-800">
          Team Tactics
        </h1>

        <div className="mb-8">
          <label
            htmlFor="formation"
            className="block text-sm font-medium text-gray-700 mb-2"
          >
            Select Formation
          </label>
          <select
            id="formation"
            className="block w-full px-3 py-2 border border-gray-300 bg-white rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm"
            value={selectedFormation}
            onChange={(e) => setSelectedFormation(e.target.value)}
          >
            <option value="">Choose a formation</option>
            {formations.map((form) => (
              <option key={form.id} value={form.name}>
                {form.name}
              </option>
            ))}
          </select>
        </div>

        {selectedFormation && (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
            {/* Лява таблица – всички играчи */}
            <div>
              <h2 className="text-2xl font-semibold mb-4 text-gray-800">
                All Players
              </h2>
              <div className="bg-white shadow-md rounded-lg overflow-hidden">
                <table className="min-w-full divide-y divide-gray-200">
                  <thead className="bg-gray-50">
                    <tr>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Name
                      </th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Position
                      </th>
                    </tr>
                  </thead>
                  <tbody className="bg-white divide-y divide-gray-200">
                    {Object.entries(players).flatMap(([pos, list]) =>
                      list.map((p) => (
                        <tr key={p.id} className="hover:bg-gray-50">
                          <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                            {p.name}
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                            {pos}
                          </td>
                        </tr>
                      ))
                    )}
                  </tbody>
                </table>
              </div>
            </div>

            {/* Дясна таблица – титуляри */}
            <div>
              <h2 className="text-2xl font-semibold mb-4 text-gray-800">
                Starting Lineup
              </h2>
              <div className="bg-white shadow-md rounded-lg overflow-hidden">
                <table className="min-w-full divide-y divide-gray-200">
                  <thead className="bg-gray-50">
                    <tr>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Position
                      </th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Player
                      </th>
                    </tr>
                  </thead>
                  <tbody className="bg-white divide-y divide-gray-200">
                    {renderSlots("GK", slots.GK, players.Goalkeeper)}
                    {renderSlots("DEF", slots.DEF, players.Defender)}
                    {renderSlots("MID", slots.MID, players.Midfielder)}
                    {renderSlots("FWD", slots.FWD, players.Forward)}
                  </tbody>
                </table>
              </div>
              <button
                onClick={handleSave}
                className="mt-4 px-4 py-2 bg-indigo-600 text-white font-medium rounded-md hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
              >
                Save
              </button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default Tactics;
