import React, { useState, useEffect, useMemo } from "react";
import { useNavigate } from "react-router-dom";

const Squad = ({ gameSaveId }) => {
  const [players, setPlayers] = useState([]);
  const [teamName, setTeamName] = useState("");
  const [sortConfig, setSortConfig] = useState({ key: "fullName", direction: "asc" });

  const [search, setSearch] = useState("");
  const [positionFilter, setPositionFilter] = useState("");
  const [countryFilter, setCountryFilter] = useState("");

  // Section visibility
  const [showInfo, setShowInfo] = useState(true);
  const [showAttributes, setShowAttributes] = useState(true);
  const [showStats, setShowStats] = useState(true);

  const navigate = useNavigate();

  const allAttributeNames = useMemo(() => {
    const names = new Set();
    players.forEach((p) => {
      p.attributes?.forEach((a) => names.add(a.name));
    });
    return Array.from(names);
  }, [players]);

  useEffect(() => {
    if (!gameSaveId) return;
    const fetchSquad = async () => {
      try {
        const res = await fetch(`/api/team/by-save/${gameSaveId}`, { credentials: "include" });
        if (!res.ok) throw new Error("Error while loading the team");
        const data = await res.json();
        setPlayers(data.players);
        setTeamName(data.teamName);
      } catch (err) {
        console.error(err);
      }
    };
    fetchSquad();
  }, [gameSaveId]);

  const filteredPlayers = useMemo(() => {
    let result = [...players];

    if (search) {
      result = result.filter((p) =>
        p.fullName.toLowerCase().includes(search.toLowerCase())
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

      let aValue = a[key];
      let bValue = b[key];

      if (aValue === undefined && bValue === undefined) {
        aValue = a.attributes?.find((x) => x.name === key)?.value ?? 0;
        bValue = b.attributes?.find((x) => x.name === key)?.value ?? 0;
      }

      if (aValue < bValue) return direction === "asc" ? -1 : 1;
      if (aValue > bValue) return direction === "asc" ? 1 : -1;
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

  const uniquePositions = [...new Set(players.map((p) => p.position))];
  const uniqueCountries = [...new Set(players.map((p) => p.country).filter(Boolean))];

  // Helper for formatting prices
  const formatPrice = (value) => {
    if (value == null) return "-";
    return value.toLocaleString("en-US");
  };

  return (
    <div className="min-h-screen bg-gray-100 p-6">
      <div className="max-w-7xl mx-auto">
        <h1 className="text-3xl font-bold text-center mb-8 text-gray-800">
          {teamName ? `${teamName}` : "Loading..."}
        </h1>

        {/* Filters */}
        <div className="flex flex-wrap gap-4 mb-6">
          <input
            type="text"
            placeholder="Search by name..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="border rounded px-3 py-2"
          />

          <select
            value={positionFilter}
            onChange={(e) => setPositionFilter(e.target.value)}
            className="border rounded px-3 py-2"
          >
            <option value="">All positions</option>
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
            <option value="">All countries</option>
            {uniqueCountries.map((c) => (
              <option key={c} value={c}>
                {c}
              </option>
            ))}
          </select>
        </div>

        {/* Section toggles */}
        <div className="flex gap-6 mb-4">
          <label className="flex items-center gap-2">
            <input type="checkbox" checked={showInfo} onChange={() => setShowInfo(!showInfo)} />
            Player Info
          </label>
          <label className="flex items-center gap-2">
            <input
              type="checkbox"
              checked={showAttributes}
              onChange={() => setShowAttributes(!showAttributes)}
            />
            Attributes
          </label>
          <label className="flex items-center gap-2">
            <input type="checkbox" checked={showStats} onChange={() => setShowStats(!showStats)} />
            Stats
          </label>
        </div>

        <div className="bg-white shadow-md rounded-lg overflow-x-auto">
          <table className="min-w-full divide-y divide-gray-200 text-sm">
            <thead>
              <tr className="bg-gray-50">
                <th
                  onClick={() => sortPlayers("fullName")}
                  className="px-4 py-2 cursor-pointer sticky left-0 bg-gray-50 z-10"
                >
                  Name {getSortIndicator("fullName")}
                </th>
                <th
                  onClick={() => sortPlayers("position")}
                  className="px-4 py-2 cursor-pointer sticky left-[120px] bg-gray-50 z-10"
                >
                  Position {getSortIndicator("position")}
                </th>

                {showInfo && (
                  <>
                    <th onClick={() => sortPlayers("age")} className="px-4 py-2 cursor-pointer">
                      Age {getSortIndicator("age")}
                    </th>
                    <th className="px-4 py-2">Country</th>
                    <th className="px-4 py-2">Height (cm)</th>
                    <th className="px-4 py-2">Weight (kg)</th>
                    <th className="px-4 py-2">Price</th>
                  </>
                )}

                {showAttributes &&
                  allAttributeNames.map((attr) => (
                    <th
                      key={attr}
                      onClick={() => sortPlayers(attr)}
                      className="px-4 py-2 cursor-pointer bg-gray-200"
                    >
                      {attr} {getSortIndicator(attr)}
                    </th>
                  ))}

                {showStats && (
                  <th className="px-4 py-2 bg-gray-100">Season Stats</th>
                )}
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {filteredPlayers.map((p) => (
                <tr key={p.id} 
                    className="hover:bg-gray-50"
                    onClick={() => navigate(`/player/${p.id}`)}>
                  <td className="px-4 py-2 sticky left-0 bg-white z-10">{p.fullName}</td>
                  <td className="px-4 py-2 sticky left-[120px] bg-white z-10">{p.position}</td>

                  {showInfo && (
                    <>
                      <td className="px-4 py-2">{p.age}</td>
                      <td className="px-4 py-2">{p.country}</td>
                      <td className="px-4 py-2">{p.heightCm}</td>
                      <td className="px-4 py-2">{p.weightKg}</td>
                      <td className="px-4 py-2">{formatPrice(p.price)}</td>
                    </>
                  )}

                  {showAttributes &&
                    allAttributeNames.map((attr) => {
                      const attribute = p.attributes?.find((a) => a.name === attr);
                      return (
                        <td key={attr} className="px-4 py-2 text-center bg-gray-200">
                          {attribute ? attribute.value : "-"}
                        </td>
                      );
                    })}

                  {showStats && (
                    <td className="px-4 py-2 bg-gray-100">
                      {p.seasonStats && p.seasonStats.length > 0 ? (
                        <ul className="list-disc list-inside">
                          {p.seasonStats.map((s, i) => (
                            <li key={i}>
                              Season {s.seasonId}: {s.goals}G / {s.assists}A / {s.matchesPlayed}M
                            </li>
                          ))}
                        </ul>
                      ) : (
                        "-"
                      )}
                    </td>
                  )}
                </tr>
              ))}
              {filteredPlayers.length === 0 && (
                <tr>
                  <td
                    colSpan={10 + allAttributeNames.length}
                    className="text-center py-6 text-gray-500"
                  >
                    No players match the filter.
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
