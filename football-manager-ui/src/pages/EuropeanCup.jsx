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

          if (data?.groupFixtures?.length > 0) {
            setSelectedRound(data.groupFixtures[0].round);
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

    const competitionLogoUrl = cup.logoFileName
      ? `/competitionsLogos/${cup.logoFileName}`
      : "/competitionsLogos/default.png";

    // 🔹 Сортираме и филтрираме груповите мачове
    let roundsToShow =
      selectedRound != null
        ? cup.groupFixtures.filter((r) => r.round === selectedRound)
        : cup.groupFixtures;

    if (selectedRound == null) {
      roundsToShow = [...roundsToShow].sort((a, b) => a.round - b.round);
    }
    roundsToShow = roundsToShow.map((round) => ({
      ...round,
      matches: [...round.matches].sort(
        (a, b) => new Date(a.date).getTime() - new Date(b.date).getTime()
      ),
    }));

    return (
      <div className="p-4">
        {/* Title + Competition Logo */}
        <div className="flex items-center justify-center gap-4 mb-6">
          <img
            src={competitionLogoUrl}
            alt={cup.name}
            className="w-16 h-16 object-contain border rounded-full shadow-md"
            onError={(e) => {
              e.target.src = "/competitionsLogos/default.png";
            }}
          />
          <h2 className="text-3xl font-bold text-center">{cup.name}</h2>
        </div>

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
                {cup.standings.map((s) => {
                  let rowClass = "";

                  if (s.ranking >= 1 && s.ranking <= 8) {
                    rowClass = "bg-green-200"; // по-силно зелено
                  } else if (s.ranking >= 9 && s.ranking <= 24) {
                    rowClass = "bg-green-100"; // по-светло зелено
                  } else if (s.ranking >= 25 && s.ranking <= 36) {
                    rowClass = "bg-red-100"; // червено
                  }

                  return (
                    <tr
                      key={s.teamId}
                      className={`${rowClass} border-b hover:bg-slate-50`}
                    >
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
                  );
                })}
              </tbody>
            </table>
          </div>

          {/* Group Fixtures */}
          <div className="bg-white shadow rounded-2xl p-4">
            <div className="flex justify-between items-center mb-3">
              <h3 className="text-xl font-semibold">Fixtures</h3>
              <select
                value={selectedRound ?? ""}
                onChange={(e) =>
                  setSelectedRound(
                    e.target.value === "" ? null : Number(e.target.value)
                  )
                }
                className="border rounded px-2 py-1 text-sm"
                disabled={cup.groupFixtures.length === 0}
              >
                {[...new Set(cup.groupFixtures.map((round) => round.round))]
                  .sort((a, b) => a - b)
                  .map((roundNum) => (
                    <option key={roundNum} value={roundNum}>
                      Round {roundNum}
                    </option>
                  ))}
              </select>
            </div>

            {roundsToShow.length === 0 ? (
              <p className="text-center text-slate-500 italic">
                No matches scheduled yet.
              </p>
            ) : (
              roundsToShow.map((round) => (
                <div key={round.round} className="mb-4">
                  <h4 className="text-md font-bold mb-2">Round {round.round}</h4>
                  <ul className="divide-y">
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
                </div>
              ))
            )}
          </div>
        </div>

        {/* Knockout Stage BELOW */}
        <div className="mt-8 bg-white shadow rounded-2xl p-4">
          <h3 className="text-xl font-semibold mb-3">Knockout Stage</h3>

          {!cup.knockoutFixtures || cup.knockoutFixtures.length === 0 ? (
            <p className="text-center text-slate-500 italic">
              No eliminations yet. (Coming soon)
            </p>
          ) : (
            cup.knockoutFixtures.map((round) => (
              <div key={round.round} className="mb-6">
                <h4 className="text-md font-bold mb-2">{round.name}</h4>
                <ul className="divide-y">
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
              </div>
            ))
          )}
        </div>
      </div>
    );
  }
