import React, { useEffect, useState } from "react";
import {
  Users,
  Shield,
  MapPin,
  TrendingUp,
  DollarSign,
  Star,
  BarChart3,
  Clock,
} from "lucide-react";
import TeamLogo from "../../components/TeamLogo";

const Club = () => {
  const [club, setClub] = useState(null);
  const [stats, setStats] = useState(null);
  const [fixtures, setFixtures] = useState([]);
  const [recentMatches, setRecentMatches] = useState([]);
  const [squad, setSquad] = useState([]);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState("overview");

  useEffect(() => {
    const fetchClubData = async () => {
      try {
        const saveResponse = await fetch("/api/games/current", {
          credentials: "include",
        });

        if (!saveResponse.ok) throw new Error("Failed to fetch save");
        const saveData = await saveResponse.json();
        const userTeam = saveData?.userTeam;
        if (!userTeam) {
          setLoading(false);
          return;
        }

        const [clubRes, statsRes, fixturesRes, recentRes, squadRes] =
          await Promise.all([
            fetch(`/api/clubs/${userTeam.id}`, { credentials: "include" }),
            fetch(`/api/clubs/${userTeam.id}/stats`, {
              credentials: "include",
            }),
            fetch(`/api/clubs/${userTeam.id}/fixtures?limit=5`, {
              credentials: "include",
            }),
            fetch(`/api/clubs/${userTeam.id}/recent-matches?limit=5`, {
              credentials: "include",
            }),
            fetch(`/api/clubs/${userTeam.id}/squad`, {
              credentials: "include",
            }),
          ]);

        if (clubRes.ok) setClub(await clubRes.json());
        if (statsRes.ok) setStats(await statsRes.json());
        if (fixturesRes.ok) setFixtures(await fixturesRes.json());
        if (recentRes.ok) setRecentMatches(await recentRes.json());
        if (squadRes.ok) setSquad(await squadRes.json());
      } catch (err) {
        console.error("Error fetching club data:", err);
      } finally {
        setLoading(false);
      }
    };

    fetchClubData();
  }, []);

  if (loading)
    return (
      <div className="flex justify-center items-center min-h-64 text-gray-300">
        Loading your club...
      </div>
    );

  if (!club)
    return (
      <div className="flex justify-center items-center min-h-64 text-gray-400">
        No club selected or club not found.
      </div>
    );

  // --------------------- OVERVIEW ---------------------
  const renderOverview = () => (
    <div className="space-y-6">
      {/* Stats Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <div className="bg-gray-800 rounded-2xl shadow-lg p-6 border-l-4 border-green-500">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-400">Balance</p>
              <p className="text-2xl font-bold text-white">
                €{(club.balance || 0).toLocaleString()}
              </p>
            </div>
            <DollarSign className="w-8 h-8 text-green-500" />
          </div>
        </div>

        <div className="bg-gray-800 rounded-2xl shadow-lg p-6 border-l-4 border-yellow-500">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-400">Popularity</p>
              <p className="text-2xl font-bold text-white">
                {club.popularity || 0}/100
              </p>
            </div>
            <Star className="w-8 h-8 text-yellow-500" />
          </div>
          <div className="mt-3">
            <div className="w-full bg-gray-700 rounded-full h-2">
              <div
                className="bg-yellow-500 h-2 rounded-full"
                style={{ width: `${club.popularity || 0}%` }}
              ></div>
            </div>
          </div>
        </div>

        <div className="bg-gray-800 rounded-2xl shadow-lg p-6 border-l-4 border-blue-500">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-400">Squad Size</p>
              <p className="text-2xl font-bold text-white">{squad.length}</p>
            </div>
            <Users className="w-8 h-8 text-blue-500" />
          </div>
        </div>

        {stats && (
          <div className="bg-gray-800 rounded-2xl shadow-lg p-6 border-l-4 border-purple-500">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-400">Win Rate</p>
                <p className="text-2xl font-bold text-white">
                  {stats.winPercentage?.toFixed(1)}%
                </p>
              </div>
              <TrendingUp className="w-8 h-8 text-purple-500" />
            </div>
          </div>
        )}
      </div>

      {/* Season Stats + Fixtures */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {stats && (
          <div className="bg-gray-800 rounded-2xl shadow-lg p-6">
            <h2 className="text-xl font-semibold mb-4 flex items-center gap-2 text-gray-100">
              <BarChart3 className="w-5 h-5" /> Season Statistics
            </h2>
            <div className="grid grid-cols-2 gap-4">
              <div className="text-center p-3 bg-gray-700 rounded-lg">
                <p className="text-2xl font-bold text-blue-400">
                  {stats.matchesPlayed || 0}
                </p>
                <p className="text-sm text-gray-400">Played</p>
              </div>
              <div className="text-center p-3 bg-gray-700 rounded-lg">
                <p className="text-2xl font-bold text-green-400">
                  {stats.wins || 0}
                </p>
                <p className="text-sm text-gray-400">Wins</p>
              </div>
              <div className="text-center p-3 bg-gray-700 rounded-lg">
                <p className="text-2xl font-bold text-yellow-400">
                  {stats.draws || 0}
                </p>
                <p className="text-sm text-gray-400">Draws</p>
              </div>
              <div className="text-center p-3 bg-gray-700 rounded-lg">
                <p className="text-2xl font-bold text-red-400">
                  {stats.losses || 0}
                </p>
                <p className="text-sm text-gray-400">Losses</p>
              </div>
              <div className="text-center p-3 bg-gray-700 rounded-lg col-span-2">
                <p className="text-lg font-bold text-gray-100">
                  {stats.goalsFor || 0} : {stats.goalsAgainst || 0}
                </p>
                <p className="text-sm text-gray-400">Goals For : Against</p>
              </div>
            </div>
          </div>
        )}

        <div className="bg-gray-800 rounded-2xl shadow-lg p-6">
          <h2 className="text-xl font-semibold mb-4 flex items-center gap-2 text-gray-100">
            <Clock className="w-5 h-5" /> Upcoming Fixtures
          </h2>
          {fixtures.length > 0 ? (
            <div className="space-y-3">
              {fixtures.map((f) => (
                <div
                  key={f.id}
                  className="flex items-center justify-between p-3 bg-gray-700 rounded-lg"
                >
                  <div className="flex items-center gap-3">
                    <div className="text-sm text-gray-400 w-20">
                      {new Date(f.matchDate).toLocaleDateString()}
                    </div>
                    <div>
                      <div className="text-gray-100">{f.homeTeam.name}</div>
                      <div className="text-gray-100">{f.awayTeam.name}</div>
                    </div>
                  </div>
                  <div className="text-sm text-gray-400">
                    {f.league?.name || "Friendly"}
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <p className="text-gray-400 text-center py-4">
              No upcoming fixtures
            </p>
          )}
        </div>
      </div>
    </div>
  );

  // --------------------- SQUAD ---------------------
  const renderSquad = () => (
    <div className="bg-gray-800 rounded-2xl shadow-lg p-6">
      <div className="flex items-center justify-between mb-6">
        <h2 className="text-xl font-semibold text-gray-100">Squad</h2>
        <span className="bg-blue-900 text-blue-300 text-sm font-medium px-2.5 py-0.5 rounded">
          {squad.length} players
        </span>
      </div>
      {squad.length > 0 ? (
        <div className="overflow-x-auto">
          <table className="w-full text-gray-200">
            <thead>
              <tr className="border-b border-gray-700 text-gray-400">
                <th className="text-left py-3 font-medium">Player</th>
                <th className="text-left py-3 font-medium">Position</th>
                <th className="text-left py-3 font-medium">Age</th>
                <th className="text-left py-3 font-medium">OVR</th>
                <th className="text-left py-3 font-medium">POT</th>
                <th className="text-left py-3 font-medium">Value</th>
              </tr>
            </thead>
            <tbody>
              {squad.map((p) => (
                <tr
                  key={p.id}
                  className="border-b border-gray-700 hover:bg-gray-700/50 transition"
                >
                  <td className="py-3 font-medium">{p.name}</td>
                  <td className="py-3">{p.position}</td>
                  <td className="py-3">{p.age}</td>
                  <td className="py-3 font-bold text-blue-400">{p.overall}</td>
                  <td className="py-3 text-gray-300">{p.potential}</td>
                  <td className="py-3 text-green-400">
                    €{(p.value || 0).toLocaleString()}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      ) : (
        <p className="text-gray-400 text-center py-8">No players in squad</p>
      )}
    </div>
  );

  // --------------------- MATCHES ---------------------
  const renderMatches = () => (
    <div className="bg-gray-800 rounded-2xl shadow-lg p-6 text-gray-100">
      <h2 className="text-xl font-semibold mb-6">Recent Matches</h2>
      {recentMatches.length > 0 ? (
        <div className="space-y-4">
          {recentMatches.map((m) => (
            <div
              key={m.id}
              className="flex items-center justify-between p-4 bg-gray-700 rounded-lg"
            >
              <div className="flex items-center gap-4 flex-1">
                <div className="text-sm text-gray-400 w-24">
                  {new Date(m.matchDate).toLocaleDateString()}
                </div>
                <div className="flex-1 grid grid-cols-3 items-center gap-4">
                  <div className="text-right font-medium">
                    {m.homeTeam.name}
                  </div>
                  <div className="text-center font-bold text-lg">
                    {m.homeTeamGoals} - {m.awayTeamGoals}
                  </div>
                  <div className="font-medium">{m.awayTeam.name}</div>
                </div>
                <div
                  className={`w-8 h-8 rounded-full flex items-center justify-center font-bold ${
                    m.result === "W"
                      ? "bg-green-800 text-green-300"
                      : m.result === "D"
                      ? "bg-yellow-800 text-yellow-300"
                      : "bg-red-800 text-red-300"
                  }`}
                >
                  {m.result}
                </div>
              </div>
            </div>
          ))}
        </div>
      ) : (
        <p className="text-gray-400 text-center py-8">No recent matches</p>
      )}
    </div>
  );

  return (
    <div className="p-6  text-gray-100 bg-gradient-to-b from-gray-900 via-gray-800 to-gray-900 min-h-screen">
      {/* Header */}
      <div className="flex flex-col md:flex-row items-center gap-6 bg-gray-800 rounded-2xl p-6 shadow-lg">
        <TeamLogo
          teamName={club.name}
          logoFileName={club.logoFileName}
          className="w-32 h-32"
        />
        <div className="flex-1 text-center md:text-left">
          <h1 className="text-4xl font-bold text-white">{club.name}</h1>
          <p className="text-xl text-gray-400 mt-2">
            {club.league?.name || "—"}
          </p>
          <div className="flex flex-wrap gap-4 mt-4 justify-center md:justify-start">
            <div className="flex items-center gap-2 bg-gray-700 px-3 py-1 rounded-full shadow-sm">
              <MapPin className="w-4 h-4 text-red-400" />
              <span className="text-sm">{club.country?.name || "—"}</span>
            </div>
            <div className="flex items-center gap-2 bg-gray-700 px-3 py-1 rounded-full shadow-sm">
              <Shield className="w-4 h-4 text-green-400" />
              <span className="text-sm">
                {club.stadiumName || club.stadium?.name || "—"}
              </span>
            </div>
            {club.manager && (
              <div className="flex items-center gap-2 bg-gray-700 px-3 py-1 rounded-full shadow-sm">
                <Users className="w-4 h-4 text-purple-400" />
                <span className="text-sm">{club.manager}</span>
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Tabs */}
      <div className="bg-gray-800 rounded-2xl shadow-lg">
        <div className="border-b border-gray-700">
          <nav className="flex -mb-px">
            {["overview", "squad", "matches"].map((tab) => (
              <button
                key={tab}
                onClick={() => setActiveTab(tab)}
                className={`py-6 px-6 font-medium border-b-2 transition ${
                  activeTab === tab
                    ? "border-blue-500 text-blue-400"
                    : "border-transparent text-gray-400 hover:text-gray-200"
                }`}
              >
                {tab.charAt(0).toUpperCase() + tab.slice(1)}
              </button>
            ))}
          </nav>
        </div>

        <div className="p-6">
          {activeTab === "overview" && renderOverview()}
          {activeTab === "squad" && renderSquad()}
          {activeTab === "matches" && renderMatches()}
        </div>
      </div>
    </div>
  );
};

export default Club;
