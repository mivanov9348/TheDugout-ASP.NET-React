import React, { useEffect, useState } from "react";
import TeamLogo from "../components/TeamLogo";

const League = ({ gameSaveId }) => {
  const [leagues, setLeagues] = useState([]);
  const [selectedLeague, setSelectedLeague] = useState(null);
  const [myTeamId, setMyTeamId] = useState(null);
  const [currentSeasonId, setCurrentSeasonId] = useState(null);
  const [seasons, setSeasons] = useState([]);

  useEffect(() => {
    if (!gameSaveId) return;

    const fetchLeaguesAndSeasons = async () => {
      try {
        const token = localStorage.getItem("token");

        // 1. Зареди всички сезони
        const seasonsRes = await fetch(`/api/leagues/seasons?gameSaveId=${gameSaveId}`, {
          headers: { Authorization: `Bearer ${token}` },
        });
        const seasonsData = await seasonsRes.json();
        setSeasons(seasonsData);

        const latestSeason = seasonsData.length > 0 ? seasonsData[0] : null;
        setCurrentSeasonId(latestSeason?.id);

        // 2. Зареди лигите с текущия сезон
        const res = await fetch(`/api/leagues?gameSaveId=${gameSaveId}&seasonId=${latestSeason?.id}`, {
          headers: { Authorization: `Bearer ${token}` },
        });
        const data = await res.json();
        setLeagues(data);

        // Намери моя отбор
        const myLeague = data.find(l => l.hasMyTeam);
        if (myLeague && myLeague.teams.length > 0) {
          setMyTeamId(myLeague.teams[0].id);
        }

        // Избери първата лига като подразбираща се
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

    // Препратя към API с нов сезон
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
    <div className="p-6">
      <h1 className="text-3xl font-extrabold mb-6 text-sky-700 tracking-wide">
        League Standings
      </h1>

      {leagues.length > 0 ? (
        <>
          {/* Dropdown: Избор на сезон */}
          {seasons.length > 1 && (
            <div className="mb-4">
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Season:
              </label>
              <select
                className="p-3 rounded-xl border-2 border-sky-500 bg-white text-gray-700 shadow-sm focus:outline-none focus:ring-2 focus:ring-sky-400 transition"
                value={currentSeasonId || ""}
                onChange={handleSeasonChange}
              >
                {seasons.map((season) => (
                  <option key={season.id} value={season.id}>
                    {season.name} ({new Date(season.startDate).getFullYear()} - {new Date(season.endDate).getFullYear()})
                  </option>
                ))}
              </select>
            </div>
          )}

          {/* Dropdown: Избор на лига */}
          <div className="mb-6">
            <label className="block text-sm font-medium text-gray-700 mb-1">
              League:
            </label>
            <select
              className="p-3 rounded-xl border-2 border-sky-500 bg-white text-gray-700 shadow-sm focus:outline-none focus:ring-2 focus:ring-sky-400 transition"
              onChange={handleLeagueChange}
              value={selectedLeague?.id || ""}
            >
              {leagues.map((league) => (
                <option key={league.id} value={league.id}>
                  {league.name} (Tier {league.tier})
                </option>
              ))}
            </select>
          </div>

          {selectedLeague && (
            <div className="bg-white rounded-2xl shadow-lg overflow-hidden">
              {/* League Header */}
              <div className="bg-gradient-to-r from-sky-600 to-blue-700 text-white px-6 py-4">
                <h2 className="text-xl font-bold">{selectedLeague.name}</h2>
                <p className="text-sky-100 text-sm">
                  {selectedLeague.hasMyTeam ? "🏆 Your League" : ""}
                </p>
              </div>

              {/* Table */}
              <div className="overflow-x-auto">
                <table className="w-full text-sm text-gray-700">
                  <thead className="bg-sky-100 text-sky-800 text-sm uppercase">
                    <tr>
                      <th className="px-3 py-2 text-center">#</th>
                      <th className="px-3 py-2 text-left">Team</th>
                      <th className="px-3 py-2 text-center">M</th>
                      <th className="px-3 py-2 text-center">W</th>
                      <th className="px-3 py-2 text-center">D</th>
                      <th className="px-3 py-2 text-center">L</th>
                      <th className="px-3 py-2 text-center">GF</th>
                      <th className="px-3 py-2 text-center">GA</th>
                      <th className="px-3 py-2 text-center">GD</th>
                      <th className="px-3 py-2 text-center">Pts</th>
                    </tr>
                  </thead>
                  <tbody>
                    {selectedLeague.teams
                      .filter(team => team.ranking !== undefined) // За сигурност
                      .map((team) => (
                        <tr
                          key={team.id}
                          className={`
                            ${team.id === myTeamId 
                              ? 'bg-sky-100 border-l-4 border-sky-500' 
                              : 'hover:bg-sky-50'
                            }
                            ${(team.ranking % 2 === 0) ? 'bg-white' : 'bg-gray-50'}
                            transition
                          `}
                        >
                          <td className="px-3 py-2 text-center font-bold text-gray-600">
                            {team.ranking}
                          </td>
                          <td className="px-3 py-2">
                            <div className="flex items-center gap-2">
                              <TeamLogo
                                teamName={team.name}
                                logoFileName={team.logoFileName}
                                className="w-7 h-7"
                              />
                              <span className="font-medium">{team.name}</span>
                            </div>
                          </td>
                          <td className="px-3 py-2 text-center">{team.matches}</td>
                          <td className="px-3 py-2 text-center">{team.wins}</td>
                          <td className="px-3 py-2 text-center">{team.draws}</td>
                          <td className="px-3 py-2 text-center">{team.losses}</td>
                          <td className="px-3 py-2 text-center">{team.goalsFor}</td>
                          <td className="px-3 py-2 text-center">{team.goalsAgainst}</td>
                          <td className="px-3 py-2 text-center">{team.goalDifference}</td>
                          <td className="px-3 py-2 text-center font-semibold text-sky-700">
                            {team.points}
                          </td>
                        </tr>
                      ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}
        </>
      ) : (
        <p className="text-gray-500 italic">No leagues found.</p>
      )}
    </div>
  );
};

export default League;