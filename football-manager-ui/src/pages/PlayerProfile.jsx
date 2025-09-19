// src/pages/PlayerProfile.jsx
import React, { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { ArrowLeft } from "lucide-react";

const PlayerProfile = ({ gameSaveId }) => {
  const { playerId } = useParams();
  const navigate = useNavigate();
  const [player, setPlayer] = useState(null);
  const [imgError, setImgError] = useState(false);


  useEffect(() => {
    const fetchPlayer = async () => {
      try {
        const res = await fetch(`/api/player/${playerId}`, {
          credentials: "include",
        });
        if (!res.ok) throw new Error("Грешка при зареждане на играча");
        const data = await res.json();
        setPlayer(data);
      } catch (err) {
        console.error(err);
      }
    };
    fetchPlayer();
  }, [playerId]);

  if (!player)
    return (
      <div className="min-h-screen flex items-center justify-center text-gray-600">
        Loading player...
      </div>
    );

  return (
    <div className="min-h-screen bg-gray-100 flex justify-center items-start p-6">
      <div className="bg-white shadow-lg rounded-lg max-w-6xl w-full p-6 relative">
        {/* Back Button */}
        <button
          onClick={() => navigate(-1)}
          className="absolute top-4 left-4 flex items-center text-gray-600 hover:text-gray-900"
        >
          <ArrowLeft className="w-5 h-5 mr-2" />
          <span className="hidden sm:inline">Back</span>
        </button>

        {/* Header Section */}
        <div className="flex flex-col md:flex-row items-center md:items-start mb-8 mt-8">
          <img
  src={`http://localhost:7117/Avatars/${player.AvatarUrl}`}
  alt={`${player.fullName} avatar`}
  onError={() => setImgError(true)}
  className="w-56 h-56 rounded-full object-cover mb-4 md:mb-0 md:mr-6 border-4 border-blue-500"
/>


          <div className="text-center md:text-left">
            <h1 className="text-4xl font-bold text-gray-800">
              {player.fullName}
            </h1>
            <p className="text-xl text-gray-600">
              {player.position} - {player.teamName || "Free Agent"}
            </p>
            <p className="text-lg text-gray-500">
              Age: {player.age} | Country: {player.country}
            </p>
            <p className="text-md text-gray-500">
              Height: {player.heightCm} cm | Weight: {player.weightKg} kg
            </p>
            <p className="text-lg text-gray-700 font-semibold mt-2">
              Market Value: €{player.price.toLocaleString()}
            </p>
          </div>
        </div>

        {/* Attributes Section */}
        <div className="mb-8">
          <h2 className="text-2xl font-semibold text-gray-800 mb-4">
            Player Attributes
          </h2>
          <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
            {player.attributes?.map((attr) => (
              <div
                key={attr.name}
                className="bg-gray-50 p-4 rounded-md shadow-sm"
              >
                <p className="text-sm text-gray-500 capitalize">{attr.name}</p>
                <p className="text-2xl font-bold text-blue-600">{attr.value}</p>
              </div>
            ))}
          </div>
        </div>

        {/* Season Statistics Section */}
        <div>
          <h2 className="text-2xl font-semibold text-gray-800 mb-4">
            Season Statistics
          </h2>
          <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
            {player.seasonStats?.map((stat) => (
              <div
                key={stat.seasonId}
                className="bg-gray-50 p-4 rounded-md shadow-sm"
              >
                <p className="text-sm text-gray-500">
                  Season {stat.seasonId}
                </p>
                <p className="text-md text-gray-700">
                  Matches:{" "}
                  <span className="font-bold">{stat.matchesPlayed}</span>
                </p>
                <p className="text-md text-gray-700">
                  Goals: <span className="font-bold">{stat.goals}</span>
                </p>
                <p className="text-md text-gray-700">
                  Assists: <span className="font-bold">{stat.assists}</span>
                </p>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
};

export default PlayerProfile;
