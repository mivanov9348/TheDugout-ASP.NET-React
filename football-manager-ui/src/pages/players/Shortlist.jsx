import React, { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { ChevronUp, ChevronDown } from "lucide-react";

export default function Shortlist({ gameSaveId }) {
  const [players, setPlayers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [sortConfig, setSortConfig] = useState({ key: null, direction: "asc" });

  useEffect(() => {
    if (!gameSaveId) return;

    const fetchShortlist = async () => {
      try {
        const response = await fetch(`/api/player/GetShortlist?gameSaveId=${gameSaveId}`);
        if (!response.ok) throw new Error("Failed to load shortlist.");
        const data = await response.json();
        setPlayers(data);
      } catch (err) {
        console.error(err);
        setError(err.message);
      } finally {
        setLoading(false);
      }
    };

    fetchShortlist();
  }, [gameSaveId]);

  // --- сортиране ---
  const sortedPlayers = React.useMemo(() => {
    let sortable = [...players];
    if (sortConfig.key !== null) {
      sortable.sort((a, b) => {
        const valA = a[sortConfig.key];
        const valB = b[sortConfig.key];

        // Проверка за текст vs число
        if (typeof valA === "string" && typeof valB === "string") {
          return sortConfig.direction === "asc"
            ? valA.localeCompare(valB)
            : valB.localeCompare(valA);
        } else {
          return sortConfig.direction === "asc"
            ? (valA ?? 0) - (valB ?? 0)
            : (valB ?? 0) - (valA ?? 0);
        }
      });
    }
    return sortable;
  }, [players, sortConfig]);

  const handleSort = (key) => {
    setSortConfig((prev) => {
      if (prev.key === key && prev.direction === "asc") {
        return { key, direction: "desc" };
      } else {
        return { key, direction: "asc" };
      }
    });
  };

  const renderSortIcon = (key) => {
    if (sortConfig.key !== key) return null;
    return sortConfig.direction === "asc" ? (
      <ChevronUp className="inline w-4 h-4 ml-1 text-gray-400" />
    ) : (
      <ChevronDown className="inline w-4 h-4 ml-1 text-gray-400" />
    );
  };

  if (loading) return <div className="text-gray-300 p-4">Loading shortlist...</div>;
  if (error) return <div className="text-red-400 p-4">{error}</div>;
  if (players.length === 0) return <div className="text-gray-400 p-4">No players in shortlist.</div>;

  return (
    <div className="p-4">
      <h2 className="text-2xl font-bold mb-4">My Shortlist</h2>

      <div className="overflow-x-auto rounded-2xl border border-gray-700 bg-gray-800">
        <table className="min-w-full text-sm text-gray-300">
          <thead className="bg-gray-700 text-gray-200 text-left select-none">
            <tr>
              <th className="px-4 py-3 cursor-pointer" onClick={() => handleSort("lastName")}>
                Name {renderSortIcon("lastName")}
              </th>
              <th className="px-4 py-3 cursor-pointer" onClick={() => handleSort("country")}>
                Country {renderSortIcon("country")}
              </th>
              <th className="px-4 py-3 cursor-pointer" onClick={() => handleSort("position")}>
                Position {renderSortIcon("position")}
              </th>
              <th className="px-4 py-3 cursor-pointer" onClick={() => handleSort("teamName")}>
                Team {renderSortIcon("teamName")}
              </th>
              <th className="px-4 py-3 cursor-pointer" onClick={() => handleSort("age")}>
                Age {renderSortIcon("age")}
              </th>
              <th className="px-4 py-3 cursor-pointer" onClick={() => handleSort("currentAbility")}>
                CA {renderSortIcon("currentAbility")}
              </th>
              <th className="px-4 py-3 cursor-pointer" onClick={() => handleSort("potentialAbility")}>
                PA {renderSortIcon("potentialAbility")}
              </th>
              <th className="px-4 py-3 cursor-pointer" onClick={() => handleSort("price")}>
                Price (€) {renderSortIcon("price")}
              </th>
              <th className="px-4 py-3 cursor-pointer" onClick={() => handleSort("heightCm")}>
                Height (cm) {renderSortIcon("heightCm")}
              </th>
              <th className="px-4 py-3 cursor-pointer" onClick={() => handleSort("weightKg")}>
                Weight (kg) {renderSortIcon("weightKg")}
              </th>
              <th className="px-4 py-3 text-right">Actions</th>
            </tr>
          </thead>

          <tbody>
            {sortedPlayers.map((player) => (
              <tr
                key={player.id}
                className="border-t border-gray-700 hover:bg-gray-750 transition-colors"
              >
                <td className="px-4 py-3">
                  <Link
                    to={`/player/${player.id}?gameSaveId=${gameSaveId}`}
                    className="text-blue-400 hover:text-blue-300 font-medium"
                  >
                    {player.firstName} {player.lastName}
                  </Link>
                </td>
                <td className="px-4 py-3">{player.country || "-"}</td>
                <td className="px-4 py-3">{player.position || "-"}</td>
                <td className="px-4 py-3">{player.teamName || "Free Agent"}</td>
                <td className="px-4 py-3">{player.age}</td>
                <td className="px-4 py-3">{player.currentAbility}</td>
                <td className="px-4 py-3">{player.potentialAbility}</td>
                <td className="px-4 py-3">{player.price?.toLocaleString() || "-"}</td>
                <td className="px-4 py-3">{player.heightCm}</td>
                <td className="px-4 py-3">{player.weightKg}</td>
                <td className="px-4 py-3 text-right">
                  <button
                    onClick={async () => {
                      await fetch(`/api/player/${player.id}/shortlist?gameSaveId=${gameSaveId}`, {
                        method: "DELETE",
                      });
                      setPlayers(players.filter((p) => p.id !== player.id));
                    }}
                    className="px-3 py-1 bg-red-600 hover:bg-red-700 rounded-lg text-white transition-all"
                  >
                    Remove
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
