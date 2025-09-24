import { useState, useEffect } from "react";
import { useParams } from "react-router-dom";

export default function MatchPage() {
  const { fixtureId } = useParams();
  const [match, setMatch] = useState(null);

  useEffect(() => {
    fetch(`/api/matches/${fixtureId}/match`, {
      headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
    })
      .then((res) => res.json())
      .then(setMatch)
      .catch((err) => console.error("Error loading match", err));
  }, [fixtureId]);

  if (!match) {
    return (
      <div className="min-h-screen flex items-center justify-center text-white">
        Loading match...
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-b from-gray-900 to-black text-white p-4 flex flex-col">
      {/* Scoreboard */}
      <div className="w-full flex justify-between items-center bg-gray-800 rounded-2xl shadow-lg p-4 text-2xl font-bold">
        <span>{match.homeTeam.name}</span>
        <span className="text-4xl">
          {match.score.home} : {match.score.away}
        </span>
        <span>{match.awayTeam.name}</span>
      </div>

      {/* Main Content */}
      <div className="flex flex-1 mt-4 gap-4">
        {/* Home Team */}
        <div className="w-1/4 bg-gray-800 rounded-xl p-3 overflow-y-auto shadow-lg">
          <h2 className="text-center font-bold mb-2">{match.homeTeam.name}</h2>
          <table className="w-full text-sm">
            <thead>
              <tr className="text-gray-400 border-b border-gray-700">
                <th>#</th>
                <th>Pos</th>
                <th>Name</th>
                <th>G</th>
                <th>P</th>
              </tr>
            </thead>
            <tbody>
              {match.homeTeam.players.map((p, i) => (
                <tr key={i} className="border-b border-gray-700">
                  <td>{p.number}</td>
                  <td>{p.position}</td>
                  <td>{p.name}</td>
                  <td className="text-center">{p.stats.goals}</td>
                  <td className="text-center">{p.stats.passes}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        {/* Commentary */}
        <div className="w-2/4 bg-gray-900 rounded-xl p-4 flex flex-col shadow-inner">
          <h2 className="text-center font-bold text-xl mb-2">Live Commentary</h2>
          <div className="flex-1 overflow-y-auto space-y-2 text-gray-400">
            {/* Засега празно, по-натам ще връзваме live events */}
            <p>No events yet...</p>
          </div>
        </div>

        {/* Away Team */}
        <div className="w-1/4 bg-gray-800 rounded-xl p-3 overflow-y-auto shadow-lg">
          <h2 className="text-center font-bold mb-2">{match.awayTeam.name}</h2>
          <table className="w-full text-sm">
            <thead>
              <tr className="text-gray-400 border-b border-gray-700">
                <th>#</th>
                <th>Pos</th>
                <th>Name</th>
                <th>G</th>
                <th>P</th>
              </tr>
            </thead>
            <tbody>
              {match.awayTeam.players.map((p, i) => (
                <tr key={i} className="border-b border-gray-700">
                  <td>{p.number}</td>
                  <td>{p.position}</td>
                  <td>{p.name}</td>
                  <td className="text-center">{p.stats.goals}</td>
                  <td className="text-center">{p.stats.passes}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
