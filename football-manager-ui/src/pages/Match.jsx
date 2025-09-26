import { useState, useEffect } from "react";
import { useParams } from "react-router-dom";

export default function Match() {
  const { matchId } = useParams();
  const [match, setMatch] = useState(null);

  useEffect(() => {
    fetch(`/api/matches/${matchId}`, {
      headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
    })
      .then(async (res) => {
        if (!res.ok) {
          const text = await res.text();
          throw new Error(`Failed to load match: ${res.status} ${text}`);
        }
        return res.json();
      })
      .then((data) => {
        console.log("Match data:", data);

        setMatch({
          home: {
            name: data.homeTeam.name,
            score: data.score.home,
            starters: data.homeTeam.starters || [],
            subs: data.homeTeam.subs || [],
          },
          away: {
            name: data.awayTeam.name,
            score: data.score.away,
            starters: data.awayTeam.starters || [],
            subs: data.awayTeam.subs || [],
          },
          minute: data.minute,
          status: data.status,
        });
      })
      .catch((err) => console.error("Error loading match", err));
  }, [matchId]);

  if (!match) {
    return (
      <div className="min-h-screen flex items-center justify-center text-white">
        Loading match...
      </div>
    );
  }

  const { home, away, minute, status } = match;

  return (
    <div className="min-h-screen bg-gradient-to-b from-gray-900 to-black text-white p-4 flex flex-col">
      <div className="text-center mb-2 text-gray-400 italic">
        {status} - {minute}&apos;
      </div>

      {/* Scoreboard */}
      <div className="w-full flex justify-between items-center bg-gray-800 rounded-2xl shadow-lg p-4 text-2xl font-bold">
        <span>{home.name}</span>
        <span className="text-4xl">
          {home.score} : {away.score}
        </span>
        <span>{away.name}</span>
      </div>

      {/* Content */}
      <div className="flex flex-1 mt-4 gap-4">
        <TeamStats team={home} />
        <div className="w-2/4 bg-gray-900 rounded-xl p-4 flex flex-col shadow-inner">
          <h2 className="text-center font-bold text-xl mb-2">
            Live Commentary
          </h2>
          <div className="flex-1 overflow-y-auto space-y-2 text-gray-400">
            <p>No events yet...</p>
          </div>
        </div>
        <TeamStats team={away} />
      </div>
    </div>
  );
}

function TeamStats({ team }) {
  return (
    <div className="w-1/4 bg-gray-800 rounded-xl p-3 overflow-y-auto shadow-lg">
      <h2 className="text-center font-bold mb-2">{team.name}</h2>

      {/* Starters */}
      <h3 className="text-green-400 text-sm font-semibold mt-2 mb-1">
        Starters
      </h3>
      <PlayerTable players={team.starters} />

      {/* Subs */}
      <h3 className="text-yellow-400 text-sm font-semibold mt-4 mb-1">
        Substitutes
      </h3>
      <PlayerTable players={team.subs} />
    </div>
  );
}

function PlayerTable({ players }) {
  return (
    <table className="w-full text-sm mb-2">
      <thead>
        <tr className="text-gray-400 border-b border-gray-700">
          <th>#</th>
          <th>Pos</th>
          <th>Name</th>
        </tr>
      </thead>
      <tbody>
        {players.map((p, i) => (
          <tr key={i} className="border-b border-gray-700">
            <td>{p.number}</td>
            <td>{p.position}</td>
            <td>{p.name}</td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}
