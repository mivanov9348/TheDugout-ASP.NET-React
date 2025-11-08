import React, { useEffect, useState } from "react";
import { Link } from "react-router-dom";

export default function Shortlist({ gameSaveId }) {
  const [players, setPlayers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    if (!gameSaveId) return;

    const fetchShortlist = async () => {
      try {
        const response = await fetch(`/api/player/GetShortlist?gameSaveId=${gameSaveId}`);
        if (!response.ok) throw new Error("Failed to load shortlist.");
        const data = await response.json();
        setPlayers(data);
      } catch (err) {
        console.error(err);
        setError(err.message);
      } finally {
        setLoading(false);
      }
    };

    fetchShortlist();
  }, [gameSaveId]);

  if (loading) return <div className="text-gray-300 p-4">Loading shortlist...</div>;
  if (error) return <div className="text-red-400 p-4">{error}</div>;
  if (players.length === 0) return <div className="text-gray-400 p-4">No players in shortlist.</div>;

  return (
    <div className="space-y-4">
      <h2 className="text-2xl font-bold mb-4">My Shortlist</h2>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {players.map((player) => (
          <div
            key={player.id}
            className="bg-gray-800 border border-gray-700 rounded-2xl p-4 shadow-md hover:shadow-lg transition-all duration-200"
          >
            {/* 🔗 Име на играча с линк към PlayerProfile */}
            <div className="flex justify-between items-center mb-2">
              <Link
                to={`/player/${player.id}?gameSaveId=${gameSaveId}`}
                className="text-xl font-semibold text-white hover:text-blue-400 transition-colors"
              >
                {player.firstName} {player.lastName}
              </Link>
              <span className="text-sm text-gray-400">{player.position}</span>
            </div>

            <div className="flex justify-between text-gray-300 text-sm mb-2">
              <span>Age: {player.age}</span>
              <span>Overall: {player.overall}</span>
            </div>

            <div className="flex justify-between items-center mt-3">
              <span className="text-blue-400 font-medium">
                {player.teamName || "Free Agent"}
              </span>

              <button
                onClick={async () => {
                  await fetch(`/api/player/${player.id}/shortlist?gameSaveId=${gameSaveId}`, {
                    method: "DELETE",
                  });
                  setPlayers(players.filter((p) => p.id !== player.id));
                }}
                className="px-3 py-1 bg-red-600 hover:bg-red-700 rounded-lg text-sm text-white transition-all"
              >
                Remove
              </button>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
