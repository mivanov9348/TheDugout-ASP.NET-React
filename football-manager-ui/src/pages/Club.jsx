import React, { useEffect, useState } from "react";
import { Users, Trophy, Shield, MapPin, Flag } from "lucide-react";
import TeamLogo from "../components/TeamLogo";

const Club = () => {
  const [club, setClub] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchSave = async () => {
      try {
        const response = await fetch("/api/games/current", {
          credentials: "include",
        });

        if (!response.ok) throw new Error("Failed to fetch save");

        const data = await response.json();
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
    return (
      <p className="text-center text-gray-500 mt-10 animate-pulse">
        Loading your club...
      </p>
    );
  }

  if (!club) {
    return (
      <p className="text-center text-gray-500 mt-10">
        No club selected.
      </p>
    );
  }

  return (
    <div className="p-6 max-w-6xl mx-auto">
      {/* Header */}
      <div className="flex flex-col items-center mb-8">
        <TeamLogo
          teamName={club.name}
          logoFileName={club.logoFileName}
          className="w-24 h-24 mb-4"
        />
        <h1 className="text-3xl font-bold text-center">{club.name}</h1>
        <p className="text-gray-500">{club.league?.name || "—"}</p>
      </div>

      {/* Overview */}
      <div className="bg-white rounded-2xl shadow-lg p-6 mb-8">
        <h2 className="text-xl font-semibold mb-4">Club Overview</h2>
        <div className="grid sm:grid-cols-2 gap-4 text-gray-700">
          <div className="flex items-center gap-2">
            <Flag className="w-5 h-5 text-blue-500" />
            <span className="font-medium">Country:</span>{" "}
            {club.country?.name}
          </div>
          <div className="flex items-center gap-2">
            <Shield className="w-5 h-5 text-green-600" />
            <span className="font-medium">Stadium:</span>{" "}
            {club.stadiumName || "—"}
          </div>
          <div className="flex items-center gap-2">
            <MapPin className="w-5 h-5 text-red-500" />
            <span className="font-medium">Founded:</span>{" "}
            {club.founded || "—"}
          </div>
          <div className="flex items-center gap-2">
            <Users className="w-5 h-5 text-purple-500" />
            <span className="font-medium">Manager:</span>{" "}
            {club.managerName || "Unknown"}
          </div>
        </div>
      </div>

      {/* Players */}
      <div className="bg-white rounded-2xl shadow-lg p-6 mb-8">
        <h2 className="text-xl font-semibold mb-4">Players</h2>
        {club.players?.length > 0 ? (
          <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-4">
            {club.players.map((player, index) => (
              <div
                key={index}
                className="bg-gray-50 rounded-xl p-3 text-center hover:shadow-md transition"
              >
                <p className="font-medium text-gray-800 truncate">
                  {player.name}
                </p>
                <p className="text-sm text-gray-500">{player.position}</p>
              </div>
            ))}
          </div>
        ) : (
          <p className="text-gray-500">No players available</p>
        )}
      </div>

      {/* Extra Info */}
      <div className="bg-white rounded-2xl shadow-lg p-6">
        <h2 className="text-xl font-semibold mb-4">Extra Info</h2>
        <div className="grid sm:grid-cols-2 gap-4 text-gray-700">
          <div className="flex items-center gap-2">
            <Trophy className="w-5 h-5 text-yellow-500" />
            <span className="font-medium">Trophies:</span>{" "}
            {club.trophies || 0}
          </div>
          <div className="flex items-center gap-2">
            <Users className="w-5 h-5 text-pink-500" />
            <span className="font-medium">Rivals:</span>{" "}
            {club.rivals?.join(", ") || "—"}
          </div>
        </div>
      </div>
    </div>
  );
};

export default Club;
