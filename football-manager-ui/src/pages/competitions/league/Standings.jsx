import { useOutletContext } from "react-router-dom";
import TeamLogo from "../../../components/TeamLogo";

export default function LeagueStandings() {
  const { league } = useOutletContext();

  if (!league) return <p className="text-center text-slate-500 italic">No league selected.</p>;
  if (!league.standings || league.standings.length === 0)
    return <p className="text-center text-slate-500 italic">No standings to show.</p>;

  return (
    <div className="bg-white shadow-xl rounded-2xl p-5 transition-all">
      <table className="w-full text-sm border-collapse">
        <thead>
          <tr className="bg-slate-200 text-slate-700">
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
            <tr key={s.teamId} className="border-b hover:bg-slate-50 transition">
              <td className="p-2 font-medium text-slate-600 text-center">{s.ranking}</td>
              <td className="p-2 flex items-center gap-2">
                <TeamLogo teamName={s.teamName} logoFileName={s.teamLogo} className="w-6 h-6" />
                {s.teamName}
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
  );
}
