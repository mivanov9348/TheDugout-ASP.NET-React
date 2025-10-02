import { useEffect, useState } from "react";
import TeamLogo from "../components/TeamLogo";

const Fixtures = ({ gameSaveId, seasonId }) => {
  const [fixtures, setFixtures] = useState([]);
  const [loading, setLoading] = useState(false);
  const [round, setRound] = useState("1");
  const [league, setLeague] = useState("");
  const [leagues, setLeagues] = useState([]);

  // Load leagues
  useEffect(() => {
    if (!gameSaveId) return;

    const fetchLeagues = async () => {
      try {
        const res = await fetch(`/api/leagues?gameSaveId=${gameSaveId}`, {
          credentials: "include",
        });
        if (!res.ok) throw new Error("Failed to load leagues");
        const data = await res.json();

        setLeagues(data);

        // Set default league to the one with the lowest tier
        if (data.length > 0 && !league) {
          const first = data.sort((a, b) => a.tier - b.tier)[0];
          setLeague(first.id.toString());
        }
      } catch (err) {
        console.error("Error fetching leagues:", err);
      }
    };

    fetchLeagues();
  }, [gameSaveId]);

  // Load fixtures
  useEffect(() => {
    if (!gameSaveId || !seasonId || !league) return;

    const fetchFixtures = async () => {
      try {
        setLoading(true);
        const url = `/api/fixtures/${gameSaveId}/${seasonId}?round=${round}&leagueId=${league}`;
        const res = await fetch(url, { credentials: "include" });
        if (!res.ok) throw new Error("Failed to load fixtures");
        const data = await res.json();

        setFixtures(Array.isArray(data) ? data : []);
      } catch (err) {
        console.error("Error fetching fixtures:", err);
        setFixtures([]);
      } finally {
        setLoading(false);
      }
    };

    fetchFixtures();
  }, [gameSaveId, seasonId, round, league]);

  // Get the number of rounds for the selected league
  const selectedLeague = leagues.find((l) => l.id.toString() === league);
  const maxRounds = selectedLeague?.rounds || 38; // Default to 38 if rounds not available

  return (
    <div className="min-h-screen bg-gray-50 py-8 px-4 sm:px-6 lg:px-8">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
       

        {/* Filters */}
        <div className="flex flex-col sm:flex-row sm:items-center gap-4 mb-8 bg-white p-6 rounded-2xl shadow-md">
          <div className="flex-1">
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Select Round
            </label>
            <select
              className="w-full p-3 rounded-lg border border-gray-300 bg-white text-gray-700 focus:outline-none focus:ring-2 focus:ring-blue-500 transition duration-200"
              value={round}
              onChange={(e) => setRound(e.target.value)}
            >
              {[...Array(maxRounds)].map((_, i) => (
                <option key={i + 1} value={i + 1}>
                  Round {i + 1}
                </option>
              ))}
            </select>
          </div>

          <div className="flex-1">
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Select League
            </label>
            <select
              className="w-full p-3 rounded-lg border border-gray-300 bg-white text-gray-700 focus:outline-none focus:ring-2 focus:ring-blue-500 transition duration-200"
              value={league}
              onChange={(e) => {
                setLeague(e.target.value);
                setRound("1");
              }}
            >
              <option value="">Select a league</option>
              {leagues.map((l) => (
                <option key={l.id} value={l.id}>
                  {l.name} 
                </option>
              ))}
            </select>
          </div>
        </div>

        {/* Fixtures Table */}
        {loading ? (
          <div className="flex justify-center py-12">
            <div className="animate-spin h-12 w-12 border-4 border-blue-500 border-t-transparent rounded-full"></div>
          </div>
        ) : fixtures.length === 0 ? (
          <div className="bg-white p-6 rounded-2xl shadow-md text-center">
            <p className="text-gray-500 text-lg italic">
              No fixtures available for the selected round and league.
            </p>
          </div>
        ) : (
          <div className="bg-white rounded-2xl shadow-md overflow-hidden">
            {/* League & Round Header */}
            <div className="bg-gradient-to-r from-blue-600 to-indigo-700 text-white px-6 py-4">
              <h2 className="text-xl font-semibold">
                {selectedLeague?.name || "Selected League"} – Round {round}
              </h2>
            </div>

            <div className="overflow-x-auto">
              <table className="w-full text-sm text-gray-700">
                <thead className="bg-blue-50 text-blue-800 uppercase text-xs">
                  <tr>
                    <th className="px-6 py-3 text-left">Date</th>
                    <th className="px-6 py-3 text-left">Home Team</th>
                    <th className="px-6 py-3 text-center">Score</th>
                    <th className="px-6 py-3 text-left">Away Team</th>
                  </tr>
                </thead>
                <tbody>
                  {fixtures.map((match, idx) => (
                    <tr
                      key={match.id}
                      className={`${
                        idx % 2 === 0 ? "bg-white" : "bg-gray-50"
                      } hover:bg-blue-50 transition duration-150`}
                    >
                      <td className="px-6 py-4 text-gray-600">
                        {match?.date
                          ? new Date(match.date).toLocaleDateString("en-GB", {
                              day: "2-digit",
                              month: "short",
                              year: "numeric",
                            })
                          : "—"}
                      </td>
                      <td className="px-6 py-4">
                        <div className="flex items-center gap-3">
                          <TeamLogo
                            teamName={match?.homeTeam}
                            logoFileName={match?.homeLogoFileName}
                            className="w-8 h-8"
                          />
                          <span className="font-semibold text-gray-800">
                            {match?.homeTeam ?? "—"}
                          </span>
                        </div>
                      </td>
                      <td className="px-6 py-4 text-center font-bold text-blue-600">
                        {typeof match?.homeTeamGoals === "number" &&
                        typeof match?.awayTeamGoals === "number"
                          ? `${match.homeTeamGoals} - ${match.awayTeamGoals}`
                          : "- : -"}
                      </td>
                      <td className="px-6 py-4">
                        <div className="flex items-center gap-3">
                          <TeamLogo
                            teamName={match?.awayTeam}
                            logoFileName={match?.awayLogoFileName}
                            className="w-8 h-8"
                          />
                          <span className="font-semibold text-gray-800">
                            {match?.awayTeam ?? "—"}
                          </span>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default Fixtures;