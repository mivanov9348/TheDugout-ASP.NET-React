import { useEffect, useState } from "react";
import { Loader2, Filter } from "lucide-react";
import { Link } from "react-router-dom";
import Swal from "sweetalert2";

export default function SearchPlayers({ gameSaveId }) {
  const [players, setPlayers] = useState([]);
  const [loading, setLoading] = useState(false);
  const [filters, setFilters] = useState({
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
  });
  const [totalCount, setTotalCount] = useState(0);

  useEffect(() => {
    fetchPlayers();
  }, [filters]);

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
      });

      fetchPlayers();
    } catch (err) {
      Swal.fire({
        title: "❌ Error",
        text: err.message,
        icon: "error",
      });
    }
  };

  return (
    <div className="space-y-4">
      {/* 🔍 Филтри */}
      <div className="bg-white p-4 rounded-xl shadow border border-slate-200">
        <h2 className="text-xl font-semibold flex items-center gap-2 mb-4">
          <Filter size={20} /> Player Filters
        </h2>

        <div className="grid grid-cols-6 gap-3">
          <input
            type="text"
            name="search"
            placeholder="Search name..."
            value={filters.search}
            onChange={handleChange}
            className="col-span-2 border rounded-lg px-3 py-2 text-sm"
          />
          <input
            type="text"
            name="team"
            placeholder="Team"
            value={filters.team}
            onChange={handleChange}
            className="border rounded-lg px-3 py-2 text-sm"
          />
          <input
            type="text"
            name="country"
            placeholder="Country"
            value={filters.country}
            onChange={handleChange}
            className="border rounded-lg px-3 py-2 text-sm"
          />
          <input
            type="text"
            name="position"
            placeholder="Position"
            value={filters.position}
            onChange={handleChange}
            className="border rounded-lg px-3 py-2 text-sm"
          />
          <label className="flex items-center gap-2 text-sm">
            <input
              type="checkbox"
              name="freeAgent"
              checked={filters.freeAgent}
              onChange={handleChange}
            />
            Free agents only
          </label>
        </div>

        {/* 🧮 Допълнителни филтри: възраст и цена */}
        <div className="grid grid-cols-4 gap-3 mt-4">
          <div className="flex flex-col">
            <label className="text-xs text-slate-500">Min Age</label>
            <input
              type="number"
              name="minAge"
              placeholder="e.g. 18"
              value={filters.minAge}
              onChange={handleChange}
              className="border rounded-lg px-3 py-2 text-sm"
            />
          </div>
          <div className="flex flex-col">
            <label className="text-xs text-slate-500">Max Age</label>
            <input
              type="number"
              name="maxAge"
              placeholder="e.g. 30"
              value={filters.maxAge}
              onChange={handleChange}
              className="border rounded-lg px-3 py-2 text-sm"
            />
          </div>
          <div className="flex flex-col">
            <label className="text-xs text-slate-500">Min Price (€)</label>
            <input
              type="number"
              name="minPrice"
              placeholder="e.g. 1000000"
              value={filters.minPrice}
              onChange={handleChange}
              className="border rounded-lg px-3 py-2 text-sm"
            />
          </div>
          <div className="flex flex-col">
            <label className="text-xs text-slate-500">Max Price (€)</label>
            <input
              type="number"
              name="maxPrice"
              placeholder="e.g. 5000000"
              value={filters.maxPrice}
              onChange={handleChange}
              className="border rounded-lg px-3 py-2 text-sm"
            />
          </div>
        </div>
      </div>

      {/* 📋 Таблица */}
      <div className="bg-white rounded-xl shadow border border-slate-200 overflow-hidden">
        {loading ? (
          <div className="flex justify-center items-center h-64 text-slate-500">
            <Loader2 className="animate-spin mr-2" /> Loading players...
          </div>
        ) : (
          <>
            <table className="min-w-full text-sm">
              <thead className="bg-slate-100 text-slate-700">
                <tr>
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
                          ? "cursor-pointer hover:bg-slate-200"
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
                      colSpan="7"
                      className="text-center text-slate-500 py-6 italic"
                    >
                      No players found.
                    </td>
                  </tr>
                ) : (
                  players.map((p) => (
                    <tr
                      key={p.id}
                      className="border-t hover:bg-slate-50 transition-colors"
                    >
                      <td className="px-4 py-2">
                        <Link
                          to={`/player/${p.id}`}
                          className="text-blue-600 hover:underline font-medium"
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
                            disabled
                            className="px-3 py-1 rounded bg-gray-200 text-gray-500 cursor-not-allowed text-xs"
                          >
                            Offer
                          </button>
                        ) : (
                          <button
                            onClick={() => handleSign(p.id, p.name)}
                            className="px-3 py-1 rounded bg-emerald-600 hover:bg-emerald-700 text-white text-xs"
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

            {/* 📄 Пагинация */}
            <div className="flex justify-between items-center p-3 bg-slate-50 border-t text-sm">
              <span>
                Showing {players.length} of {totalCount} players
              </span>
              <div className="flex gap-2">
                <button
                  className="px-3 py-1 border rounded disabled:opacity-50"
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
                  className="px-3 py-1 border rounded disabled:opacity-50"
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
