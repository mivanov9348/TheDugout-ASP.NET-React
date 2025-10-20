import React, { useEffect, useState } from "react";

const TransferHistory = ({ gameSaveId }) => {
  const [transfers, setTransfers] = useState([]);
  const [onlyMine, setOnlyMine] = useState(false);
  const [sortConfig, setSortConfig] = useState({ key: null, direction: "asc" });

  const loadTransfers = async () => {
    if (!gameSaveId) return;
    try {
      const res = await fetch(
        `/api/transfers/history?gameSaveId=${gameSaveId}&onlyMine=${onlyMine}`
      );
      if (!res.ok) throw new Error("Failed to load transfers");
      const data = await res.json();
      setTransfers(data);
    } catch (err) {
      console.error("Error loading transfers", err);
    }
  };

  useEffect(() => {
    loadTransfers();
  }, [onlyMine, gameSaveId]);

  const handleSort = (key) => {
    setSortConfig((prev) => {
      if (prev.key === key) {
        return { key, direction: prev.direction === "asc" ? "desc" : "asc" };
      }
      return { key, direction: "asc" };
    });
  };

  const sortedTransfers = React.useMemo(() => {
    let sortable = [...transfers];
    if (sortConfig.key) {
      sortable.sort((a, b) => {
        let aVal = a[sortConfig.key];
        let bVal = b[sortConfig.key];

        // Date parsing
        if (sortConfig.key === "gameDate") {
          aVal = aVal ? new Date(aVal) : 0;
          bVal = bVal ? new Date(bVal) : 0;
        }

        // Fee parsing
        if (sortConfig.key === "fee") {
          aVal = aVal || 0;
          bVal = bVal || 0;
        }

        // String lowercase for consistency
        if (typeof aVal === "string") aVal = aVal.toLowerCase();
        if (typeof bVal === "string") bVal = bVal.toLowerCase();

        if (aVal < bVal) return sortConfig.direction === "asc" ? -1 : 1;
        if (aVal > bVal) return sortConfig.direction === "asc" ? 1 : -1;
        return 0;
      });
    }
    return sortable;
  }, [transfers, sortConfig]);

  return (
    <div className="p-4">
      <h2 className="text-xl font-bold mb-3">Transfer History</h2>

      <label className="flex items-center mb-3">
        <input
          type="checkbox"
          className="mr-2"
          checked={onlyMine}
          onChange={(e) => setOnlyMine(e.target.checked)}
        />
        Show only my transfers
      </label>

      <table className="table-auto border-collapse w-full text-sm">
        <thead>
          <tr className="bg-gray-100">
            <th className="p-2 border cursor-pointer" onClick={() => handleSort("gameDate")}>
              Date {sortConfig.key === "gameDate" && (sortConfig.direction === "asc" ? "▲" : "▼")}
            </th>
            <th className="p-2 border cursor-pointer" onClick={() => handleSort("player")}>
              Player {sortConfig.key === "player" && (sortConfig.direction === "asc" ? "▲" : "▼")}
            </th>
            <th className="p-2 border cursor-pointer" onClick={() => handleSort("fromTeam")}>
              From {sortConfig.key === "fromTeam" && (sortConfig.direction === "asc" ? "▲" : "▼")}
            </th>
            <th className="p-2 border cursor-pointer" onClick={() => handleSort("toTeam")}>
              To {sortConfig.key === "toTeam" && (sortConfig.direction === "asc" ? "▲" : "▼")}
            </th>
            <th className="p-2 border cursor-pointer" onClick={() => handleSort("fee")}>
              Fee {sortConfig.key === "fee" && (sortConfig.direction === "asc" ? "▲" : "▼")}
            </th>
            <th className="p-2 border cursor-pointer" onClick={() => handleSort("season")}>
              Season {sortConfig.key === "season" && (sortConfig.direction === "asc" ? "▲" : "▼")}
            </th>
          </tr>
        </thead>
        <tbody>
          {sortedTransfers.map((t) => (
            <tr key={t.id} className="hover:bg-gray-50">
              <td className="p-2 border">
                {t.gameDate ? new Date(t.gameDate).toLocaleDateString() : "-"}
              </td>
              <td className="p-2 border">{t.player}</td>
              <td className="p-2 border">{t.fromTeam}</td>
              <td className="p-2 border">{t.toTeam}</td>
              <td className="p-2 border">
                {t.fee ? `$${t.fee.toLocaleString()}` : "-"}
              </td>
              <td className="p-2 border">{t.season}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default TransferHistory;
