import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import TeamLogo from "../../components/TeamLogo";
import { useActiveSeason } from "../../components/useActiveSeason";

const Fixtures = ({ gameSaveId }) => {
  const { season, loading: seasonLoading, error: seasonError } = useActiveSeason(gameSaveId);
  const [fixtures, setFixtures] = useState([]);
  const [loading, setLoading] = useState(false);
  const [round, setRound] = useState("1");
  const [league, setLeague] = useState("");
  const [leagues, setLeagues] = useState([]);
  const [maxRounds, setMaxRounds] = useState(38);

  // 🏆 Зареждане на лигите
  useEffect(() => {
    if (!gameSaveId) return;

    const fetchLeagues = async () => {
      try {
        const res = await fetch(`/api/league/${gameSaveId}`, {
          credentials: "include",
        });
        if (!res.ok) throw new Error("Failed to load leagues");

        const data = await res.json();
        setLeagues(data.leagues || []);

        if (data.leagues?.length > 0) {
          setLeague(data.leagues[0].id.toString());
          setMaxRounds(data.leagues[0].rounds || 38);
        }
      } catch (err) {
        console.error("Error fetching leagues:", err);
      }
    };

    fetchLeagues();
  }, [gameSaveId]);

  // ⚽ Зареждане на мачовете по активен сезон
  useEffect(() => {
    if (!gameSaveId || !season?.id) return;

    let url = `/api/fixtures/${gameSaveId}?round=${round}&seasonId=${season.id}`;
    if (league) url += `&leagueId=${league}`;

    const fetchFixtures = async () => {
      try {
        setLoading(true);
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
  }, [gameSaveId, season?.id, round, league]);

  // 🔄 При смяна на лига, рестартираме рунда
  useEffect(() => {
    if (league && leagues.length > 0) {
      const selected = leagues.find((l) => l.id.toString() === league);
      if (selected) {
        setMaxRounds(selected.rounds || 38);
        setRound("1");
      }
    }
  }, [league, leagues]);

  const formatMatchDate = (dateString) => {
    if (!dateString) return "—";
    const date = new Date(dateString);
    return {
      day: date.getDate(),
      month: date.toLocaleDateString("en-GB", { month: "short" }),
      time: date.toLocaleTimeString("en-GB", {
        hour: "2-digit",
        minute: "2-digit",
      }),
    };
  };

  const MatchRow = ({ match, index }) => {
    const dateInfo = formatMatchDate(match.date);
    const isPlayed =
      typeof match.homeTeamGoals === "number" &&
      typeof match.awayTeamGoals === "number";

    return (
      <Link
        to={`/match/${match.id}`}
        className={`flex items-center justify-between p-6 ${
          index % 2 === 0 ? "bg-white" : "bg-gray-50"
        } hover:bg-blue-50 transition-colors duration-200 border-b border-gray-100`}
      >
        <div className="w-24 text-center">
          <div className="text-sm font-semibold text-gray-900">
            {dateInfo.day} {dateInfo.month}
          </div>
          <div className="text-xs text-gray-500 mt-1">{dateInfo.time}</div>
        </div>

        <div className="flex-1 flex items-center justify-between max-w-2xl mx-8">
          <div className="flex items-center space-x-4 flex-1 justify-end">
            <span className="font-semibold text-gray-900 text-right">
              {match.homeTeam ?? "—"}
            </span>
            <TeamLogo
              teamName={match.homeTeam}
              logoFileName={match.homeLogoFileName}
              className="w-10 h-10"
            />
          </div>

          <div className="mx-6">
            {isPlayed ? (
              <div className="bg-gray-100 px-4 py-2 rounded-lg text-center">
                <span className="font-bold text-gray-900 text-lg">
                  {match.homeTeamGoals} - {match.awayTeamGoals}
                </span>
              </div>
            ) : (
              <div className="text-gray-400 font-semibold">VS</div>
            )}
          </div>

          <div className="flex items-center space-x-4 flex-1 justify-start">
            <TeamLogo
              teamName={match.awayTeam}
              logoFileName={match.awayLogoFileName}
              className="w-10 h-10"
            />
            <span className="font-semibold text-gray-900">
              {match.awayTeam ?? "—"}
            </span>
          </div>
        </div>

        <div className="w-20 text-right">
          <span
            className={`inline-flex items-center px-2 py-1 rounded-full text-xs font-medium ${
              isPlayed
                ? "bg-green-100 text-green-800"
                : "bg-blue-100 text-blue-800"
            }`}
          >
            {isPlayed ? "FT" : "SCH"}
          </span>
        </div>
      </Link>
    );
  };

  if (seasonLoading) return <div className="text-center py-20">Loading active season...</div>;
  if (seasonError) return <div className="text-center py-20 text-red-500">Error loading season.</div>;
  if (!season) return <div className="text-center py-20 text-gray-500">No active season.</div>;

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-blue-50 py-8 px-4 sm:px-6 lg:px-8">
      <div className="max-w-7xl mx-auto">
        <div className="bg-white rounded-2xl shadow-xl p-6 mb-8">
          <div className="flex flex-col sm:flex-row gap-4">
            <div className="flex-1">
              <label className="block text-sm font-semibold mb-2">League</label>
              <select
                className="w-full p-3 rounded-xl border border-gray-200"
                value={league}
                onChange={(e) => setLeague(e.target.value)}
              >
                {leagues.map((l) => (
                  <option key={l.id} value={l.id}>
                    {l.name}
                  </option>
                ))}
              </select>
            </div>

            <div className="flex-1">
              <label className="block text-sm font-semibold mb-2">Round</label>
              <select
                className="w-full p-3 rounded-xl border border-gray-200"
                value={round}
                onChange={(e) => setRound(e.target.value)}
                disabled={loading}
              >
                {[...Array(maxRounds)].map((_, i) => (
                  <option key={i + 1} value={i + 1}>
                    Round {i + 1}
                  </option>
                ))}
              </select>
            </div>
          </div>
        </div>

        {loading ? (
          <div className="text-center py-20">Loading fixtures...</div>
        ) : fixtures.length === 0 ? (
          <div className="text-center py-20 text-gray-500">
            No fixtures found.
          </div>
        ) : (
          <div className="bg-white rounded-2xl shadow-xl overflow-hidden">
            {fixtures.map((match, index) => (
              <MatchRow key={match.id} match={match} index={index} />
            ))}
          </div>
        )}
      </div>
    </div>
  );
};

export default Fixtures;
