import React, { useState, useEffect } from "react";
import Swal from "sweetalert2";

const Tactics = ({ gameSaveId, teamId }) => {
  const [formations, setFormations] = useState([]);
  const [players, setPlayers] = useState([]);
  const [selectedFormation, setSelectedFormation] = useState("");
  const [lineup, setLineup] = useState({});
  const [substitutes, setSubstitutes] = useState({});
  const [loading, setLoading] = useState(true);
  const [allAttributes, setAllAttributes] = useState([]);

  const getRowClass = (position) => {
    switch (position) {
      case "Goalkeeper":
        return "bg-yellow-50";
      case "Defender":
        return "bg-blue-50";
      case "Midfielder":
        return "bg-green-50";
      case "Attacker":
        return "bg-red-50";
      default:
        return "";
    }
  };

  // Зареждаме играчите
  useEffect(() => {
    const fetchPlayers = async () => {
      try {
        const res = await fetch(`/api/team/by-save/${gameSaveId}`, {
          credentials: "include",
        });
        if (!res.ok) throw new Error("Failed to load team");
        const data = await res.json();

        const mappedPlayers = data.players.map((p) => ({
          id: p.id,
          name: p.fullName,
          position: p.position,
          positionId: p.positionId,
          age: p.age,
          attributes: p.attributes || [],
        }));

        const attrSet = new Set();
        mappedPlayers.forEach((p) =>
          p.attributes.forEach((a) => attrSet.add(a.name))
        );

        mappedPlayers.sort((a, b) => a.positionId - b.positionId);

        setAllAttributes(Array.from(attrSet));
        setPlayers(mappedPlayers);
      } catch (err) {
        console.error("Грешка при fetch на играчите:", err);
      } finally {
        setLoading(false);
      }
    };

    if (gameSaveId) fetchPlayers();
  }, [gameSaveId]);

  // Зареждаме формациите
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

  // Проверяваме дали има вече запазена тактика
  useEffect(() => {
    const fetchTeamTactic = async () => {
      try {
        if (!teamId || !gameSaveId || formations.length === 0) return;

        const res = await fetch(`/api/tactics/${teamId}?saveId=${gameSaveId}`, {
          credentials: "include",
        });

        if (!res.ok) {
          console.log("Няма запазена тактика за отбора.");
          return;
        }

        const data = await res.json();
        console.log("Loaded team tactic:", data);

        const tactic = formations.find((f) => f.id === data.tacticId);
        if (tactic) setSelectedFormation(tactic.name);

        if (data.lineupJson) {
          try {
            const parsedLineup = JSON.parse(data.lineupJson);
            setLineup(parsedLineup);
          } catch {
            console.warn("Invalid lineupJson:", data.lineupJson);
          }
        }

        if (data.substitutesJson) {
          try {
            const parsedSubs = JSON.parse(data.substitutesJson);
            setSubstitutes(parsedSubs);
          } catch {
            console.warn("Invalid substitutesJson:", data.substitutesJson);
          }
        }
      } catch (err) {
        console.error("Грешка при зареждане на тактиката:", err);
      }
    };

    fetchTeamTactic();
  }, [teamId, gameSaveId, formations]);

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
          substitutes,
        }),
      });

      const rawText = await res.text();
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

  const handleReset = () => {
    setLineup({});
    setSubstitutes({});
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

  const getSelectedPlayerIds = () => [
    ...Object.values(lineup),
    ...Object.values(substitutes),
  ].filter(Boolean);

  const renderSlots = (position, count, availablePlayers) => {
    const slotsArray = [];
    const selectedIds = getSelectedPlayerIds();

    for (let i = 1; i <= count; i++) {
      const slotKey = `${position}-${i}`;
      slotsArray.push(
        <tr key={slotKey}>
          <td className="px-6 py-4 text-sm font-medium text-gray-900">{`${position} ${i}`}</td>
          <td className="px-6 py-4">
            <select
              className="block w-full px-3 py-2 border border-gray-300 bg-white rounded-lg shadow-sm focus:ring-indigo-500 focus:border-indigo-500"
              value={lineup[slotKey] || ""}
              onChange={(e) =>
                setLineup((prev) => ({ ...prev, [slotKey]: e.target.value }))
              }
            >
              <option value="">Select Player</option>
              {availablePlayers
                .filter(
                  (p) =>
                    !selectedIds.includes(p.id.toString()) ||
                    lineup[slotKey] === p.id.toString()
                )
                .map((p) => (
                  <option key={p.id} value={p.id}>
                    {p.name}
                  </option>
                ))}
            </select>
          </td>
        </tr>
      );
    }
    return slotsArray;
  };

  const renderSubstitutes = () => {
    const slots = ["SUB1", "SUB2", "SUB3", "SUB4", "SUB5"];
    const selectedIds = getSelectedPlayerIds();

    return slots.map((slotKey) => (
      <tr key={slotKey}>
        <td className="px-6 py-4 text-sm font-medium text-gray-900">{slotKey}</td>
        <td className="px-6 py-4">
          <select
            className="block w-full px-3 py-2 border border-gray-300 bg-white rounded-lg shadow-sm focus:ring-indigo-500 focus:border-indigo-500"
            value={substitutes[slotKey] || ""}
            onChange={(e) =>
              setSubstitutes((prev) => ({
                ...prev,
                [slotKey]: e.target.value,
              }))
            }
          >
            <option value="">Select Player</option>
            {players
              .filter(
                (p) =>
                  !selectedIds.includes(p.id.toString()) ||
                  substitutes[slotKey] === p.id.toString()
              )
              .map((p) => (
                <option key={p.id} value={p.id}>
                  {p.name}
                </option>
              ))}
          </select>
        </td>
      </tr>
    ));
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center h-screen">
        <span className="animate-spin rounded-full h-10 w-10 border-b-2 border-indigo-600"></span>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-100 to-gray-200 p-6">
      <div className="max-w-7xl mx-auto">
        <h1 className="text-4xl font-extrabold text-center mb-8 text-indigo-700">
          ⚽ Team Tactics
        </h1>

        {/* Formation selector */}
        <div className="mb-8">
          <label
            htmlFor="formation"
            className="block text-lg font-medium mb-2 text-gray-700"
          >
            Select Formation
          </label>
          <select
            id="formation"
            className="block w-full px-4 py-3 border border-gray-300 bg-white rounded-lg shadow-sm focus:ring-indigo-500 focus:border-indigo-500"
            value={selectedFormation}
            onChange={(e) => {
              setSelectedFormation(e.target.value);
              setLineup({});
              setSubstitutes({});
            }}
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
            <div className="bg-white shadow-lg rounded-xl p-4">
              <h2 className="text-2xl font-semibold mb-4 text-indigo-600">
                All Players
              </h2>
              <div className="overflow-auto max-h-[70vh] rounded-lg">
                <table className="min-w-full divide-y divide-gray-200 text-sm">
                  <thead className="bg-gray-50 sticky top-0">
                    <tr>
                      <th className="px-4 py-2 text-left font-semibold text-gray-700">
                        Name
                      </th>
                      <th className="px-4 py-2 text-left font-semibold text-gray-700">
                        Position
                      </th>
                      <th className="px-4 py-2 text-left font-semibold text-gray-700">
                        Age
                      </th>
                      {allAttributes.map((attr) => (
                        <th
                          key={attr}
                          className="px-4 py-2 text-left font-semibold text-gray-700"
                        >
                          {attr}
                        </th>
                      ))}
                    </tr>
                  </thead>
                  <tbody className="bg-white divide-y divide-gray-200">
                    {players.map((p) => (
                      <tr
                        key={p.id}
                        className={`${getRowClass(
                          p.position
                        )} hover:bg-gray-100 transition`}
                      >
                        <td className="px-4 py-2 font-medium">{p.name}</td>
                        <td className="px-4 py-2">{p.position}</td>
                        <td className="px-4 py-2">{p.age}</td>
                        {allAttributes.map((attr) => {
                          const found = p.attributes.find(
                            (a) => a.name === attr
                          );
                          return (
                            <td key={attr} className="px-4 py-2 text-center">
                              {found ? found.value : "-"}
                            </td>
                          );
                        })}
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>

            {/* Lineup + Subs */}
            <div className="bg-white shadow-lg rounded-xl p-4">
              <h2 className="text-2xl font-semibold mb-4 text-indigo-600">
                Starting Lineup
              </h2>
              <div className="overflow-auto rounded-lg mb-6">
                <table className="min-w-full divide-y divide-gray-200">
                  <thead className="bg-gray-50 sticky top-0">
                    <tr>
                      <th className="px-6 py-3 text-left text-sm font-semibold text-gray-700">
                        Position
                      </th>
                      <th className="px-6 py-3 text-left text-sm font-semibold text-gray-700">
                        Player
                      </th>
                    </tr>
                  </thead>
                  <tbody>
                    {renderSlots(
                      "GK",
                      slots.GK,
                      players.filter((p) => p.position === "Goalkeeper")
                    )}
                    {renderSlots(
                      "DF",
                      slots.DF,
                      players.filter((p) => p.position === "Defender")
                    )}
                    {renderSlots(
                      "MID",
                      slots.MID,
                      players.filter((p) => p.position === "Midfielder")
                    )}
                    {renderSlots(
                      "ATT",
                      slots.ATT,
                      players.filter((p) => p.position === "Attacker")
                    )}
                  </tbody>
                </table>
              </div>

              <h2 className="text-2xl font-semibold mb-4 text-indigo-600">
                Substitutes
              </h2>
              <div className="overflow-auto rounded-lg">
                <table className="min-w-full divide-y divide-gray-200">
                  <thead className="bg-gray-50">
                    <tr>
                      <th className="px-6 py-3 text-left text-sm font-semibold text-gray-700">
                        Slot
                      </th>
                      <th className="px-6 py-3 text-left text-sm font-semibold text-gray-700">
                        Player
                      </th>
                    </tr>
                  </thead>
                  <tbody>{renderSubstitutes()}</tbody>
                </table>
              </div>

              <div className="mt-6 flex space-x-3">
                <button
                  onClick={handleSave}
                  className="flex-1 px-4 py-3 bg-indigo-600 text-white font-semibold rounded-lg shadow hover:bg-indigo-700 transition"
                >
                  Save
                </button>
                <button
                  onClick={handleReset}
                  className="flex-1 px-4 py-3 bg-gray-500 text-white font-semibold rounded-lg shadow hover:bg-gray-600 transition"
                >
                  Reset
                </button>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default Tactics;
