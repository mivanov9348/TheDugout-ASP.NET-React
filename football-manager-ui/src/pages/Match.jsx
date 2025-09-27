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

  const { home, away, minute } = match;

  return (
    <div className="min-h-screen bg-gradient-to-b from-gray-900 to-black text-white p-6 flex flex-col">
      {/* Start Button */}
      <div className="w-full flex justify-center mb-4">
        <button className="px-6 py-2 bg-sky-600 hover:bg-sky-500 rounded-xl shadow-md text-lg font-bold transition">
          Start
        </button>
      </div>

      {/* Scoreboard */}
      <div className="w-full flex flex-col items-center bg-gray-800 rounded-2xl shadow-lg p-4 border border-gray-700">
        <div className="w-full flex justify-between items-center text-2xl font-bold">
          <span className="truncate">{home.name}</span>
          <span className="text-4xl text-sky-400 drop-shadow-lg">
            {home.score} : {away.score}
          </span>
          <span className="truncate">{away.name}</span>
        </div>
        <div className="text-sm mt-2">
          <span className="px-3 py-1 rounded-full bg-gray-700 text-sky-300 font-semibold shadow-sm">
            {minute}&apos;
          </span>
        </div>
      </div>

      {/* Content */}
      <div className="flex flex-1 mt-6 gap-6">
        <TeamStats team={home} />
        <div className="w-2/4 bg-gray-900 rounded-xl p-4 flex flex-col shadow-inner border border-gray-700">
          <h2 className="text-center font-bold text-xl mb-3 text-sky-300">
            Live Commentary
          </h2>
          <div className="flex-1 overflow-y-auto space-y-2 text-gray-300">
            <p className="italic text-gray-500">No events yet...</p>
          </div>
        </div>
        <TeamStats team={away} />
      </div>
    </div>
  );
}

function TeamStats({ team }) {
  return (
    <div className="w-1/4 bg-gray-800 rounded-xl p-3 overflow-y-auto shadow-lg border border-gray-700">
      <h2 className="text-center font-bold text-lg mb-3 text-sky-400">
        {team.name}
      </h2>

      {/* Starters */}
      <h3 className="text-green-400 text-sm font-semibold mt-2 mb-1 uppercase tracking-wide">
        Starters
      </h3>
      <PlayerTable players={team.starters} />

      {/* Subs */}
      <h3 className="text-yellow-400 text-sm font-semibold mt-4 mb-1 uppercase tracking-wide">
        Substitutes
      </h3>
      <PlayerTable players={team.subs} />
    </div>
  );
}

function PlayerTable({ players }) {
  return (
    <table className="w-full text-xs md:text-sm mb-2 border-collapse">
      <thead>
        <tr className="text-gray-400 border-b border-gray-700 text-left">
          {players.some((p) => p.slot) && <th className="py-1 px-2">Slot</th>}
          <th className="py-1 px-2">#</th>
          <th className="py-1 px-2">Pos</th>
          <th className="py-1 px-2">Name</th>
          <th className="py-1 px-2 text-center">G</th>
          <th className="py-1 px-2 text-center">P</th>
          <th className="py-1 px-2 text-center">A</th>
        </tr>
      </thead>
      <tbody>
        {players.map((p, i) => (
          <tr
            key={i}
            className={`border-b border-gray-700 hover:bg-gray-700/40 transition ${
              i % 2 === 0 ? "bg-gray-800/40" : "bg-gray-900/40"
            }`}
          >
            {p.slot && <td className="py-1 px-2">{p.slot}</td>}
            <td className="py-1 px-2">{p.number}</td>
            <td className="py-1 px-2">{p.position}</td>
            <td className="py-1 px-2">{p.name}</td>
            <td className="py-1 px-2 text-center">{p.goals ?? 0}</td>
            <td className="py-1 px-2 text-center">{p.passes ?? 0}</td>
            <td className="py-1 px-2 text-center">{p.assists ?? 0}</td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}
