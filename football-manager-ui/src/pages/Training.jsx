import React, { useEffect, useState } from "react";

const Training = ({ gameSaveId }) => {
  const [players, setPlayers] = useState([]);
  const [selectedSkills, setSelectedSkills] = useState({});
  const [trainingProgress, setTrainingProgress] = useState([]);

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
      }
    };

    loadTeam();
  }, [gameSaveId]);

  const handleSkillChange = (playerId, skill) => {
    setSelectedSkills((prev) => ({ ...prev, [playerId]: skill }));
  };

  const estimateEfficiencyGain = (skill, position, player) => {
    const attr = player.attributes?.find((a) => a.name === skill);
    const baseValue = attr ? 100 - attr.value : 50;
    const randomness = Math.floor(Math.random() * 5);

    const positionBoost =
      position === "Forward" && skill === "Shooting"
        ? 5
        : position === "Midfielder" && skill === "Passing"
        ? 4
        : position === "Defender" && skill === "Defending"
        ? 4
        : 0;

    return Math.max(1, Math.round(baseValue / 20 + positionBoost + randomness));
  };

  const handleSaveAssignments = () => {
    const results = players
      .filter((p) => selectedSkills[p.id])
      .map((player) => {
        const skill = selectedSkills[player.id];
        const gain = estimateEfficiencyGain(skill, player.position, player);
        return {
          id: player.id,
          name: player.fullName, 
          skill,
          efficiency: gain,
          sessions: Math.floor(Math.random() * 5) + 1,
        };
      });

    setTrainingProgress(results);
  };

  return (
    <div className="min-h-screen bg-gray-100 p-6">
      <div className="max-w-6xl mx-auto">
        <h1 className="text-3xl font-bold text-center mb-8 text-gray-800">
          Training Management
        </h1>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
          {/* Left: Assign Training */}
          <div>
            <h2 className="text-2xl font-semibold mb-4 text-gray-800">
              Assign Training
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
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Skill to Train
                    </th>
                  </tr>
                </thead>
                <tbody className="bg-white divide-y divide-gray-200">
                  {players.map((player) => (
                    <tr key={player.id} className="hover:bg-gray-50">
                      <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                        {player.fullName} {/* <-- поправено */}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                        {player.position}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        <select
                          className="block w-full px-3 py-2 border border-gray-300 bg-white rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm"
                          value={selectedSkills[player.id] || ""}
                          onChange={(e) =>
                            handleSkillChange(player.id, e.target.value)
                          }
                        >
                          <option value="">Select Skill</option>
                          {player.attributes?.map((attr) => (
                            <option key={attr.attributeId} value={attr.name}>
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
            <button
              onClick={handleSaveAssignments}
              className="mt-4 px-4 py-2 bg-indigo-600 text-white font-medium rounded-md hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
            >
              Save Assignments
            </button>
          </div>

          {/* Right: Training Progress */}
          <div>
            <h2 className="text-2xl font-semibold mb-4 text-gray-800">
              Training Progress
            </h2>
            <div className="bg-white shadow-md rounded-lg overflow-hidden">
              <table className="min-w-full divide-y divide-gray-200">
                <thead className="bg-gray-50">
                  <tr>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Name
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Skill
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Efficiency Gain
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Sessions
                    </th>
                  </tr>
                </thead>
                <tbody className="bg-white divide-y divide-gray-200">
                  {trainingProgress.map((progress) => (
                    <tr key={progress.id} className="hover:bg-gray-50">
                      <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                        {progress.name}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                        {progress.skill}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-green-600 font-semibold">
                        +{progress.efficiency}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                        {progress.sessions}
                      </td>
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
