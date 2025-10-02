import { useEffect, useState } from "react";
import TeamLogo from "../components/TeamLogo";

const Cup = ({ gameSaveId, seasonId }) => {
  const [cups, setCups] = useState([]);
  const [selectedCupId, setSelectedCupId] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchCups = async () => {
      if (!gameSaveId || !seasonId) return;
      try {
        setLoading(true);
        const res = await fetch(`/api/cup/${gameSaveId}/${seasonId}`, {
          credentials: "include",
        });
        if (!res.ok) throw new Error("Error loading cups");
        const data = await res.json();
        setCups(data);
        if (data.length > 0) setSelectedCupId(data[0].id);
      } catch (err) {
        console.error(err);
      } finally {
        setLoading(false);
      }
    };

    fetchCups();
  }, [gameSaveId, seasonId]);

  const selectedCup = cups.find((c) => c.id === selectedCupId);

  return (
    <div className="p-6 bg-gray-50 min-h-screen">
      <h2 className="text-3xl font-bold text-gray-800 mb-6">Cups</h2>

      {/* Cup selector */}
      <div className="mb-8">
        <label
          htmlFor="cup-select"
          className="block text-sm font-medium text-gray-700 mb-2"
        >
          Select a cup:
        </label>
        <select
          id="cup-select"
          value={selectedCupId || ""}
          onChange={(e) => setSelectedCupId(parseInt(e.target.value))}
          className="appearance-none bg-white border border-gray-300 rounded-lg px-4 py-3 pr-10 focus:outline-none focus:ring-2 focus:ring-sky-500 focus:border-transparent shadow-sm text-gray-700 cursor-pointer"
        >
          {cups.map((cup) => (
            <option key={cup.id} value={cup.id}>
              {cup.templateName} ({cup.countryName})
            </option>
          ))}
        </select>
      </div>

      {/* Rounds */}
      {loading ? (
        <div className="text-gray-500">Loading...</div>
      ) : !selectedCup ? (
        <div className="text-gray-500">No cups available</div>
      ) : (
        <div className="space-y-8">
          {selectedCup.rounds.map((round) => (
            <div
              key={round.id}
              className="bg-white rounded-xl shadow-md p-6 border border-gray-200"
            >
              <h3 className="text-2xl font-semibold text-gray-800 mb-6">
                {round.name}
              </h3>

              {round.fixtures.length === 0 ? (
                <div className="text-center py-6 text-gray-500">TBD</div>
              ) : (
                <div className="space-y-3">
                  {round.fixtures.map((match) => (
                    <div
                      key={match.id}
                      className="flex items-center justify-between p-4 bg-gray-50 hover:bg-gray-100 transition-colors rounded-lg border border-gray-200"
                    >
                      {/* Home team */}
                      <div className="flex items-center flex-1 gap-3">
                        <TeamLogo
                          teamName={match.homeTeam.name}
                          logoFileName={match.homeTeam.logoFileName}
                          className="w-10 h-10"
                        />
                        <div>
                          <div className="font-medium text-gray-800">
                            {match.homeTeam.name}
                          </div>
                          <div className="text-sm text-gray-500">
                            {new Date(match.date).toLocaleDateString()}
                          </div>
                        </div>
                      </div>

                      {/* Score */}
                      <div className="mx-4 text-center font-bold text-sky-600 text-lg">
                        {match.status === 1 // MatchStatus.Played
                          ? `${match.homeTeamGoals} - ${match.awayTeamGoals}`
                          : "—"}
                      </div>

                      {/* Away team */}
                      <div className="flex items-center flex-1 justify-end gap-3">
                        <div className="text-right">
                          <div className="font-medium text-gray-800">
                            {match.awayTeam.name}
                          </div>
                          <div className="text-sm text-gray-500">
                            {new Date(match.date).toLocaleDateString()}
                          </div>
                        </div>
                        <TeamLogo
                          teamName={match.awayTeam.name}
                          logoFileName={match.awayTeam.logoFileName}
                          className="w-10 h-10"
                        />
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default Cup;
