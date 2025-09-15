// src/pages/SearchPlayers.jsx
import { useEffect, useState, useMemo } from "react";
import { useNavigate } from "react-router-dom";
import { ArrowLeft } from "lucide-react";
import Swal from "sweetalert2";

function useDebounce(value, delay) {
  const [debouncedValue, setDebouncedValue] = useState(value);
  useEffect(() => {
    const handler = setTimeout(() => setDebouncedValue(value), delay);
    return () => clearTimeout(handler);
  }, [value, delay]);
  return debouncedValue;
}

export default function SearchPlayers({ gameSaveId }) {
  const [players, setPlayers] = useState([]);
  const [search, setSearch] = useState("");
  const [sortBy, setSortBy] = useState("name");
  const [sortOrder, setSortOrder] = useState("asc");
  const [currentPage, setCurrentPage] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [filterTeam, setFilterTeam] = useState("");
  const [filterCountry, setFilterCountry] = useState("");
  const [filterPosition, setFilterPosition] = useState("");
  const [filterFreeAgents, setFilterFreeAgents] = useState(false);

  // toggles
  const [showInfo, setShowInfo] = useState(true);
  const [showAttributes, setShowAttributes] = useState(true);
  const [showStats, setShowStats] = useState(true);

  // modal state
  const [selectedPlayer, setSelectedPlayer] = useState(null);
  const [isModalOpen, setIsModalOpen] = useState(false);

  const openBuyModal = (player) => {
    setSelectedPlayer(player);
    setIsModalOpen(true);
  };

  const closeBuyModal = () => {
    setSelectedPlayer(null);
    setIsModalOpen(false);
  };

  const playersPerPage = 15;
  const debouncedSearch = useDebounce(search, 400);

  const navigate = useNavigate();

  // Collect attribute names dynamically
  const allAttributeNames = useMemo(() => {
    const names = new Set();
    players.forEach((p) => {
      p.attributes?.forEach((a) => names.add(a.name));
    });
    return Array.from(names);
  }, [players]);

  // Fetch players
  useEffect(() => {
    if (!gameSaveId) return;

    const queryParams = new URLSearchParams({
      gameSaveId,
      search: debouncedSearch,
      sortBy,
      sortOrder,
      page: currentPage,
      pageSize: playersPerPage,
      team: filterTeam,
      country: filterCountry,
      position: filterPosition,
      freeAgent: filterFreeAgents,
    });

    fetch(`/api/transfers/players?${queryParams}`)
      .then((res) => res.json())
      .then((data) => {
        setPlayers(data.players || []);
        setTotalCount(data.totalCount || 0);
      })
      .catch((err) => console.error("Failed to load players:", err));
  }, [
    gameSaveId,
    debouncedSearch,
    sortBy,
    sortOrder,
    currentPage,
    filterTeam,
    filterCountry,
    filterPosition,
    filterFreeAgents,
  ]);

  const totalPages = Math.ceil(totalCount / playersPerPage);

  const formatPrice = (value) => {
    if (value == null) return "-";
    return value.toLocaleString("en-US");
  };

  const handleConfirmBuy = async () => {
    if (!selectedPlayer) return;

    try {
      const res = await fetch("/api/transfers/buy", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          gameSaveId,
          playerId: selectedPlayer.id,
        }),
      });

      const data = await res.json();

      if (!res.ok || !data.success) {
        Swal.fire({
          icon: "error",
          title: "Failed",
          text: data.error || "Could not complete the transfer.",
        });
      } else {
        Swal.fire({
          icon: "success",
          title: "Success",
          text: `You signed ${selectedPlayer.name}!`,
        });
        // reload list
        setPlayers((prev) => prev.filter((p) => p.id !== selectedPlayer.id));
      }
    } catch (e) {
      Swal.fire({
        icon: "error",
        title: "Error",
        text: "Server error occurred.",
      });
    } finally {
      closeBuyModal();
    }
  };

  // 🔹 Проверка дали да показваме колоната "Agency"
  const showAgencyColumn = players.some((p) => p.agency);

  return (
    <div className="mt-6 p-6 border rounded-xl bg-white shadow-lg max-w-[1400px] mx-auto overflow-hidden">
      {/* Header */}
      <div className="flex items-center gap-3 mb-6">
        <button
          onClick={() => navigate(-1)}
          className="p-2 rounded-full hover:bg-sky-100 text-sky-600 transition"
        >
          <ArrowLeft size={22} />
        </button>
        <h2 className="text-2xl font-bold text-sky-700">Search Players</h2>
      </div>

      {/* Filters */}
      <div className="flex flex-wrap gap-3 mb-6 items-center">
        <input
          type="text"
          className="border p-2 rounded-lg flex-1 shadow-sm focus:ring-2 focus:ring-sky-400"
          placeholder="Search by name..."
          value={search}
          onChange={(e) => {
            setSearch(e.target.value);
            setCurrentPage(1);
          }}
        />
        <input
          type="text"
          placeholder="Filter by team..."
          className="border p-2 rounded-lg shadow-sm"
          value={filterTeam}
          onChange={(e) => setFilterTeam(e.target.value)}
        />
        <input
          type="text"
          placeholder="Filter by country..."
          className="border p-2 rounded-lg shadow-sm"
          value={filterCountry}
          onChange={(e) => setFilterCountry(e.target.value)}
        />
        <input
          type="text"
          placeholder="Filter by position..."
          className="border p-2 rounded-lg shadow-sm"
          value={filterPosition}
          onChange={(e) => setFilterPosition(e.target.value)}
        />
        <select
          className="border p-2 rounded-lg shadow-sm"
          value={sortBy}
          onChange={(e) => setSortBy(e.target.value)}
        >
          <option value="name">Name</option>
          <option value="team">Team</option>
          <option value="country">Country</option>
          <option value="position">Position</option>
          <option value="age">Age</option>
          <option value="price">Price</option>
          <option value="freeagent">Free Agent</option>
        </select>
        <select
          className="border p-2 rounded-lg shadow-sm"
          value={sortOrder}
          onChange={(e) => setSortOrder(e.target.value)}
        >
          <option value="asc">⬆ Asc</option>
          <option value="desc">⬇ Desc</option>
        </select>
        <label className="flex items-center gap-2 ml-3">
          <input
            type="checkbox"
            checked={filterFreeAgents}
            onChange={(e) => setFilterFreeAgents(e.target.checked)}
          />
          Free Agents
        </label>
      </div>

      {/* Section toggles */}
      <div className="flex gap-6 mb-4">
        <label className="flex items-center gap-2">
          <input
            type="checkbox"
            checked={showInfo}
            onChange={() => setShowInfo(!showInfo)}
          />
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
          <input
            type="checkbox"
            checked={showStats}
            onChange={() => setShowStats(!showStats)}
          />
          Stats
        </label>
      </div>

      {/* Table — FIXED: Scrollable without stretching page */}
      <div className="overflow-x-auto rounded-lg border max-w-full scrollbar-thin">
        <div className="min-w-full">
          <table className="w-full border-collapse text-sm table-fixed">
            <thead className="bg-sky-50 text-sky-700 sticky top-0">
  <tr>
    <th className="p-3 border whitespace-nowrap min-w-[180px]">Name</th>
    <th className="p-3 border whitespace-nowrap min-w-[120px]">Team</th>
    <th className="p-3 border whitespace-nowrap min-w-[100px]">Country</th>
    <th className="p-3 border whitespace-nowrap min-w-[100px]">Position</th>
    {showInfo && (
      <>
        <th className="p-3 border whitespace-nowrap min-w-[60px]">Age</th>
        <th className="p-3 border whitespace-nowrap min-w-[100px]">Price</th>
      </>
    )}
    {showAttributes &&
      allAttributeNames.map((attr) => (
        <th key={attr} className="p-3 border whitespace-nowrap min-w-[80px]">
          {attr}
        </th>
      ))}
    {showStats && (
      <th className="p-3 border whitespace-nowrap min-w-[200px]">
        Season Stats
      </th>
    )}
    {showAgencyColumn && (
      <th className="p-3 border whitespace-nowrap min-w-[120px]">Agency</th>
    )}
    <th className="p-3 border whitespace-nowrap min-w-[100px]">Actions</th>
  </tr>
</thead>
            <tbody>
  {players.map((p) => (
    <tr
      key={p.id}
      className="text-center hover:bg-sky-50 transition-colors"
    >
      <td
        className="p-2 border font-medium cursor-pointer whitespace-nowrap min-w-[180px]"
        onClick={() => navigate(`/player/${p.id}`)}
      >
        {p.name}
      </td>
      <td className="p-2 border whitespace-nowrap min-w-[120px]">
        {p.team || "—"}
      </td>
      <td className="p-2 border whitespace-nowrap min-w-[100px]">
        {p.country}
      </td>
      <td className="p-2 border whitespace-nowrap min-w-[100px]">
        {p.position}
      </td>
      {showInfo && (
        <>
          <td className="p-2 border whitespace-nowrap min-w-[60px]">
            {p.age}
          </td>
          <td className="p-2 border whitespace-nowrap min-w-[100px]">
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
              className="p-2 border whitespace-nowrap min-w-[80px]"
            >
              {attribute ? attribute.value : "-"}
            </td>
          );
        })}
      {showStats && (
        <td className="p-2 border min-w-[200px]">
          {p.seasonStats && p.seasonStats.length > 0 ? (
            <ul className="list-disc list-inside text-left">
              {p.seasonStats.map((s, i) => (
                <li key={i} className="whitespace-nowrap">
                  Season {s.seasonId}: {s.goals}G / {s.assists}A /{" "}
                  {s.matchesPlayed}M
                </li>
              ))}
            </ul>
          ) : (
            "-"
          )}
        </td>
      )}
      {/* 🔹 Колона за агенция */}
      {showAgencyColumn && (
        <td className="p-2 border min-w-[120px]">
          {p.agency ? (
            <div className="flex flex-col items-center gap-1">
              <img
                src={
                  `/agenciesLogos/${p.agency.logo}` || "/default-logo.png"
                }
                alt="Agency Logo"
                className="w-8 h-8 rounded-full object-cover border"
                onError={(e) => {
                  e.target.src = "/default-logo.png";
                }}
              />
              <div className="text-xs text-gray-600">{p.agency.name}</div>
              <div className="text-xs bg-blue-100 text-blue-800 px-1.5 py-0.5 rounded-full">
                ★ {p.agency.popularity}
              </div>
            </div>
          ) : (
            "—"
          )}
        </td>
      )}
      <td className="p-2 border whitespace-nowrap min-w-[100px]">
        {!p.team ? (
          <button
            onClick={() => openBuyModal(p)}
            className="px-3 py-1 bg-green-500 hover:bg-green-600 text-white rounded-lg text-xs"
          >
            Buy
          </button>
        ) : (
          <button
            onClick={() =>
              Swal.fire({
                icon: "info",
                title: "Not active yet",
                text: "Sending offers will be available in a future update.",
              })
            }
            disabled
            className="px-3 py-1 bg-gray-400 text-white rounded-lg text-xs cursor-not-allowed"
          >
            Send Offer
          </button>
        )}
      </td>
    </tr>
  ))}
  {players.length === 0 && (
    <tr>
      <td
        colSpan={
          9 + allAttributeNames.length + (showAgencyColumn ? 1 : 0)
        }
        className="text-center py-6 text-gray-500"
      >
        No players found.
      </td>
    </tr>
  )}
</tbody>
          </table>
        </div>
      </div>

      {/* Pagination */}
      <div className="flex justify-between items-center mt-6">
        <button
          className="px-4 py-2 bg-sky-100 hover:bg-sky-200 text-sky-700 rounded-lg disabled:opacity-50 transition"
          disabled={currentPage === 1}
          onClick={() => setCurrentPage((p) => p - 1)}
        >
          Назад
        </button>
        <span className="font-medium text-sky-700">
          Страница {currentPage} от {totalPages || 1}
        </span>
        <button
          className="px-4 py-2 bg-sky-100 hover:bg-sky-200 text-sky-700 rounded-lg disabled:opacity-50 transition"
          disabled={currentPage >= totalPages}
          onClick={() => setCurrentPage((p) => p + 1)}
        >
          Напред
        </button>
      </div>

      {/* Modal */}
      {isModalOpen && selectedPlayer && (
        <div className="fixed inset-0 flex items-center justify-center bg-black bg-opacity-50 z-50">
          <div className="bg-white p-6 rounded-xl shadow-xl max-w-md w-full">
            <h3 className="text-lg font-bold mb-4 text-sky-700">Confirm Buy</h3>
            <p className="mb-2">
              Do you want to sign{" "}
              <span className="font-semibold">{selectedPlayer.name}</span>?
            </p>
            <p className="mb-4 text-gray-600">
              Age: {selectedPlayer.age} | Position: {selectedPlayer.position} |{" "}
              Price: {formatPrice(selectedPlayer.price)}
            </p>
            {/* 🔹 Показваме агенцията и в модала */}
            {selectedPlayer.agency && (
              <div className="mb-4 p-3 bg-gray-50 rounded-lg">
                <div className="flex items-center gap-2">
                  <img
                    src={
                      `/agenciesLogos/${selectedPlayer.agency.logo}` ||
                      "/default-logo.png"
                    }
                    alt="Agency"
                    className="w-6 h-6 rounded-full object-cover"
                    onError={(e) => {
                      e.target.src = "/default-logo.png";
                    }}
                  />
                  <span className="text-sm">
                    Represented by{" "}
                    <strong>{selectedPlayer.agency.regionName}</strong> agency (
                    ★{selectedPlayer.agency.popularity})
                  </span>
                </div>
              </div>
            )}
            <div className="flex justify-end gap-3">
              <button
                onClick={closeBuyModal}
                className="px-4 py-2 rounded-lg border hover:bg-gray-100"
              >
                Cancel
              </button>
              <button
                onClick={handleConfirmBuy}
                className="px-4 py-2 bg-green-500 hover:bg-green-600 text-white rounded-lg"
              >
                Confirm
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}