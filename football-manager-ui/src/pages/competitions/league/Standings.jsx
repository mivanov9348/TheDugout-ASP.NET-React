import { useOutletContext } from "react-router-dom";
import TeamLogo from "../../../components/TeamLogo";

export default function LeagueStandings() {
  const { league } = useOutletContext();

  const relegationSpots = 3; // последните 3 изпадат
  const promotionSpots = 3; 

  if (!league) return <p className="text-center text-gray-400 italic">No league selected.</p>;
  if (!league.standings || league.standings.length === 0)
    return <p className="text-center text-gray-400 italic">No standings to show.</p>;

  const totalTeams = league.standings.length;

  return (
    <div className="min-h-screen bg-gradient-to-b from-gray-900 via-gray-800 to-gray-900 text-gray-200 p-6 rounded-2xl shadow-2xl">
      <div className="bg-gray-800/60 backdrop-blur-sm border border-gray-700 rounded-2xl p-5 shadow-lg transition-all">
        <table className="w-full text-sm border-collapse">
          <thead>
            <tr className="bg-gray-700/60 text-gray-300 uppercase tracking-wider text-xs">
              <th className="p-2 text-left">#</th>
              <th className="p-2 text-left">Team</th>
              <th className="p-2 text-center">P</th>
              <th className="p-2 text-center">W</th>
              <th className="p-2 text-center">D</th>
              <th className="p-2 text-center">L</th>
              <th className="p-2 text-center">GF</th>
              <th className="p-2 text-center">GA</th>
              <th className="p-2 text-center">GD</th>
              <th className="p-2 text-center">Pts</th>
            </tr>
          </thead>
          <tbody>
            {league.standings.map((s) => (
              <tr
                key={s.teamId}
                className={`
                  border-b border-gray-700 transition
                  ${
                    s.ranking <= promotionSpots
                      ? "bg-green-900/30 hover:bg-green-800/50"
                      : s.ranking > totalTeams - relegationSpots
                      ? "bg-red-900/30 hover:bg-red-800/50"
                      : "hover:bg-gray-700/50"
                  }
                `}
              >
                <td className="p-2 text-center font-semibold text-gray-300">{s.ranking}</td>
                <td className="p-2 flex items-center gap-2">
                  <TeamLogo teamName={s.teamName} logoFileName={s.teamLogo} className="w-6 h-6" />
                  <span className="text-gray-100">{s.teamName}</span>
                </td>
                <td className="p-2 text-center">{s.matches}</td>
                <td className="p-2 text-center">{s.wins}</td>
                <td className="p-2 text-center">{s.draws}</td>
                <td className="p-2 text-center">{s.losses}</td>
                <td className="p-2 text-center">{s.goalsFor}</td>
                <td className="p-2 text-center">{s.goalsAgainst}</td>
                <td className="p-2 text-center">{s.goalDifference}</td>
                <td className="p-2 text-center font-bold text-white">{s.points}</td>
              </tr>
            ))}
          </tbody>
        </table>

        {/* Легенда */}
        <div className="mt-5 flex flex-col sm:flex-row gap-x-4 gap-y-2 text-xs text-gray-400">
          <div className="flex items-center gap-2">
            <span className="block w-3 h-3 bg-green-900 border border-green-600 rounded-sm"></span>
            <span>European Qualification</span>
          </div>
          <div className="flex items-center gap-2">
            <span className="block w-3 h-3 bg-red-900 border border-red-600 rounded-sm"></span>
            <span>Relegation Zone</span>
          </div>
        </div>
      </div>
    </div>
  );
}
