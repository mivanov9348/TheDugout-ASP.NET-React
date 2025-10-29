import { useEffect, useState } from "react";
import { Outlet, NavLink, useNavigate, useLocation } from "react-router-dom";

export default function League({ gameSaveId }) {
  const [leagues, setLeagues] = useState([]);
  const [selectedLeague, setSelectedLeague] = useState(null);
  const [seasonId, setSeasonId] = useState(null);
  const [loading, setLoading] = useState(true);

  const navigate = useNavigate();
  const location = useLocation();

  // 🟢 Зареждане на лиги + първа по подразбиране
  useEffect(() => {
    if (!gameSaveId) return;

    const loadLeagues = async () => {
      setLoading(true);
      try {
        const res = await fetch(`/api/League/${gameSaveId}`, {
          credentials: "include",
        });
        if (!res.ok) throw new Error("Error fetching leagues");
        const data = await res.json();

        if (data?.leagues?.length > 0) {
          setLeagues(data.leagues);
          setSeasonId(data.seasonId);

          let leagueToLoad = data.leagues.find(
            (l) => l.id === selectedLeague?.id
          );

          if (!leagueToLoad) {
            leagueToLoad = data.leagues[0];
          }

          const res2 = await fetch(
            `/api/League/current?gameSaveId=${gameSaveId}&seasonId=${data.seasonId}&leagueId=${leagueToLoad.id}`,
            { credentials: "include" }
          );
          const leagueData = await res2.json();

          setSelectedLeague(
            leagueData.exists
              ? { ...leagueToLoad, standings: leagueData.standings }
              : { ...leagueToLoad, standings: [] }
          );

          if (location.pathname.endsWith("/league")) {
            navigate(`/competitions/league/standings`, { replace: true });
          }
        } else {
          setLeagues([]);
          setSeasonId(null);
          setSelectedLeague(null);
        }
      } catch (err) {
        console.error("❌ Error loading leagues:", err);
      } finally {
        setLoading(false);
      }
    };

    loadLeagues();
  }, [gameSaveId, navigate, location.pathname]);

  // 🟢 Смяна на лига от dropdown
  const handleLeagueChange = async (e) => {
    const leagueId = Number(e.target.value);
    const league = leagues.find((l) => l.id === leagueId);
    if (!league || !seasonId) return;

    try {
      setLoading(true);
      const res = await fetch(
        `/api/League/current?gameSaveId=${gameSaveId}&seasonId=${seasonId}&leagueId=${leagueId}`,
        { credentials: "include" }
      );
      const data = await res.json();

      if (data.exists) {
        setSelectedLeague({ ...league, standings: data.standings });
      } else {
        setSelectedLeague({ ...league, standings: [] });
      }

      const currentTab = location.pathname.includes("player-stats")
        ? "player-stats"
        : "standings";
      navigate(`/competitions/league/${currentTab}`, { replace: true });
    } catch (err) {
      console.error("❌ Error loading league standings:", err);
    } finally {
      setLoading(false);
    }
  };

  // 🟢 UI Rendering
  if (loading && !selectedLeague)
    return (
      <div className="min-h-screen flex items-center justify-center bg-gradient-to-b from-gray-900 via-gray-800 to-gray-900 text-gray-300">
        Loading leagues...
      </div>
    );

  if (!leagues.length)
    return (
      <div className="min-h-screen flex items-center justify-center bg-gradient-to-b from-gray-900 via-gray-800 to-gray-900 text-gray-400">
        No leagues found.
      </div>
    );

  return (
    <div className="min-h-screen bg-gradient-to-b from-gray-900 via-gray-800 to-gray-900 text-gray-200 p-6 rounded-2xl shadow-2xl">
      {/* Header */}
     
      {/* Dropdown за смяна на лигата */}
      <div className="flex justify-center mb-6">
        <select
          className="bg-gray-800/80 border border-gray-700 rounded-lg px-4 py-2 text-sm text-gray-200 focus:outline-none focus:ring-2 focus:ring-sky-600"
          value={selectedLeague?.id ?? ""}
          onChange={handleLeagueChange}
        >
          {leagues.map((l) => (
            <option key={l.id} value={l.id} className="bg-gray-900 text-gray-200">
              {l.name} ({l.country})
            </option>
          ))}
        </select>
      </div>

      {/* Навигация между табове */}
      <div className="flex justify-center mb-8 gap-3">
        <NavLink
          to="/competitions/league/standings"
          className={({ isActive }) =>
            `px-5 py-2 rounded-lg font-semibold transition-all duration-200 
            ${
              isActive
                ? "bg-sky-600 text-white shadow-md shadow-sky-700/40"
                : "bg-gray-800 hover:bg-gray-700 text-gray-300 border border-gray-700"
            }`
          }
        >
          Standings
        </NavLink>

        <NavLink
          to="/competitions/league/player-stats"
          className={({ isActive }) =>
            `px-5 py-2 rounded-lg font-semibold transition-all duration-200 
            ${
              isActive
                ? "bg-sky-600 text-white shadow-md shadow-sky-700/40"
                : "bg-gray-800 hover:bg-gray-700 text-gray-300 border border-gray-700"
            }`
          }
        >
          Player Stats
        </NavLink>
      </div>

      {/* Outlet с контекста на избраната лига */}
      <div className="bg-gray-800/60 backdrop-blur-sm border border-gray-700 rounded-xl shadow-lg p-5 transition-all">
        <Outlet context={{ gameSaveId, league: selectedLeague }} />
      </div>
    </div>
  );
}
