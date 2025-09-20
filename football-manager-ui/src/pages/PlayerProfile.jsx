// src/pages/PlayerProfile.jsx
import React, { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { ArrowLeft } from "lucide-react";

const categoryLabels = {
  1: "⚡ Physical",
  2: "🎯 Technical",
  3: "🧠 Mental",
  4: "🧤 Goalkeeping",
};

// функция за динамичен цвят според стойността
const getAttributeColor = (value) => {
  if (value <= 5) return "text-gray-400"; // много слаб
  if (value <= 10) return "text-yellow-500"; // посредствен
  if (value <= 15) return "text-green-600"; // добър
  return "text-red-600 font-bold"; // топ ниво
};

const PlayerProfile = () => {
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

  const groupedAttributes = player.attributes?.reduce((acc, attr) => {
    if (!acc[attr.category]) acc[attr.category] = [];
    acc[attr.category].push(attr);
    return acc;
  }, {});

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-100 to-gray-200 flex justify-center items-start p-6">
      <div className="bg-white shadow-2xl rounded-2xl max-w-6xl w-full p-8 relative">
        {/* Back Button */}
        <button
          onClick={() => navigate(-1)}
          className="absolute top-6 left-6 flex items-center text-gray-600 hover:text-blue-600 transition-colors"
        >
          <ArrowLeft className="w-5 h-5 mr-2" />
          <span className="hidden sm:inline font-medium">Back</span>
        </button>

        {/* Header Section */}
        <div className="flex flex-col lg:flex-row items-start gap-6 mt-6">
          {/* Left side - player info */}
          <div className="flex flex-col items-center lg:items-start flex-1">
            <div className="relative">
              <img
                src={
                  imgError
                    ? "https://via.placeholder.com/150"
                    : `https://localhost:7117/Avatars/${player.avatarUrl}`
                }
                alt={`${player.fullName} avatar`}
                onError={() => setImgError(true)}
                className="w-40 h-40 rounded-full object-cover border-4 border-blue-500 shadow-md"
              />
              <div className="absolute -bottom-2 -right-2 bg-blue-600 text-white text-xs px-2 py-1 rounded-md shadow">
                {player.position}
              </div>
            </div>
            <div className="mt-4 text-center lg:text-left">
              <h1 className="text-3xl font-extrabold text-gray-900">
                {player.fullName}
              </h1>
              <p className="text-lg text-gray-600">
                {player.teamName || "Free Agent"}
              </p>
              <div className="mt-2 space-y-1 text-sm text-gray-500">
                <p>
                  Age: {player.age} | Country: {player.country}
                </p>
                <p>
                  Height: {player.heightCm} cm | Weight: {player.weightKg} kg
                </p>
              </div>
              <p className="mt-3 text-lg font-semibold text-green-600">
                Market Value: €{player.price.toLocaleString()}
              </p>
            </div>
          </div>

          {/* Right side - attributes */}
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 flex-1">
            {Object.keys(groupedAttributes || {}).map((catKey) => (
              <div
                key={catKey}
                className="bg-gray-50 p-4 rounded-xl shadow-sm hover:shadow-md transition"
              >
                <h3 className="text-md font-bold text-blue-600 mb-3 border-b border-blue-200 pb-1">
                  {categoryLabels[catKey]}
                </h3>
                <div className="space-y-2">
                  {groupedAttributes[catKey].map((attr) => (
                    <div
                      key={attr.name}
                      className="flex justify-between text-sm"
                    >
                      <span className="capitalize text-gray-700">
                        {attr.name}
                      </span>
                      <span className={`${getAttributeColor(attr.value)}`}>
                        {attr.value}
                      </span>
                    </div>
                  ))}
                </div>
              </div>
            ))}
          </div>
        </div>

        {/* Season Statistics */}
        <div className="mt-10">
          <h2 className="text-2xl font-bold text-gray-900 mb-6">
            Season Statistics
          </h2>
          <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6">
            {player.seasonStats?.map((stat) => (
              <div
                key={stat.seasonId}
                className="bg-gradient-to-tr from-gray-50 to-gray-100 p-5 rounded-xl shadow-sm hover:shadow-md transition"
              >
                <p className="text-sm text-gray-500 mb-2">
                  Season {stat.seasonId}
                </p>
                <div className="space-y-1 text-sm">
                  <p>
                    Matches:{" "}
                    <span className="font-bold text-gray-800">
                      {stat.matchesPlayed}
                    </span>
                  </p>
                  <p>
                    Goals:{" "}
                    <span className="font-bold text-gray-800">
                      {stat.goals}
                    </span>
                  </p>
                  <p>
                    Assists:{" "}
                    <span className="font-bold text-gray-800">
                      {stat.assists}
                    </span>
                  </p>
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
};

export default PlayerProfile;
