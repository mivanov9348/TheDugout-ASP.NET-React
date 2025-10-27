import { useEffect, useState } from "react";
import { NavLink, Outlet } from "react-router-dom";
import { useActiveSeason } from "../../components/useActiveSeason";
const Cup = ({ gameSaveId }) => {
  const { season, loading: seasonLoading, error: seasonError } = useActiveSeason(gameSaveId);
  const [cups, setCups] = useState([]);
  const [selectedCupId, setSelectedCupId] = useState(null);
  const [loading, setLoading] = useState(true);

  // Зареждане на купите след като имаме активен сезон
  useEffect(() => {
    const fetchCups = async () => {
      if (!gameSaveId || !season?.id) return;
      try {
        setLoading(true);
        const res = await fetch(`/api/cup/${gameSaveId}/${season.id}`, {
          credentials: "include",
        });
        if (!res.ok) throw new Error("Error loading cups");
        const data = await res.json();
        setCups(data);
        if (data.length > 0) setSelectedCupId(data[0].id);
      } catch (err) {
        console.error("❌ Error loading cups:", err);
      } finally {
        setLoading(false);
      }
    };

    fetchCups();
  }, [gameSaveId, season]); // 👈 когато сезонът се зареди или смени

  const selectedCup = cups.find((c) => c.id === selectedCupId);

  // Ако сезонът още се зарежда
  if (seasonLoading)
    return <div className="text-gray-500 p-6">Loading active season...</div>;

  if (seasonError)
    return <div className="text-red-600 p-6">Error: {seasonError}</div>;

  return (
    <div className="p-6 bg-gray-50 min-h-screen rounded-xl shadow">
      <h2 className="text-3xl font-bold text-gray-800 mb-6">Cups</h2>

      {/* Cup selector */}
      <div className="mb-8">
        <label
          htmlFor="cup-select"
          className="block text-sm font-medium text-gray-700 mb-2"
        >
          Select a cup:
        </label>
        <select
          id="cup-select"
          value={selectedCupId || ""}
          onChange={(e) => setSelectedCupId(parseInt(e.target.value))}
          className="appearance-none bg-white border border-gray-300 rounded-lg px-4 py-3 pr-10 focus:outline-none focus:ring-2 focus:ring-sky-500 focus:border-transparent shadow-sm text-gray-700 cursor-pointer"
        >
          {cups.map((cup) => (
            <option key={cup.id} value={cup.id}>
              {cup.templateName} ({cup.countryName})
            </option>
          ))}
        </select>
      </div>

      {/* Поднавигация за Cup */}
      <div className="flex gap-3 border-b pb-2 mb-6">
        <NavLink
          to="/competitions/cup/knockouts"
          className={({ isActive }) =>
            `px-4 py-2 rounded-lg font-bold transition ${
              isActive ? "bg-sky-600 text-white" : "bg-slate-200 hover:bg-slate-300"
            }`
          }
        >
          Knockouts
        </NavLink>

        <NavLink
          to="/competitions/cup/player-stats"
          className={({ isActive }) =>
            `px-4 py-2 rounded-lg font-bold transition ${
              isActive ? "bg-sky-600 text-white" : "bg-slate-200 hover:bg-slate-300"
            }`
          }
        >
          Player Stats
        </NavLink>
      </div>

      {/* Съдържание на подстраниците */}
      <div className="p-4 bg-white rounded-xl shadow">
        {loading ? (
          <div className="text-gray-500">Loading cups...</div>
        ) : !selectedCup ? (
          <div className="text-gray-500">No cups available</div>
        ) : (
          <Outlet context={{ selectedCup }} />
        )}
      </div>
    </div>
  );
};

export default Cup;
