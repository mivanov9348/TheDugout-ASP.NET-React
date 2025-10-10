import { useEffect, useState } from "react";
import { Outlet, NavLink, useNavigate, useLocation } from "react-router-dom";

export default function League({ gameSaveId }) {
  const [leagues, setLeagues] = useState([]);
  const [selectedLeague, setSelectedLeague] = useState(null);
  const [loading, setLoading] = useState(true);

  const navigate = useNavigate();
  const location = useLocation();

  useEffect(() => {
    if (!gameSaveId) return;

    const loadLeagues = async () => {
      setLoading(true);
      try {
        const res = await fetch(`/api/League/${gameSaveId}`, { credentials: "include" });
        if (!res.ok) throw new Error("Error fetching leagues");
        const data = await res.json();
        if (data?.leagues?.length > 0) {
          setLeagues(data.leagues);
          setSelectedLeague(data.leagues[0]);
          // Ако сме на /league директно, пренасочваме към първата лига
          if (location.pathname.endsWith("/league")) {
            navigate(`/competitions/league/standings`, { replace: true });
          }
        }
      } catch (err) {
        console.error("❌ Error loading leagues:", err);
      } finally {
        setLoading(false);
      }
    };

    loadLeagues();
  }, [gameSaveId]);

  const handleLeagueChange = (e) => {
    const leagueId = Number(e.target.value);
    const league = leagues.find((l) => l.id === leagueId);
    setSelectedLeague(league);

    // Взимаме текущия таб (standings / player-stats)
    const currentTab = location.pathname.includes("player-stats")
      ? "player-stats"
      : "standings";

    // Навигираме към същия таб, но новата лига
    navigate(`/competitions/league/${currentTab}`, { replace: true });
  };



  if (loading) return <p>Loading leagues...</p>;
  if (!leagues.length) return <p>No leagues found.</p>;

  return (
    <div className="p-4">
      {/* Header */}
      <div className="flex items-center justify-center gap-4 mb-6">
        <img
          src={selectedLeague.standings[0]?.teamLogo ?? "/competitionsLogos/default.png"}
          alt={selectedLeague.name}
          className="w-16 h-16 object-contain border rounded-full shadow-md"
          onError={(e) => (e.target.src = "/competitionsLogos/default.png")}
        />
        <h2 className="text-3xl font-bold text-center">{selectedLeague.name}</h2>
      </div>

      {/* Dropdown за смяна на лигата */}
      <div className="flex justify-center mb-4">
        <select
          className="border rounded px-3 py-1 text-sm"
          value={selectedLeague.id}
          onChange={handleLeagueChange}
        >
          {leagues.map((l) => (
            <option key={l.id} value={l.id}>
              {l.name} ({l.country})
            </option>
          ))}
        </select>
      </div>

      {/* Navigation Links (absolute paths) */}
      <div className="flex justify-center mb-6 gap-2">
        <NavLink
          to="/competitions/league/standings"
          className={({ isActive }) =>
            `px-4 py-2 rounded-md font-medium ${isActive
              ? "bg-blue-600 text-white"
              : "bg-slate-200 text-slate-700 hover:bg-slate-300"
            }`
          }
        >
          Standings
        </NavLink>
        <NavLink
          to="/competitions/league/player-stats"
          className={({ isActive }) =>
            `px-4 py-2 rounded-md font-medium ${isActive
              ? "bg-blue-600 text-white"
              : "bg-slate-200 text-slate-700 hover:bg-slate-300"
            }`
          }
        >
          Player Stats
        </NavLink>
      </div>

      {/* Outlet за подстраници с контекст на избраната лига */}
      <Outlet context={{ gameSaveId, league: selectedLeague }} />
    </div>
  );
}
