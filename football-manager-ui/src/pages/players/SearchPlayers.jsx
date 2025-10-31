import { useEffect, useState } from "react";
import { Loader2, Filter } from "lucide-react";
import { Link } from "react-router-dom";
import Swal from "sweetalert2";
import PlayerAvatar from "../../components/PlayerAvatar";
import { useGame } from "../../context/GameContext";

export default function SearchPlayers({ gameSaveId }) {
  const { refreshGameStatus } = useGame();

  const [players, setPlayers] = useState([]);
  const [loading, setLoading] = useState(false);
  const [userTeamId, setUserTeamId] = useState(null);
  const [totalCount, setTotalCount] = useState(0);

  const savedFilters =
    JSON.parse(sessionStorage.getItem("playerFilters")) || null;

  const [filters, setFilters] = useState(
    savedFilters || {
      search: "",
      team: "",
      country: "",
      position: "",
      freeAgent: false,
      minAge: "",
      maxAge: "",
      minPrice: "",
      maxPrice: "",
      sortBy: "name",
      sortOrder: "asc",
      page: 1,
      pageSize: 50,
    }
  );

  useEffect(() => {
    sessionStorage.setItem("playerFilters", JSON.stringify(filters));
  }, [filters]);

  useEffect(() => {
    fetchPlayers();
  }, [filters, gameSaveId]);

  const fetchPlayers = async () => {
    if (!gameSaveId) return;
    setLoading(true);
    try {
      const params = new URLSearchParams({
        gameSaveId,
        ...filters,
      });
      const res = await fetch(`/api/transfers/players?${params.toString()}`);
      if (!res.ok) throw new Error("Failed to fetch players");
      const data = await res.json();

      setPlayers(data.players);
      setTotalCount(data.totalCount);
    } catch (err) {
      console.error("Error fetching players:", err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    const fetchUserTeam = async () => {
      if (!gameSaveId) return;
      try {
        const res = await fetch(`/api/transfers/userteam/${gameSaveId}`);
        if (!res.ok) throw new Error("Failed to load game save");
        const data = await res.json();
        setUserTeamId(data.userTeamId);
      } catch (err) {
        console.error("Error loading user team:", err);
      }
    };
    fetchUserTeam();
  }, [gameSaveId]);

  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;
    setFilters({
      ...filters,
      [name]: type === "checkbox" ? checked : value,
      page: 1,
    });
  };

  const handleSort = (column) => {
    setFilters((prev) => ({
      ...prev,
      sortBy: column,
      sortOrder:
        prev.sortBy === column && prev.sortOrder === "asc" ? "desc" : "asc",
    }));
  };

  const handleSign = async (playerId, playerName) => {
    if (!gameSaveId) return;

    const result = await Swal.fire({
      title: `Sign ${playerName}?`,
      text: "Are you sure you want to sign this free agent?",
      icon: "question",
      showCancelButton: true,
      confirmButtonText: "Yes, sign him!",
      cancelButtonText: "Cancel",
      confirmButtonColor: "#16a34a",
      cancelButtonColor: "#6b7280",
      background: "#1f2937",
      color: "#f3f4f6",
    });

    if (!result.isConfirmed) return;

    try {
      const res = await fetch(`/api/transfers/buy`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ gameSaveId, playerId }),
      });

      const data = await res.json();
      if (!res.ok) throw new Error(data.error || "Failed to sign player");

      await Swal.fire({
        title: "✅ Success!",
        text: `${playerName} has signed for your team.`,
        icon: "success",
        timer: 2000,
        showConfirmButton: false,
        background: "#1f2937",
        color: "#f3f4f6",
      });
      await refreshGameStatus();
      fetchPlayers();
    } catch (err) {
      Swal.fire({
        title: "❌ Error",
        text: err.message,
        icon: "error",
        background: "#1f2937",
        color: "#f3f4f6",
      });
    }
  };

  const handleOffer = async (player) => {
    if (!gameSaveId || !userTeamId) {
      Swal.fire({
        icon: "error",
        title: "❌ Missing data",
        text: "User team or game save not loaded yet.",
        background: "#1f2937",
        color: "#f3f4f6",
      });
      return;
    }

    const { value: amount } = await Swal.fire({
      title: `💰 Offer for ${player.name}`,
      html: `
        <div style="text-align: left; color: #f3f4f6;">
          <p><b>Team:</b> ${player.team || "Free agent"}</p>
          <p><b>Current Price:</b> ${player.price ? player.price.toLocaleString() : "-"
        } €</p>
          <label style="display:block;margin-top:10px;">Enter your offer (€):</label>
          <input type="number" id="offerAmount" class="swal2-input" placeholder="Enter amount">
        </div>
      `,
      background: "#1f2937",
      color: "#f3f4f6",
      focusConfirm: false,
      showCancelButton: true,
      confirmButtonText: "Send Offer",
      cancelButtonText: "Cancel",
      preConfirm: () => {
        const input = document.getElementById("offerAmount").value;
        if (!input || isNaN(input) || Number(input) <= 0) {
          Swal.showValidationMessage("Enter a valid offer amount!");
          return false;
        }
        return Number(input);
      },
    });

    if (!amount) return;

    try {
      const res = await fetch(`/api/transfers/offer`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          gameSaveId,
          playerId: player.id,
          fromTeamId: userTeamId,
          toTeamId: player.teamId,
          offerAmount: amount,
        }),
      });

      const data = await res.json();
      if (!res.ok) throw new Error(data.error || "Offer failed");

      Swal.fire({
        icon: "success",
        title: "✅ Offer sent!",
        text: `You offered €${amount.toLocaleString()} for ${player.name}.`,
        timer: 2500,
        showConfirmButton: false,
        background: "#1f2937",
        color: "#f3f4f6",
      });
    } catch (err) {
      Swal.fire({
        icon: "error",
        title: "❌ Error",
        text: err.message,
        background: "#1f2937",
        color: "#f3f4f6",
      });
    }
  };

  return (
    <div className="space-y-4 bg-gradient-to-br from-gray-900 via-gray-800 to-gray-900 text-gray-100 p-4 rounded-xl">
      {/* Filters */}
      <div className="bg-gray-800 p-4 rounded-xl shadow border border-gray-700">
        <h2 className="text-xl font-semibold flex items-center gap-2 mb-4 text-gray-100">
          <Filter size={20} /> Player Filters
        </h2>

        <div className="grid grid-cols-6 gap-3">
          {["search", "team", "country", "position"].map((field) => (
            <input
              key={field}
              type="text"
              name={field}
              placeholder={field.charAt(0).toUpperCase() + field.slice(1)}
              value={filters[field]}
              onChange={handleChange}
              className="col-span-1 border border-gray-700 bg-gray-900 rounded-lg px-3 py-2 text-sm text-gray-200 placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-blue-600"
            />
          ))}
          <label className="flex items-center gap-2 text-sm text-gray-300">
            <input
              type="checkbox"
              name="freeAgent"
              checked={filters.freeAgent}
              onChange={handleChange}
              className="accent-blue-600"
            />
            Free agents only
          </label>
        </div>

        <div className="grid grid-cols-4 gap-3 mt-4">
          {[
            ["minAge", "Min Age"],
            ["maxAge", "Max Age"],
            ["minPrice", "Min Price (€)"],
            ["maxPrice", "Max Price (€)"],
          ].map(([name, label]) => (
            <div key={name} className="flex flex-col">
              <label className="text-xs text-gray-400">{label}</label>
              <input
                type="number"
                name={name}
                value={filters[name]}
                onChange={handleChange}
                className="border border-gray-700 bg-gray-900 rounded-lg px-3 py-2 text-sm text-gray-200 focus:outline-none focus:ring-2 focus:ring-blue-600"
              />
            </div>
          ))}
        </div>
      </div>

      {/* Table */}
      <div className="bg-gray-800 rounded-xl shadow border border-gray-700 overflow-hidden">
        {loading ? (
          <div className="flex justify-center items-center h-64 text-gray-400">
            <Loader2 className="animate-spin mr-2" /> Loading players...
          </div>
        ) : (
          <>
            <table className="min-w-full text-sm text-gray-200">
              <thead className="bg-gray-700 text-gray-300">
                <tr>
                  <th className="px-3 py-2 text-left">Logo</th>
                  {[
                    { key: "name", label: "Name" },
                    { key: "team", label: "Team" },
                    { key: "country", label: "Country" },
                    { key: "position", label: "Position" },
                    { key: "age", label: "Age" },
                    { key: "price", label: "Price (€)" },
                    { key: "actions", label: "Actions" },
                  ].map((col) => (
                    <th
                      key={col.key}
                      onClick={
                        col.key !== "actions"
                          ? () => handleSort(col.key)
                          : undefined
                      }
                      className={`px-4 py-2 font-medium text-left ${col.key !== "actions"
                        ? "cursor-pointer hover:bg-gray-600"
                        : ""
                        }`}
                    >
                      {col.label}
                      {filters.sortBy === col.key && (
                        <span className="ml-1 text-xs">
                          {filters.sortOrder === "asc" ? "▲" : "▼"}
                        </span>
                      )}
                    </th>
                  ))}
                </tr>
              </thead>

              <tbody>
                {players.length === 0 ? (
                  <tr>
                    <td
                      colSpan="8"
                      className="text-center text-gray-400 py-6 italic"
                    >
                      No players found.
                    </td>
                  </tr>
                ) : (
                  players.map((p) => (
                    <tr
                      key={p.id}
                      className="border-t border-gray-700 hover:bg-gray-700/50 transition-colors"
                    >
                      <td className="px-3 py-2">

                        <PlayerAvatar
                          playerName={p.name}
                          imageFileName={p.avatarFileName}
                          className="w-8 h-8"
                        />

                      </td>

                      <td className="px-4 py-2">
                        <Link
                          to={`/player/${p.id}`}
                          className="text-blue-500 hover:underline font-medium"
                        >
                          {p.name}
                        </Link>
                      </td>
                      <td className="px-4 py-2">{p.team || "-"}</td>
                      <td className="px-4 py-2">{p.country || "-"}</td>
                      <td className="px-4 py-2">{p.position || "-"}</td>
                      <td className="px-4 py-2">{p.age}</td>
                      <td className="px-4 py-2">
                        {p.price ? `${p.price.toLocaleString()} €` : "-"}
                      </td>
                      <td className="px-4 py-2">
                        {p.team ? (
                          <button
                            disabled={!userTeamId}
                            onClick={() => handleOffer(p)}
                            className={`px-3 py-1 rounded text-white text-xs ${userTeamId
                              ? "bg-blue-600 hover:bg-blue-700"
                              : "bg-gray-500 cursor-not-allowed"
                              }`}
                          >
                            Offer
                          </button>
                        ) : (
                          <button
                            disabled={!userTeamId}
                            onClick={() => handleSign(p.id, p.name)}
                            className={`px-3 py-1 rounded text-white text-xs ${userTeamId
                              ? "bg-emerald-600 hover:bg-emerald-700"
                              : "bg-gray-500 cursor-not-allowed"
                              }`}
                          >
                            Sign
                          </button>
                        )}
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>

            <div className="flex justify-between items-center p-3 bg-gray-700 border-t border-gray-600 text-sm text-gray-300">
              <span>
                Showing {players.length} of {totalCount} players
              </span>
              <div className="flex gap-2">
                <button
                  className="px-3 py-1 border border-gray-600 rounded bg-gray-800 hover:bg-gray-700 disabled:opacity-50"
                  disabled={filters.page === 1}
                  onClick={() =>
                    setFilters((f) => ({ ...f, page: f.page - 1 }))
                  }
                >
                  Prev
                </button>
                <span>
                  Page {filters.page} /{" "}
                  {Math.ceil(totalCount / filters.pageSize) || 1}
                </span>
                <button
                  className="px-3 py-1 border border-gray-600 rounded bg-gray-800 hover:bg-gray-700 disabled:opacity-50"
                  disabled={filters.page * filters.pageSize >= totalCount}
                  onClick={() =>
                    setFilters((f) => ({ ...f, page: f.page + 1 }))
                  }
                >
                  Next
                </button>
              </div>
            </div>
          </>
        )}
      </div>
    </div>
  );
}
