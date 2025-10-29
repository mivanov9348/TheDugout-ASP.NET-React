import React, { useState, useEffect, useRef } from "react";
import Swal from "sweetalert2";

const Tactics = ({ gameSaveId, teamId }) => {
  const [formations, setFormations] = useState([]);
  const [players, setPlayers] = useState([]);
  const [selectedFormation, setSelectedFormation] = useState("");
  const [lineup, setLineup] = useState({});
  const [substitutes, setSubstitutes] = useState({});
  const [loading, setLoading] = useState(true);
  const [allAttributes, setAllAttributes] = useState([]);

  const suppressClearRef = useRef(false);
  const appliedSavedTacticRef = useRef(false);

  const getRowClass = (position) => {
    switch (position) {
      case "Goalkeeper":
        return "bg-yellow-900/30";
      case "Defender":
        return "bg-blue-900/30";
      case "Midfielder":
        return "bg-green-900/30";
      case "Attacker":
        return "bg-red-900/30";
      default:
        return "";
    }
  };

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

  useEffect(() => {
    const fetchTeamTactic = async () => {
      try {
        if (!teamId || !gameSaveId || formations.length === 0 || players.length === 0)
          return;
        if (appliedSavedTacticRef.current) return;

        const res = await fetch(`/api/tactics/${teamId}?saveId=${gameSaveId}`, {
          credentials: "include",
        });
        if (!res.ok) return;
        const data = await res.json();

        let parsedLineup = {};
        let parsedSubs = {};

        if (data.lineupJson) {
          try {
            const temp = JSON.parse(data.lineupJson);
            parsedLineup = Object.fromEntries(
              Object.entries(temp).map(([key, value]) => {
                const newKey = key.replace(/([A-Z]+)(\d+)/, "$1-$2");
                return [newKey, value];
              })
            );
          } catch (e) {
            console.error("Invalid lineup JSON", e);
          }
        }

        if (data.substitutesJson) {
          try {
            parsedSubs = JSON.parse(data.substitutesJson);
          } catch (e) {
            console.error("Invalid substitutes JSON", e);
          }
        }

        const tactic = formations.find((f) => f.id === data.tacticId);
        if (tactic) {
          suppressClearRef.current = true;
          setSelectedFormation(tactic.name);
          setTimeout(() => {
            setLineup(parsedLineup);
            setSubstitutes(parsedSubs);
            appliedSavedTacticRef.current = true;
            suppressClearRef.current = false;
          }, 0);
        }
      } catch (err) {
        console.error("Грешка при зареждане на тактиката:", err);
      }
    };
    fetchTeamTactic();
  }, [teamId, gameSaveId, formations, players]);

  const handleSave = async () => {
    try {
      const tacticId = formations.find((f) => f.name === selectedFormation)?.id;
      if (!tacticId)
        return Swal.fire("Грешка", "Моля избери формация преди да запазиш.", "error");

      const res = await fetch(`/api/tactics/${teamId}`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        credentials: "include",
        body: JSON.stringify({ tacticId, customName: selectedFormation, lineup, substitutes }),
      });

      if (!res.ok) throw new Error(await res.text());
      Swal.fire("✅ Успех!", "Тактиката е запазена!", "success");
    } catch (err) {
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
    return { GK: 1, DF: form.defenders, MID: form.midfielders, ATT: form.forwards };
  };
  const slots = getPositionSlots();

  const isStarter = (id) => Object.values(lineup).includes(id.toString());
  const isSub = (id) => Object.values(substitutes).includes(id.toString());

  const renderSlots = (position, count, availablePlayers) => {
    const slotsArray = [];
    for (let i = 1; i <= count; i++) {
      const slotKey = `${position}-${i}`;
      slotsArray.push(
        <tr key={slotKey}>
          <td className="px-6 py-3 font-semibold text-gray-200 bg-gray-700 hover:bg-gray-600 transition-colors rounded-l-lg">
            {`${position} ${i}`}
          </td>
          <td className="px-6 py-3 bg-gray-700 hover:bg-gray-600 transition-colors rounded-r-lg">
            <select
              className="block w-full px-3 py-2 border border-gray-600 bg-gray-800 text-gray-100 rounded-lg focus:ring-2 focus:ring-gray-400 transition"
              value={lineup[slotKey] || ""}
              onChange={(e) => setLineup((prev) => ({ ...prev, [slotKey]: e.target.value }))}
            >
              <option value="">Select Player</option>
              {availablePlayers.map((p) => (
                <option
                  key={p.id}
                  value={p.id}
                  disabled={isStarter(p.id) && lineup[slotKey] !== p.id.toString()}
                  className={
                    isSub(p.id)
                      ? "text-amber-400 font-semibold"
                      : isStarter(p.id)
                        ? "text-blue-400 font-semibold"
                        : "text-gray-200"
                  }
                >
                  {p.name}
                  {isSub(p.id) ? " (Sub)" : isStarter(p.id) ? " (Starter)" : ""}
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
    const slotsArr = ["SUB1", "SUB2", "SUB3", "SUB4", "SUB5"];
    return slotsArr.map((slotKey) => (
      <tr key={slotKey}>
        <td className="px-6 py-3 font-semibold text-gray-200 bg-gray-700 hover:bg-gray-600 transition-colors rounded-l-lg">
          {slotKey}
        </td>
        <td className="px-6 py-3 bg-gray-700 hover:bg-gray-600 transition-colors rounded-r-lg">
          <select
            className="block w-full px-3 py-2 border border-gray-600 bg-gray-800 text-gray-100 rounded-lg focus:ring-2 focus:ring-gray-400 transition"
            value={substitutes[slotKey] || ""}
            onChange={(e) =>
              setSubstitutes((prev) => ({ ...prev, [slotKey]: e.target.value }))
            }
          >
            <option value="">Select Player</option>
            {players.map((p) => (
              <option
                key={p.id}
                value={p.id}
                disabled={isSub(p.id) && substitutes[slotKey] !== p.id.toString()}
                className={
                  isStarter(p.id)
                    ? "text-blue-400 font-semibold"
                    : isSub(p.id)
                      ? "text-amber-400 font-semibold"
                      : "text-gray-200"
                }
              >
                {p.name}
                {isStarter(p.id) ? " (Starter)" : isSub(p.id) ? " (Sub)" : ""}
              </option>
            ))}
          </select>
        </td>
      </tr>
    ));
  };

  if (loading || formations.length === 0) {
    return (
      <div className="flex items-center justify-center h-screen bg-gradient-to-br from-gray-900 via-gray-800 to-gray-900">
        <span className="animate-spin rounded-full h-12 w-12 border-4 border-gray-400 border-t-transparent"></span>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-900 via-gray-800 to-gray-900 p-8">
      <div className="max-w-7xl mx-auto space-y-10">

        <div className="bg-gray-700 backdrop-blur p-6 rounded-2xl shadow-lg border border-gray-600">
          <label htmlFor="formation" className="block text-xl font-semibold mb-3 text-gray-200">
            Select Formation
          </label>
          <select
            id="formation"
            className="block w-full px-4 py-3 border border-gray-600 bg-gray-800 text-gray-100 rounded-lg shadow-sm focus:ring-gray-400 focus:border-gray-400"
            value={selectedFormation}
            onChange={(e) => {
              const newForm = e.target.value;
              if (suppressClearRef.current) {
                suppressClearRef.current = false;
                setSelectedFormation(newForm);
                return;
              }
              setSelectedFormation(newForm);
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

        {selectedFormation && players.length > 0 && (
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-10">
            <div className="bg-gray-700 backdrop-blur shadow-xl rounded-2xl p-6 border border-gray-600">
              <h2 className="text-2xl font-bold mb-4 text-gray-200">All Players</h2>
              <div className="overflow-auto max-h-[70vh] rounded-lg">
                <table className="min-w-full text-sm">
                  <thead className="bg-gray-800 sticky top-0 shadow-sm">
                    <tr>
                      <th className="px-4 py-2 text-left font-semibold text-gray-300">Name</th>
                      <th className="px-4 py-2 text-left font-semibold text-gray-300">Position</th>
                      <th className="px-4 py-2 text-left font-semibold text-gray-300">Age</th>
                      {allAttributes.map((attr) => (
                        <th key={attr} className="px-4 py-2 text-center font-semibold text-gray-300">
                          {attr}
                        </th>
                      ))}
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-600">
                    {players.map((p) => (
                      <tr key={p.id} className={`${getRowClass(p.position)} hover:bg-gray-600`}>
                        <td className="px-4 py-2 font-medium text-gray-200">{p.name}</td>
                        <td className="px-4 py-2 text-gray-300">{p.position}</td>
                        <td className="px-4 py-2 text-gray-300">{p.age}</td>
                        {allAttributes.map((attr) => {
                          const found = p.attributes.find((a) => a.name === attr);
                          return (
                            <td key={attr} className="px-4 py-2 text-center text-gray-300">
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

            <div className="bg-gray-700 backdrop-blur shadow-xl rounded-2xl p-6 border border-gray-600">
              <h2 className="text-2xl font-bold mb-4 text-gray-200">Starting Lineup</h2>
              <div className="overflow-auto rounded-lg mb-6">
                <table className="min-w-full divide-y divide-gray-600">
                  <thead className="bg-gray-800 sticky top-0">
                    <tr>
                      <th className="px-6 py-3 text-left font-semibold text-gray-300">Position</th>
                      <th className="px-6 py-3 text-left font-semibold text-gray-300">Player</th>
                    </tr>
                  </thead>
                  <tbody>
                    {renderSlots("GK", slots.GK, players.filter((p) => p.position === "Goalkeeper"))}
                    {renderSlots("DF", slots.DF, players.filter((p) => p.position === "Defender"))}
                    {renderSlots("MID", slots.MID, players.filter((p) => p.position === "Midfielder"))}
                    {renderSlots("ATT", slots.ATT, players.filter((p) => p.position === "Attacker"))}
                  </tbody>
                </table>
              </div>

              <h2 className="text-2xl font-bold mb-4 text-gray-200">Substitutes</h2>
              <div className="overflow-auto rounded-lg">
                <table className="min-w-full divide-y divide-gray-600">
                  <thead className="bg-gray-800">
                    <tr>
                      <th className="px-6 py-3 text-left font-semibold text-gray-300">Slot</th>
                      <th className="px-6 py-3 text-left font-semibold text-gray-300">Player</th>
                    </tr>
                  </thead>
                  <tbody>{renderSubstitutes()}</tbody>
                </table>
              </div>

              <div className="mt-6 flex space-x-3">
                <button
                  onClick={handleSave}
                  className="flex-1 px-4 py-3 bg-gray-600 hover:bg-gray-500 text-gray-100 font-semibold rounded-lg shadow transition"
                >
                  💾 Save
                </button>
                <button
                  onClick={handleReset}
                  className="flex-1 px-4 py-3 bg-gray-700 hover:bg-gray-600 text-gray-100 font-semibold rounded-lg shadow transition border border-gray-600"
                >
                  ♻ Reset
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