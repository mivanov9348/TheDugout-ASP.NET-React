import React, { useState, useEffect } from "react";
import Swal from "sweetalert2";

const Tactics = ({ gameSaveId, teamId }) => {
  const [formations, setFormations] = useState([]);
  const [players, setPlayers] = useState({
    Goalkeeper: [],
    Defender: [],
    Midfielder: [],
    Forward: [],
  });
  const [selectedFormation, setSelectedFormation] = useState("");
  const [lineup, setLineup] = useState({});
  const [loading, setLoading] = useState(true);

  // Load players
  useEffect(() => {
    const fetchPlayers = async () => {
      try {
        const res = await fetch(`/api/team/by-save/${gameSaveId}`, {
          credentials: "include",
        });
        if (!res.ok) throw new Error("Failed to load team");
        const data = await res.json();

        const grouped = {
          Goalkeeper: [],
          Defender: [],
          Midfielder: [],
          Forward: [],
        };

        data.players.forEach((p) => {
          const playerObj = {
            id: p.id,
            name: p.fullName,
          };

          const posMap = {
            Goalkeeper: "Goalkeeper",
            Defender: "Defender",
            Midfielder: "Midfielder",
            Forward: "Forward",
            Attacker: "Forward",
          };

          const key = posMap[p.position] ?? posMap[p.Position];
          if (key && grouped[key]) grouped[key].push(playerObj);
        });

        setPlayers(grouped);
      } catch (err) {
        console.error(err);
      } finally {
        setLoading(false);
      }
    };

    if (gameSaveId) fetchPlayers();
  }, [gameSaveId]);

  // Load formations
  useEffect(() => {
    const fetchFormations = async () => {
      try {
        const res = await fetch("/api/tactics", { credentials: "include" });
        if (!res.ok) throw new Error("Failed to load tactics");
        const data = await res.json();
        setFormations(data);
      } catch (err) {
        console.error(err);
      }
    };

    fetchFormations();
  }, []);

  // Save
  const handleSave = async () => {
  try {
    const tacticId = formations.find((f) => f.name === selectedFormation)?.id;

    if (!tacticId) {
      Swal.fire("Грешка", "Моля избери формация преди да запазиш.", "error");
      return;
    }

    const res = await fetch(`/api/tactics/${teamId}`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      credentials: "include",
      body: JSON.stringify({
        tacticId,
        customName: selectedFormation,
        lineup,
      }),
    });

    const rawText = await res.text(); // взимаме суровия отговор
    console.log("Response status:", res.status);
    console.log("Response headers:", [...res.headers.entries()]);
    console.log("Raw response text:", rawText);

    if (!res.ok) {
      let errorMessage = "Неуспешно запазване";

      try {
        const parsed = JSON.parse(rawText);
        errorMessage = parsed.error || rawText || errorMessage;
      } catch {
        errorMessage = rawText || errorMessage;
      }

      Swal.fire("Грешка", errorMessage, "error");
      return;
    }

    Swal.fire("Успех!", "Тактиката е запазена!", "success");
  } catch (err) {
    console.error("Save error:", err);
    Swal.fire("Грешка", err.message, "error");
  }
};

  const getPositionSlots = () => {
    if (!selectedFormation) return { GK: 1, DF: 0, MID: 0, ATT: 0 };
    const form = formations.find((f) => f.name === selectedFormation);
    if (!form) return { GK: 1, DF: 0, MID: 0, ATT: 0 };
    return {
      GK: 1,
      DF: form.defenders,
      MID: form.midfielders,
      ATT: form.forwards,
    };
  };
  const slots = getPositionSlots();

  const getSelectedPlayerIds = () => Object.values(lineup).filter(Boolean);

  const renderSlots = (position, count, availablePlayers) => {
    const slotsArray = [];
    const selectedIds = getSelectedPlayerIds();

    for (let i = 1; i <= count; i++) {
      const slotKey = `${position}-${i}`;
      slotsArray.push(
        <tr key={slotKey}>
          <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
            {`${position} ${i}`}
          </td>
          <td className="px-6 py-4 whitespace-nowrap">
            <select
              className="block w-full px-3 py-2 border border-gray-300 bg-white rounded-md shadow-sm"
              value={lineup[slotKey] || ""}
              onChange={(e) =>
                setLineup((prev) => ({ ...prev, [slotKey]: e.target.value }))
              }
            >
              <option value="">Select Player</option>
              {availablePlayers
                .filter(
                  (player) =>
                    !selectedIds.includes(player.id.toString()) ||
                    lineup[slotKey] === player.id.toString()
                )
                .map((player) => (
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
        Loading...
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-100 p-6">
      <div className="max-w-6xl mx-auto">
        <h1 className="text-3xl font-bold text-center mb-8 text-gray-800">
          Team Tactics
        </h1>

        {/* Formation selector */}
        <div className="mb-8">
          <label
            htmlFor="formation"
            className="block text-sm font-medium text-gray-700 mb-2"
          >
            Select Formation
          </label>
          <select
            id="formation"
            className="block w-full px-3 py-2 border border-gray-300 bg-white rounded-md shadow-sm"
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
            {/* Players */}
            <div>
              <h2 className="text-2xl font-semibold mb-4 text-gray-800">
                All Players
              </h2>
              <div className="bg-white shadow-md rounded-lg overflow-hidden">
                <table className="min-w-full divide-y divide-gray-200">
                  <thead className="bg-gray-50">
                    <tr>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                        Name
                      </th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                        Position
                      </th>
                    </tr>
                  </thead>
                  <tbody className="bg-white divide-y divide-gray-200">
                    {Object.entries(players).flatMap(([pos, list]) =>
                      list.map((p) => (
                        <tr key={p.id} className="hover:bg-gray-50">
                          <td className="px-6 py-4 text-sm font-medium text-gray-900">
                            {p.name}
                          </td>
                          <td className="px-6 py-4 text-sm text-gray-500">
                            {pos}
                          </td>
                        </tr>
                      ))
                    )}
                  </tbody>
                </table>
              </div>
            </div>

            {/* Lineup */}
            <div>
              <h2 className="text-2xl font-semibold mb-4 text-gray-800">
                Starting Lineup
              </h2>
              <div className="bg-white shadow-md rounded-lg overflow-hidden">
                <table className="min-w-full divide-y divide-gray-200">
                  <thead className="bg-gray-50">
                    <tr>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                        Position
                      </th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                        Player
                      </th>
                    </tr>
                  </thead>
                  <tbody className="bg-white divide-y divide-gray-200">
                    {renderSlots("GK", slots.GK, players.Goalkeeper)}
                    {renderSlots("DF", slots.DF, players.Defender)}
                    {renderSlots("MID", slots.MID, players.Midfielder)}
                    {renderSlots("ATT", slots.ATT, players.Forward)}
                  </tbody>
                </table>
              </div>
              <button
                onClick={handleSave}
                className="mt-4 px-4 py-2 bg-indigo-600 text-white rounded-md hover:bg-indigo-700"
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
