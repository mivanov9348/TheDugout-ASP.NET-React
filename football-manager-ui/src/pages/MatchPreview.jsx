import React, { useEffect, useState } from "react";
import { useParams } from "react-router-dom";

export default function MatchPreview() {
  const { fixtureId } = useParams();
  const [match, setMatch] = useState(null);

  useEffect(() => {
    if (!fixtureId) return;
    fetch(`/api/matches/${fixtureId}/preview`, {
      headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
    })
      .then(res => res.json())
      .then(setMatch)
      .catch(err => console.error("Error loading match preview", err));
  }, [fixtureId]);

  if (!match) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <p className="text-gray-600">Loading match preview...</p>
      </div>
    );
  }

  const { competition, home, away, minute, status } = match;

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-50 to-slate-100 p-6">
      <div className="max-w-6xl mx-auto space-y-8">
        {/* Header */}
        <div className="text-center">
          <h1 className="text-3xl font-bold text-gray-800 mb-2">
            {competition}
          </h1>
          <p className="text-gray-600">Fixture #{fixtureId}</p>
        </div>

        {/* Scoreboard */}
        <div className="bg-gradient-to-r from-blue-600 to-purple-700 rounded-3xl shadow-2xl p-8 text-white transform hover:scale-[1.02] transition-transform duration-300">
          <div className="flex items-center justify-between">
            {/* Home Team */}
            <div className="text-center flex-1">
              <h2 className="text-2xl font-bold mb-2">{home.name}</h2>
              <div className="text-5xl font-black text-yellow-300">
                {home.score}
              </div>
            </div>

            {/* Match Info */}
            <div className="flex flex-col items-center mx-8">
              <div className="text-6xl font-black mb-2">
                {home.score} - {away.score}
              </div>
              <div className="flex items-center space-x-2">
                <span className="w-3 h-3 bg-red-500 rounded-full animate-pulse"></span>
                <span className="text-lg font-semibold">
                  {status} • {minute}'
                </span>
              </div>
            </div>

            {/* Away Team */}
            <div className="text-center flex-1">
              <h2 className="text-2xl font-bold mb-2">{away.name}</h2>
              <div className="text-5xl font-black text-yellow-300">
                {away.score}
              </div>
            </div>
          </div>
        </div>

        {/* Lineups */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
          {/* Home Team Lineup */}
          <div className="bg-white rounded-2xl shadow-xl p-6 transform hover:scale-[1.01] transition-transform duration-300">
            <div className="flex items-center space-x-3 mb-6">
              <div className="w-3 h-8 bg-blue-600 rounded-full"></div>
              <h3 className="text-2xl font-bold text-gray-800">
                {home.name} Lineup
              </h3>
            </div>
            <div className="space-y-3">
              {home.lineup.map((player, idx) => (
                <div
                  key={idx}
                  className="flex items-center justify-between bg-gradient-to-r from-blue-50 to-indigo-50 p-4 rounded-xl border border-blue-100 hover:shadow-md transition-shadow"
                >
                  <div className="flex items-center space-x-4">
                    <span className="w-8 h-8 bg-blue-500 text-white rounded-full flex items-center justify-center font-bold">
                      {player.number}
                    </span>
                    <span className="font-semibold text-gray-800">
                      {player.name}
                    </span>
                  </div>
                  <span className="px-3 py-1 bg-blue-100 text-blue-800 rounded-full text-sm font-medium">
                    {player.position}
                  </span>
                </div>
              ))}
            </div>
          </div>

          {/* Away Team Lineup */}
          <div className="bg-white rounded-2xl shadow-xl p-6 transform hover:scale-[1.01] transition-transform duration-300">
            <div className="flex items-center space-x-3 mb-6">
              <div className="w-3 h-8 bg-red-600 rounded-full"></div>
              <h3 className="text-2xl font-bold text-gray-800">
                {away.name} Lineup
              </h3>
            </div>
            <div className="space-y-3">
              {away.lineup.map((player, idx) => (
                <div
                  key={idx}
                  className="flex items-center justify-between bg-gradient-to-r from-red-50 to-pink-50 p-4 rounded-xl border border-red-100 hover:shadow-md transition-shadow"
                >
                  <div className="flex items-center space-x-4">
                    <span className="w-8 h-8 bg-red-500 text-white rounded-full flex items-center justify-center font-bold">
                      {player.number}
                    </span>
                    <span className="font-semibold text-gray-800">
                      {player.name}
                    </span>
                  </div>
                  <span className="px-3 py-1 bg-red-100 text-red-800 rounded-full text-sm font-medium">
                    {player.position}
                  </span>
                </div>
              ))}
            </div>
          </div>
        </div>

        {/* Additional Match Info */}
        <div className="bg-white rounded-2xl shadow-xl p-6">
          <h3 className="text-xl font-bold text-gray-800 mb-4">
            Match Details
          </h3>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-center">
            <div className="p-4 bg-green-50 rounded-lg">
              <div className="text-2xl font-bold text-green-600">{minute}'</div>
              <div className="text-sm text-gray-600">Current Minute</div>
            </div>
            <div className="p-4 bg-blue-50 rounded-lg">
              <div className="text-2xl font-bold text-blue-600">
                {home.score + away.score}
              </div>
              <div className="text-sm text-gray-600">Total Goals</div>
            </div>
            <div className="p-4 bg-orange-50 rounded-lg">
              <div className="text-2xl font-bold text-orange-600">
                {home.score}-{away.score}
              </div>
              <div className="text-sm text-gray-600">Scoreline</div>
            </div>
            <div className="p-4 bg-purple-50 rounded-lg">
              <div className="text-2xl font-bold text-purple-600">{status}</div>
              <div className="text-sm text-gray-600">Status</div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
