import React, { useEffect, useState } from "react";

const League = ({ gameSaveId }) => {
  const [leagues, setLeagues] = useState([]);
  const [selectedLeague, setSelectedLeague] = useState(null);

  useEffect(() => {
    if (!gameSaveId) return;
    const fetchLeagues = async () => {
      try {
        const token = localStorage.getItem("token"); // JWT token
        const res = await fetch(`/api/leagues?gameSaveId=${gameSaveId}`, {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        });
        const data = await res.json();
        setLeagues(data);
        if (data.length > 0) {
          setSelectedLeague(data[0]); // по подразбиране първата лига
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
      <h1 className="text-2xl font-bold mb-4">Leagues</h1>

      {leagues.length > 0 ? (
        <>
          <select
            className="mb-4 p-2 border rounded"
            onChange={handleLeagueChange}
            value={selectedLeague?.id || ""}
          >
            {leagues.map((league) => (
              <option key={league.id} value={league.id}>
                {league.name} (Tier {league.tier})
              </option>
            ))}
          </select>

          {selectedLeague && (
            <div>
              <h2 className="text-xl font-semibold mb-2">{selectedLeague.name}</h2>
              <table className="table-auto border-collapse w-full border border-gray-300">
                <thead>
                  <tr className="bg-gray-200">
                    <th className="border px-2">#</th>
                    <th className="border px-2">Team</th>
                    <th className="border px-2">Pts</th>
                    <th className="border px-2">W</th>
                    <th className="border px-2">D</th>
                    <th className="border px-2">L</th>
                    <th className="border px-2">GF</th>
                    <th className="border px-2">GA</th>
                    <th className="border px-2">GD</th>
                  </tr>
                </thead>
                <tbody>
                  {selectedLeague.teams.map((team, index) => (
                    <tr key={team.id} className="text-center">
                      <td className="border px-2">{index + 1}</td>
                      <td className="border px-2 text-left">{team.name}</td>
                      <td className="border px-2">{team.points}</td>
                      <td className="border px-2">{team.wins}</td>
                      <td className="border px-2">{team.draws}</td>
                      <td className="border px-2">{team.losses}</td>
                      <td className="border px-2">{team.goalsFor}</td>
                      <td className="border px-2">{team.goalsAgainst}</td>
                      <td className="border px-2">{team.goalDifference}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </>
      ) : (
        <p>No leagues found.</p>
      )}
    </div>
  );
};

export default League;
