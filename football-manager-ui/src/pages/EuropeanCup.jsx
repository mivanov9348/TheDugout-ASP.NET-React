import { useEffect, useState } from "react";
import TeamLogo from "../components/TeamLogo";

export default function EuropeanCup({ gameSaveId, seasonId }) {
  const [cup, setCup] = useState(null);
  const [loading, setLoading] = useState(true);
  const [selectedRound, setSelectedRound] = useState(null);

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

        // по подразбиране избира първия рунд
        if (data?.fixtures?.length > 0) {
          setSelectedRound(data.fixtures[0].round);
        }
      } catch (err) {
        console.error(err);
      } finally {
        setLoading(false);
      }
    };

    loadCup();
  }, [gameSaveId, seasonId]);

  if (loading) return <p>Loading...</p>;
  if (!cup?.exists) return <p>No European Cup for this season.</p>;

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
                    <TeamLogo
                      teamName={s.name}
                      logoFileName={s.logoFileName}
                      className="w-6 h-6"
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

        {/* Fixtures with round selection */}
        <div className="bg-white shadow rounded-2xl p-4">
          <div className="flex justify-between items-center mb-3">
            <h3 className="text-xl font-semibold">Fixtures</h3>
            <select
              value={selectedRound ?? ""}
              onChange={(e) => setSelectedRound(Number(e.target.value))}
              className="border rounded px-2 py-1 text-sm"
            >
              {cup.fixtures.map((round) => (
                <option key={round.round} value={round.round}>
                  Round {round.round}
                </option>
              ))}
            </select>
          </div>

          {/* Matches for selected round */}
          {cup.fixtures
            .filter((r) => r.round === selectedRound)
            .map((round) => (
              <ul key={round.round} className="divide-y">
                {round.matches.map((m) => (
                  <li
                    key={m.id}
                    className="flex items-center justify-between p-2 hover:bg-slate-50 rounded"
                  >
                    <div className="flex-1 text-right pr-2 font-medium flex items-center justify-end gap-2">
                      <span>{m.homeTeam?.name}</span>
                      <TeamLogo
                        teamName={m.homeTeam?.name}
                        logoFileName={m.homeTeam?.logoFileName}
                      />
                    </div>
                    <div className="w-28 text-center font-bold">
                      {m.homeTeamGoals != null && m.awayTeamGoals != null
                        ? `${m.homeTeamGoals} : ${m.awayTeamGoals}`
                        : "vs"}
                    </div>
                    <div className="flex-1 pl-2 font-medium flex items-center gap-2">
                      <TeamLogo
                        teamName={m.awayTeam?.name}
                        logoFileName={m.awayTeam?.logoFileName}
                      />
                      <span>{m.awayTeam?.name}</span>
                    </div>
                    <div className="ml-4 text-xs text-slate-500 whitespace-nowrap">
                      {new Date(m.date).toLocaleDateString()}
                    </div>
                  </li>
                ))}
              </ul>
            ))}
        </div>
      </div>
    </div>
  );
}
