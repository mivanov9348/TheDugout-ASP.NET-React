import { useOutletContext } from "react-router-dom";
import TeamLogo from "../../../components/TeamLogo";

export default function Standings() {
  const { selectedLeague, myTeamId } = useOutletContext();

  if (!selectedLeague) return <p>Loading standings...</p>;

  return (
    <div>
      <h2 className="text-xl font-bold mb-4 text-sky-700">{selectedLeague.name} Standings</h2>
      <table className="w-full text-sm text-gray-700">
        <thead className="bg-sky-100 text-sky-800 text-sm uppercase">
          <tr>
            <th className="px-3 py-2 text-center">#</th>
            <th className="px-3 py-2 text-left">Team</th>
            <th className="px-3 py-2 text-center">M</th>
            <th className="px-3 py-2 text-center">W</th>
            <th className="px-3 py-2 text-center">D</th>
            <th className="px-3 py-2 text-center">L</th>
            <th className="px-3 py-2 text-center">Pts</th>
          </tr>
        </thead>
        <tbody>
          {selectedLeague.teams.map((team) => (
            <tr
              key={team.id}
              className={`${
                team.id === myTeamId
                  ? "bg-sky-100 border-l-4 border-sky-500"
                  : "hover:bg-sky-50"
              } transition`}
            >
              <td className="px-3 py-2 text-center">{team.ranking}</td>
              <td className="px-3 py-2 flex items-center gap-2">
                <TeamLogo
                  teamName={team.name}
                  logoFileName={team.logoFileName}
                  className="w-6 h-6"
                />
                {team.name}
              </td>
              <td className="px-3 py-2 text-center">{team.matches}</td>
              <td className="px-3 py-2 text-center">{team.wins}</td>
              <td className="px-3 py-2 text-center">{team.draws}</td>
              <td className="px-3 py-2 text-center">{team.losses}</td>
              <td className="px-3 py-2 text-center font-bold text-sky-700">
                {team.points}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
