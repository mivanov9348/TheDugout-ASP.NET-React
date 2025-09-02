// src/pages/SearchPlayers.jsx
import { useEffect, useState } from "react";

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

  const playersPerPage = 15;
  const debouncedSearch = useDebounce(search, 400);

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
      freeAgent: filterFreeAgents
    });

    fetch(`/api/transfers/players?${queryParams}`)
      .then(res => res.json())
      .then(data => {
        setPlayers(data.players || []);
        setTotalCount(data.totalCount || 0);
      })
      .catch(err => console.error("Failed to load players:", err));
  }, [gameSaveId, debouncedSearch, sortBy, sortOrder, currentPage, filterTeam, filterCountry, filterPosition, filterFreeAgents]);

  const totalPages = Math.ceil(totalCount / playersPerPage);

  return (
    <div className="mt-6 p-6 border rounded-xl bg-white shadow-lg">
      <h2 className="text-2xl font-bold mb-6 text-sky-700">Search Players</h2>

      {/* Filters */}
      <div className="flex flex-wrap gap-3 mb-6 items-center">
        <input
          type="text"
          className="border p-2 rounded-lg flex-1 shadow-sm focus:ring-2 focus:ring-sky-400"
          placeholder="Search by name..."
          value={search}
          onChange={e => {
            setSearch(e.target.value);
            setCurrentPage(1);
          }}
        />

        {/* Team Filter */}
        <input
          type="text"
          placeholder="Filter by team..."
          className="border p-2 rounded-lg shadow-sm"
          value={filterTeam}
          onChange={e => setFilterTeam(e.target.value)}
        />

        {/* Country Filter */}
        <input
          type="text"
          placeholder="Filter by country..."
          className="border p-2 rounded-lg shadow-sm"
          value={filterCountry}
          onChange={e => setFilterCountry(e.target.value)}
        />

        {/* Position Filter */}
        <input
          type="text"
          placeholder="Filter by position..."
          className="border p-2 rounded-lg shadow-sm"
          value={filterPosition}
          onChange={e => setFilterPosition(e.target.value)}
        />

        {/* Sort */}
        <select
          className="border p-2 rounded-lg shadow-sm"
          value={sortBy}
          onChange={e => setSortBy(e.target.value)}
        >
          <option value="name">Name</option>
          <option value="team">Team</option>
          <option value="country">Country</option>
          <option value="position">Position</option>
          <option value="age">Age</option>
          <option value="price">Price</option>
        </select>

        <select
          className="border p-2 rounded-lg shadow-sm"
          value={sortOrder}
          onChange={e => setSortOrder(e.target.value)}
        >
          <option value="asc">⬆ Asc</option>
          <option value="desc">⬇ Desc</option>
        </select>

        {/* Free agent checkbox */}
        <label className="flex items-center gap-2 ml-3">
          <input
            type="checkbox"
            checked={filterFreeAgents}
            onChange={e => setFilterFreeAgents(e.target.checked)}
          />
          Free Agents
        </label>
      </div>

      {/* Table */}
      <div className="overflow-x-auto rounded-lg border">
        <table className="w-full border-collapse">
          <thead>
            <tr className="bg-sky-50 text-sky-700">
              <th className="p-3 border">Name</th>
              <th className="p-3 border">Team</th>
              <th className="p-3 border">Country</th>
              <th className="p-3 border">Position</th>
              <th className="p-3 border">Age</th>
              <th className="p-3 border">Price</th>
            </tr>
          </thead>
          <tbody>
            {players.map(p => (
              <tr key={p.id} className="text-center hover:bg-sky-50 transition-colors">
                <td className="p-2 border font-medium">{p.name}</td>
                <td className="p-2 border">{p.team}</td>
                <td className="p-2 border">{p.country}</td>
                <td className="p-2 border">{p.position}</td>
                <td className="p-2 border">{p.age}</td>
                <td className="p-2 border">{p.price}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Pagination */}
      <div className="flex justify-between items-center mt-6">
        <button
          className="px-4 py-2 bg-sky-100 hover:bg-sky-200 text-sky-700 rounded-lg disabled:opacity-50 transition"
          disabled={currentPage === 1}
          onClick={() => setCurrentPage(p => p - 1)}
        >
          Назад
        </button>
        <span className="font-medium text-sky-700">
          Страница {currentPage} от {totalPages || 1}
        </span>
        <button
          className="px-4 py-2 bg-sky-100 hover:bg-sky-200 text-sky-700 rounded-lg disabled:opacity-50 transition"
          disabled={currentPage >= totalPages}
          onClick={() => setCurrentPage(p => p + 1)}
        >
          Напред
        </button>
      </div>
    </div>
  );
}
