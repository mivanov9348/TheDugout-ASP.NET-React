// src/pages/Players.jsx
import { useEffect, useState, useMemo } from "react";

function Players({ gameSaveId }) {
  const [players, setPlayers] = useState([]);
  const [filters, setFilters] = useState({
    minAge: "",
    maxAge: "",
    search: "",
    sortBy: "name",
    sortOrder: "asc",
  });

  const [page, setPage] = useState(1);
  const pageSize = 20;

  // 1. Зареждаме всички играчи веднъж
  useEffect(() => {
    if (gameSaveId) {
      fetch(`/api/players?gameSaveId=${gameSaveId}`)
        .then((res) => res.json())
        .then((data) => setPlayers(data));
    }
  }, [gameSaveId]);

  // 2. Прилагаме филтри и сортиране локално
  const filteredPlayers = useMemo(() => {
    let result = [...players];

    // Филтър по възраст
    if (filters.minAge) result = result.filter((p) => p.age >= Number(filters.minAge));
    if (filters.maxAge) result = result.filter((p) => p.age <= Number(filters.maxAge));

    // Филтър по търсене (име/отбор/държава/позиция)
    if (filters.search) {
      const s = filters.search.toLowerCase();
      result = result.filter(
        (p) =>
          p.name.toLowerCase().includes(s) ||
          p.team.toLowerCase().includes(s) ||
          p.country.toLowerCase().includes(s) ||
          p.position.toLowerCase().includes(s)
      );
    }

    // Сортиране
    result.sort((a, b) => {
      let valA, valB;
      switch (filters.sortBy) {
        case "team":
          valA = a.team;
          valB = b.team;
          break;
        case "country":
          valA = a.country;
          valB = b.country;
          break;
        case "age":
          valA = a.age;
          valB = b.age;
          break;
        case "position":
          valA = a.position;
          valB = b.position;
          break;
        default:
          valA = a.name;
          valB = b.name;
      }
      if (typeof valA === "string") {
        valA = valA.toLowerCase();
        valB = valB.toLowerCase();
      }
      if (valA < valB) return filters.sortOrder === "asc" ? -1 : 1;
      if (valA > valB) return filters.sortOrder === "asc" ? 1 : -1;
      return 0;
    });

    return result;
  }, [players, filters]);

  // 3. Педжиране
  const totalPages = Math.ceil(filteredPlayers.length / pageSize);
  const pagedPlayers = filteredPlayers.slice((page - 1) * pageSize, page * pageSize);

  const handleChange = (e) => {
    setFilters({ ...filters, [e.target.name]: e.target.value });
    setPage(1); // връща се на първа страница при нов филтър
  };

  return (
    <div className="p-4 bg-white rounded-xl shadow">
      <h1 className="text-xl font-bold mb-4">Играчите в лигата</h1>

      {/* Филтри */}
      <div className="grid grid-cols-6 gap-2 mb-4">
        <input
          type="number"
          name="minAge"
          placeholder="Мин. възраст"
          value={filters.minAge}
          onChange={handleChange}
          className="border p-2 rounded"
        />
        <input
          type="number"
          name="maxAge"
          placeholder="Макс. възраст"
          value={filters.maxAge}
          onChange={handleChange}
          className="border p-2 rounded"
        />
        <input
          type="text"
          name="search"
          placeholder="Търсене..."
          value={filters.search}
          onChange={handleChange}
          className="border p-2 rounded col-span-2"
        />
        <select
          name="sortBy"
          value={filters.sortBy}
          onChange={handleChange}
          className="border p-2 rounded"
        >
          <option value="name">Име</option>
          <option value="team">Отбор</option>
          <option value="country">Държава</option>
          <option value="age">Възраст</option>
          <option value="position">Позиция</option>
        </select>
        <select
          name="sortOrder"
          value={filters.sortOrder}
          onChange={handleChange}
          className="border p-2 rounded"
        >
          <option value="asc">Възходящо</option>
          <option value="desc">Низходящо</option>
        </select>
      </div>

      {/* Таблица */}
      <table className="w-full border-collapse">
        <thead>
          <tr className="bg-slate-200">
            <th className="p-2 border">Име</th>
            <th className="p-2 border">Отбор</th>
            <th className="p-2 border">Държава</th>
            <th className="p-2 border">Позиция</th>
            <th className="p-2 border">Възраст</th>
            <th className="p-2 border">Атрибути</th>
          </tr>
        </thead>
        <tbody>
          {pagedPlayers.map((p) => (
            <tr key={p.id} className="hover:bg-slate-100">
              <td className="p-2 border">{p.name}</td>
              <td className="p-2 border">{p.team}</td>
              <td className="p-2 border">{p.country}</td>
              <td className="p-2 border">{p.position}</td>
              <td className="p-2 border">{p.age}</td>
              <td className="p-2 border">
                {p.attributes.map((a) => (
                  <span key={a.name} className="mr-2">
                    {a.name}: {a.value}
                  </span>
                ))}
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      {/* Педжинация */}
      <div className="flex justify-center mt-4 gap-2">
        <button
          onClick={() => setPage((p) => Math.max(1, p - 1))}
          disabled={page === 1}
          className="px-3 py-1 border rounded disabled:opacity-50"
        >
          Назад
        </button>
        <span className="px-3 py-1">
          Стр. {page} от {totalPages}
        </span>
        <button
          onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
          disabled={page === totalPages}
          className="px-3 py-1 border rounded disabled:opacity-50"
        >
          Напред
        </button>
      </div>
    </div>
  );
}

export default Players;
