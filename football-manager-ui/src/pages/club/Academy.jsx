import React, { useEffect, useState } from "react";
import { Users, Ruler, Weight, Globe } from "lucide-react";
import PlayerAvatar from "../../components/PlayerAvatar";
import Swal from "sweetalert2";

export default function Academy({ teamId }) {
  const [players, setPlayers] = useState([]);
  const [loading, setLoading] = useState(true);

  // ---- Promote Player ----
  const handlePromote = async (playerId, playerName) => {
    try {
      const res = await fetch(`/api/player/academy/promote/${playerId}`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
      });

      const data = await res.json();
      if (!res.ok) throw new Error(data.message || "Promotion failed.");

      await Swal.fire({
        icon: "success",
        title: "Player Promoted",
        text:
          data.message ||
          `${playerName} has been successfully promoted to the senior team.`,
        confirmButtonColor: "#10b981",
      });

      setPlayers((prev) => prev.filter((p) => p.id !== playerId));
    } catch (err) {
      await Swal.fire({
        icon: "error",
        title: "Promotion Failed",
        text: err.message || "An unexpected error occurred.",
        confirmButtonColor: "#ef4444",
      });
    }
  };

  // ---- Release Player ----
  const handleRelease = async (playerId, playerName) => {
    const confirm = await Swal.fire({
      title: `Release ${playerName}?`,
      text: "This action cannot be undone.",
      icon: "warning",
      showCancelButton: true,
      confirmButtonText: "Yes, release",
      cancelButtonText: "Cancel",
      confirmButtonColor: "#ef4444",
      cancelButtonColor: "#6b7280",
    });

    if (!confirm.isConfirmed) return;

    try {
      const res = await fetch(`/api/player/academy/release/${playerId}`, {
        method: "DELETE",
      });

      const data = await res.json();
      if (!res.ok) throw new Error(data.message || "Failed to release player.");

      await Swal.fire({
        icon: "success",
        title: "Player Released",
        text: data.message || `${playerName} has been released from the academy.`,
        confirmButtonColor: "#10b981",
      });

      setPlayers((prev) => prev.filter((p) => p.id !== playerId));
    } catch (err) {
      await Swal.fire({
        icon: "error",
        title: "Release Failed",
        text: err.message || "An unexpected error occurred.",
        confirmButtonColor: "#ef4444",
      });
    }
  };

  // ---- Fetch Players ----
  useEffect(() => {
    const fetchAcademyPlayers = async () => {
      try {
        const res = await fetch(`/api/player/academy/team/${teamId}`);
        if (!res.ok) throw new Error("Failed to load academy players.");
        const data = await res.json();
        setPlayers(data);
      } catch (err) {
        console.error(err);
        Swal.fire({
          icon: "error",
          title: "Loading Error",
          text: "Failed to fetch academy players.",
          confirmButtonColor: "#ef4444",
        });
      } finally {
        setLoading(false);
      }
    };

    if (teamId) fetchAcademyPlayers();
  }, [teamId]);

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-gray-900 via-gray-800 to-gray-900 text-gray-200 text-lg animate-pulse">
        Loading academy players...
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-900 via-gray-800 to-gray-900 text-gray-200 px-6 py-10">
      {/* Header */}
      <div className="flex items-center gap-3 mb-10">
        <Users className="text-sky-400" size={30} />
        <h1 className="text-3xl font-extrabold tracking-tight text-white drop-shadow">
          Youth Academy
        </h1>
      </div>

      {/* Players */}
      {players.length === 0 ? (
        <p className="text-slate-400">No youth players found.</p>
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-8">
          {players.map((player) => (
            <div
              key={player.id}
              className="bg-gray-800/60 backdrop-blur-md border border-gray-700 rounded-3xl overflow-hidden shadow-xl hover:shadow-sky-600/30 hover:scale-[1.02] transition-transform duration-300"
            >
              {/* Avatar */}
              <div className="w-full h-52 flex items-center justify-center bg-gradient-to-b from-gray-900 via-gray-800 to-gray-900">
                <PlayerAvatar
                  playerName={player.fullName}
                  imageFileName={player.avatarFileName}
                  className="w-32 h-32 rounded-full border-4 border-sky-500 shadow-[0_0_20px_rgba(56,189,248,0.5)]"
                />
              </div>

              {/* Info */}
              <div className="p-5 text-center">
                <h2 className="text-2xl font-semibold mb-1 text-white tracking-tight">
                  {player.fullName}
                </h2>
                <p className="text-gray-400 text-sm mb-4">
                  {player.position} • {player.country} • {player.age} yrs
                </p>

                {/* Personal info */}
                <div className="flex justify-around text-slate-300 text-sm mb-3">
                  <span className="flex items-center gap-1">
                    <Ruler size={14} /> {player.heightCm} cm
                  </span>
                  <span className="flex items-center gap-1">
                    <Weight size={14} /> {player.weightKg} kg
                  </span>
                  <span className="flex items-center gap-1">
                    <Globe size={14} /> {player.country}
                  </span>
                </div>

                {/* Attributes */}
                <div className="mt-4">
                  <h3 className="text-sky-400 font-semibold mb-2">Attributes</h3>
                  <div className="grid grid-cols-2 text-left gap-1 text-sm text-gray-300">
                    {player.attributes && player.attributes.length > 0 ? (
                      player.attributes.map((attr) => (
                        <div key={attr.attributeId} className="flex justify-between">
                          <span>{attr.name}</span>
                          <span className="font-semibold text-gray-100">
                            {attr.value}
                          </span>
                        </div>
                      ))
                    ) : (
                      <p className="text-gray-500 italic text-center col-span-2">
                        No attributes data
                      </p>
                    )}
                  </div>
                </div>

                {/* Stats */}
                {player.seasonStats && player.seasonStats.length > 0 && (
                  <div className="grid grid-cols-3 text-center text-slate-300 mt-5 mb-5">
                    <div>
                      <p className="text-sky-400 font-semibold text-lg">
                        {player.seasonStats[0].matchesPlayed}
                      </p>
                      <p className="text-xs">Apps</p>
                    </div>
                    <div>
                      <p className="text-sky-400 font-semibold text-lg">
                        {player.seasonStats[0].goals}
                      </p>
                      <p className="text-xs">Goals</p>
                    </div>
                    <div>
                      <p className="text-sky-400 font-semibold text-lg">
                        {player.seasonStats[0].seasonRating?.toFixed(1) ?? "-"}
                      </p>
                      <p className="text-xs">Rating</p>
                    </div>
                  </div>
                )}

                {/* Buttons */}
                <div className="flex justify-center gap-4 mt-2">
                  <button
                    className="bg-emerald-600 hover:bg-emerald-700 text-white font-semibold py-1.5 px-5 rounded-lg shadow-md shadow-emerald-900/50 transition"
                    onClick={() => handlePromote(player.id, player.fullName)}
                  >
                    Promote
                  </button>

                  <button
                    className="bg-red-600 hover:bg-red-700 text-white font-semibold py-1.5 px-5 rounded-lg shadow-md shadow-red-900/50 transition"
                    onClick={() => handleRelease(player.id, player.fullName)}
                  >
                    Release
                  </button>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
