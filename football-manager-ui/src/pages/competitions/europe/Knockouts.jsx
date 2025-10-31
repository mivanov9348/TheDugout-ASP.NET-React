import TeamLogo from "../../../components/TeamLogo";

export default function Knockouts({ cup }) {
  if (!cup?.knockoutFixtures || cup.knockoutFixtures.length === 0)
    return (
      <div className="text-center text-slate-400 italic py-10 bg-gradient-to-b from-gray-900 to-gray-950 rounded-2xl shadow-xl border border-gray-800">
        ⚽ Все още няма елиминации.
      </div>
    );

  return (
    <div className="p-6 bg-gradient-to-b from-gray-900 to-gray-950 rounded-2xl shadow-2xl border border-gray-800  mx-auto">


      {cup.knockoutFixtures.map((round, i) => (
        <div
          key={round.round}
          className="mb-10 bg-gray-800/40 rounded-xl p-4 shadow-inner hover:shadow-green-500/10 transition-shadow"
        >
          <h4 className="text-xl font-bold mb-4 text-center text-green-400 uppercase tracking-wide">
            {round.name}
          </h4>

          <ul className="divide-y divide-gray-700">
            {round.matches.map((m) => (
              <li
                key={m.id}
                className="flex items-center justify-between p-3 rounded-lg hover:bg-gray-800/60 transition-all duration-200"
              >
                {/* 🏠 Home Team */}
                <div className="flex-1 text-right pr-2 font-medium flex items-center justify-end gap-2">
                  <span className="text-gray-200">{m.homeTeam?.name}</span>
                  <div className="w-7 h-7">
                    <TeamLogo
                      teamName={m.homeTeam?.name}
                      logoFileName={m.homeTeam?.logoFileName}
                    />
                  </div>
                </div>

                {/* ⚽ Result */}
                <div className="w-32 text-center font-extrabold text-lg text-white bg-gray-900/80 py-2 rounded-md shadow-inner border border-gray-700">
                  {m.homeTeamGoals != null && m.awayTeamGoals != null ? (
                    <>
                      <span
                        className={`${m.homeTeamGoals > m.awayTeamGoals
                            ? "text-green-400"
                            : m.homeTeamGoals < m.awayTeamGoals
                              ? "text-red-400"
                              : "text-yellow-400"
                          }`}
                      >
                        {m.homeTeamGoals}
                      </span>{" "}
                      :{" "}
                      <span
                        className={`${m.awayTeamGoals > m.homeTeamGoals
                            ? "text-green-400"
                            : m.awayTeamGoals < m.homeTeamGoals
                              ? "text-red-400"
                              : "text-yellow-400"
                          }`}
                      >
                        {m.awayTeamGoals}
                      </span>
                      {m.homeTeamGoals === m.awayTeamGoals &&
                        (m.homeTeamPenalties != null ||
                          m.awayTeamPenalties != null) && (
                          <span className="text-xs text-slate-400 block mt-1">
                            (pen. {m.homeTeamPenalties} : {m.awayTeamPenalties})
                          </span>
                        )}
                    </>
                  ) : (
                    <span className="text-slate-400 italic">vs</span>
                  )}
                </div>

                {/* 🏃 Away Team */}
                <div className="flex-1 pl-2 font-medium flex items-center gap-2 justify-start">
                  <div className="w-7 h-7">
                    <TeamLogo
                      teamName={m.awayTeam?.name}
                      logoFileName={m.awayTeam?.logoFileName}
                    />
                  </div>
                  <span className="text-gray-200">{m.awayTeam?.name}</span>
                </div>

                {/* 🗓 Date */}
                <div className="ml-4 text-xs text-slate-400 whitespace-nowrap">
                  {new Date(m.date).toLocaleDateString()}
                </div>
              </li>
            ))}
          </ul>
        </div>
      ))}

      <div className="text-center mt-8 text-sm text-slate-500 italic">
        ⚔️ Elimination Phase — {cup?.seasonName || "сезон"}
      </div>
    </div>
  );
}
