import React, { useEffect, useState } from "react";
import { Users, Activity, Star } from "lucide-react";
import PlayerAvatar from "../../components/PlayerAvatar";

export default function Academy({ teamId }) {
  const [players, setPlayers] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchAcademyPlayers = async () => {
      try {
        const res = await fetch(`/api/player/academy/team/${teamId}`);
        if (!res.ok) throw new Error("Failed to load academy players");
        const data = await res.json();
        setPlayers(data);
      } catch (err) {
        console.error(err);
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
      {/* Заглавие */}
      <div className="flex items-center gap-3 mb-10">
        <Users className="text-sky-400" size={30} />
        <h1 className="text-3xl font-extrabold tracking-tight text-white drop-shadow">
          Youth Academy
        </h1>
      </div>

      {/* Играчите */}
      {players.length === 0 ? (
        <p className="text-slate-400">No youth players found.</p>
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-8">
          {players.map((player) => (
            <div
              key={player.id}
              className="bg-gray-800/60 backdrop-blur-md border border-gray-700 rounded-3xl overflow-hidden shadow-xl hover:shadow-sky-600/30 hover:scale-[1.02] transition-transform duration-300"
            >
              {/* Аватар */}
              <div className="w-full h-52 flex items-center justify-center bg-gradient-to-b from-gray-900 via-gray-800 to-gray-900">
                <PlayerAvatar
                  playerName={player.name}
                  imageFileName={player.avatarFileName}
                  className="w-32 h-32 rounded-full border-4 border-sky-500 shadow-[0_0_20px_rgba(56,189,248,0.5)]"
                />
              </div>

              {/* Инфо */}
              <div className="p-5 text-center">
                <h2 className="text-2xl font-semibold mb-1 text-white tracking-tight">
                  {player.name}
                </h2>
                <p className="text-gray-400 text-sm mb-4">
                  {player.position} • {player.nationality} • {player.age} yrs
                </p>

                <div className="flex justify-between text-slate-300 text-sm mb-3">
                  <span className="flex items-center gap-1">
                    <Activity size={14} />
                    CA: <strong>{player.currentAbility}</strong>
                  </span>
                  <span className="flex items-center gap-1">
                    <Star size={14} className="text-yellow-400" />
                    PA: <strong>{player.potentialAbility}</strong>
                  </span>
                </div>

                {/* Статистика */}
                <div className="grid grid-cols-3 text-center text-slate-300 mt-4 mb-5">
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

                {/* Бутоните */}
                <div className="flex justify-center gap-4 mt-2">
                  <button
                    className="bg-emerald-600 hover:bg-emerald-700 text-white font-semibold py-1.5 px-5 rounded-lg shadow-md shadow-emerald-900/50 transition"
                    onClick={() => console.log(`Promote ${player.name}`)}
                  >
                    Promote
                  </button>
                  <button
                    className="bg-red-600 hover:bg-red-700 text-white font-semibold py-1.5 px-5 rounded-lg shadow-md shadow-red-900/50 transition"
                    onClick={() => console.log(`Release ${player.name}`)}
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
