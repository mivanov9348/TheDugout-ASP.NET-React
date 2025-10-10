// src/pages/PlayerProfile.jsx
import React, { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { ArrowLeft, Flag, Activity, TrendingUp } from "lucide-react";

const categoryLabels = {
  1: "⚡ Physical",
  2: "🎯 Technical",
  3: "🧠 Mental",
  4: "🧤 Goalkeeping",
};

const getAttributeColor = (value) => {
  if (value <= 5) return "bg-gray-300";
  if (value <= 10) return "bg-yellow-400";
  if (value <= 15) return "bg-green-500";
  return "bg-red-600";
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
      <div className="min-h-screen flex items-center justify-center text-gray-600 text-lg">
        Loading player...
      </div>
    );

  const groupedAttributes = player.attributes?.reduce((acc, attr) => {
    if (!acc[attr.category]) acc[attr.category] = [];
    acc[attr.category].push(attr);
    return acc;
  }, {});

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-100 via-gray-200 to-blue-100 flex justify-center items-start py-10 px-4">
      <div className="bg-white/90 backdrop-blur-md shadow-2xl rounded-3xl max-w-6xl w-full p-10 relative border border-gray-200 transition-all">
        {/* Back Button */}
        <button
          onClick={() => navigate(-1)}
          className="absolute top-6 left-6 flex items-center text-gray-600 hover:text-blue-600 transition-colors"
        >
          <ArrowLeft className="w-5 h-5 mr-2" />
          <span className="hidden sm:inline font-medium">Back</span>
        </button>

        {/* Header Section */}
        <div className="flex flex-col lg:flex-row items-start gap-10 mt-10">
          {/* Player Info */}
          <div className="flex flex-col items-center lg:items-start flex-1">
            <div className="relative group">
              <img
                src={
                  imgError
                    ? "https://via.placeholder.com/150"
                    : `https://localhost:7117/Avatars/${player.avatarUrl}`
                }
                alt={`${player.fullName} avatar`}
                onError={() => setImgError(true)}
                className="w-44 h-44 rounded-full object-cover border-4 border-blue-500 shadow-lg group-hover:scale-105 transition-transform duration-300"
              />
              <div className="absolute -bottom-2 -right-2 bg-gradient-to-r from-blue-600 to-blue-400 text-white text-xs px-3 py-1 rounded-md shadow-lg font-semibold">
                {player.position}
              </div>
            </div>

            <div className="mt-6 text-center lg:text-left space-y-1">
              <h1 className="text-4xl font-extrabold text-gray-900 tracking-tight">
                {player.fullName}
              </h1>
              <p className="text-lg text-gray-500 font-medium">
                {player.teamName || "Free Agent"}
              </p>
              <div className="flex flex-wrap gap-4 mt-3 text-sm text-gray-600 justify-center lg:justify-start">
                <span className="flex items-center gap-1">
                  <Activity className="w-4 h-4 text-blue-500" /> Age: {player.age}
                </span>
                <span className="flex items-center gap-1">
                  <Flag className="w-4 h-4 text-green-500" /> {player.country}
                </span>
                <span>Height: {player.heightCm} cm</span>
                <span>Weight: {player.weightKg} kg</span>
              </div>
              <p className="mt-4 text-xl font-bold text-green-600">
                Market Value: €{player.price.toLocaleString()}
              </p>
            </div>
          </div>

          {/* Attributes */}
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-6 flex-1">
            {Object.keys(groupedAttributes || {}).map((catKey) => (
              <div
                key={catKey}
                className="bg-gradient-to-br from-gray-50 to-gray-100 p-5 rounded-2xl shadow-md border border-gray-200 hover:shadow-lg hover:-translate-y-1 transition-all"
              >
                <h3 className="text-md font-bold text-blue-700 mb-3 border-b border-blue-200 pb-1">
                  {categoryLabels[catKey]}
                </h3>
                <div className="space-y-2">
                  {groupedAttributes[catKey].map((attr) => (
                    <div key={attr.name} className="text-sm">
                      <div className="flex justify-between mb-1 text-gray-700">
                        <span className="capitalize">{attr.name}</span>
                        <span className="font-semibold">{attr.value}</span>
                      </div>
                      <div className="w-full h-2 bg-gray-200 rounded-full overflow-hidden">
                        <div
                          className={`${getAttributeColor(
                            attr.value
                          )} h-full transition-all duration-500 rounded-full`}
                          style={{ width: `${(attr.value / 20) * 100}%` }}
                        ></div>
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            ))}
          </div>
        </div>

        {/* Season Statistics */}
        <div className="mt-14">
          <h2 className="text-2xl font-bold text-gray-900 mb-6 flex items-center gap-2">
            <TrendingUp className="w-6 h-6 text-blue-600" /> Season Statistics
          </h2>
          <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6">
            {player.seasonStats?.map((stat, i) => (
              <div
                key={stat.seasonId}
                className="bg-gradient-to-tr from-blue-50 to-gray-100 p-5 rounded-2xl shadow-md border border-gray-200 hover:shadow-lg hover:-translate-y-1 transition-all"
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
                    <span className="font-bold text-blue-700">
                      {stat.goals}
                    </span>
                  </p>
                  <p>
                    Assists:{" "}
                    <span className="font-bold text-green-700">
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
