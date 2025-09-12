import { useEffect, useState } from "react";

export default function EuropeanCup({ gameSaveId, seasonId }) {
  const [cup, setCup] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!gameSaveId || !seasonId) return;

    const loadCup = async () => {
      try {
        const res = await fetch(
          `/api/EuropeanCup/current?gameSaveId=${gameSaveId}&seasonId=${seasonId}`,
          { credentials: "include" }
        );
        if (!res.ok) throw new Error("Error while loading European Cup");
        const data = await res.json();
        setCup(data);
      } catch (err) {
        console.error(err);
      } finally {
        setLoading(false);
      }
    };

    loadCup();
  }, [gameSaveId, seasonId]);

  if (loading) return <p>Loading...</p>;
  if (!cup) return <p>No European Cup for this season.</p>;

  return (
    <div className="p-4">
      {/* Title */}
      <h2 className="text-3xl font-bold mb-6 text-center">{cup.name}</h2>

      {/* Grid layout: standings on the left, fixtures on the right */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        {/* Standings */}
        <div className="bg-white shadow rounded-2xl p-4">
          <h3 className="text-xl font-semibold mb-3">Standings</h3>
          <table className="w-full text-sm border-collapse">
            <thead>
              <tr className="bg-slate-200">
                <th className="p-2 text-left">#</th>
                <th className="p-2 text-left">Team</th>
                <th className="p-2">P</th>
                <th className="p-2">W</th>
                <th className="p-2">D</th>
                <th className="p-2">L</th>
                <th className="p-2">GF</th>
                <th className="p-2">GA</th>
                <th className="p-2">GD</th>
                <th className="p-2">Pts</th>
              </tr>
            </thead>
            <tbody>
              {cup.standings.map((s) => (
                <tr key={s.teamId} className="border-b hover:bg-slate-50">
                  <td className="p-2">{s.ranking}</td>
                  <td className="p-2 flex items-center gap-2">
                    <img
                      src={`/logos/${s.logoFileName}`}
                      alt={s.name}
                      className="w-6 h-6 object-contain"
                    />
                    {s.name}
                  </td>
                  <td className="p-2 text-center">{s.matches}</td>
                  <td className="p-2 text-center">{s.wins}</td>
                  <td className="p-2 text-center">{s.draws}</td>
                  <td className="p-2 text-center">{s.losses}</td>
                  <td className="p-2 text-center">{s.goalsFor}</td>
                  <td className="p-2 text-center">{s.goalsAgainst}</td>
                  <td className="p-2 text-center">{s.goalDifference}</td>
                  <td className="p-2 text-center font-bold">{s.points}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        {/* Fixtures grouped by round */}
        <div className="bg-white shadow rounded-2xl p-4">
          <h3 className="text-xl font-semibold mb-3">Fixtures</h3>
          {cup.fixtures.map((round) => (
            <div key={round.round} className="mb-4">
              <h4 className="text-lg font-medium bg-slate-100 p-2 rounded">
                Round {round.round}
              </h4>
              <ul className="divide-y">
                {round.matches.map((m) => (
                  <li key={m.id} className="flex justify-between items-center p-2">
                    <div className="flex-1 text-right">{m.homeTeam}</div>
                    <div className="w-20 text-center font-semibold">
                      {m.homeTeamGoals != null && m.awayTeamGoals != null
                        ? `${m.homeTeamGoals} - ${m.awayTeamGoals}`
                        : "vs"}
                    </div>
                    <div className="flex-1">{m.awayTeam}</div>
                    <div className="ml-4 text-sm text-slate-500">
                      {new Date(m.date).toLocaleDateString()}
                    </div>
                  </li>
                ))}
              </ul>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
