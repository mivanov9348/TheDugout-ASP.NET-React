import { useState, useEffect, useMemo } from "react";
import { ChevronUp, ChevronDown } from "lucide-react";
import { useOutletContext, Link } from "react-router-dom";

export default function CupPlayerStats() {
  const { selectedCup } = useOutletContext();
  const [playerStats, setPlayerStats] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [sortConfig, setSortConfig] = useState({ key: "goals", direction: "desc" });

  useEffect(() => {
    if (!selectedCup || !selectedCup.id) return;

    const fetchStats = async () => {
      setLoading(true);
      setError(null);
      try {
        const res = await fetch(`/api/cup/${selectedCup.id}/player-stats`, {
          credentials: "include",
        });

        if (!res.ok) {
          const text = await res.text();
          throw new Error(text || "Error loading player stats");
        }

        const data = await res.json();
        setPlayerStats(data);
      } catch (err) {
        console.error("Error fetching player stats:", err);
        setError(err.message);
      } finally {
        setLoading(false);
      }
    };

    fetchStats();
  }, [selectedCup?.id]);

  const sortedStats = useMemo(() => {
    const sorted = [...playerStats];
    const dir = sortConfig.direction === "asc" ? 1 : -1;
    sorted.sort((a, b) => {
      switch (sortConfig.key) {
        case "goals":
          return (a.goals - b.goals) * dir;
        case "matches":
          return (a.matches - b.matches) * dir;
        case "name":
          return a.name.localeCompare(b.name) * dir;
        case "teamName":
          return a.teamName.localeCompare(b.teamName) * dir;
        default:
          return 0;
      }
    });
    return sorted;
  }, [playerStats, sortConfig]);

  const handleSort = (key) => {
    setSortConfig((prev) => ({
      key,
      direction: prev.key === key && prev.direction === "asc" ? "desc" : "asc",
    }));
  };

  const SortIcon = ({ column }) => {
    if (sortConfig.key !== column) return <span className="opacity-20">↕</span>;
    return sortConfig.direction === "asc" ? (
      <ChevronUp className="inline w-4 h-4 ml-1 text-gray-400" />
    ) : (
      <ChevronDown className="inline w-4 h-4 ml-1 text-gray-400" />
    );
  };

  if (!selectedCup) {
    return <div className="text-gray-400 italic">Select a cup to see stats.</div>;
  }

  if (loading) {
    return <div className="text-gray-400">Loading player stats...</div>;
  }

  if (error) {
    return (
      <div className="bg-red-900/40 border border-red-700 text-red-300 p-4 rounded-xl">
        <p className="font-semibold">Failed to load stats:</p>
        <p>{error}</p>
      </div>
    );
  }

  if (!playerStats.length) {
    return (
      <div className="bg-gradient-to-br from-gray-900 via-gray-800 to-gray-900 rounded-2xl shadow-xl p-6 border border-gray-700">
        <h2 className="text-xl font-bold mb-4 text-gray-200">
          {selectedCup.templateName} – Top Scorers
        </h2>
        <p className="text-center text-gray-400 italic">
          No player statistics available for this cup.
        </p>
      </div>
    );
  }

  return (
    <div className="bg-gradient-to-br from-gray-900 via-gray-800 to-gray-900 rounded-2xl shadow-xl p-6 border border-gray-700">
    
      <table className="w-full text-sm border-collapse">
        <thead>
          <tr className="bg-gray-800 text-gray-300">
            <th className="p-3 text-left w-10">#</th>
            <th
              className="p-3 text-left cursor-pointer hover:bg-gray-700/50 transition"
              onClick={() => handleSort("name")}
            >
              Player <SortIcon column="name" />
            </th>
            <th
              className="p-3 text-left cursor-pointer hover:bg-gray-700/50 transition"
              onClick={() => handleSort("teamName")}
            >
              Team <SortIcon column="teamName" />
            </th>
            <th
              className="p-3 text-center cursor-pointer hover:bg-gray-700/50 transition"
              onClick={() => handleSort("goals")}
            >
              Goals <SortIcon column="goals" />
            </th>
            <th
              className="p-3 text-center cursor-pointer hover:bg-gray-700/50 transition"
              onClick={() => handleSort("matches")}
            >
              Matches <SortIcon column="matches" />
            </th>
          </tr>
        </thead>
        <tbody>
          {sortedStats.map((player, index) => (
            <tr
              key={player.id}
              className={`border-b border-gray-700 hover:bg-gray-700/30 transition ${
                index % 2 === 0 ? "bg-gray-900/50" : "bg-gray-800/50"
              }`}
            >
              <td className="p-3 font-medium text-gray-400 text-center">{index + 1}</td>
              <td className="p-3 font-semibold text-gray-100">
                <Link
                  to={`/player/${player.id}`}
                  className="text-blue-400 hover:text-blue-300 transition-colors hover:underline"
                >
                  {player.name}
                </Link>
              </td>
              <td className="p-3 text-gray-300">{player.teamName}</td>
              <td className="p-3 text-center font-bold text-gray-100">{player.goals}</td>
              <td className="p-3 text-center text-gray-400">{player.matches}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
