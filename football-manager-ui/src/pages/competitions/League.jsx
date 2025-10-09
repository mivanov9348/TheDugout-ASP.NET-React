import React, { useEffect, useState } from "react";
import { NavLink, useLocation, useNavigate } from "react-router-dom";
import Standings from "./league/Standings";
import PlayerStats from "./league/PlayerStats";

const League = ({ gameSaveId }) => {
  const [leagues, setLeagues] = useState([]);
  const [selectedLeagueId, setSelectedLeagueId] = useState(null);
  const [standings, setStandings] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const location = useLocation();
  const navigate = useNavigate();

  // 🔹 1. Зареждаме лигите
  useEffect(() => {
    if (!gameSaveId) return;

    const fetchLeagues = async () => {
      try {
        setLoading(true);
        const res = await fetch(`/api/League/${gameSaveId}`);
        if (!res.ok) throw new Error(`HTTP ${res.status}`);
        const data = await res.json();

        const leaguesList = data.leagues || data.Leagues || [];
        console.log("📦 Leagues from backend:", leaguesList);

        setLeagues(leaguesList);

        if (leaguesList.length > 0) {
          const firstLeague = leaguesList[0];
          setSelectedLeagueId(firstLeague.Id);
          setStandings(firstLeague.Standings || []);

          // ✅ Ако не сме на /standings, го правим
          if (!location.pathname.endsWith("standings")) {
            navigate("standings", { replace: true });
          }
        }
      } catch (err) {
        console.error("❌ Error fetching leagues:", err);
        setError("Грешка при зареждане на лигите.");
      } finally {
        setLoading(false);
      }
    };

    fetchLeagues();
  }, [gameSaveId]);

  // 🔹 2. При промяна на dropdown
  const handleLeagueChange = (id) => {
    setSelectedLeagueId(id);
    const league = leagues.find((l) => l.Id === Number(id));
    if (league) {
      console.log("⚽ Selected League:", league);
      setStandings(league.Standings || []);
    }
  };

  if (loading)
    return <p className="text-gray-400 text-center mt-4">⏳ Зареждане...</p>;
  if (error)
    return (
      <p className="text-red-500 text-center mt-4 font-semibold">{error}</p>
    );
  if (leagues.length === 0)
    return (
      <p className="text-gray-400 text-center mt-4">
        Няма намерени лиги за този сейв.
      </p>
    );

  const isStandings = location.pathname.endsWith("standings");
  const isPlayerStats = location.pathname.endsWith("player-stats");

  return (
    <div className="text-white bg-gray-900 rounded-xl p-6 shadow-lg space-y-6">
      {/* Dropdown за избор на лига */}
      <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-3">
        <label className="text-gray-300 font-semibold text-lg">
          Избери лига:
        </label>
        <select
          value={selectedLeagueId || ""}
          onChange={(e) => handleLeagueChange(e.target.value)}
          className="bg-gray-800 text-white px-4 py-2 rounded-md border border-gray-700 focus:outline-none focus:ring-2 focus:ring-sky-500"
        >
          {leagues.map((league) => (
            <option key={league.Id} value={league.Id}>
              {league.Name} ({league.Country})
            </option>
          ))}
        </select>
      </div>

      {/* Табове */}
      <div className="flex gap-3 border-b border-gray-700 pb-2">
        <NavLink
          to="standings"
          className={({ isActive }) =>
            `px-4 py-2 rounded-lg font-bold transition ${
              isActive
                ? "bg-sky-600 text-white"
                : "bg-gray-700 hover:bg-gray-600"
            }`
          }
        >
          Standings
        </NavLink>
        <NavLink
          to="player-stats"
          className={({ isActive }) =>
            `px-4 py-2 rounded-lg font-bold transition ${
              isActive
                ? "bg-sky-600 text-white"
                : "bg-gray-700 hover:bg-gray-600"
            }`
          }
        >
          Player Stats
        </NavLink>
      </div>

      {/* Съдържание */}
      <div className="pt-4">
        {isStandings && <Standings standings={standings} />}
        {isPlayerStats && <PlayerStats leagueId={selectedLeagueId} />}
      </div>
    </div>
  );
};

export default League;
