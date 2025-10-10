import { useOutletContext } from "react-router-dom";
import React, { useEffect, useState } from "react";
import { ArrowUp, ArrowDown } from "lucide-react";
import { Link } from "react-router-dom";

const LeaguePlayerStats = () => {
  const { gameSaveId, league } = useOutletContext();

  const [playerStats, setPlayerStats] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [sortField, setSortField] = useState("goals");
  const [sortDirection, setSortDirection] = useState("desc");

  useEffect(() => {
    if (!gameSaveId || !league?.id) return;

    const fetchStats = async () => {
      setLoading(true);
      setError(null);
      try {
        const res = await fetch(`/api/League/${gameSaveId}/${league.id}/top-scorers`, {
          credentials: "include",
        });
        if (!res.ok) throw new Error("Failed to fetch stats");
        const data = await res.json();
        setPlayerStats(Array.isArray(data) ? data : []);
      } catch (err) {
        setError("Неуспешно зареждане на статистиката.");
      } finally {
        setLoading(false);
      }
    };

    fetchStats();
  }, [gameSaveId, league?.id]);

  const handleSort = (field) => {
    if (sortField === field) {
      setSortDirection(sortDirection === "asc" ? "desc" : "asc");
    } else {
      setSortField(field);
      setSortDirection("asc");
    }
  };

  const sortedStats = [...playerStats].sort((a, b) => {
    let valA = a[sortField];
    let valB = b[sortField];
    if (typeof valA === "string") valA = valA.toLowerCase();
    if (typeof valB === "string") valB = valB.toLowerCase();

    if (valA < valB) return sortDirection === "asc" ? -1 : 1;
    if (valA > valB) return sortDirection === "asc" ? 1 : -1;
    return 0;
  });

  const SortIcon = ({ field }) => {
    if (sortField !== field) return <ArrowUp className="opacity-0" size={14} />;
    return sortDirection === "asc" ? (
      <ArrowUp size={14} className="text-green-400" />
    ) : (
      <ArrowDown size={14} className="text-green-400" />
    );
  };

  // === Loading / Error States ===
  if (loading)
    return (
      <div className="flex justify-center items-center h-32 text-gray-400 animate-pulse">
        ⏳ Зареждане на статистиката...
      </div>
    );

  if (error)
    return (
      <div className="text-red-500 text-center font-semibold mt-6">{error}</div>
    );

  if (playerStats.length === 0)
    return (
      <p className="text-gray-500 italic text-center mt-6">
        Няма налични данни за голмайстори.
      </p>
    );

  // === Table ===
  return (
    <div className="p-6 bg-gradient-to-b from-gray-900 to-gray-950 rounded-2xl shadow-2xl border border-gray-800 max-w-5xl mx-auto">


      <div className="overflow-x-auto">
        <table className="min-w-full border-separate border-spacing-y-1">
          <thead>
            <tr className="bg-gray-800/80 text-gray-300 text-sm uppercase tracking-wider">
              <th className="px-4 py-3 text-left w-12">#</th>
              <th className="px-4 py-3 text-left">Player</th>
              <th
                className="px-4 py-3 text-left cursor-pointer hover:text-white transition-colors"
                onClick={() => handleSort("teamName")}
              >
                <div className="flex items-center gap-2">
                  Team
                  <SortIcon field="teamName" />
                </div>
              </th>
              <th
                className="px-4 py-3 text-center cursor-pointer hover:text-white transition-colors"
                onClick={() => handleSort("goals")}
              >
                <div className="flex items-center justify-center gap-2">
                  Goals
                  <SortIcon field="goals" />
                </div>
              </th>
              <th className="px-4 py-3 text-center">Matches</th>
            </tr>
          </thead>

          <tbody>
            {sortedStats.map((p, index) => (
              <tr
                key={p.id || index}
                className="bg-gray-800/60 hover:bg-gray-700 transition-all duration-150 rounded-lg"
              >
                <td className="px-4 py-3 text-gray-400 text-center">
                  {index + 1}
                </td>
                <td className="p-2 font-medium text-slate-800">
                  <Link
                    to={`/player/${p.id}`}
                    className="text-blue-500 hover:text-blue-700 transition-colors font-semibold hover:underline"
                  >
                    {p.name}
                  </Link>
                </td>
                <td className="px-4 py-3 text-gray-300">{p.teamName}</td>
                <td className="px-4 py-3 text-center font-bold text-green-400 text-lg">
                  {p.goals}
                </td>
                <td className="px-4 py-3 text-center text-gray-300">
                  {p.matches}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Decorative footer */}
      <div className="text-center mt-6 text-sm text-gray-500 italic">
        Данните са за текущия сезон ({league.country})
      </div>
    </div>
  );
};

export default LeaguePlayerStats;
