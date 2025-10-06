import TeamLogo from "../../../components/TeamLogo";

export default function Knockouts({ cup }) {
  if (!cup?.knockoutFixtures || cup.knockoutFixtures.length === 0)
    return <p>No eliminations yet.</p>;

  return (
    <div className="bg-white shadow rounded-2xl p-4">
      {cup.knockoutFixtures.map((round) => (
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
                  {m.homeTeamGoals != null && m.awayTeamGoals != null ? (
                    <>
                      {m.homeTeamGoals} : {m.awayTeamGoals}
                      {m.homeTeamGoals === m.awayTeamGoals &&
                        (m.homeTeamPenalties != null ||
                          m.awayTeamPenalties != null) && (
                          <span className="text-sm text-slate-500">
                            {" "}
                            ({m.homeTeamPenalties} : {m.awayTeamPenalties})
                          </span>
                        )}
                    </>
                  ) : (
                    "vs"
                  )}
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
      ))}
    </div>
  );
}
