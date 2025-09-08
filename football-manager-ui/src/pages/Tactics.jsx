  import React, { useState, useEffect } from "react";
  import Swal from "sweetalert2";

  const Tactics = ({ gameSaveId, teamId }) => {
    const [formations, setFormations] = useState([]);
    const [players, setPlayers] = useState([]);
    const [selectedFormation, setSelectedFormation] = useState("");
    const [lineup, setLineup] = useState({});
    const [loading, setLoading] = useState(true);
    const [allAttributes, setAllAttributes] = useState([]);

    const getRowClass = (position) => {
    switch (position) {
      case "Goalkeeper":
        return "bg-yellow-50"; 
      case "Defender":
        return "bg-blue-50";   
      case "Midfielder":
        return "bg-green-100"; 
      case "Attacker":
        return "bg-red-100";   
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

    const handleReset = () => setLineup({});

    const getPositionSlots = () => {
      if (!selectedFormation) return { GK: 1, DF: 0, MID: 0, ATT: 0 };
      const form = formations.find((f) => f.name === selectedFormation);
      if (!form) return { GK: 1, DF: 0, MID: 0, ATT: 0 };
      return { GK: 1, DF: form.defenders, MID: form.midfielders, ATT: form.forwards };
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
            <td className="px-6 py-4 text-sm font-medium text-gray-900">{`${position} ${i}`}</td>
            <td className="px-6 py-4">
              <select
                className="block w-full px-3 py-2 border border-gray-300 bg-white rounded-md shadow-sm focus:ring-indigo-500 focus:border-indigo-500"
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

    if (loading) {
      return <div className="flex items-center justify-center h-screen">Loading...</div>;
    }

    return (
      <div className="min-h-screen bg-gray-100 p-6">
        <div className="max-w-6xl mx-auto">
          <h1 className="text-3xl font-bold text-center mb-8">Team Tactics</h1>

          {/* Formation selector */}
          <div className="mb-8">
            <label htmlFor="formation" className="block text-sm font-medium mb-2">
              Select Formation
            </label>
            <select
              id="formation"
              className="block w-full px-3 py-2 border border-gray-300 bg-white rounded-md shadow-sm focus:ring-indigo-500 focus:border-indigo-500"
              value={selectedFormation}
              onChange={(e) => {
                setSelectedFormation(e.target.value);
                setLineup({});
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
              <div>
                <h2 className="text-2xl font-semibold mb-4">All Players</h2>
                <div className="bg-white shadow-md rounded-lg overflow-auto">
                  <table className="min-w-full divide-y divide-gray-200 text-sm">
                    <thead className="bg-gray-50">
                      <tr>
                        <th className="px-4 py-2 text-left font-medium">Name</th>
                        <th className="px-4 py-2 text-left font-medium">Position</th>
                        <th className="px-4 py-2 text-left font-medium">Age</th>
                        {allAttributes.map((attr) => (
                          <th key={attr} className="px-4 py-2 text-left font-medium">
                            {attr}
                          </th>
                        ))}
                      </tr>
                    </thead>
                    <tbody className="bg-white divide-y divide-gray-200">
    {players.map((p) => (
      <tr
        key={p.id}
        className={`${getRowClass(p.position)} hover:bg-gray-100`}
      >
        <td className="px-4 py-2 font-medium">{p.name}</td>
        <td className="px-4 py-2">{p.position}</td>
        <td className="px-4 py-2">{p.age}</td>
        {allAttributes.map((attr) => {
          const found = p.attributes.find((a) => a.name === attr);
          return (
            <td key={attr} className="px-4 py-2">
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

              {/* Lineup */}
              <div>
                <h2 className="text-2xl font-semibold mb-4">Starting Lineup</h2>
                <div className="bg-white shadow-md rounded-lg overflow-hidden">
                  <table className="min-w-full divide-y divide-gray-200">
                    <thead className="bg-gray-50">
                      <tr>
                        <th className="px-6 py-3 text-left text-xs font-medium">Position</th>
                        <th className="px-6 py-3 text-left text-xs font-medium">Player</th>
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
                <div className="mt-4 flex space-x-2">
                  <button
                    onClick={handleSave}
                    className="px-4 py-2 bg-indigo-600 text-white rounded-md hover:bg-indigo-700"
                  >
                    Save
                  </button>
                  <button
                    onClick={handleReset}
                    className="px-4 py-2 bg-gray-500 text-white rounded-md hover:bg-gray-600"
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
