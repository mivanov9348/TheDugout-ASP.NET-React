// src/pages/club/Academy.jsx
import React, { useEffect, useState } from "react";
import { Users, Activity, Star } from "lucide-react";

export default function Academy({ gameSaveId }) {
  const [players, setPlayers] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchAcademyPlayers = async () => {
      try {
        const res = await fetch(`/api/player/academy/${gameSaveId}`);
        if (!res.ok) throw new Error("Failed to load academy players");
        const data = await res.json();
        setPlayers(data);
      } catch (err) {
        console.error(err);
      } finally {
        setLoading(false);
      }
    };

    fetchAcademyPlayers();
  }, [gameSaveId]);

  if (loading) {
    return (
      <div className="p-6 text-white text-center">
        <p>Loading academy players...</p>
      </div>
    );
  }

  return (
    <div className="p-6">
      {/* Заглавие */}
      <div className="flex items-center gap-3 mb-6">
        <Users className="text-sky-500" size={28} />
        <h1 className="text-3xl font-bold text-white">Youth Academy</h1>
      </div>

      {/* Играчите */}
      {players.length === 0 ? (
        <p className="text-slate-400">No youth players found.</p>
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-5">
          {players.map((player) => (
            <div
              key={player.id}
              className="bg-slate-800 border border-slate-700 rounded-2xl overflow-hidden shadow-lg hover:shadow-sky-700/40 hover:scale-[1.02] transition"
            >
              <img
                src={player.photo}
                alt={player.name}
                className="w-full h-48 object-cover"
              />
              <div className="p-4 text-white">
                <h2 className="text-xl font-semibold mb-1">{player.name}</h2>
                <p className="text-slate-400 text-sm mb-3">
                  {player.position} • {player.nationality} • {player.age} yrs
                </p>

                <div className="flex justify-between text-slate-300 text-sm mb-2">
                  <span className="flex items-center gap-1">
                    <Activity size={14} />
                    CA: <strong>{player.currentAbility}</strong>
                  </span>
                  <span className="flex items-center gap-1">
                    <Star size={14} className="text-yellow-400" />
                    PA: <strong>{player.potentialAbility}</strong>
                  </span>
                </div>

                <div className="grid grid-cols-3 text-center text-slate-300 mt-3">
                  <div>
                    <p className="text-sky-400 font-semibold text-lg">
                      {player.appearances}
                    </p>
                    <p className="text-xs">Apps</p>
                  </div>
                  <div>
                    <p className="text-sky-400 font-semibold text-lg">
                      {player.goals}
                    </p>
                    <p className="text-xs">Goals</p>
                  </div>
                  <div>
                    <p className="text-sky-400 font-semibold text-lg">
                      {player.assists}
                    </p>
                    <p className="text-xs">Assists</p>
                  </div>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
