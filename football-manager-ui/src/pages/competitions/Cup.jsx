import { useEffect, useState } from "react";
import { NavLink, Outlet } from "react-router-dom";
import { useActiveSeason } from "../../components/useActiveSeason";

const Cup = ({ gameSaveId }) => {
  const { season, loading: seasonLoading, error: seasonError } = useActiveSeason(gameSaveId);
  const [cups, setCups] = useState([]);
  const [selectedCupId, setSelectedCupId] = useState(null);
  const [loading, setLoading] = useState(true);

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
  }, [gameSaveId, season]);

  const selectedCup = cups.find((c) => c.id === selectedCupId);

  if (seasonLoading)
    return <div className="text-gray-300 p-6">Loading active season...</div>;

  if (seasonError)
    return <div className="text-red-400 p-6">Error: {seasonError}</div>;

  return (
    <div className="min-h-screen bg-gradient-to-b from-gray-900 via-gray-800 to-gray-900 text-gray-100 p-6">
      <div >       

        {/* Cup selector */}
        <div className="mb-10">
          <label
            htmlFor="cup-select"
            className="block text-sm font-medium text-gray-300 mb-2"
          >
            Select a cup:
          </label>
          <select
            id="cup-select"
            value={selectedCupId || ""}
            onChange={(e) => setSelectedCupId(parseInt(e.target.value))}
            className="w-full bg-gray-800 border border-gray-700 rounded-lg px-4 py-3 pr-10 
                       focus:outline-none focus:ring-2 focus:ring-sky-500 focus:border-transparent
                       shadow-sm text-gray-200 cursor-pointer"
          >
            {cups.map((cup) => (
              <option key={cup.id} value={cup.id} className="bg-gray-800 text-gray-200">
                {cup.templateName} ({cup.countryName})
              </option>
            ))}
          </select>
        </div>

        {/* Поднавигация */}
        <div className="flex gap-3 border-b border-gray-700 pb-3 mb-6 justify-center">
          <NavLink
            to="/competitions/cup/knockouts"
            className={({ isActive }) =>
              `px-5 py-2 rounded-lg font-semibold transition duration-200 ${
                isActive
                  ? "bg-sky-600 text-white shadow-lg"
                  : "bg-gray-700 hover:bg-gray-600 text-gray-200"
              }`
            }
          >
            Knockouts
          </NavLink>

          <NavLink
            to="/competitions/cup/player-stats"
            className={({ isActive }) =>
              `px-5 py-2 rounded-lg font-semibold transition duration-200 ${
                isActive
                  ? "bg-sky-600 text-white shadow-lg"
                  : "bg-gray-700 hover:bg-gray-600 text-gray-200"
              }`
            }
          >
            Player Stats
          </NavLink>
        </div>

        {/* Съдържание */}
        <div >
          {loading ? (
            <div className="text-gray-400">Loading cups...</div>
          ) : !selectedCup ? (
            <div className="text-gray-400">No cups available</div>
          ) : (
            <Outlet context={{ selectedCup }} />
          )}
        </div>
      </div>
    </div>
  );
};

export default Cup;
