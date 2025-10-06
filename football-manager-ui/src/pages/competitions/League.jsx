import { NavLink, Outlet, useLocation } from "react-router-dom";
import React, { useEffect, useState } from "react";
import TeamLogo from "../../components/TeamLogo";

const League = ({ gameSaveId }) => {
  const [leagues, setLeagues] = useState([]);
  const [selectedLeague, setSelectedLeague] = useState(null);
  const [myTeamId, setMyTeamId] = useState(null);
  const [currentSeasonId, setCurrentSeasonId] = useState(null);
  const [seasons, setSeasons] = useState([]);

  const location = useLocation();

  useEffect(() => {
    if (!gameSaveId) return;

    const fetchLeaguesAndSeasons = async () => {
      try {
        const token = localStorage.getItem("token");
        const seasonsRes = await fetch(`/api/leagues/seasons?gameSaveId=${gameSaveId}`, {
          headers: { Authorization: `Bearer ${token}` },
        });
        const seasonsData = await seasonsRes.json();
        setSeasons(seasonsData);

        const latestSeason = seasonsData.length > 0 ? seasonsData[0] : null;
        setCurrentSeasonId(latestSeason?.id);

        const res = await fetch(`/api/leagues?gameSaveId=${gameSaveId}&seasonId=${latestSeason?.id}`, {
          headers: { Authorization: `Bearer ${token}` },
        });
        const data = await res.json();
        setLeagues(data);

        const myLeague = data.find(l => l.hasMyTeam);
        if (myLeague && myLeague.teams.length > 0) {
          setMyTeamId(myLeague.teams[0].id);
        }
        setSelectedLeague(data[0]);
      } catch (err) {
        console.error("Error fetching leagues or seasons:", err);
      }
    };

    fetchLeaguesAndSeasons();
  }, [gameSaveId]);

  const handleLeagueChange = (e) => {
    const leagueId = parseInt(e.target.value);
    const league = leagues.find((l) => l.id === leagueId);
    setSelectedLeague(league);
  };

  const handleSeasonChange = (e) => {
    const seasonId = parseInt(e.target.value);
    setCurrentSeasonId(seasonId);

    const token = localStorage.getItem("token");
    fetch(`/api/leagues?gameSaveId=${gameSaveId}&seasonId=${seasonId}`, {
      headers: { Authorization: `Bearer ${token}` },
    })
      .then(res => res.json())
      .then(data => {
        setLeagues(data);
        setSelectedLeague(data[0]);
      })
      .catch(err => console.error("Error fetching league with new season:", err));
  };

  if (!gameSaveId) return <div className="p-6 text-gray-500">Зареждане...</div>;

  return (
    <div className="p-6 flex flex-col gap-4">
      {/* Табове */}
      <div className="flex gap-3 border-b pb-2">
        <NavLink
          to="/competitions/league/standings"
          className={({ isActive }) =>
            `px-4 py-2 rounded-lg font-bold transition 
            ${isActive ? "bg-sky-600 text-white" : "bg-slate-200 hover:bg-slate-300"}`
          }
          end
        >
          Standings
        </NavLink>
        <NavLink
          to="/competitions/league/player-stats"
          className={({ isActive }) =>
            `px-4 py-2 rounded-lg font-bold transition 
            ${isActive ? "bg-sky-600 text-white" : "bg-slate-200 hover:bg-slate-300"}`
          }
          end
        >
          Player Stats
        </NavLink>
      </div>

      {/* Съдържание на избрания таб */}
      <div className="p-4 bg-white rounded-xl shadow">
        <Outlet
          context={{
            selectedLeague,
            leagues,
            myTeamId,
            seasons,
            currentSeasonId,
            handleLeagueChange,
            handleSeasonChange,
          }}
        />
      </div>
    </div>
  );
};

export default League;
