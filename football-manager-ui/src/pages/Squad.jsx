// src/pages/Squad.jsx
import React, { useState, useEffect, useMemo } from "react";
import { useNavigate } from "react-router-dom";
import { Search, Filter, Flag, User } from "lucide-react";

const Squad = ({ gameSaveId }) => {
  const [players, setPlayers] = useState([]);
  const [teamName, setTeamName] = useState("");
  const [sortConfig, setSortConfig] = useState({ key: "fullName", direction: "asc" });

  const [search, setSearch] = useState("");
  const [positionFilter, setPositionFilter] = useState("");
  const [countryFilter, setCountryFilter] = useState("");

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
      result = result.filter((p) => p.fullName.toLowerCase().includes(search.toLowerCase()));
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
    sortConfig.key === key ? (
      <span className="ml-1 text-xs text-gray-500">
        {sortConfig.direction === "asc" ? "▲" : "▼"}
      </span>
    ) : null;

  const uniquePositions = [...new Set(players.map((p) => p.position))];
  const uniqueCountries = [...new Set(players.map((p) => p.country).filter(Boolean))];

  const formatPrice = (value) => {
    if (value == null) return "-";
    return new Intl.NumberFormat("en-US", {
      style: "currency",
      currency: "EUR",
      maximumFractionDigits: 0,
    }).format(value);
  };

  const getAttributeColor = (val) => {
    if (val >= 80) return "text-emerald-600 font-semibold";
    if (val >= 60) return "text-amber-600 font-medium";
    return "text-rose-600";
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-50 to-blue-50 p-4 md:p-6">
      <div className="max-w-7xl mx-auto">
        {/* Team Header */}
        <div className="text-center mb-8">
          <h1 className="text-4xl md:text-5xl font-extrabold bg-gradient-to-r from-blue-600 to-sky-500 bg-clip-text text-transparent mb-2">
            {teamName || "Loading Squad..."}
          </h1>
          <p className="text-gray-500 text-lg">Click any player to view full profile</p>
        </div>

        {/* Filters Card */}
        <div className="bg-white shadow-xl rounded-2xl p-5 mb-6 border border-gray-100">
          <div className="flex flex-wrap gap-4 items-center">
            <div className="relative flex-1 min-w-[280px]">
              <div className="absolute inset-y-0 left-0 flex items-center pl-3 pointer-events-none">
                <Search className="w-5 h-5 text-gray-400" />
              </div>
              <input
                type="text"
                placeholder="Search by name..."
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                className="w-full pl-10 pr-4 py-3 border border-gray-200 rounded-xl focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all outline-none shadow-sm"
              />
            </div>

            <div className="flex gap-3 flex-wrap">
              <div className="relative">
                <select
                  value={positionFilter}
                  onChange={(e) => setPositionFilter(e.target.value)}
                  className="appearance-none bg-white border border-gray-200 rounded-xl px-4 py-3 pr-8 focus:ring-2 focus:ring-blue-500 focus:border-transparent cursor-pointer shadow-sm"
                >
                  <option value="">All Positions</option>
                  {uniquePositions.map((pos) => (
                    <option key={pos} value={pos}>
                      {pos}
                    </option>
                  ))}
                </select>
                <div className="absolute inset-y-0 right-0 flex items-center pr-3 pointer-events-none">
                  <Filter className="w-4 h-4 text-gray-400" />
                </div>
              </div>

              <div className="relative">
                <select
                  value={countryFilter}
                  onChange={(e) => setCountryFilter(e.target.value)}
                  className="appearance-none bg-white border border-gray-200 rounded-xl px-4 py-3 pr-8 focus:ring-2 focus:ring-blue-500 focus:border-transparent cursor-pointer shadow-sm"
                >
                  <option value="">All Countries</option>
                  {uniqueCountries.map((c) => (
                    <option key={c} value={c}>
                      {c}
                    </option>
                  ))}
                </select>
                <div className="absolute inset-y-0 right-0 flex items-center pr-3 pointer-events-none">
                  <Flag className="w-4 h-4 text-gray-400" />
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* Section toggles - Modern Pills */}
        <div className="flex flex-wrap gap-3 mb-6 justify-center">
          {[
            { label: "Player Info", checked: showInfo, setter: setShowInfo },
            { label: "Attributes", checked: showAttributes, setter: setShowAttributes },
            { label: "Stats", checked: showStats, setter: setShowStats },
          ].map((toggle, idx) => (
            <label
              key={idx}
              className={`flex items-center gap-2 px-4 py-2 rounded-full text-sm font-medium cursor-pointer transition-all duration-200 border-2 ${
                toggle.checked
                  ? "bg-blue-500 text-white border-blue-500 shadow-md"
                  : "bg-white text-gray-700 border-gray-300 hover:border-gray-400"
              }`}
            >
              <input
                type="checkbox"
                checked={toggle.checked}
                onChange={() => toggle.setter(!toggle.checked)}
                className="hidden"
              />
              {toggle.label}
            </label>
          ))}
        </div>

        {/* Player Table */}
        <div className="bg-white rounded-2xl shadow-xl overflow-hidden border border-gray-100">
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200 text-sm">
              <thead className="bg-gradient-to-r from-blue-50 to-sky-50">
                <tr>
                  <th
                    onClick={() => sortPlayers("fullName")}
                    className="px-6 py-4 text-left text-xs font-bold text-gray-700 uppercase tracking-wider cursor-pointer sticky left-0 bg-gradient-to-r from-blue-50 to-sky-50 z-20"
                  >
                    <div className="flex items-center gap-1">
                      Player {getSortIndicator("fullName")}
                    </div>
                  </th>
                  <th
                    onClick={() => sortPlayers("position")}
                    className="px-6 py-4 text-left text-xs font-bold text-gray-700 uppercase tracking-wider cursor-pointer sticky left-[220px] bg-gradient-to-r from-blue-50 to-sky-50 z-20"
                  >
                    <div className="flex items-center gap-1">
                      Position {getSortIndicator("position")}
                    </div>
                  </th>

                  {showInfo && (
                    <>
                      <th
                        onClick={() => sortPlayers("age")}
                        className="px-6 py-4 text-center text-xs font-bold text-gray-700 uppercase tracking-wider cursor-pointer"
                      >
                        <div className="flex items-center justify-center gap-1">
                          Age {getSortIndicator("age")}
                        </div>
                      </th>
                      <th className="px-6 py-4 text-center text-xs font-bold text-gray-700 uppercase tracking-wider">
                        Country
                      </th>
                      <th className="px-6 py-4 text-center text-xs font-bold text-gray-700 uppercase tracking-wider">
                        Height
                      </th>
                      <th className="px-6 py-4 text-center text-xs font-bold text-gray-700 uppercase tracking-wider">
                        Weight
                      </th>
                      <th className="px-6 py-4 text-right text-xs font-bold text-gray-700 uppercase tracking-wider">
                        Value
                      </th>
                    </>
                  )}

                  {showAttributes &&
                    allAttributeNames.map((attr) => (
                      <th
                        key={attr}
                        onClick={() => sortPlayers(attr)}
                        className="px-3 py-4 text-center text-xs font-bold text-gray-700 uppercase tracking-wider cursor-pointer min-w-[50px]"
                      >
                        <div className="flex flex-col items-center">
                          <span className="truncate max-w-[60px]">{attr}</span>
                          {getSortIndicator(attr)}
                        </div>
                      </th>
                    ))}

                  {showStats && (
                    <th className="px-6 py-4 text-left text-xs font-bold text-gray-700 uppercase tracking-wider">
                      Season Stats
                    </th>
                  )}
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {filteredPlayers.map((p) => (
                  <tr
                    key={p.id}
                    className="hover:bg-blue-50 transition-colors duration-150 cursor-pointer group"
                    onClick={() => navigate(`/player/${p.id}`)}
                  >
                    {/* Player Avatar + Name */}
<td className="px-6 py-4 whitespace-nowrap sticky left-0 bg-white z-10">
  <div className="flex items-center gap-3">
    <div className="w-10 h-10 rounded-full bg-gradient-to-br from-blue-400 to-blue-600 flex items-center justify-center text-white font-bold text-sm shadow-md relative">
      {p.AvatarFileName ? (
        <img
          src={`https://localhost:7117/Avatars/${p.AvatarFileName}`}
          alt={p.fullName}
          className="w-10 h-10 rounded-full object-cover border-2 border-white shadow-sm"
          onError={(e) => {
            e.target.style.display = "none";
            const parent = e.target.closest("div");
            if (parent) {
              parent.innerHTML = `
                <div class="w-10 h-10 rounded-full bg-gradient-to-br from-blue-400 to-blue-600 flex items-center justify-center text-white font-bold text-sm">
                  ${p.fullName
                    .split(" ")
                    .map((n) => n[0])
                    .join("")
                    .substring(0, 2)
                    .toUpperCase()}
                </div>
              `;
            }
          }}
        />
      ) : (
        <span>
          {p.fullName
            .split(" ")
            .map((n) => n[0])
            .join("")
            .substring(0, 2)
            .toUpperCase()}
        </span>
      )}
    </div>
    <div>
      <div className="font-medium text-gray-900 group-hover:text-blue-600 transition-colors">
        {p.fullName}
      </div>
      <div className="text-xs text-gray-500">#{p.KitNumber || "N/A"}</div>
    </div>
  </div>
</td>

                    {/* Position */}
                    <td className="px-6 py-4 whitespace-nowrap sticky left-[220px] bg-white z-10">
                      <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
                        {p.position}
                      </span>
                    </td>

                    {showInfo && (
                      <>
                        <td className="px-6 py-4 whitespace-nowrap text-center text-gray-700">
                          {p.age}
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-center">
                          <div className="flex items-center justify-center gap-1">
                            <Flag className="w-4 h-4 text-gray-500" />
                            <span className="text-gray-700">{p.country}</span>
                          </div>
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-center text-gray-700">
                          {p.heightCm} cm
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-center text-gray-700">
                          {p.weightKg} kg
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-right font-medium text-gray-900">
                          {formatPrice(p.price)}
                        </td>
                      </>
                    )}

                    {showAttributes &&
                      allAttributeNames.map((attr) => {
                        const attribute = p.attributes?.find((a) => a.name === attr);
                        return (
                          <td
                            key={attr}
                            className={`px-3 py-4 whitespace-nowrap text-center font-medium ${attribute ? getAttributeColor(attribute.value) : "text-gray-400"}`}
                          >
                            {attribute ? attribute.value : "-"}
                          </td>
                        );
                      })}

                    {showStats && (
                      <td className="px-6 py-4">
                        {p.seasonStats && p.seasonStats.length > 0 ? (
                          <div className="flex flex-wrap gap-2">
                            {p.seasonStats.map((s, i) => (
                              <span
                                key={i}
                                className="bg-gray-100 hover:bg-gray-200 text-gray-700 px-2.5 py-1 rounded-md text-xs font-medium transition-colors"
                              >
                                S{s.seasonId}: {s.goals}G {s.assists}A {s.matchesPlayed}M
                              </span>
                            ))}
                          </div>
                        ) : (
                          <span className="text-gray-400">-</span>
                        )}
                      </td>
                    )}
                  </tr>
                ))}

                {filteredPlayers.length === 0 && (
                  <tr>
                    <td
                      colSpan={100}
                      className="px-6 py-12 text-center text-gray-500 text-lg font-medium"
                    >
                      🔍 No players match your filters.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Squad;