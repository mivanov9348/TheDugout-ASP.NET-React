import React from "react";

const Standings = ({ standings }) => {
  if (!standings || standings.length === 0)
    return (
      <p className="text-gray-400 text-center mt-4">
        Няма налично класиране за тази лига.
      </p>
    );

  return (
    <div className="overflow-x-auto">
      <table className="w-full text-sm text-gray-300">
        <thead className="bg-gray-800 text-gray-200 uppercase text-xs">
          <tr>
            <th className="px-3 py-2 text-left">#</th>
            <th className="px-3 py-2 text-left">Отбор</th>
            <th className="px-3 py-2 text-center">М</th>
            <th className="px-3 py-2 text-center">П</th>
            <th className="px-3 py-2 text-center">Р</th>
            <th className="px-3 py-2 text-center">З</th>
            <th className="px-3 py-2 text-center">ГВ</th>
            <th className="px-3 py-2 text-center">ГП</th>
            <th className="px-3 py-2 text-center">ГР</th>
            <th className="px-3 py-2 text-center">Т</th>
          </tr>
        </thead>
        <tbody>
          {standings.map((team) => (
            <tr
              key={team.TeamId}
              className="border-b border-gray-700 hover:bg-gray-800"
            >
              <td className="px-3 py-2 text-center">{team.Ranking}</td>
              <td className="px-3 py-2 flex items-center gap-2">
                <img
                  src={`/images/logos/${team.TeamLogo}`}
                  alt={team.TeamName}
                  className="w-6 h-6 rounded"
                />
                {team.TeamName}
              </td>
              <td className="px-3 py-2 text-center">{team.Matches}</td>
              <td className="px-3 py-2 text-center">{team.Wins}</td>
              <td className="px-3 py-2 text-center">{team.Draws}</td>
              <td className="px-3 py-2 text-center">{team.Losses}</td>
              <td className="px-3 py-2 text-center">{team.GoalsFor}</td>
              <td className="px-3 py-2 text-center">{team.GoalsAgainst}</td>
              <td className="px-3 py-2 text-center">
                {team.GoalDifference >= 0 ? "+" : ""}
                {team.GoalDifference}
              </td>
              <td className="px-3 py-2 text-center font-bold text-sky-400">
                {team.Points}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default Standings;
