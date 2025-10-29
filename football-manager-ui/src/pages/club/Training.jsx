import React, { useEffect, useState } from "react";
import Swal from "sweetalert2";
import { Dumbbell, PlayCircle, Sparkles } from "lucide-react";

const Training = ({ gameSaveId }) => {
  const [team, setTeam] = useState(null);
  const [players, setPlayers] = useState([]);
  const [selectedSkills, setSelectedSkills] = useState({});
  const [trainingProgress, setTrainingProgress] = useState([]);
  const [loadingAuto, setLoadingAuto] = useState(false);
  const [autoProposals, setAutoProposals] = useState({});

  const showError = (message) => {
    Swal.fire({
      icon: "error",
      title: "Грешка",
      text: message || "Възникна проблем",
      confirmButtonColor: "#ef4444",
    });
  };

  useEffect(() => {
    if (!gameSaveId) return;
    const loadTeam = async () => {
      try {
        const res = await fetch(`/api/team/by-save/${gameSaveId}`, {
          credentials: "include",
        });
        if (!res.ok) throw new Error("Грешка при зареждане на отбора");
        const data = await res.json();
        setTeam(data);
        const sortedPlayers = (data.players ?? []).sort(
          (a, b) => (a.positionId ?? 0) - (b.positionId ?? 0)
        );
        setPlayers(sortedPlayers);
      } catch (err) {
        console.error(err);
        showError(err.message);
      }
    };
    loadTeam();
  }, [gameSaveId]);

  const handleSkillChange = (playerId, attrId) => {
    setSelectedSkills((prev) => ({ ...prev, [playerId]: Number(attrId) }));
  };

  const handleAutoComplete = async () => {
    if (!team || !players.length) {
      showError("Играчите или отборът не са заредени още.");
      return;
    }
    try {
      setLoadingAuto(true);
      const url = `/api/training/auto-assign/${team.teamId}/${gameSaveId}`;
      const res = await fetch(url, { credentials: "include" });
      if (!res.ok) {
        let errText = await res.text();
        let errData = null;
        try {
          errData = JSON.parse(errText);
        } catch {}
        throw new Error(
          errData?.message || "Грешка при авто-назначаване на уменията"
        );
      }
      const data = await res.json();
      const autoSelected = {};
      const proposals = {};
      data.forEach((a) => {
        autoSelected[a.playerId] = a.attributeId;
        proposals[a.playerId] = { name: a.attributeName, value: a.currentValue };
      });
      setSelectedSkills(autoSelected);
      setAutoProposals(proposals);
    } catch (err) {
      console.error("❌ handleAutoComplete failed:", err);
      showError(err.message);
    } finally {
      setLoadingAuto(false);
    }
  };

  const handleStartTraining = async () => {
    if (!team || !players.length) {
      showError("Играчите или отборът не са заредени още.");
      return;
    }

    try {
      const assignments = players
        .filter((p) => selectedSkills[p.id])
        .map((player) => ({
          playerId: player.id,
          attributeId: selectedSkills[player.id],
        }));

      if (assignments.length === 0) {
        showError("Моля, избери поне един играч и умение за трениране.");
        return;
      }

      const payload = {
        gameSaveId,
        teamId: team.teamId,
        seasonId: 1,
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
        throw new Error(
          errData.message || "Грешка при стартиране на тренировката"
        );
      }

      const data = await res.json();

      setTrainingProgress(
        data.map((result) => ({
          playerId: result.playerId,
          name: result.playerName,
          skill: result.attributeName,
          efficiency: result.newValue - result.oldValue,
          progressGain: result.progressGain,
          totalProgress: result.totalProgress,
          currentValue: result.newValue,
          sessions: 1,
        }))
      );

      Swal.fire({
        icon: "success",
        title: "Тренировката е успешна!",
        showConfirmButton: false,
        timer: 1500,
      });
    } catch (err) {
      console.error(err);
      showError(err.message || "Неуспешно стартиране на тренировката");
    }
  };

  return (
    <div className="relative min-h-screen bg-gradient-to-br from-gray-900 via-gray-800 to-gray-900 text-gray-100 p-8 overflow-hidden">
      {/* Текстура за фон */}
      <div className="absolute inset-0 bg-[url('https://www.transparenttextures.com/patterns/football-no-lines.png')] opacity-10 pointer-events-none"></div>

      <div className="relative z-10 max-w-7xl mx-auto">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-10">
          {/* Лява част */}
          <div className="bg-white/10 hover:bg-white/15 backdrop-blur-md rounded-2xl border border-white/10 shadow-xl p-6 transition-all duration-300">
            <h2 className="text-2xl font-semibold mb-6 text-white flex items-center gap-2">
              <Sparkles className="text-green-400" /> Assign Training
            </h2>
            <div className="overflow-hidden rounded-xl border border-white/10">
              <table className="min-w-full text-sm text-gray-200">
                <thead className="bg-gray-800/60 text-gray-100">
                  <tr>
                    <th className="px-4 py-2 text-left font-semibold">Name</th>
                    <th className="px-4 py-2 text-left font-semibold">Position</th>
                    <th className="px-4 py-2 text-left font-semibold">Skill to Train</th>
                  </tr>
                </thead>
                <tbody>
                  {players.map((player) => (
                    <tr
                      key={player.id}
                      className="hover:bg-gray-800/40 transition"
                    >
                      <td className="px-4 py-3">{player.fullName}</td>
                      <td className="px-4 py-3">{player.position}</td>
                      <td className="px-4 py-3">
                        <select
                          className="block w-full bg-gray-900/70 border border-gray-600 rounded-md p-2 text-gray-200 focus:ring-2 focus:ring-green-400"
                          value={selectedSkills[player.id] || ""}
                          onChange={(e) =>
                            handleSkillChange(player.id, e.target.value)
                          }
                        >
                          <option value="">Select Skill</option>
                          {player.attributes?.map((attr) => (
                            <option
                              key={attr.attributeId}
                              value={attr.attributeId}
                            >
                              {attr.name} ({attr.value})
                            </option>
                          ))}
                        </select>
                        {autoProposals[player.id] && (
                          <p className="text-xs text-gray-400 mt-1 italic">
                            Suggested:{" "}
                            <span className="text-green-400 font-semibold">
                              {autoProposals[player.id].name}
                            </span>{" "}
                            ({autoProposals[player.id].value})
                          </p>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            <div className="mt-6 flex gap-3">
              <button
                onClick={handleAutoComplete}
                disabled={!players.length || loadingAuto}
                className={`flex-1 px-4 py-3 rounded-lg font-semibold flex items-center justify-center gap-2 transition-all duration-300 ${
                  !players.length || loadingAuto
                    ? "bg-gray-600 text-gray-300 cursor-not-allowed"
                    : "bg-green-600 hover:bg-green-700 text-white shadow-md"
                }`}
              >
                <Sparkles className="w-5 h-5" />
                {loadingAuto ? "Assigning..." : "Auto Complete"}
              </button>

              <button
                onClick={handleStartTraining}
                className="flex-1 px-4 py-3 bg-indigo-600 hover:bg-indigo-700 text-white rounded-lg font-semibold flex items-center justify-center gap-2 shadow-md transition-all duration-300"
              >
                <PlayCircle className="w-5 h-5" /> Start Training
              </button>
            </div>
          </div>

          {/* Дясна част */}
          <div className="bg-white/10 hover:bg-white/15 backdrop-blur-md rounded-2xl border border-white/10 shadow-xl p-6 transition-all duration-300">
            <h2 className="text-2xl font-semibold mb-6 text-white flex items-center gap-2">
              <Dumbbell className="text-blue-400" /> Training Progress
            </h2>
            <div className="overflow-hidden rounded-xl border border-white/10">
              <table className="min-w-full text-sm text-gray-200">
                <thead className="bg-gray-800/60 text-gray-100">
                  <tr>
                    <th className="px-4 py-2 text-left font-semibold">Name</th>
                    <th className="px-4 py-2 text-left font-semibold">Skill</th>
                    <th className="px-4 py-2 text-left font-semibold">Efficiency Gain</th>
                    <th className="px-4 py-2 text-left font-semibold">Sessions</th>
                  </tr>
                </thead>
                <tbody>
                  {trainingProgress.map((progress) => (
                    <tr
                      key={progress.playerId}
                      className="hover:bg-gray-800/40 transition"
                    >
                      <td className="px-4 py-3 font-medium">{progress.name}</td>
                      <td className="px-4 py-3">
                        {progress.skill}{" "}
                        <span className="text-gray-400">
                          ({progress.currentValue})
                        </span>
                      </td>
                      <td className="px-4 py-3 text-green-400 font-semibold">
                        +{progress.efficiency}
                        {progress.progressGain > 0 && (
                          <span className="text-sm text-gray-400">
                            {" "}
                            ({progress.progressGain.toFixed(3)})
                          </span>
                        )}
                        <div className="text-xs text-blue-400">
                          Total: {progress.totalProgress.toFixed(3)}
                        </div>
                        <div className="w-full bg-gray-700 rounded h-2 mt-1">
                          <div
                            className="bg-blue-500 h-2 rounded transition-all duration-300"
                            style={{
                              width: `${Math.min(
                                (progress.totalProgress % 1) * 100,
                                100
                              )}%`,
                            }}
                          ></div>
                        </div>
                      </td>
                      <td className="px-4 py-3">{progress.sessions}</td>
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
