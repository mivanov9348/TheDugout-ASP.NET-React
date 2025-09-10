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
    <div className="p-6">
      {/* Header */}
      <h1 className="text-3xl font-extrabold mb-6 text-sky-700 tracking-wide">
        Fixtures
      </h1>

      {/* Filters */}
      <div className="flex flex-wrap gap-4 mb-6">
        {/* Rounds */}
        <select
          className="p-3 rounded-xl border-2 border-sky-500 bg-white text-gray-700 shadow-sm focus:outline-none focus:ring-2 focus:ring-sky-400 transition"
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
          className="p-3 rounded-xl border-2 border-sky-500 bg-white text-gray-700 shadow-sm focus:outline-none focus:ring-2 focus:ring-sky-400 transition"
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
        <div className="flex justify-center py-12">
          <div className="animate-spin h-10 w-10 border-4 border-sky-400 border-t-transparent rounded-full"></div>
        </div>
      ) : fixtures.length === 0 ? (
        <p className="text-gray-500 italic">No fixtures available.</p>
      ) : (
        <div className="mb-6 bg-white rounded-2xl shadow-lg overflow-hidden">
          {/* League & Round header */}
          <div className="bg-gradient-to-r from-sky-600 to-blue-700 text-white px-6 py-4">
            <h2 className="text-lg font-semibold">
              {leagues.find((l) => l.id.toString() === league)?.name} – Round {round}
            </h2>
          </div>

          <div className="overflow-x-auto">
            <table className="w-full text-sm text-gray-700">
              <thead className="bg-sky-100 text-sky-800 uppercase">
                <tr>
                  <th className="px-3 py-2 text-left">Date</th>
                  <th className="px-3 py-2 text-left">Home</th>
                  <th className="px-3 py-2 text-center">Score</th>
                  <th className="px-3 py-2 text-left">Away</th>
                </tr>
              </thead>
              <tbody>
                {fixtures.map((m, idx) => (
                  <tr
                    key={m.id}
                    className={`${
                      idx % 2 === 0 ? "bg-white" : "bg-gray-50"
                    } hover:bg-sky-50 transition`}
                  >
                    <td className="px-3 py-2 text-gray-600">
                      {m?.date
                        ? new Date(m.date).toLocaleDateString("en-GB", {
                            day: "2-digit",
                            month: "short",
                            year: "numeric",
                          })
                        : "—"}
                    </td>

                    {/* Home team */}
                    <td className="px-3 py-2">
                      <div className="flex items-center gap-2">
                        <TeamLogo
                          teamName={m?.homeTeam}
                          logoFileName={m?.homeLogoFileName}
                          className="w-6 h-6"
                        />
                        <span className="font-medium">{m?.homeTeam ?? "—"}</span>
                      </div>
                    </td>

                    {/* Score */}
                    <td className="px-3 py-2 text-center font-bold text-sky-700">
                      {typeof m?.homeTeamGoals === "number" &&
                      typeof m?.awayTeamGoals === "number"
                        ? `${m.homeTeamGoals} - ${m.awayTeamGoals}`
                        : "- : -"}
                    </td>

                    {/* Away team */}
                    <td className="px-3 py-2">
                      <div className="flex items-center gap-2">
                        <TeamLogo
                          teamName={m?.awayTeam}
                          logoFileName={m?.awayLogoFileName}
                          className="w-6 h-6"
                        />
                        <span className="font-medium">{m?.awayTeam ?? "—"}</span>
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
  );
};

export default Fixtures;
