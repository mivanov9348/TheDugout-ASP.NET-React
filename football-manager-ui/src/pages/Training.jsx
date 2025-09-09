import React, { useEffect, useState } from "react";

const Training = ({ gameSaveId }) => {
  const [players, setPlayers] = useState([]);
  const [selectedSkills, setSelectedSkills] = useState({});
  const [trainingProgress, setTrainingProgress] = useState([]);
  const [loadingAuto, setLoadingAuto] = useState(false);

  // Зареждане на отбора
  useEffect(() => {
    if (!gameSaveId) return;

    const loadTeam = async () => {
      try {
        const res = await fetch(`/api/team/by-save/${gameSaveId}`, {
          credentials: "include",
        });
        if (!res.ok) throw new Error("Грешка при зареждане на отбора");
        const data = await res.json();
        setPlayers(data.players ?? []);
      } catch (err) {
        console.error(err);
        alert(err.message);
      }
    };

    loadTeam();
  }, [gameSaveId]);

  const handleSkillChange = (playerId, attrId) => {
    setSelectedSkills((prev) => ({ ...prev, [playerId]: Number(attrId) }));
  };

  // Auto Assign Attributes
  const handleAutoComplete = async () => {
    if (!players.length || !players[0].teamId) {
      alert("Играчите или отборът не са заредени още.");
      return;
    }

    try {
      setLoadingAuto(true);

      const res = await fetch(
        `/api/training/auto-assign/${players[0].teamId}/${gameSaveId}`,
        { credentials: "include" }
      );

      if (!res.ok) {
        const errData = await res.json().catch(() => null);
        throw new Error(errData?.message || "Грешка при авто-назначаване на уменията");
      }

      const data = await res.json();

      const autoSelected = {};
      data.forEach((a) => {
        autoSelected[a.playerId] = a.attributeId;
      });

      // Попълваме избраните умения
      setSelectedSkills(autoSelected);
    } catch (err) {
      console.error(err);
      alert(err.message);
    } finally {
      setLoadingAuto(false);
    }
  };

  // Стартиране на тренировката
  const handleStartTraining = async () => {
    try {
      const assignments = players
        .filter((p) => selectedSkills[p.id])
        .map((player) => ({
          playerId: player.id,
          attributeId: selectedSkills[player.id],
        }));

      if (assignments.length === 0) {
        alert("Моля, избери поне един играч и умение за трениране.");
        return;
      }

      const payload = {
        gameSaveId,
        teamId: players[0]?.teamId ?? 0,
        seasonId: 1, // TODO: реален сезон
        date: new Date().toISOString(),
        assignments,
      };

      const res = await fetch(`/api/training/start`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        credentials: "include",
        body: JSON.stringify(payload),
      });

      if (!res.ok) {
        const errData = await res.json();
        throw new Error(errData.message || "Грешка при стартиране на тренировката");
      }

      const data = await res.json();

      setTrainingProgress(
        data.map((result) => ({
          playerId: result.playerId,
          name: result.playerName,
          skill: result.attributeName,
          efficiency: result.newValue - result.oldValue,
          sessions: 1,
        }))
      );
    } catch (err) {
      console.error(err);
      alert(err.message || "Неуспешно стартиране на тренировката");
    }
  };

  return (
    <div className="min-h-screen bg-gray-100 p-6">
      <div className="max-w-6xl mx-auto">
        <h1 className="text-3xl font-bold text-center mb-8 text-gray-800">
          Training Management
        </h1>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
          {/* Лява част - Assign Training */}
          <div>
            <h2 className="text-2xl font-semibold mb-4 text-gray-800">
              Assign Training
            </h2>
            <div className="bg-white shadow-md rounded-lg overflow-hidden">
              <table className="min-w-full divide-y divide-gray-200">
                <thead className="bg-gray-50">
                  <tr>
                    <th className="px-6 py-3">Name</th>
                    <th className="px-6 py-3">Position</th>
                    <th className="px-6 py-3">Skill to Train</th>
                  </tr>
                </thead>
                <tbody className="bg-white divide-y divide-gray-200">
                  {players.map((player) => (
                    <tr key={player.id} className="hover:bg-gray-50">
                      <td className="px-6 py-4">{player.fullName}</td>
                      <td className="px-6 py-4">{player.position}</td>
                      <td className="px-6 py-4">
                        <select
                          className="block w-full border rounded-md"
                          value={selectedSkills[player.id] || ""}
                          onChange={(e) =>
                            handleSkillChange(player.id, e.target.value)
                          }
                        >
                          <option value="">Select Skill</option>
                          {player.attributes?.map((attr) => (
                            <option key={attr.attributeId} value={attr.attributeId}>
                              {attr.name} ({attr.value})
                            </option>
                          ))}
                        </select>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            <div className="mt-4 flex gap-2">
              <button
                onClick={handleAutoComplete}
                disabled={!players.length || loadingAuto}
                className={`px-4 py-2 rounded-md text-white ${
                  !players.length || loadingAuto ? "bg-gray-400" : "bg-green-600"
                }`}
              >
                {loadingAuto ? "Assigning..." : "Auto Complete Attributes"}
              </button>

              <button
                onClick={handleStartTraining}
                className="px-4 py-2 bg-indigo-600 text-white rounded-md"
              >
                Start Training
              </button>
            </div>
          </div>

          {/* Дясна част - Training Progress */}
          <div>
            <h2 className="text-2xl font-semibold mb-4 text-gray-800">
              Training Progress
            </h2>
            <div className="bg-white shadow-md rounded-lg overflow-hidden">
              <table className="min-w-full divide-y divide-gray-200">
                <thead className="bg-gray-50">
                  <tr>
                    <th className="px-6 py-3">Name</th>
                    <th className="px-6 py-3">Skill</th>
                    <th className="px-6 py-3">Efficiency Gain</th>
                    <th className="px-6 py-3">Sessions</th>
                  </tr>
                </thead>
                <tbody className="bg-white divide-y divide-gray-200">
                  {trainingProgress.map((progress) => (
                    <tr key={progress.playerId}>
                      <td className="px-6 py-4">{progress.name}</td>
                      <td className="px-6 py-4">{progress.skill}</td>
                      <td className="px-6 py-4 text-green-600 font-semibold">
                        +{progress.efficiency}
                      </td>
                      <td className="px-6 py-4">{progress.sessions}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Training;
