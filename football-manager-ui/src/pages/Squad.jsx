// src/pages/Squad.jsx
import React, { useState, useEffect, useMemo } from "react";
import { useNavigate } from "react-router-dom";
import { Search, Filter, Flag } from "lucide-react";
import Swal from "sweetalert2";

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
    players.forEach((p) => p.attributes?.forEach((a) => names.add(a.name)));
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
    if (search)
      result = result.filter((p) => p.fullName.toLowerCase().includes(search.toLowerCase()));
    if (positionFilter) result = result.filter((p) => p.position === positionFilter);
    if (countryFilter) result = result.filter((p) => p.country === countryFilter);

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
    if (sortConfig.key === key && sortConfig.direction === "asc") direction = "desc";
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
        {/* Header */}
        <div className="text-center mb-8">
          <h1 className="text-4xl md:text-5xl font-extrabold bg-gradient-to-r from-blue-600 to-sky-500 bg-clip-text text-transparent mb-2">
            {teamName || "Loading Squad..."}
          </h1>
          <p className="text-gray-500 text-lg">Click a player to view full profile</p>
        </div>

        {/* Filters */}
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

        {/* Toggles */}
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

        {/* Table */}
        <div className="bg-white rounded-2xl shadow-xl overflow-hidden border border-gray-100">
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200 text-sm">
              <thead className="bg-gradient-to-r from-blue-50 to-sky-50">
                <tr>
                  <th
                    className="px-6 py-4 text-left text-xs font-bold text-gray-700 uppercase cursor-pointer hover:text-blue-600"
                    onClick={() => sortPlayers("fullName")}
                  >
                    Player {getSortIndicator("fullName")}
                  </th>
                  <th
                    className="px-6 py-4 text-left text-xs font-bold text-gray-700 uppercase cursor-pointer hover:text-blue-600"
                    onClick={() => sortPlayers("position")}
                  >
                    Position {getSortIndicator("position")}
                  </th>

                  {showInfo && (
                    <>
                      {["age", "country", "heightCm", "weightKg", "price"].map((key, i) => (
                        <th
                          key={i}
                          className="px-6 py-4 text-center text-xs font-bold text-gray-700 uppercase cursor-pointer hover:text-blue-600"
                          onClick={() => sortPlayers(key)}
                        >
                          {key === "heightCm"
                            ? "Height"
                            : key === "weightKg"
                            ? "Weight"
                            : key === "price"
                            ? "Value"
                            : key.charAt(0).toUpperCase() + key.slice(1)}
                          {getSortIndicator(key)}
                        </th>
                      ))}
                    </>
                  )}

                  {showAttributes &&
                    allAttributeNames.map((attr) => (
                      <th
                        key={attr}
                        onClick={() => sortPlayers(attr)}
                        className="px-3 py-4 text-center text-xs font-bold text-gray-700 uppercase cursor-pointer hover:text-blue-600"
                      >
                        {attr}
                        {getSortIndicator(attr)}
                      </th>
                    ))}

                  {showStats && (
                    <th className="px-6 py-4 text-left text-xs font-bold text-gray-700 uppercase">
                      Season Stats
                    </th>
                  )}

                  <th className="px-6 py-4 text-center text-xs font-bold text-gray-700 uppercase">
                    Release
                  </th>
                </tr>
              </thead>

              <tbody className="bg-white divide-y divide-gray-200">
                {filteredPlayers.map((p) => (
                  <tr
                    key={p.id}
                    className="hover:bg-blue-50 transition-colors duration-150 cursor-pointer"
                    onClick={() => navigate(`/player/${p.id}`)}
                  >
                    <td className="px-6 py-4 whitespace-nowrap text-blue-600 hover:text-blue-800 font-medium">
                      {p.fullName}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">{p.position}</td>

                    {showInfo && (
                      <>
                        <td className="px-6 py-4 text-center">{p.age}</td>
                        <td className="px-6 py-4 text-center">{p.country}</td>
                        <td className="px-6 py-4 text-center">{p.heightCm} cm</td>
                        <td className="px-6 py-4 text-center">{p.weightKg} kg</td>
                        <td className="px-6 py-4 text-right">{formatPrice(p.price)}</td>
                      </>
                    )}

                    {showAttributes &&
                      allAttributeNames.map((attr) => {
                        const a = p.attributes?.find((x) => x.name === attr);
                        return (
                          <td
                            key={attr}
                            className={`px-3 py-4 text-center ${
                              a ? getAttributeColor(a.value) : "text-gray-400"
                            }`}
                          >
                            {a ? a.value : "-"}
                          </td>
                        );
                      })}

                    {showStats && (
                      <td className="px-6 py-4">
                        {p.seasonStats?.length > 0 ? (
                          <div className="flex flex-wrap gap-2">
                            {p.seasonStats.map((s, i) => (
                              <span
                                key={i}
                                className="bg-gray-100 text-gray-700 px-2 py-1 rounded-md text-xs"
                              >
                                S{s.seasonId}: {s.goals}G {s.assists}A
                              </span>
                            ))}
                          </div>
                        ) : (
                          <span className="text-gray-400">-</span>
                        )}
                      </td>
                    )}

                    <td className="px-6 py-4 text-center">
                      <button
                        className="bg-red-500 hover:bg-red-600 text-white text-xs font-semibold px-4 py-2 rounded-full shadow-sm transition"
                        onClick={async (e) => {
                          e.stopPropagation();
                          const result = await Swal.fire({
                            title: `Release ${p.fullName}?`,
                            text: "Are you sure you want to release this player?",
                            icon: "warning",
                            showCancelButton: true,
                            confirmButtonColor: "#d33",
                            cancelButtonColor: "#3085d6",
                            confirmButtonText: "Yes, release",
                            cancelButtonText: "Cancel",
                          });
                          if (!result.isConfirmed) return;
                          try {
                            const res = await fetch("/api/transfers/release", {
                              method: "POST",
                              headers: { "Content-Type": "application/json" },
                              credentials: "include",
                              body: JSON.stringify({ gameSaveId, playerId: p.id }),
                            });
                            const data = await res.json();
                            if (res.ok && data.success) {
                              await Swal.fire({
                                title: "Released!",
                                text: `${p.fullName} has been successfully released.`,
                                icon: "success",
                              });
                              setPlayers((prev) => prev.filter((x) => x.id !== p.id));
                            } else {
                              await Swal.fire({
                                title: "Error",
                                text: data.error || "Error releasing player.",
                                icon: "error",
                              });
                            }
                          } catch {
                            await Swal.fire({
                              title: "Error",
                              text: "Unexpected error while releasing player.",
                              icon: "error",
                            });
                          }
                        }}
                      >
                        Release
                      </button>
                    </td>
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
// src/pages/Squad.jsx