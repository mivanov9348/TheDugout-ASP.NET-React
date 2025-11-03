// src/pages/PlayerProfile.jsx
import React, { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { ArrowLeft, Flag, Activity, TrendingUp, Star } from "lucide-react";
import PlayerAvatar from "../../components/PlayerAvatar";

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
  player.seasonStats?.length > 0
    ? player.seasonStats.reduce((a, b) => a + (b.seasonRating || 0), 0) /
      player.seasonStats.length
    : null;



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
              <PlayerAvatar
                playerName={player.fullName}
                imageFileName={player.avatarFileName}
                className="w-44 h-44 border-4 border-blue-400 shadow-[0_0_25px_rgba(59,130,246,0.8)] group-hover:scale-105 transition-transform duration-500"
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
  Avg Rating: {avgRating ? avgRating.toFixed(2) : "N/A"}
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

        {/* Competition Stats */}
        <div className="mt-14">

          <div className="overflow-x-auto rounded-2xl border border-white/10 bg-white/5 backdrop-blur-sm shadow-lg">
            <table className="min-w-full text-sm text-gray-200">
              <thead className="bg-white/10 text-gray-100">
                <tr>
                  <th className="px-4 py-3 text-left">Competition</th>
                  <th className="px-4 py-3 text-right">Matches</th>
                  <th className="px-4 py-3 text-right">Goals</th>
                </tr>
              </thead>
              <tbody>
                {player.competitionStats?.map((stat) => (
                  <tr
                    key={stat.competitionId}
                    className="border-t border-white/10 hover:bg-white/10 transition"
                  >
                    <td className="px-4 py-2 font-medium">{stat.competitionName}</td>
                    <td className="px-4 py-2 text-right">{stat.matchesPlayed}</td>
                    <td className="px-4 py-2 text-right text-blue-400 font-semibold">
                      {stat.goals}
                    </td>
                  </tr>
                ))}

                {player.competitionStats?.length > 0 && (
                  <tr className="border-t border-white/20 bg-white/10 font-bold">
                    <td className="px-4 py-3 text-right">Total</td>
                    <td className="px-4 py-3 text-right">
                      {player.competitionStats.reduce((a, b) => a + b.matchesPlayed, 0)}
                    </td>
                    <td className="px-4 py-3 text-right text-emerald-400">
                      {player.competitionStats.reduce((a, b) => a + b.goals, 0)}
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </div>

      </div>
    </div>
  );
};

export default PlayerProfile;
