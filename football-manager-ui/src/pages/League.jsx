import React, { useEffect, useState } from "react";
import TeamLogo from "../components/TeamLogo";

const League = ({ gameSaveId }) => {
  const [leagues, setLeagues] = useState([]);
  const [selectedLeague, setSelectedLeague] = useState(null);

  useEffect(() => {
    if (!gameSaveId) return;
    const fetchLeagues = async () => {
      try {
        const token = localStorage.getItem("token");
        const res = await fetch(`/api/leagues?gameSaveId=${gameSaveId}`, {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        });
        const data = await res.json();
        setLeagues(data);
        if (data.length > 0) {
          setSelectedLeague(data[0]);
        }
      } catch (err) {
        console.error("Error fetching leagues:", err);
      }
    };
    fetchLeagues();
  }, [gameSaveId]);

  const handleLeagueChange = (e) => {
    const leagueId = parseInt(e.target.value);
    const league = leagues.find((l) => l.id === leagueId);
    setSelectedLeague(league);
  };

  return (
    <div className="p-6">
      <h1 className="text-3xl font-extrabold mb-6 text-sky-700 tracking-wide">
        League Standings
      </h1>

      {leagues.length > 0 ? (
        <>
          {/* Dropdown */}
          <div className="mb-6">
            <select
              className="p-3 rounded-xl border-2 border-sky-500 bg-white text-gray-700 shadow-sm focus:outline-none focus:ring-2 focus:ring-sky-400 transition"
              onChange={handleLeagueChange}
              value={selectedLeague?.id || ""}
            >
              {leagues.map((league) => (
                <option key={league.id} value={league.id}>
                  {league.name}
                </option>
              ))}
            </select>
          </div>

          {selectedLeague && (
            <div className="bg-white rounded-2xl shadow-lg overflow-hidden">
              {/* League Header */}
              <div className="bg-gradient-to-r from-sky-600 to-blue-700 text-white px-6 py-4">
                <h2 className="text-xl font-bold">{selectedLeague.name}</h2>
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
                    {selectedLeague.teams.map((team, index) => (
                      <tr
                        key={team.id}
                        className={`${
                          index % 2 === 0 ? "bg-white" : "bg-gray-50"
                        } hover:bg-sky-50 transition`}
                      >
                        <td className="px-3 py-2 text-center font-bold text-gray-600">
                          {index + 1}
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
