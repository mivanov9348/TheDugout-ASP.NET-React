import { useEffect, useMemo, useState } from "react";

export default function Players({ gameSaveId }) {
  const [players, setPlayers] = useState([]);
  const [attributes, setAttributes] = useState([]);
  const [search, setSearch] = useState("");
  const [sortBy, setSortBy] = useState("name");
  const [sortOrder, setSortOrder] = useState("asc");
  const [currentPage, setCurrentPage] = useState(1);
  const playersPerPage = 15;

  useEffect(() => {
    if (gameSaveId) {
      fetch(`/api/players?gameSaveId=${gameSaveId}`)
        .then(res => res.json())
        .then(data => setPlayers(data));

      fetch(`/api/players/attributes?gameSaveId=${gameSaveId}`)
        .then(res => res.json())
        .then(data => setAttributes(data));
    }
  }, [gameSaveId]);

  const filteredPlayers = useMemo(() => {
    let filtered = players.filter(p =>
      p.name.toLowerCase().includes(search.toLowerCase())
    );

    filtered.sort((a, b) => {
      let valA, valB;

      if (sortBy.startsWith("attribute:")) {
        const attrId = parseInt(sortBy.split(":")[1]);
        valA = a.attributes.find(x => x.attributeId === attrId)?.value ?? 0;
        valB = b.attributes.find(x => x.attributeId === attrId)?.value ?? 0;
      } else {
        switch (sortBy) {
          case "name": valA = a.name; valB = b.name; break;
          case "team": valA = a.team; valB = b.team; break;
          case "country": valA = a.country; valB = b.country; break;
          case "position": valA = a.position; valB = b.position; break;
          case "age": valA = a.age; valB = b.age; break;
          default: valA = ""; valB = ""; break;
        }
      }

      if (valA < valB) return sortOrder === "asc" ? -1 : 1;
      if (valA > valB) return sortOrder === "asc" ? 1 : -1;
      return 0;
    });

    return filtered;
  }, [players, search, sortBy, sortOrder]);

  const pagedPlayers = filteredPlayers.slice(
    (currentPage - 1) * playersPerPage,
    currentPage * playersPerPage
  );

  return (
    <div className="mt-6 p-4 border rounded bg-white shadow">
      <h2 className="text-xl font-bold mb-4">Играчите</h2>

      <div className="flex gap-2 mb-4">
        <input
          type="text"
          className="border p-2 rounded w-full"
          placeholder="Search by name..."
          value={search}
          onChange={e => setSearch(e.target.value)}
        />
        <select
          className="border p-2 rounded"
          value={sortBy}
          onChange={e => setSortBy(e.target.value)}
        >
          <option value="name">Name</option>
          <option value="team">Team</option>
          <option value="country">Country</option>
          <option value="position">Position</option>
          <option value="age">Age</option>
          {attributes.map(attr => (
            <option key={attr.id} value={`attribute:${attr.id}`}>
              {attr.name}
            </option>
          ))}
        </select>
        <select
          className="border p-2 rounded"
          value={sortOrder}
          onChange={e => setSortOrder(e.target.value)}
        >
          <option value="asc">⬆</option>
          <option value="desc">⬇</option>
        </select>
      </div>

      <div className="overflow-x-auto">
        <table className="w-full border-collapse border">
          <thead>
            <tr className="bg-gray-100">
              <th className="p-2 border">Name</th>
              <th className="p-2 border">Team</th>
              <th className="p-2 border">Country</th>
              <th className="p-2 border">Position</th>
              <th className="p-2 border">Age</th>
              {attributes.map(attr => (
                <th key={attr.id} className="p-2 border">{attr.name}</th>
              ))}
            </tr>
          </thead>
          <tbody>
            {pagedPlayers.map(p => (
              <tr key={p.id} className="text-center">
                <td className="p-2 border">{p.name}</td>
                <td className="p-2 border">{p.team}</td>
                <td className="p-2 border">{p.country}</td>
                <td className="p-2 border">{p.position}</td>
                <td className="p-2 border">{p.age}</td>
                {attributes.map(attr => {
                  const found = p.attributes.find(a => a.attributeId === attr.id);
                  return (
                    <td key={attr.id} className="p-2 border">
                      {found ? found.value : "-"}
                    </td>
                  );
                })}
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Pagination */}
      <div className="flex justify-between items-center mt-4">
        <button
          className="px-4 py-2 bg-gray-200 rounded disabled:opacity-50"
          disabled={currentPage === 1}
          onClick={() => setCurrentPage(p => p - 1)}
        >
          Назад
        </button>
        <span>
          Страница {currentPage} от{" "}
          {Math.ceil(filteredPlayers.length / playersPerPage)}
        </span>
        <button
          className="px-4 py-2 bg-gray-200 rounded disabled:opacity-50"
          disabled={currentPage >= Math.ceil(filteredPlayers.length / playersPerPage)}
          onClick={() => setCurrentPage(p => p + 1)}
        >
          Напред
        </button>
      </div>
    </div>
  );
}
