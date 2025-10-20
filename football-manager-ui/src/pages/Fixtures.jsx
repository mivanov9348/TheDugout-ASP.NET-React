import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import TeamLogo from "../components/TeamLogo";

const Fixtures = ({ gameSaveId, seasonId }) => {
  const [fixtures, setFixtures] = useState([]);
  const [loading, setLoading] = useState(false);
  const [round, setRound] = useState("1");
  const [league, setLeague] = useState("");
  const [leagues, setLeagues] = useState([]);
  const [maxRounds, setMaxRounds] = useState(38);

  // ✅ Зарежда лигите
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

        if (data.leagues && data.leagues.length > 0) {
          setLeague(data.leagues[0].id.toString());
          setMaxRounds(data.leagues[0].rounds || 38);
        }

      } catch (err) {
        console.error("Error fetching leagues:", err);
        setLeagues([]);
      }
    };

    fetchLeagues();
  }, [gameSaveId]);

  // ✅ Зарежда мачовете при промяна
  useEffect(() => {
    if (!gameSaveId || !seasonId) return;

    let url = `/api/fixtures/${gameSaveId}/${seasonId}?round=${round}`;
    if (league) {
      url += `&leagueId=${league}`;
    }

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
  }, [gameSaveId, seasonId, round, league]);

  useEffect(() => {
    if (league && leagues.length > 0) {
      const selectedLeague = leagues.find(l => l.id.toString() === league);
      if (selectedLeague) {
        setMaxRounds(selectedLeague.rounds || 38);
        setRound("1");
      }
    }
  }, [league, leagues]);

  const selectedLeague = leagues.find(l => l.id.toString() === league);

  const formatMatchDate = (dateString) => {
    if (!dateString) return "—";
    const date = new Date(dateString);
    return {
      day: date.getDate(),
      month: date.toLocaleDateString("en-GB", { month: "short" }),
      weekday: date.toLocaleDateString("en-GB", { weekday: "short" }),
      time: date.toLocaleTimeString("en-GB", { hour: "2-digit", minute: "2-digit" })
    };
  };

  const MatchRow = ({ match, index }) => {
    const dateInfo = formatMatchDate(match.date);
    const isPlayed = typeof match.homeTeamGoals === "number" && typeof match.awayTeamGoals === "number";

    return (
      <Link
        to={`/match/${match.id}`}
        className={`flex items-center justify-between p-6 ${
          index % 2 === 0 ? "bg-white" : "bg-gray-50"
        } hover:bg-blue-50 transition-colors duration-200 border-b border-gray-100 last:border-b-0`}
      >
        {/* Дата */}
        <div className="w-24 flex-shrink-0">
          <div className="text-center">
            <div className="text-sm font-semibold text-gray-900">
              {dateInfo.day} {dateInfo.month}
            </div>
            <div className="text-xs text-gray-500 mt-1">
              {dateInfo.time}
            </div>
          </div>
        </div>

        {/* Отбори и резултат */}
        <div className="flex-1 flex items-center justify-between max-w-2xl mx-8">
          {/* Домакин */}
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

          {/* Резултат */}
          <div className="mx-6">
            {isPlayed ? (
              <div className="bg-gray-100 px-4 py-2 rounded-lg min-w-[70px] text-center">
                <span className="font-bold text-gray-900 text-lg">
                  {match.homeTeamGoals} - {match.awayTeamGoals}
                </span>
              </div>
            ) : (
              <div className="text-gray-400 font-semibold">VS</div>
            )}
          </div>

          {/* Гост */}
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

        {/* Статус */}
        <div className="w-20 flex-shrink-0 text-right">
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

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-blue-50 py-8 px-4 sm:px-6 lg:px-8">
      <div className="max-w-7xl mx-auto">

        {/* Контроли */}
        <div className="bg-white/80 backdrop-blur-sm rounded-2xl shadow-xl border border-white/20 p-6 mb-8">
          <div className="flex flex-col sm:flex-row gap-4">
            <div className="flex-1">
              <label className="block text-sm font-semibold text-gray-700 mb-2">
                📊 League
              </label>
              <select
                className="w-full p-3 rounded-xl border border-gray-200 bg-white text-gray-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all duration-200 shadow-sm"
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
              <label className="block text-sm font-semibold text-gray-700 mb-2">
                🔄 Round
              </label>
              <select
                className="w-full p-3 rounded-xl border border-gray-200 bg-white text-gray-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all duration-200 shadow-sm"
                value={round}
                onChange={(e) => setRound(e.target.value)}
                disabled={loading || fixtures.length === 0}
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

        {/* Инфо за селекцията */}
        <div className="bg-gradient-to-r from-blue-600 to-indigo-700 rounded-2xl shadow-lg p-6 mb-8 text-white">
          <h2 className="text-2xl font-bold mb-2">
            {selectedLeague?.name || "Fixtures"}
          </h2>
          <p className="text-blue-100">
            Round {round} • {fixtures.length} matches
          </p>
        </div>

        {/* Списък с мачове */}
        {loading ? (
          <div className="flex justify-center items-center py-20">
            <div className="text-center">
              <div className="animate-spin h-16 w-16 border-4 border-blue-500 border-t-transparent rounded-full mx-auto mb-4"></div>
              <p className="text-gray-600 text-lg">Loading fixtures...</p>
            </div>
          </div>
        ) : fixtures.length === 0 ? (
          <div className="bg-white/80 rounded-2xl shadow-xl p-12 text-center">
            <div className="text-6xl mb-4">⚽</div>
            <h3 className="text-2xl font-bold text-gray-900 mb-2">
              No Fixtures Found
            </h3>
            <p className="text-gray-600 max-w-md mx-auto">
              There are no matches scheduled for the selected round and league.
            </p>
          </div>
        ) : (
          <div className="bg-white/80 backdrop-blur-sm rounded-2xl shadow-xl border border-white/20 overflow-hidden">
            <div className="bg-gray-50 px-6 py-4 border-b border-gray-200">
              <div className="flex items-center text-sm font-semibold text-gray-700 uppercase tracking-wide">
                <div className="w-24">Date & Time</div>
                <div className="flex-1 text-center">Match</div>
                <div className="w-20 text-right">Status</div>
              </div>
            </div>
            <div className="divide-y divide-gray-100">
              {fixtures.map((match, index) => (
                <MatchRow key={match.id} match={match} index={index} />
              ))}
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default Fixtures;
