import React, { useEffect, useState } from "react";
import { 
  Users, 
  Trophy, 
  Shield, 
  MapPin, 
  Flag, 
  TrendingUp, 
  DollarSign, 
  Target,
  Calendar,
  Star,
  Award,
  BarChart3,
  Clock,
  Search
} from "lucide-react";
import TeamLogo from "../../components/TeamLogo";

const Club = () => {
  const [club, setClub] = useState(null);
  const [stats, setStats] = useState(null);
  const [fixtures, setFixtures] = useState([]);
  const [recentMatches, setRecentMatches] = useState([]);
  const [squad, setSquad] = useState([]);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState('overview');

  useEffect(() => {
    const fetchClubData = async () => {
      try {
        // Fetch current game for user team
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

        // Fetch all club data in parallel
        const [clubRes, statsRes, fixturesRes, recentRes, squadRes] = await Promise.all([
          fetch(`/api/clubs/${userTeam.id}`, { credentials: "include" }),
          fetch(`/api/clubs/${userTeam.id}/stats`, { credentials: "include" }),
          fetch(`/api/clubs/${userTeam.id}/fixtures?limit=5`, { credentials: "include" }),
          fetch(`/api/clubs/${userTeam.id}/recent-matches?limit=5`, { credentials: "include" }),
          fetch(`/api/clubs/${userTeam.id}/squad`, { credentials: "include" })
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

  if (loading) {
    return (
      <div className="flex justify-center items-center min-h-64">
        <p className="text-center text-gray-500 animate-pulse">
          Loading your club...
        </p>
      </div>
    );
  }

  if (!club) {
    return (
      <div className="flex justify-center items-center min-h-64">
        <p className="text-center text-gray-500">
          No club selected or club not found.
        </p>
      </div>
    );
  }

  const renderOverview = () => (
    <div className="space-y-6">
      {/* Stats Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <div className="bg-white rounded-2xl shadow-lg p-6 border-l-4 border-green-500">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium text-gray-600">Balance</p>
              <p className="text-2xl font-bold text-gray-900">
                €{(club.balance || 0).toLocaleString()}
              </p>
            </div>
            <DollarSign className="w-8 h-8 text-green-500" />
          </div>
        </div>

        <div className="bg-white rounded-2xl shadow-lg p-6 border-l-4 border-yellow-500">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium text-gray-600">Popularity</p>
              <p className="text-2xl font-bold text-gray-900">{club.popularity || 0}/100</p>
            </div>
            <Star className="w-8 h-8 text-yellow-500" />
          </div>
          <div className="mt-2">
            <div className="w-full bg-gray-200 rounded-full h-2">
              <div 
                className="bg-yellow-500 h-2 rounded-full" 
                style={{ width: `${club.popularity || 0}%` }}
              ></div>
            </div>
          </div>
        </div>

        <div className="bg-white rounded-2xl shadow-lg p-6 border-l-4 border-blue-500">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium text-gray-600">Squad Size</p>
              <p className="text-2xl font-bold text-gray-900">{squad.length || 0}</p>
            </div>
            <Users className="w-8 h-8 text-blue-500" />
          </div>
        </div>

        {stats && (
          <div className="bg-white rounded-2xl shadow-lg p-6 border-l-4 border-purple-500">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-gray-600">Win Rate</p>
                <p className="text-2xl font-bold text-gray-900">
                  {stats.winPercentage?.toFixed(1)}%
                </p>
              </div>
              <TrendingUp className="w-8 h-8 text-purple-500" />
            </div>
          </div>
        )}
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Season Statistics */}
        {stats && (
          <div className="bg-white rounded-2xl shadow-lg p-6">
            <h2 className="text-xl font-semibold mb-4 flex items-center gap-2">
              <BarChart3 className="w-5 h-5" />
              Season Statistics
            </h2>
            <div className="grid grid-cols-2 gap-4">
              <div className="text-center p-3 bg-gray-50 rounded-lg">
                <p className="text-2xl font-bold text-blue-600">{stats.matchesPlayed || 0}</p>
                <p className="text-sm text-gray-600">Played</p>
              </div>
              <div className="text-center p-3 bg-gray-50 rounded-lg">
                <p className="text-2xl font-bold text-green-600">{stats.wins || 0}</p>
                <p className="text-sm text-gray-600">Wins</p>
              </div>
              <div className="text-center p-3 bg-gray-50 rounded-lg">
                <p className="text-2xl font-bold text-yellow-600">{stats.draws || 0}</p>
                <p className="text-sm text-gray-600">Draws</p>
              </div>
              <div className="text-center p-3 bg-gray-50 rounded-lg">
                <p className="text-2xl font-bold text-red-600">{stats.losses || 0}</p>
                <p className="text-sm text-gray-600">Losses</p>
              </div>
              <div className="text-center p-3 bg-gray-50 rounded-lg col-span-2">
                <p className="text-lg font-bold text-gray-800">
                  {stats.goalsFor || 0} : {stats.goalsAgainst || 0}
                </p>
                <p className="text-sm text-gray-600">Goals For : Against</p>
              </div>
            </div>
          </div>
        )}

        {/* Upcoming Fixtures */}
        <div className="bg-white rounded-2xl shadow-lg p-6">
          <h2 className="text-xl font-semibold mb-4 flex items-center gap-2">
            <Clock className="w-5 h-5" />
            Upcoming Fixtures
          </h2>
          {fixtures.length > 0 ? (
            <div className="space-y-3">
              {fixtures.map((fixture) => (
                <div key={fixture.id} className="flex items-center justify-between p-3 bg-gray-50 rounded-lg">
                  <div className="flex items-center gap-3">
                    <div className="text-sm font-medium text-right w-20">
                      {new Date(fixture.matchDate).toLocaleDateString()}
                    </div>
                    <div className="flex-1">
                      <div className="font-medium">{fixture.homeTeam.name}</div>
                      <div className="font-medium">{fixture.awayTeam.name}</div>
                    </div>
                  </div>
                  <div className="text-sm text-gray-500">
                    {fixture.league?.name || "Friendly"}
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <p className="text-gray-500 text-center py-4">No upcoming fixtures</p>
          )}
        </div>
      </div>
    </div>
  );

  const renderSquad = () => (
    <div className="bg-white rounded-2xl shadow-lg p-6">
      <div className="flex items-center justify-between mb-6">
        <h2 className="text-xl font-semibold">Squad</h2>
        <span className="bg-blue-100 text-blue-800 text-sm font-medium px-2.5 py-0.5 rounded">
          {squad.length} players
        </span>
      </div>
      {squad.length > 0 ? (
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead>
              <tr className="border-b">
                <th className="text-left py-3 font-medium">Player</th>
                <th className="text-left py-3 font-medium">Position</th>
                <th className="text-left py-3 font-medium">Age</th>
                <th className="text-left py-3 font-medium">OVR</th>
                <th className="text-left py-3 font-medium">POT</th>
                <th className="text-left py-3 font-medium">Value</th>
              </tr>
            </thead>
            <tbody>
              {squad.map((player) => (
                <tr key={player.id} className="border-b hover:bg-gray-50">
                  <td className="py-3 font-medium">{player.name}</td>
                  <td className="py-3">{player.position}</td>
                  <td className="py-3">{player.age}</td>
                  <td className="py-3 font-bold">{player.overall}</td>
                  <td className="py-3">{player.potential}</td>
                  <td className="py-3">€{(player.value || 0).toLocaleString()}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      ) : (
        <p className="text-gray-500 text-center py-8">No players in squad</p>
      )}
    </div>
  );

  const renderMatches = () => (
    <div className="bg-white rounded-2xl shadow-lg p-6">
      <h2 className="text-xl font-semibold mb-6">Recent Matches</h2>
      {recentMatches.length > 0 ? (
        <div className="space-y-4">
          {recentMatches.map((match) => (
            <div key={match.id} className="flex items-center justify-between p-4 bg-gray-50 rounded-lg">
              <div className="flex items-center gap-4 flex-1">
                <div className="text-sm text-gray-500 w-24">
                  {new Date(match.matchDate).toLocaleDateString()}
                </div>
                <div className="flex-1 grid grid-cols-3 items-center gap-4">
                  <div className="text-right font-medium">{match.homeTeam.name}</div>
                  <div className="text-center font-bold text-lg">
                    {match.homeTeamGoals} - {match.awayTeamGoals}
                  </div>
                  <div className="font-medium">{match.awayTeam.name}</div>
                </div>
                <div className={`w-8 h-8 rounded-full flex items-center justify-center font-bold ${
                  match.result === 'W' ? 'bg-green-100 text-green-800' :
                  match.result === 'D' ? 'bg-yellow-100 text-yellow-800' :
                  'bg-red-100 text-red-800'
                }`}>
                  {match.result}
                </div>
              </div>
            </div>
          ))}
        </div>
      ) : (
        <p className="text-gray-500 text-center py-8">No recent matches</p>
      )}
    </div>
  );

  return (
    <div className="p-6 max-w-7xl mx-auto space-y-6">
      {/* Header */}
      <div className="flex flex-col md:flex-row items-center gap-6 bg-gradient-to-r from-blue-50 to-indigo-50 rounded-2xl p-6 shadow-lg">
        <TeamLogo
          teamName={club.name}
          logoFileName={club.logoFileName}
          className="w-32 h-32"
        />
        <div className="flex-1 text-center md:text-left">
          <h1 className="text-4xl font-bold text-gray-900">{club.name}</h1>
          <p className="text-xl text-gray-600 mt-2">{club.league?.name || "—"}</p>
          <div className="flex flex-wrap gap-4 mt-4 justify-center md:justify-start">
            <div className="flex items-center gap-2 bg-white px-3 py-1 rounded-full shadow-sm">
              <MapPin className="w-4 h-4 text-red-500" />
              <span className="text-sm font-medium">{club.country?.name || "—"}</span>
            </div>
            <div className="flex items-center gap-2 bg-white px-3 py-1 rounded-full shadow-sm">
              <Shield className="w-4 h-4 text-green-600" />
              <span className="text-sm font-medium">{club.stadiumName || club.stadium?.name || "—"}</span>
            </div>
            {club.manager && (
              <div className="flex items-center gap-2 bg-white px-3 py-1 rounded-full shadow-sm">
                <Users className="w-4 h-4 text-purple-500" />
                <span className="text-sm font-medium">{club.manager}</span>
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Navigation Tabs */}
      <div className="bg-white rounded-2xl shadow-lg">
        <div className="border-b">
          <nav className="flex -mb-px">
            {['overview', 'squad', 'matches'].map((tab) => (
              <button
                key={tab}
                onClick={() => setActiveTab(tab)}
                className={`py-4 px-6 font-medium border-b-2 transition ${
                  activeTab === tab
                    ? 'border-blue-500 text-blue-600'
                    : 'border-transparent text-gray-500 hover:text-gray-700'
                }`}
              >
                {tab.charAt(0).toUpperCase() + tab.slice(1)}
              </button>
            ))}
          </nav>
        </div>
        
        <div className="p-6">
          {activeTab === 'overview' && renderOverview()}
          {activeTab === 'squad' && renderSquad()}
          {activeTab === 'matches' && renderMatches()}
        </div>
      </div>
    </div>
  );
};

export default Club;