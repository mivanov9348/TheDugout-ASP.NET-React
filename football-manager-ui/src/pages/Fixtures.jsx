import { useEffect, useState } from "react";

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
        if (!res.ok) throw new Error("Error loading leagues");
        const data = await res.json();

        setLeagues(data);

        if (data.length > 0 && !league) {
          const first = data.sort((a, b) => a.tier - b.tier)[0];
          setLeague(first.id.toString());
        }
      } catch (err) {
        console.error(err);
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
        if (!res.ok) throw new Error("Error loading fixtures");
        const data = await res.json();

        // Backend връща вече плосък списък
        setFixtures(Array.isArray(data) ? data : []);
      } catch (err) {
        console.error(err);
        setFixtures([]);
      } finally {
        setLoading(false);
      }
    };

    fetchFixtures();
  }, [gameSaveId, seasonId, round, league]);

  return (
    <div className="bg-white shadow-lg rounded-xl p-4">
      <h1 className="text-xl font-bold mb-4">Fixtures</h1>

      {/* Filters */}
      <div className="flex flex-wrap gap-4 mb-6">
        {/* Rounds */}
        <select
          className="border rounded-md px-3 py-2"
          value={round}
          onChange={(e) => setRound(e.target.value)}
        >
          {[...Array(
            leagues.find((l) => l.id.toString() === league)?.rounds || 0
          )].map((_, i) => (
            <option key={i + 1} value={i + 1}>
              Round {i + 1}
            </option>
          ))}
        </select>

        {/* Leagues */}
        <select
          className="border rounded-md px-3 py-2"
          value={league}
          onChange={(e) => {
            setLeague(e.target.value);
            setRound("1");
          }}
        >
          {leagues.map((l) => (
            <option key={l.id} value={l.id}>
              {l.name}
            </option>
          ))}
        </select>
      </div>

      {/* Fixtures Table */}
      {loading ? (
        <div className="flex justify-center py-6">
          <div className="animate-spin h-6 w-6 border-2 border-gray-400 border-t-transparent rounded-full"></div>
        </div>
      ) : fixtures.length === 0 ? (
        <p className="text-gray-500">No fixtures available.</p>
      ) : (
        <div className="mb-6">
          <h2 className="text-lg font-semibold mb-2">
            {leagues.find((l) => l.id.toString() === league)?.name} – Round{" "}
            {round}
          </h2>
          <div className="overflow-x-auto">
            <table className="w-full border-collapse">
              <thead className="bg-gray-100">
                <tr>
                  <th className="w-1/4 text-left p-2 font-semibold text-gray-700">Date</th>
                  <th className="w-1/4 text-left p-2 font-semibold text-gray-700">Home</th>
                  <th className="w-1/4 text-center p-2 font-semibold text-gray-700">Score</th>
                  <th className="w-1/4 text-left p-2 font-semibold text-gray-700">Away</th>
                </tr>
              </thead>
              <tbody>
                {fixtures.map((m) => (
                  <tr key={m.id} className="border-b">
                    <td className="p-2 text-gray-600">
                      {m?.date
                        ? new Date(m.date).toLocaleDateString("en-GB", {
                            day: "2-digit",
                            month: "short",
                            year: "numeric",
                          })
                        : "—"}
                    </td>
                    <td className="p-2 text-gray-600">{m?.homeTeam ?? "—"}</td>
                    <td className="p-2 text-gray-600 text-center">
                      {typeof m?.homeTeamGoals === "number" &&
                      typeof m?.awayTeamGoals === "number"
                        ? `${m.homeTeamGoals} - ${m.awayTeamGoals}`
                        : "- : -"}
                    </td>
                    <td className="p-2 text-gray-600">{m?.awayTeam ?? "—"}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
};

export default Fixtures;
