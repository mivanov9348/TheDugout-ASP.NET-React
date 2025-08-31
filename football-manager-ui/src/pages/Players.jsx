// src/pages/Players.jsx
import { useEffect, useState } from "react";

function Players({ gameSaveId }) {
  const [players, setPlayers] = useState([]);
  const [filters, setFilters] = useState({
    teamId: "",
    countryId: "",
    positionId: "",
    minAge: "",
    maxAge: "",
    sortBy: "name",
    sortOrder: "asc"
  });

  useEffect(() => {
    if (gameSaveId) {
      fetchPlayers();
    }
  }, [filters, gameSaveId]);

  const fetchPlayers = async () => {
    const query = new URLSearchParams(
      Object.fromEntries(
        Object.entries(filters).filter(([_, v]) => v !== "" && v !== null)
      )
    );

const res = await fetch(`/api/players?gameSaveId=${gameSaveId}&${query}`);
    if (res.ok) {
      const data = await res.json();
      setPlayers(data);
    }
  };


  const handleChange = (e) => {
    setFilters({ ...filters, [e.target.name]: e.target.value });
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
        <select name="sortBy" value={filters.sortBy} onChange={handleChange} className="border p-2 rounded">
          <option value="name">Име</option>
          <option value="team">Отбор</option>
          <option value="country">Държава</option>
          <option value="age">Възраст</option>
          <option value="position">Позиция</option>
        </select>
        <select name="sortOrder" value={filters.sortOrder} onChange={handleChange} className="border p-2 rounded">
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
          {players.map((p) => (
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
    </div>
  );
}

export default Players;
