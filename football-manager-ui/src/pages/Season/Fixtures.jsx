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

  // 🏆 Определяне на победителя
  let winner = null;
  if (isPlayed) {
    if (match.homeTeamGoals > match.awayTeamGoals) winner = "home";
    else if (match.awayTeamGoals > match.homeTeamGoals) winner = "away";
  }

  return (
    <Link
      to={`/match/${match.id}`}
      className={`flex items-center justify-between p-6 ${
        index % 2 === 0 ? "bg-gray-800" : "bg-gray-900"
      } hover:bg-gray-700 transition-colors duration-200 border-b border-gray-700`}
    >
      <div className="w-24 text-center">
        <div className="text-sm font-semibold text-white">
          {dateInfo.day} {dateInfo.month}
        </div>
        <div className="text-xs text-gray-400 mt-1">{dateInfo.time}</div>
      </div>

      <div className="flex-1 flex items-center justify-between max-w-2xl mx-8">
        {/* 🏠 Домакин */}
        <div
          className={`flex items-center space-x-4 flex-1 justify-end ${
            winner === "home" ? "text-green-400 drop-shadow-[0_0_6px_rgba(34,197,94,0.7)]" : ""
          }`}
        >
          <span className={`font-semibold text-right ${winner === "home" ? "text-green-400" : "text-white"}`}>
            {match.homeTeam ?? "—"}
          </span>
          <TeamLogo
            teamName={match.homeTeam}
            logoFileName={match.homeLogoFileName}
            className={`w-10 h-10 ${winner === "home" ? "brightness-125" : ""}`}
          />
        </div>

        {/* ⚽ Резултат */}
        <div className="mx-6">
          {isPlayed ? (
            <div className="bg-gray-700 px-4 py-2 rounded-lg text-center">
              <span className="font-bold text-white text-lg">
                {match.homeTeamGoals} - {match.awayTeamGoals}
              </span>
            </div>
          ) : (
            <div className="text-gray-400 font-semibold">VS</div>
          )}
        </div>

        {/* 🛫 Гост */}
        <div
          className={`flex items-center space-x-4 flex-1 justify-start ${
            winner === "away" ? "text-green-400 drop-shadow-[0_0_6px_rgba(34,197,94,0.7)]" : ""
          }`}
        >
          <TeamLogo
            teamName={match.awayTeam}
            logoFileName={match.awayLogoFileName}
            className={`w-10 h-10 ${winner === "away" ? "brightness-125" : ""}`}
          />
          <span className={`font-semibold ${winner === "away" ? "text-green-400" : "text-white"}`}>
            {match.awayTeam ?? "—"}
          </span>
        </div>
      </div>

      <div className="w-20 text-right">
        <span
          className={`inline-flex items-center px-2 py-1 rounded-full text-xs font-medium ${
            isPlayed
              ? "bg-green-900 text-green-200"
              : "bg-blue-900 text-blue-200"
          }`}
        >
          {isPlayed ? "FT" : "SCH"}
        </span>
      </div>
    </Link>
  );
};


  if (seasonLoading) return <div className="text-center py-20 text-white">Loading active season...</div>;
  if (seasonError) return <div className="text-center py-20 text-red-400">Error loading season.</div>;
  if (!season) return <div className="text-center py-20 text-gray-400">No active season.</div>;

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-900 via-gray-800 to-gray-900 py-8 px-4 sm:px-6 lg:px-8">
      <div className="max-w-7xl mx-auto">
        <div className="bg-gray-800 rounded-2xl shadow-xl p-6 mb-8 border border-gray-700">
          <div className="flex flex-col sm:flex-row gap-4">
            <div className="flex-1">
              <label className="block text-sm font-semibold mb-2 text-white">League</label>
              <select
                className="w-full p-3 rounded-xl border border-gray-600 bg-gray-700 text-white focus:border-gray-500 focus:ring-2 focus:ring-gray-500"
                value={league}
                onChange={(e) => setLeague(e.target.value)}
              >
                {leagues.map((l) => (
                  <option key={l.id} value={l.id} className="bg-gray-700">
                    {l.name}
                  </option>
                ))}
              </select>
            </div>

            <div className="flex-1">
              <label className="block text-sm font-semibold mb-2 text-white">Round</label>
              <select
                className="w-full p-3 rounded-xl border border-gray-600 bg-gray-700 text-white focus:border-gray-500 focus:ring-2 focus:ring-gray-500"
                value={round}
                onChange={(e) => setRound(e.target.value)}
                disabled={loading}
              >
                {[...Array(maxRounds)].map((_, i) => (
                  <option key={i + 1} value={i + 1} className="bg-gray-700">
                    Round {i + 1}
                  </option>
                ))}
              </select>
            </div>
          </div>
        </div>

        {loading ? (
          <div className="text-center py-20 text-white">Loading fixtures...</div>
        ) : fixtures.length === 0 ? (
          <div className="text-center py-20 text-gray-400">
            No fixtures found.
          </div>
        ) : (
          <div className="bg-gray-800 rounded-2xl shadow-xl overflow-hidden border border-gray-700">
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