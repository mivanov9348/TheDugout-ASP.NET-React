import React, { useState, useEffect, useMemo } from "react";

const Squad = ({ gameSaveId }) => {
  const [players, setPlayers] = useState([]);
  const [teamName, setTeamName] = useState("");
  const [sortConfig, setSortConfig] = useState({ key: "name", direction: "asc" });

  const [search, setSearch] = useState("");
  const [positionFilter, setPositionFilter] = useState("");
  const [countryFilter, setCountryFilter] = useState("");

  useEffect(() => {
    if (!gameSaveId) return;
    const fetchSquad = async () => {
      try {
        const res = await fetch(`/api/team/by-save/${gameSaveId}`, { credentials: "include" });
        if (!res.ok) throw new Error("Грешка при зареждане на отбора");
        const data = await res.json();
        setPlayers(data.players);
        setTeamName(data.teamName);
      } catch (err) {
        console.error(err);
      }
    };
    fetchSquad();
  }, [gameSaveId]);

  // Филтриране + сортиране
  const filteredPlayers = useMemo(() => {
    let result = [...players];

    if (search) {
      result = result.filter((p) =>
        p.name.toLowerCase().includes(search.toLowerCase())
      );
    }
    if (positionFilter) {
      result = result.filter((p) => p.position === positionFilter);
    }
    if (countryFilter) {
      result = result.filter((p) => p.country === countryFilter);
    }

    result.sort((a, b) => {
      const { key, direction } = sortConfig;
      if (a[key] < b[key]) return direction === "asc" ? -1 : 1;
      if (a[key] > b[key]) return direction === "asc" ? 1 : -1;
      return 0;
    });

    return result;
  }, [players, search, positionFilter, countryFilter, sortConfig]);

  const sortPlayers = (key) => {
    let direction = "asc";
    if (sortConfig.key === key && sortConfig.direction === "asc") {
      direction = "desc";
    }
    setSortConfig({ key, direction });
  };

  const getSortIndicator = (key) =>
    sortConfig.key === key ? (sortConfig.direction === "asc" ? " ↑" : " ↓") : "";

  // уникални филтри
  const uniquePositions = [...new Set(players.map((p) => p.position))];
  const uniqueCountries = [...new Set(players.map((p) => p.country).filter(Boolean))];

  return (
    <div className="min-h-screen bg-gray-100 p-6">
      <div className="max-w-7xl mx-auto">
        <h1 className="text-3xl font-bold text-center mb-8 text-gray-800">
          {teamName ? `${teamName}` : "Зареждане..."}
        </h1>

        {/* Филтри */}
        <div className="flex flex-wrap gap-4 mb-6">
          <input
            type="text"
            placeholder="Търси по име..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="border rounded px-3 py-2"
          />

          <select
            value={positionFilter}
            onChange={(e) => setPositionFilter(e.target.value)}
            className="border rounded px-3 py-2"
          >
            <option value="">Всички позиции</option>
            {uniquePositions.map((pos) => (
              <option key={pos} value={pos}>
                {pos}
              </option>
            ))}
          </select>

          <select
            value={countryFilter}
            onChange={(e) => setCountryFilter(e.target.value)}
            className="border rounded px-3 py-2"
          >
            <option value="">Всички държави</option>
            {uniqueCountries.map((c) => (
              <option key={c} value={c}>
                {c}
              </option>
            ))}
          </select>
        </div>

        <div className="bg-white shadow-md rounded-lg overflow-x-auto">
          <table className="min-w-full divide-y divide-gray-200 text-sm">
            <thead className="bg-gray-50">
              <tr>
                <th onClick={() => sortPlayers("name")} className="px-4 py-2 cursor-pointer">Name {getSortIndicator("name")}</th>
                <th onClick={() => sortPlayers("position")} className="px-4 py-2 cursor-pointer">Position {getSortIndicator("position")}</th>
                <th onClick={() => sortPlayers("number")} className="px-4 py-2 cursor-pointer">Number {getSortIndicator("number")}</th>
                <th onClick={() => sortPlayers("age")} className="px-4 py-2 cursor-pointer">Age {getSortIndicator("age")}</th>
                <th className="px-4 py-2">Birth Date</th>
                <th onClick={() => sortPlayers("country")} className="px-4 py-2 cursor-pointer">Country {getSortIndicator("country")}</th>
                <th className="px-4 py-2">Height (cm)</th>
                <th className="px-4 py-2">Weight (kg)</th>
                <th className="px-4 py-2">Price</th>
                <th className="px-4 py-2">Active</th>
                <th className="px-4 py-2">Attributes</th>
                <th className="px-4 py-2">Season Stats</th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {filteredPlayers.map((p) => (
                <tr key={p.id} className="hover:bg-gray-50">
                  <td className="px-4 py-2">{p.name}</td>
                  <td className="px-4 py-2">{p.position}</td>
                  <td className="px-4 py-2">{p.number}</td>
                  <td className="px-4 py-2">{p.age}</td>
                  <td className="px-4 py-2">{p.birthDate}</td>
                  <td className="px-4 py-2">{p.country}</td>
                  <td className="px-4 py-2">{p.heightCm}</td>
                  <td className="px-4 py-2">{p.weightKg}</td>
                  <td className="px-4 py-2">{p.price}</td>
                  <td className="px-4 py-2">{p.isActive ? "✔️" : "❌"}</td>
                  <td className="px-4 py-2">
                    {p.attributes && p.attributes.length > 0 ? (
                      <ul className="list-disc list-inside">
                        {p.attributes.map((a) => (
                          <li key={a.attributeId}>{a.name}: {a.value}</li>
                        ))}
                      </ul>
                    ) : (
                      "-"
                    )}
                  </td>
                  <td className="px-4 py-2">
                    {p.seasonStats && p.seasonStats.length > 0 ? (
                      <ul className="list-disc list-inside">
                        {p.seasonStats.map((s, i) => (
                          <li key={i}>Season {s.seasonId}: {s.goals}G / {s.assists}A / {s.matchesPlayed}M</li>
                        ))}
                      </ul>
                    ) : (
                      "-"
                    )}
                  </td>
                </tr>
              ))}
              {filteredPlayers.length === 0 && (
                <tr>
                  <td colSpan="12" className="text-center py-6 text-gray-500">
                    Няма играчи, които да отговарят на филтъра.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
};

export default Squad;
