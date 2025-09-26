import React, { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { useNavigate } from "react-router-dom";

export default function MatchPreview() {
  const { fixtureId } = useParams();
  const [match, setMatch] = useState(null);

  const navigate = useNavigate();

  useEffect(() => {
    if (!fixtureId) return;
    fetch(`/api/matches/${fixtureId}/preview`, {
      headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
    })
      .then((res) => res.json())
      .then(setMatch)
      .catch((err) => console.error("Error loading match preview", err));
  }, [fixtureId]);

  if (!match) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <p className="text-gray-600">Loading match preview...</p>
      </div>
    );
  }

  const handleStartMatch = () => {
    fetch(`/api/matches/${fixtureId}/match`, {
      headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
    })
      .then((res) => res.json())
      .then((data) => {
        navigate(`/match/${data.matchId}`); 
      })
      .catch((err) => console.error("Error starting match", err));
  };

  const { home, away } = match;

  return (
    <div className="min-h-screen bg-gradient-to-b from-gray-900 to-black text-white p-4 flex flex-col">
      {/* Start Match Button */}
      <div className="flex justify-center mb-4">
        <button
          onClick={handleStartMatch}
          className="px-4 py-2 text-sm bg-green-600 hover:bg-green-700 text-white font-semibold rounded-full shadow-md transition-colors"
        >
          Start Match
        </button>
      </div>

      {/* Scoreboard */}
      <div className="w-full flex justify-between items-center bg-gray-800 rounded-2xl shadow-lg p-4 text-2xl font-bold">
        <span>{home.name}</span>
        <span className="text-4xl">
          {home.score} : {away.score}
        </span>
        <span>{away.name}</span>
      </div>

      {/* Main Content */}
      <div className="flex flex-1 mt-4 gap-4">
        {/* Home Team */}
        <div className="w-1/2 bg-gray-800 rounded-xl p-3 overflow-y-auto shadow-lg">
          <h2 className="text-center font-bold mb-2">{home.name}</h2>
          <table className="w-full text-sm">
            <thead>
              <tr className="text-gray-400 border-b border-gray-700">
                <th>#</th>
                <th>Pos</th>
                <th>Name</th>
              </tr>
            </thead>
            <tbody>
              {home.lineup.map((p, idx) => (
                <tr key={idx} className="border-b border-gray-700">
                  <td>{p.number}</td>
                  <td>{p.position}</td>
                  <td>{p.name}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        {/* Away Team */}
        <div className="w-1/2 bg-gray-800 rounded-xl p-3 overflow-y-auto shadow-lg">
          <h2 className="text-center font-bold mb-2">{away.name}</h2>
          <table className="w-full text-sm">
            <thead>
              <tr className="text-gray-400 border-b border-gray-700">
                <th>#</th>
                <th>Pos</th>
                <th>Name</th>
              </tr>
            </thead>
            <tbody>
              {away.lineup.map((p, idx) => (
                <tr key={idx} className="border-b border-gray-700">
                  <td>{p.number}</td>
                  <td>{p.position}</td>
                  <td>{p.name}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
