import { useEffect, useState } from "react";
import { Outlet, NavLink, useNavigate, useLocation } from "react-router-dom";

export default function League({ gameSaveId }) {
  const [leagues, setLeagues] = useState([]);
  const [selectedLeague, setSelectedLeague] = useState(null);
  const [seasonId, setSeasonId] = useState(null);
  const [loading, setLoading] = useState(true);

  const navigate = useNavigate();
  const location = useLocation();

  // 🟢 Зареждаме всички лиги + първата по подразбиране
  // League.jsx

  // 🟢 Зареждаме всички лиги + първата по подразбиране
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

          // --- ❌ ИЗТРИЙ СТАРАТА ЛОГИКА ---
          // if (!selectedLeague) { ... }

          // --- ✅ ДОБАВИ НОВАТА ЛОГИКА ---

          // 1. Опитай се да намериш текущо избраната лига в новия списък
          // (Това ще се провали при смяна на gameSaveId, защото ID-тата ще са различни, 
          // но ще работи, ако просто презареждаш същия save)
          let leagueToLoad = data.leagues.find(
            (l) => l.id === selectedLeague?.id
          );

          // 2. Ако не е намерена (или при първо зареждане), вземи първата от списъка
          if (!leagueToLoad) {
            leagueToLoad = data.leagues[0];
          }

          // 3. Винаги зареждай класирането за тази лига
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
          // --- Край на новата логика ---

        } else {
          // Ако новият save няма лиги, изчисти всичко
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
    // Добави `location.pathname`, за да се изпълни навигацията, ако е нужно
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

      // 🟢 Навигация към текущия таб
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
  if (loading && !selectedLeague) return <p>Loading leagues...</p>;
  if (!leagues.length) return <p>No leagues found.</p>;

  return (
    <div className="p-4">
      {/* Header */}
      <div className="flex items-center justify-center gap-4 mb-6">
        <img
          src={
            selectedLeague?.standings?.[0]?.teamLogo ??
            "/competitionsLogos/default.png"
          }
          alt={selectedLeague?.name ?? "League"}
          className="w-16 h-16 object-contain border rounded-full shadow-md"
          onError={(e) => (e.target.src = "/competitionsLogos/default.png")}
        />
        <h2 className="text-3xl font-bold text-center">
          {selectedLeague?.name ?? "Unknown League"}
        </h2>
      </div>

      {/* Dropdown за смяна на лигата */}
      <div className="flex justify-center mb-4">
        <select
          className="border rounded px-3 py-1 text-sm"
          value={selectedLeague?.id ?? ""}
          onChange={handleLeagueChange}
        >
          {leagues.map((l) => (
            <option key={l.id} value={l.id}>
              {l.name} ({l.country})
            </option>
          ))}
        </select>
      </div>

      {/* Навигация между табове */}
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

      {/* Outlet с контекста на избраната лига */}
      <Outlet context={{ gameSaveId, league: selectedLeague }} />
    </div>
  );
}
