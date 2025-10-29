import { useState } from "react";
import { ChevronUp, ChevronDown } from "lucide-react";
import { Link } from "react-router-dom";

export default function PlayerStats({ cup }) {
  const [sortConfig, setSortConfig] = useState({ key: "goals", direction: "desc" });

  if (!cup?.playerStats || cup.playerStats.length === 0)
    return (
      <div className="text-center text-gray-400 italic py-10 bg-gradient-to-b from-gray-900 via-gray-800 to-gray-900 rounded-2xl border border-gray-800 shadow-lg">
        ⚽ Loading...
      </div>
    );

  const sortedStats = [...cup.playerStats].sort((a, b) => {
    if (sortConfig.key === "goals") {
      return sortConfig.direction === "asc" ? a.goals - b.goals : b.goals - a.goals;
    }
    if (sortConfig.key === "name") {
      return sortConfig.direction === "asc"
        ? a.name.localeCompare(b.name)
        : b.name.localeCompare(a.name);
    }
    if (sortConfig.key === "teamName") {
      return sortConfig.direction === "asc"
        ? a.teamName.localeCompare(b.teamName)
        : b.teamName.localeCompare(a.teamName);
    }
    return 0;
  });

  const handleSort = (key) => {
    setSortConfig((prev) => ({
      key,
      direction: prev.key === key && prev.direction === "asc" ? "desc" : "asc",
    }));
  };

  const SortIcon = ({ column }) => {
    if (sortConfig.key !== column)
      return <span className="opacity-30 text-gray-500">↕</span>;
    return sortConfig.direction === "asc" ? (
      <ChevronUp className="inline w-4 h-4 ml-1 text-sky-400" />
    ) : (
      <ChevronDown className="inline w-4 h-4 ml-1 text-sky-400" />
    );
  };

  return (
    <div className="p-6 bg-gradient-to-b from-gray-900 via-gray-800 to-gray-900 rounded-2xl shadow-2xl border border-gray-800 max-w-4xl mx-auto text-gray-100">
      <h2 className="text-2xl font-bold text-sky-400 mb-6 text-center">Player Statistics</h2>

      <div className="overflow-x-auto">
        <table className="min-w-full text-sm text-gray-300 rounded-xl overflow-hidden">
          <thead>
            <tr className="bg-gray-800/70 uppercase text-xs tracking-wide text-gray-300">
              <th className="px-3 py-3 text-center w-10">#</th>
              <th
                className="px-3 py-3 text-left cursor-pointer hover:bg-gray-800/50 transition-colors"
                onClick={() => handleSort("name")}
              >
                Player <SortIcon column="name" />
              </th>
              <th
                className="px-3 py-3 text-left cursor-pointer hover:bg-gray-800/50 transition-colors"
                onClick={() => handleSort("teamName")}
              >
                Team <SortIcon column="teamName" />
              </th>
              <th
                className="px-3 py-3 text-center cursor-pointer hover:bg-gray-800/50 transition-colors"
                onClick={() => handleSort("goals")}
              >
                ⚽ Goals <SortIcon column="goals" />
              </th>
              <th className="px-3 py-3 text-center">🎯 Matches</th>
            </tr>
          </thead>

          <tbody>
            {sortedStats.map((p, i) => (
              <tr
                key={p.id}
                className={`transition-all duration-150 ${
                  i % 2 === 0 ? "bg-gray-900/60" : "bg-gray-800/60"
                } hover:bg-gray-700/60`}
              >
                <td className="px-3 py-3 text-center font-semibold text-gray-400">
                  {i + 1}
                </td>
                <td className="p-2 font-medium text-gray-200">
                  <Link
                    to={`/player/${p.id}`}
                    className="text-sky-400 hover:text-sky-300 transition-colors font-semibold hover:underline underline-offset-4"
                  >
                    {p.name}
                  </Link>
                </td>
                <td className="px-3 py-3 text-gray-400">{p.teamName}</td>
                <td className="px-3 py-3 text-center font-extrabold text-green-400">
                  {p.goals}
                </td>
                <td className="px-3 py-3 text-center text-gray-400">
                  {p.matches}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <div className="text-center mt-6 text-sm text-gray-500 italic">
        📊 Click a column to sort.
      </div>
    </div>
  );
}
