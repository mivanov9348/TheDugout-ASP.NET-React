import { useState } from "react";
import { ChevronUp, ChevronDown } from "lucide-react";

export default function PlayerStats({ cup }) {
  const [sortConfig, setSortConfig] = useState({ key: "goals", direction: "desc" });

  if (!cup?.playerStats || cup.playerStats.length === 0)
    return <p className="text-center text-slate-500 italic">No player stats yet.</p>;

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
      direction:
        prev.key === key && prev.direction === "asc" ? "desc" : "asc",
    }));
  };

  const SortIcon = ({ column }) => {
    if (sortConfig.key !== column) return <span className="opacity-20">↕</span>;
    return sortConfig.direction === "asc" ? (
      <ChevronUp className="inline w-4 h-4 ml-1 text-slate-600" />
    ) : (
      <ChevronDown className="inline w-4 h-4 ml-1 text-slate-600" />
    );
  };

  return (
    <div className="bg-white shadow-xl rounded-2xl p-5 transition-all">     
      <table className="w-full text-sm border-collapse">
        <thead>
          <tr className="bg-slate-100 text-slate-700">
            <th className="p-2 text-left w-10">#</th>
            <th
              className="p-2 text-left cursor-pointer hover:bg-slate-200"
              onClick={() => handleSort("name")}
            >
              Player <SortIcon column="name" />
            </th>
            <th
              className="p-2 text-left cursor-pointer hover:bg-slate-200"
              onClick={() => handleSort("teamName")}
            >
              Team <SortIcon column="teamName" />
            </th>
            <th
              className="p-2 text-center cursor-pointer hover:bg-slate-200"
              onClick={() => handleSort("goals")}
            >
              Goals <SortIcon column="goals" />
            </th>
            <th className="p-2 text-center">Matches</th>
          </tr>
        </thead>
        <tbody>
          {sortedStats.map((p, i) => (
            <tr
              key={p.id}
              className={`border-b hover:bg-slate-50 transition ${
                i % 2 === 0 ? "bg-white" : "bg-slate-50/50"
              }`}
            >
              <td className="p-2 font-medium text-slate-600 text-center">
                {i + 1}
              </td>
              <td className="p-2 font-medium text-slate-800">{p.name}</td>
              <td className="p-2 text-slate-700">{p.teamName}</td>
              <td className="p-2 text-center font-semibold text-slate-900">
                {p.goals}
              </td>
              <td className="p-2 text-center text-slate-600">{p.matches}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
