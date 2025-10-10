import { useState, useEffect, useMemo } from "react";
import { ChevronUp, ChevronDown } from "lucide-react";
import { useOutletContext } from "react-router-dom";
import { Link } from "react-router-dom";

export default function CupPlayerStats() {
  const { selectedCup } = useOutletContext();
  const [playerStats, setPlayerStats] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [sortConfig, setSortConfig] = useState({ key: "goals", direction: "desc" });

  useEffect(() => {
    // ако няма избрана купа, не правим заявка
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
  }, [selectedCup?.id]); // изрично следим ID на купата

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
      <ChevronUp className="inline w-4 h-4 ml-1 text-slate-600" />
    ) : (
      <ChevronDown className="inline w-4 h-4 ml-1 text-slate-600" />
    );
  };

  // различни състояния на UI
  if (!selectedCup) {
    return <div className="text-slate-500 italic">Select a cup to see stats.</div>;
  }

  if (loading) {
    return <div className="text-gray-500">Loading player stats...</div>;
  }

  if (error) {
    return (
      <div className="bg-red-100 border border-red-400 text-red-700 p-4 rounded-lg">
        <p className="font-semibold">Failed to load stats:</p>
        <p>{error}</p>
      </div>
    );
  }

  if (!playerStats.length) {
    return (
      <div className="bg-white shadow-xl rounded-2xl p-5">
        <h2 className="text-xl font-bold mb-4 text-sky-700">
          {selectedCup.templateName} – Top Scorers
        </h2>
        <p className="text-center text-slate-500 italic">
          No player statistics available for this cup.
        </p>
      </div>
    );
  }

  // таблица с резултати
  return (
    <div className="bg-white shadow-xl rounded-2xl p-5 transition-all">
      <h2 className="text-xl font-bold mb-4 text-sky-700">
        {selectedCup.templateName} – Top Scorers
      </h2>

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
            <th
              className="p-2 text-center cursor-pointer hover:bg-slate-200"
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
              className={`border-b hover:bg-slate-50 transition ${index % 2 === 0 ? "bg-white" : "bg-slate-50/50"
                }`}
            >
              <td className="p-2 font-medium text-slate-600 text-center">{index + 1}</td>
              <td className="p-2 font-medium text-slate-800">
                <Link
                  to={`/player/${player.id}`}
                  className="text-blue-500 hover:text-blue-700 transition-colors font-semibold hover:underline"
                >
                  {player.name}
                </Link>
              </td>              <td className="p-2 text-slate-700">{player.teamName}</td>
              <td className="p-2 text-center font-semibold text-slate-900">{player.goals}</td>
              <td className="p-2 text-center text-slate-600">{player.matches}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
