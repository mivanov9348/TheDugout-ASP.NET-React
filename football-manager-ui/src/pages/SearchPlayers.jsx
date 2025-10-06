// src/pages/SearchPlayers.jsx
import { useEffect, useState } from "react";
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

  const [showInfo, setShowInfo] = useState(true);

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

  useEffect(() => {
    if (!gameSaveId) return;
    const controller = new AbortController();

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

    fetch(`/api/transfers/players?${queryParams}`, {
      signal: controller.signal,
    })
      .then((res) => res.json())
      .then((data) => {
        setPlayers(data.players || []);
        setTotalCount(data.totalCount || 0);
      })
      .catch((err) => {
        if (err.name !== "AbortError") {
          console.error("Failed to load players:", err);
        }
      });

    return () => controller.abort();
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
        setPlayers((prev) => prev.filter((p) => p.id !== selectedPlayer.id));
      }
    } catch {
      Swal.fire({
        icon: "error",
        title: "Error",
        text: "Server error occurred.",
      });
    } finally {
      closeBuyModal();
    }
  };

  const showAgencyColumn = players.some((p) => p.agency);

  return (
    <div className="flex flex-col h-full bg-gray-50 overflow-auto">
      {/* Header */}
      <div className="flex items-center gap-3 mb-4 flex-shrink-0 p-4 bg-white border-b">
        <button
          onClick={() => navigate(-1)}
          className="p-2 rounded-full hover:bg-sky-100 text-sky-600 transition"
        >
          <ArrowLeft size={22} />
        </button>
        <h2 className="text-2xl font-bold text-sky-700">Search Players</h2>
      </div>

      {/* Filters */}
      <div className="flex flex-wrap gap-3 mb-4 items-center flex-shrink-0 p-4 bg-white border-b">
        <input
          type="text"
          className="border p-2 rounded-lg flex-1 min-w-[200px] shadow-sm focus:ring-2 focus:ring-sky-400"
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
          className="border p-2 rounded-lg shadow-sm min-w-[150px]"
          value={filterTeam}
          onChange={(e) => setFilterTeam(e.target.value)}
        />
        <input
          type="text"
          placeholder="Filter by country..."
          className="border p-2 rounded-lg shadow-sm min-w-[150px]"
          value={filterCountry}
          onChange={(e) => setFilterCountry(e.target.value)}
        />
        <input
          type="text"
          placeholder="Filter by position..."
          className="border p-2 rounded-lg shadow-sm min-w-[150px]"
          value={filterPosition}
          onChange={(e) => setFilterPosition(e.target.value)}
        />
        <select
          className="border p-2 rounded-lg shadow-sm min-w-[120px]"
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
          className="border p-2 rounded-lg shadow-sm min-w-[100px]"
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
          <span className="text-sm">Free Agents</span>
        </label>
      </div>

      {/* Section toggles */}
      <div className="flex gap-6 mb-4 flex-shrink-0 p-4 bg-white border-b">
        <label className="flex items-center gap-2 text-sm">
          <input
            type="checkbox"
            checked={showInfo}
            onChange={() => setShowInfo(!showInfo)}
          />
          Player Info
        </label>
      </div>

      {/* Table container with integrated pagination */}
      <div className="flex flex-col border rounded-lg bg-white shadow overflow-hidden mx-0 mb-0 h-full">
        {/* Scrollable table */}
        <div className="flex-1 overflow-auto">
          <table className="min-w-max w-full border-collapse text-xs">
            <thead className="bg-sky-50 text-sky-700 sticky top-0 z-10">
              <tr>
                <th className="p-2 border-r border-b whitespace-nowrap font-medium min-w-[150px]">Name</th>
                <th className="p-2 border-r border-b whitespace-nowrap font-medium min-w-[100px]">Team</th>
                <th className="p-2 border-r border-b whitespace-nowrap font-medium min-w-[80px]">Country</th>
                <th className="p-2 border-r border-b whitespace-nowrap font-medium min-w-[80px]">Position</th>
                {showInfo && (
                  <>
                    <th className="p-2 border-r border-b whitespace-nowrap font-medium min-w-[50px]">Age</th>
                    <th className="p-2 border-r border-b whitespace-nowrap font-medium min-w-[90px]">Price</th>
                  </>
                )}
                {showAgencyColumn && <th className="p-2 border-r border-b whitespace-nowrap font-medium min-w-[100px]">Agency</th>}
                <th className="p-2 border-b whitespace-nowrap font-medium min-w-[90px]">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200">
              {players.map((p) => (
                <tr
                  key={p.id}
                  className="hover:bg-sky-50 transition-colors"
                >
                  <td
                    className="p-2 border-r font-medium cursor-pointer max-w-[150px] truncate"
                    title={p.name}
                    onClick={() => navigate(`/player/${p.id}`)}
                  >
                    {p.name}
                  </td>
                  <td className="p-2 border-r max-w-[100px] truncate">{p.team || "—"}</td>
                  <td className="p-2 border-r max-w-[80px] truncate">{p.country}</td>
                  <td className="p-2 border-r max-w-[80px] truncate">{p.position}</td>
                  {showInfo && (
                    <>
                      <td className="p-2 border-r max-w-[50px] truncate text-center">{p.age}</td>
                      <td className="p-2 border-r max-w-[90px] truncate text-right pr-2">{formatPrice(p.price)}</td>
                    </>
                  )}
                  {showAgencyColumn && (
                    <td className="p-2 border-r max-w-[100px]">
                      {p.agency ? (
                        <div className="flex flex-col items-center gap-1 py-1">
                          <img
                            src={
                              `/agenciesLogos/${p.agency.logo}` ||
                              "/default-logo.png"
                            }
                            alt="Agency Logo"
                            className="w-6 h-6 rounded-full object-cover border"
                            onError={(e) => {
                              e.target.src = "/default-logo.png";
                            }}
                          />
                          <div className="text-xs text-gray-600 truncate text-center w-full">
                            {p.agency.name}
                          </div>
                          <div className="text-xs bg-blue-100 text-blue-800 px-1 py-0.5 rounded-full">
                            ★{p.agency.popularity}
                          </div>
                        </div>
                      ) : (
                        "—"
                      )}
                    </td>
                  )}
                  <td className="p-2 text-center">
                    {!p.team ? (
                      <button
                        onClick={() => openBuyModal(p)}
                        className="px-2 py-1 bg-green-500 hover:bg-green-600 text-white rounded text-xs whitespace-nowrap"
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
                        className="px-2 py-1 bg-gray-400 text-white rounded text-xs cursor-not-allowed"
                      >
                        Offer
                      </button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        {/* Pagination integrated */}
        {totalPages > 1 && (
          <div className="flex justify-between items-center p-3 border-t bg-sky-50 flex-shrink-0">
            <button
              className="px-3 py-1.5 bg-sky-100 hover:bg-sky-200 text-sky-700 rounded disabled:opacity-50 transition text-sm"
              disabled={currentPage === 1}
              onClick={() => setCurrentPage((p) => p - 1)}
            >
              Назад
            </button>
            <span className="font-medium text-sky-700 text-sm">
              Страница {currentPage} от {totalPages}
            </span>
            <button
              className="px-3 py-1.5 bg-sky-100 hover:bg-sky-200 text-sky-700 rounded disabled:opacity-50 transition text-sm"
              disabled={currentPage >= totalPages}
              onClick={() => setCurrentPage((p) => p + 1)}
            >
              Напред
            </button>
          </div>
        )}
      </div>

      {/* Modal */}
      {isModalOpen && selectedPlayer && (
        <div className="fixed inset-0 flex items-center justify-center bg-black bg-opacity-50 z-50">
          <div className="bg-white rounded-xl shadow-xl max-w-md w-full mx-4 flex flex-col max-h-[80vh] overflow-hidden">
            {/* Modal header */}
            <div className="p-4 border-b flex justify-between items-center">
              <h3 className="text-lg font-bold text-sky-700">Confirm Buy</h3>
              <button
                onClick={closeBuyModal}
                className="text-gray-500 hover:text-black text-xl"
              >
                ✕
              </button>
            </div>

            {/* Modal content */}
            <div className="p-4 flex-1 overflow-y-auto">
              <p className="mb-2 text-sm">
                Do you want to sign{" "}
                <span className="font-semibold">{selectedPlayer.name}</span>?
              </p>
              <p className="mb-4 text-gray-600 text-sm">
                Age: {selectedPlayer.age} | Position: {selectedPlayer.position} | Price:{" "}
                {formatPrice(selectedPlayer.price)}
              </p>
              {selectedPlayer.agency && (
                <div className="mb-4 p-3 bg-gray-50 rounded-lg">
                  <div className="flex items-center gap-2">
                    <img
                      src={
                        `/agenciesLogos/${selectedPlayer.agency.logo}` ||
                        "/default-logo.png"
                      }
                      alt="Agency"
                      className="w-5 h-5 rounded-full object-cover"
                      onError={(e) => {
                        e.target.src = "/default-logo.png";
                      }}
                    />
                    <span className="text-xs">
                      Represented by{" "}
                      <strong>{selectedPlayer.agency.regionName}</strong> agency (★
                      {selectedPlayer.agency.popularity})
                    </span>
                  </div>
                </div>
              )}
            </div>

            {/* Modal actions */}
            <div className="flex justify-end gap-2 p-4 border-t bg-gray-50">
              <button
                onClick={closeBuyModal}
                className="px-3 py-1.5 rounded border hover:bg-gray-100 text-sm"
              >
                Cancel
              </button>
              <button
                onClick={handleConfirmBuy}
                className="px-3 py-1.5 bg-green-500 hover:bg-green-600 text-white rounded text-sm"
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