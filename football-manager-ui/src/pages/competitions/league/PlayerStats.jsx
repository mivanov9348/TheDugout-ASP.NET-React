import React, { useEffect, useState } from "react";

const PlayerStats = ({ leagueId }) => {
  const [players, setPlayers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    if (!leagueId) return;

    const fetchStats = async () => {
      try {
        setLoading(true);
        const res = await fetch(`/api/PlayerStats/league/${leagueId}`);
        if (!res.ok) throw new Error(`HTTP ${res.status}`);
        const data = await res.json();
        setPlayers(data || []);
      } catch (err) {
        console.error("❌ Error fetching player stats:", err);
        setError("Грешка при зареждане на статистиките.");
      } finally {
        setLoading(false);
      }
    };

    fetchStats();
  }, [leagueId]);

  if (loading)
    return <p className="text-gray-400 text-center mt-4">⏳ Зареждане...</p>;
  if (error)
    return (
      <p className="text-red-500 text-center mt-4 font-semibold">{error}</p>
    );
  if (players.length === 0)
    return (
      <p className="text-gray-400 text-center mt-4">
        Няма намерени играчи с голова статистика.
      </p>
    );

  return (
    <div className="overflow-x-auto">
      <table className="w-full text-sm text-gray-300">
        <thead className="bg-gray-800 text-gray-200 uppercase text-xs">
          <tr>
            <th className="px-3 py-2 text-left">#</th>
            <th className="px-3 py-2 text-left">Играч</th>
            <th className="px-3 py-2 text-left">Отбор</th>
            <th className="px-3 py-2 text-center">Голове</th>
            <th className="px-3 py-2 text-center">Асистенции</th>
            <th className="px-3 py-2 text-center">Мачове</th>
          </tr>
        </thead>
        <tbody>
          {players.map((p, idx) => (
            <tr
              key={p.PlayerId}
              className="border-b border-gray-700 hover:bg-gray-800"
            >
              <td className="px-3 py-2 text-center">{idx + 1}</td>
              <td className="px-3 py-2 flex items-center gap-2">
                <img
                  src={`/images/players/${p.PlayerPhoto || "default.png"}`}
                  alt={p.PlayerName}
                  className="w-6 h-6 rounded-full"
                />
                {p.PlayerName}
              </td>
              <td className="px-3 py-2">{p.TeamName}</td>
              <td className="px-3 py-2 text-center font-bold text-sky-400">
                {p.Goals}
              </td>
              <td className="px-3 py-2 text-center">{p.Assists}</td>
              <td className="px-3 py-2 text-center">{p.Matches}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default PlayerStats;
