import { useState, useEffect, useRef } from "react";
import { useParams } from "react-router-dom";

export default function Match() {
  const { matchId } = useParams();
  const [match, setMatch] = useState(null);
  const [isRunning, setIsRunning] = useState(false);
  const intervalRef = useRef(null);

  // Load match initially
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
        const matchData = data.view;

        setMatch({
          home: {
            name: matchData.homeTeam?.name ?? "Unknown",
            score: matchData.score?.home ?? 0,
            starters: matchData.homeTeam?.starters || [],
            subs: matchData.homeTeam?.subs || [],
          },
          away: {
            name: matchData.awayTeam?.name ?? "Unknown",
            score: matchData.score?.away ?? 0,
            starters: matchData.awayTeam?.starters || [],
            subs: matchData.awayTeam?.subs || [],
          },
          minute: matchData.minute ?? 0,
          status: matchData.status ?? "Unknown",
          commentary:
            data.events?.map((e) => ({
              minute: e.minute,
              text: e.text,
              team: e.team,
              player: e.player,
              type: e.eventType,
              outcome: e.outcome,
            })) || [],
        });
      })

      .catch((err) => console.error("Error loading match", err));
  }, [matchId]);

  // Step function
  const playStep = async () => {
    try {
      const res = await fetch(`/api/matches/${matchId}/step`, {
        method: "POST",
        headers: {
          Authorization: `Bearer ${localStorage.getItem("token")}`,
        },
      });
      const data = await res.json();

      setMatch((prev) => {
        if (!prev) return prev;

        const newComment = data.matchEvent
          ? {
              minute: data.matchEvent.minute,
              text: data.matchEvent.description,
            }
          : null;

        return {
          ...prev,
          home: {
            ...prev.home,
            score: data.HomeScore ?? prev.home.score,
          },
          away: {
            ...prev.away,
            score: data.matchEvent?.awayScore ?? prev.away.score,
          },
          minute: data.minute,
          status: data.matchStatus,
          currentPlayerId: data.matchEvent?.playerId ?? null,
          commentary: data.events.map((e) => ({
            minute: e.minute,
            text: e.description,
          })),
        };
      });

      // Ако е свършил мача → стоп
      if (data.finished) {
        clearInterval(intervalRef.current);
        setIsRunning(false);
      }
    } catch (err) {
      console.error("Step error:", err);
      clearInterval(intervalRef.current);
      setIsRunning(false);
    }
  };

  // Start button logic
  const handleStart = () => {
    if (!isRunning) {
      setIsRunning(true);
      intervalRef.current = setInterval(playStep, 5000); // 5 сек на стъпка
    }
  };

  if (!match) {
    return (
      <div className="min-h-screen flex items-center justify-center text-white">
        Loading match...
      </div>
    );
  }

  const { home, away, minute, commentary } = match;

  return (
    <div className="min-h-screen bg-gradient-to-b from-gray-900 to-black text-white p-6 flex flex-col">
      {/* Control Buttons */}
      <div className="w-full flex justify-center gap-4 mb-4">
        {/* Start button */}
        <button
          onClick={handleStart}
          disabled={isRunning}
          className={`px-6 py-2 rounded-xl shadow-md text-lg font-bold transition ${
            isRunning
              ? "bg-gray-600 cursor-not-allowed"
              : "bg-sky-600 hover:bg-sky-500"
          }`}
        >
          {isRunning ? "Running..." : "Start"}
        </button>

        {/* Next Step button */}
        <button
          onClick={playStep}
          disabled={isRunning} // деактивиран ако тече автоматичен режим
          className={`px-6 py-2 rounded-xl shadow-md text-lg font-bold transition ${
            isRunning
              ? "bg-gray-600 cursor-not-allowed"
              : "bg-emerald-600 hover:bg-emerald-500"
          }`}
        >
          Next Step
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
        <TeamStats team={home} currentPlayerId={match.currentPlayerId} />

        <div className="w-2/4 bg-gray-900 rounded-xl p-4 flex flex-col shadow-inner border border-gray-700">
          <h2 className="text-center font-bold text-xl mb-3 text-sky-300">
            Live Commentary
          </h2>
          <div className="flex-1 overflow-y-auto space-y-3">
            {commentary?.length ? (
              commentary.map((c, i) => (
                <div
                  key={i}
                  className="flex items-start gap-3 bg-gray-800/50 rounded-lg p-2 hover:bg-gray-800/70 transition"
                >
                  <div className="flex-shrink-0">
                    <span className="flex items-center justify-center w-10 h-10 rounded-full bg-sky-700 text-sky-200 font-bold shadow">
                      {c.minute}'
                    </span>
                  </div>
                  <div className="flex-1">
                    <p className="text-sm text-gray-200 leading-snug">
                      {c.text}
                    </p>
                  </div>
                </div>
              ))
            ) : (
              <p className="italic text-gray-500">No events yet...</p>
            )}
          </div>
        </div>

        <TeamStats team={away} currentPlayerId={match.currentPlayerId} />
      </div>
    </div>
  );
}

function TeamStats({ team, currentPlayerId }) {
  return (
    <div className="w-1/4 bg-gray-800 rounded-xl p-3 overflow-y-auto shadow-lg border border-gray-700">
      <h2 className="text-center font-bold text-lg mb-3 text-sky-400">
        {team.name}
      </h2>
      <h3 className="text-green-400 text-sm font-semibold mt-2 mb-1 uppercase tracking-wide">
        Starters
      </h3>
      <PlayerTable players={team.starters} currentPlayerId={currentPlayerId} />
      <h3 className="text-yellow-400 text-sm font-semibold mt-4 mb-1 uppercase tracking-wide">
        Substitutes
      </h3>
      <PlayerTable players={team.subs} currentPlayerId={currentPlayerId} />
    </div>
  );
}

function PlayerTable({ players, currentPlayerId }) {
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
        {players.map((p, i) => {
          const isActive = p.id === currentPlayerId;
          return (
            <tr
              key={i}
              className={`border-b border-gray-700 hover:bg-gray-700/40 transition ${
                i % 2 === 0 ? "bg-gray-800/40" : "bg-gray-900/40"
              } ${
                isActive ? "bg-yellow-500/30 font-bold text-yellow-300" : ""
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
          );
        })}
      </tbody>
    </table>
  );
}
