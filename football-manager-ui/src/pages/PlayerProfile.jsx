// src/pages/PlayerProfile.jsx
import React, { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { ArrowLeft, Flag, Activity, TrendingUp, Star } from "lucide-react";

const categoryLabels = {
  1: "⚡ Physical",
  2: "🎯 Technical",
  3: "🧠 Mental",
  4: "🧤 Goalkeeping",
};

const getAttributeColor = (value) => {
  if (value <= 5) return "bg-gradient-to-r from-gray-300 to-gray-400";
  if (value <= 10) return "bg-gradient-to-r from-yellow-400 to-amber-500";
  if (value <= 15) return "bg-gradient-to-r from-green-400 to-emerald-500";
  return "bg-gradient-to-r from-blue-500 to-indigo-600";
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
      <div className="min-h-screen flex items-center justify-center bg-slate-900 text-white text-lg animate-pulse">
        Loading player...
      </div>
    );

  const groupedAttributes = player.attributes?.reduce((acc, attr) => {
    if (!acc[attr.category]) acc[attr.category] = [];
    acc[attr.category].push(attr);
    return acc;
  }, {});

  // Среден рейтинг (пример)
  const avgRating =
    player.attributes?.reduce((a, b) => a + b.value, 0) /
      player.attributes?.length || 0;

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-900 via-blue-900 to-indigo-800 text-gray-100 flex justify-center items-start py-14 px-6 relative overflow-hidden">
      {/* Background stadium lights */}
      <div className="absolute inset-0 bg-[url('https://i.imgur.com/Wv1Z0hO.jpg')] bg-cover bg-center opacity-10 blur-sm"></div>
      <div className="absolute inset-0 bg-gradient-to-b from-slate-900/80 via-slate-800/60 to-blue-900/90"></div>

      <div className="relative z-10 bg-white/10 backdrop-blur-lg shadow-2xl rounded-3xl max-w-6xl w-full p-10 border border-white/20 transition-all text-gray-50">
        {/* Back Button */}
        <button
          onClick={() => navigate(-1)}
          className="absolute top-6 left-6 flex items-center text-gray-300 hover:text-white transition-colors"
        >
          <ArrowLeft className="w-5 h-5 mr-2" />
          <span className="hidden sm:inline font-medium">Back</span>
        </button>

        {/* Header */}
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
                className="w-44 h-44 rounded-full object-cover border-4 border-blue-400 shadow-[0_0_25px_rgba(59,130,246,0.8)] group-hover:scale-105 transition-transform duration-500"
              />
              <div className="absolute -bottom-2 -right-2 bg-blue-600 text-white text-xs px-3 py-1 rounded-md shadow-lg font-semibold">
                {player.position}
              </div>
            </div>

            <div className="mt-6 text-center lg:text-left space-y-2">
              <h1 className="text-4xl font-extrabold text-white tracking-tight drop-shadow-lg">
                {player.fullName}
              </h1>
              <p className="text-lg text-gray-300 font-medium">
                {player.teamName || "Free Agent"}
              </p>

              <div className="flex flex-wrap gap-4 mt-3 text-sm text-gray-300 justify-center lg:justify-start">
                <span className="flex items-center gap-1">
                  <Activity className="w-4 h-4 text-blue-400" /> Age: {player.age}
                </span>
                <span className="flex items-center gap-1">
                  <Flag className="w-4 h-4 text-green-400" /> {player.country}
                </span>
                <span>Height: {player.heightCm} cm</span>
                <span>Weight: {player.weightKg} kg</span>
              </div>

              <div className="mt-5 flex flex-col sm:flex-row items-center gap-6">
                <p className="text-xl font-bold text-emerald-400">
                  Market Value: €{player.price.toLocaleString()}
                </p>
                <div className="flex items-center gap-2 bg-white/10 px-4 py-2 rounded-xl border border-white/20 shadow-lg">
                  <Star className="text-yellow-400 w-5 h-5" />
                  <span className="font-semibold text-lg">
                    Avg Rating: {avgRating.toFixed(1)}
                  </span>
                </div>
              </div>
            </div>
          </div>

          {/* Attributes */}
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-6 flex-1">
            {Object.keys(groupedAttributes || {}).map((catKey) => (
              <div
                key={catKey}
                className="bg-white/5 p-5 rounded-2xl shadow-md border border-white/10 hover:bg-white/10 hover:shadow-blue-500/20 transition-all"
              >
                <h3 className="text-md font-bold text-blue-300 mb-3 border-b border-blue-400/30 pb-1">
                  {categoryLabels[catKey]}
                </h3>
                <div className="space-y-2">
                  {groupedAttributes[catKey].map((attr) => (
                    <div key={attr.name} className="text-sm">
                      <div className="flex justify-between mb-1 text-gray-200">
                        <span className="capitalize">{attr.name}</span>
                        <span className="font-semibold">{attr.value}</span>
                      </div>
                      <div className="w-full h-2 bg-gray-700 rounded-full overflow-hidden">
                        <div
                          className={`${getAttributeColor(
                            attr.value
                          )} h-full transition-all duration-700 rounded-full`}
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

        {/* Season Stats */}
        <div className="mt-14">
          <h2 className="text-2xl font-bold text-white mb-6 flex items-center gap-2">
            <TrendingUp className="w-6 h-6 text-blue-400" /> Season Statistics
          </h2>
          <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6">
            {player.seasonStats?.map((stat) => (
              <div
                key={stat.seasonId}
                className="bg-white/5 p-5 rounded-2xl border border-white/10 hover:bg-white/15 hover:shadow-xl transition-all"
              >
                <p className="text-sm text-gray-300 mb-2">
                  Season {stat.seasonId}
                </p>
                <div className="space-y-1 text-sm">
                  <p>
                    Matches:{" "}
                    <span className="font-bold text-white">
                      {stat.matchesPlayed}
                    </span>
                  </p>
                  <p>
                    Goals:{" "}
                    <span className="font-bold text-blue-400">
                      {stat.goals}
                    </span>
                  </p>
                  <p>
                    Assists:{" "}
                    <span className="font-bold text-emerald-400">
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
