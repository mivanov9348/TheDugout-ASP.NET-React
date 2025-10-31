import TeamLogo from "../../../components/TeamLogo";
import { useState, useEffect } from "react";
import { Link } from "react-router-dom";

export default function GroupStage({ cup }) {
  const [selectedRound, setSelectedRound] = useState(null);

  useEffect(() => {
    if (cup?.groupFixtures?.length > 0) {
      const firstRound = Math.min(...cup.groupFixtures.map((r) => r.round));
      setSelectedRound(firstRound);
    }
  }, [cup]);

  if (!cup?.groupFixtures || cup.groupFixtures.length === 0)
    return <p>No group fixtures yet.</p>;

  let roundsToShow = cup.groupFixtures.filter(
    (r) => r.round === selectedRound
  );

  roundsToShow = roundsToShow.map((round) => ({
    ...round,
    matches: [...round.matches].sort(
      (a, b) => new Date(a.date).getTime() - new Date(b.date).getTime()
    ),
  }));

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
      {/* Standings */}
      <div className="bg-gradient-to-b from-gray-900 via-gray-800 to-gray-900 shadow rounded-2xl p-4 text-gray-100">
        <h3 className="text-xl font-semibold mb-3 text-sky-400">Standings</h3>
        <table className="w-full text-sm border-collapse">
          <thead>
            <tr className="bg-gray-800">
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
            {cup.standings.map((s) => {
              let rowClass = "";
              if (s.ranking >= 1 && s.ranking <= 8) rowClass = "bg-green-800/40";
              else if (s.ranking <= 24) rowClass = "bg-green-700/20";
              else if (s.ranking <= 36) rowClass = "bg-red-700/20";

              return (
                <tr key={s.teamId} className={`${rowClass} border-b border-gray-700`}>
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
                  <td className="p-2 text-center font-bold text-sky-400">{s.points}</td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>

      {/* Fixtures */}
      <div className="bg-gradient-to-b from-gray-900 via-gray-800 to-gray-900 shadow rounded-2xl p-4 text-gray-100">
        <div className="flex justify-between items-center mb-3">
          <h3 className="text-xl font-semibold text-sky-400">Fixtures</h3>
          <select
            value={selectedRound ?? ""}
            onChange={(e) =>
              setSelectedRound(
                e.target.value === "" ? null : Number(e.target.value)
              )
            }
            className="border border-gray-700 bg-gray-800 rounded px-2 py-1 text-sm text-gray-100"
          >
            {[...new Set(cup.groupFixtures.map((r) => r.round))]
              .sort((a, b) => a - b)
              .map((roundNum) => (
                <option key={roundNum} value={roundNum} className="bg-gray-900">
                  Round {roundNum}
                </option>
              ))}
          </select>
        </div>

        {roundsToShow.map((round) => (
          <div key={round.round} className="mb-4">
            <h4 className="text-md font-bold mb-2 text-gray-300">Round {round.round}</h4>
            <ul className="divide-y divide-gray-700">
              {round.matches.map((m) => (
                <li
                  key={m.id}
                  className="p-3 rounded hover:bg-gray-800/70 transition"
                >
                  <div className="flex items-center justify-between mb-2">
                    <div className="flex-1 text-right pr-2 font-medium flex items-center justify-end gap-2">
                      <span>{m.homeTeam?.name}</span>
                      <TeamLogo
                        teamName={m.homeTeam?.name}
                        logoFileName={m.homeTeam?.logoFileName}
                      />
                    </div>
                    <div className="w-24 text-center font-bold text-sky-400">
                      <Link
                        to={`/match/${m.id}`}
                        className="hover:text-sky-300 underline decoration-dotted"
                      >
                        {m.homeTeamGoals != null && m.awayTeamGoals != null
                          ? `${m.homeTeamGoals} : ${m.awayTeamGoals}`
                          : "vs"}
                      </Link>
                    </div>

                    <div className="flex-1 pl-2 font-medium flex items-center gap-2">
                      <TeamLogo
                        teamName={m.awayTeam?.name}
                        logoFileName={m.awayTeam?.logoFileName}
                      />
                      <span>{m.awayTeam?.name}</span>
                    </div>
                    <div className="ml-4 text-xs text-gray-500 whitespace-nowrap">
                      {new Date(m.date).toLocaleDateString()}
                    </div>
                  </div>

                  {m.goalscorers && m.goalscorers.length > 0 && (
                    <div className="mt-2 text-xs text-gray-400">
                      <div className="grid grid-cols-2 gap-2">
                        <div className="text-right">
                          {m.goalscorers
                            .filter(goal => goal.teamId === m.homeTeam?.id)
                            .map((goal, index) => (
                              <div key={index} className="font-mono">
                                {goal.playerName} {goal.minute}'
                              </div>
                            ))}
                        </div>

                        <div className="text-left">
                          {m.goalscorers
                            .filter(goal => goal.teamId === m.awayTeam?.id)
                            .map((goal, index) => (
                              <div key={index} className="font-mono">
                                {goal.minute}' {goal.playerName}
                              </div>
                            ))}
                        </div>
                      </div>
                    </div>
                  )}
                </li>
              ))}
            </ul>
          </div>
        ))}
      </div>
    </div>

  );
}