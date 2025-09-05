import React, { useEffect, useState } from "react";

const Club = () => {
  const [club, setClub] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchSave = async () => {
      try {
        const response = await fetch("/api/games/current", {
          credentials: "include", // за да прати auth cookie
        });

        if (!response.ok) throw new Error("Failed to fetch save");

        const data = await response.json();
        console.log("SAVE DATA:", data);
        setClub(data?.userTeam || null);
      } catch (err) {
        console.error(err);
      } finally {
        setLoading(false);
      }
    };

    fetchSave();
  }, []);

  if (loading) {
    return <p className="text-center text-gray-500 mt-10">Loading...</p>;
  }

  if (!club) {
    return <p className="text-center text-gray-500 mt-10">No club selected.</p>;
  }

  return (
    <div className="p-6">
      <h1 className="text-3xl font-bold mb-6 text-center">🏟️ {club.name}</h1>

      {/* Club Overview */}
      <div className="bg-white/80 backdrop-blur-sm rounded-2xl shadow-lg p-6 mb-6">
        <h2 className="text-xl font-semibold mb-4">Club Overview</h2>
        <ul className="text-gray-700 space-y-2">
          <li><span className="font-medium">Name:</span> {club.name}</li>
          <li><span className="font-medium">Country:</span> {club.country?.name}</li>
          <li><span className="font-medium">League:</span> {club.league?.name || "—"}</li>
          <li><span className="font-medium">Stadium:</span> {club.stadiumName || "—"}</li>
          <li><span className="font-medium">Founded:</span> {club.founded || "—"}</li>
        </ul>
      </div>

      {/* Players */}
      <div className="bg-white/80 backdrop-blur-sm rounded-2xl shadow-lg p-6 mb-6">
        <h2 className="text-xl font-semibold mb-4">Players</h2>
        {club.players?.length > 0 ? (
          <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
            {club.players.map((player, index) => (
              <div
                key={index}
                className="bg-gray-100 rounded-xl shadow-md p-3 text-center hover:scale-105 transition-transform duration-200"
              >
                <p className="font-medium text-gray-800">{player.name}</p>
                <p className="text-sm text-gray-500">{player.position}</p>
              </div>
            ))}
          </div>
        ) : (
          <p className="text-gray-500">No players available</p>
        )}
      </div>

      {/* Extra Info */}
      <div className="bg-white/80 backdrop-blur-sm rounded-2xl shadow-lg p-6">
        <h2 className="text-xl font-semibold mb-4">Extra Info</h2>
        <ul className="text-gray-700 space-y-2">
          <li><span className="font-medium">Manager:</span> {club.managerName || "Unknown"}</li>
          <li><span className="font-medium">Trophies:</span> {club.trophies || 0}</li>
          <li><span className="font-medium">Rivals:</span> {club.rivals?.join(", ") || "—"}</li>
        </ul>
      </div>
    </div>
  );
};

export default Club;
