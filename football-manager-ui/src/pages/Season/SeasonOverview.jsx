import { useState, useEffect } from "react";
import { ArrowLeft, ArrowRight, Trophy, Star, Users } from "lucide-react";
import { useParams, useNavigate } from "react-router-dom";

export default function SeasonOverview() {
  const { seasonId } = useParams();
  const navigate = useNavigate();

  const [overview, setOverview] = useState(null);
  const [index, setIndex] = useState(0);
  const [animating, setAnimating] = useState(false);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchOverview = async () => {
      try {
        setLoading(true);
        const res = await fetch(`/api/season/${seasonId}/overview`);
        if (!res.ok) throw new Error("Error while loading data");
        const data = await res.json();
        setOverview(data);
      } catch (err) {
        console.error(err);
      } finally {
        setLoading(false);
      }
    };
    fetchOverview();
  }, [seasonId]);

  const next = () => {
    if (!overview || animating) return;
    setAnimating(true);
    setTimeout(() => {
      setIndex((prev) => (prev + 1) % overview.competitions.length);
      setAnimating(false);
    }, 300);
  };

  const prev = () => {
    if (!overview || animating) return;
    setAnimating(true);
    setTimeout(() => {
      setIndex(
        (prev) => (prev - 1 + overview.competitions.length) % overview.competitions.length
      );
      setAnimating(false);
    }, 300);
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center h-screen bg-slate-900 text-white">
        <p className="text-xl animate-pulse">Loading season overview...</p>
      </div>
    );
  }

  if (!overview || overview.competitions.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center h-screen bg-slate-900 text-white">
        <h1 className="text-3xl font-bold mb-4">No data available for this season</h1>
        <button
          onClick={() => navigate(-1)}
          className="px-4 py-2 bg-slate-700 hover:bg-slate-600 rounded-lg transition"
        >
          🔙 Go Back
        </button>
      </div>
    );
  }

  const competition = overview.competitions[index];
  const topScorer = competition.awards?.find((a) => a.awardType === "TopScorer");

  return (
    <div className="flex flex-col items-center justify-start h-screen w-screen bg-gradient-to-b from-slate-900 via-slate-800 to-slate-900 text-white overflow-hidden relative p-8">
      {/* Header */}
      <div className="flex justify-between items-center w-full max-w-6xl mb-8">
        <h1 className="text-5xl font-extrabold tracking-wide flex items-center gap-3">
          <Trophy className="w-10 h-10 text-yellow-400" /> {competition.name}
        </h1>
        <div className="text-slate-400 text-sm">
          Slide {index + 1} / {overview.competitions.length}
        </div>
      </div>

      {/* Competition Card */}
      <div className="relative w-full max-w-6xl flex flex-col md:flex-row gap-6 bg-slate-800/40 rounded-3xl p-8 shadow-2xl border border-slate-700 backdrop-blur-md transition-all duration-500">
        <button
          onClick={prev}
          className="absolute left-4 top-1/2 -translate-y-1/2 p-3 bg-slate-700/60 hover:bg-slate-600 rounded-full transition"
        >
          <ArrowLeft />
        </button>

        <button
          onClick={next}
          className="absolute right-4 top-1/2 -translate-y-1/2 p-3 bg-slate-700/60 hover:bg-slate-600 rounded-full transition"
        >
          <ArrowRight />
        </button>

        {/* Left Side - Competition Summary */}
        <div className="flex-1 flex flex-col justify-center text-center md:text-left">
          {competition.championTeam && (
            <p className="text-xl mb-3">
              🏆 <strong>Champion:</strong> {competition.championTeam}
            </p>
          )}

          {competition.runnerUpTeam && (
            <p className="text-lg text-slate-300 mb-2">
              🥈 Runner-up: {competition.runnerUpTeam}
            </p>
          )}

          {topScorer && (
            <p className="text-lg mb-2">
              ⚽ <strong>Top Scorer:</strong> {topScorer.playerName} ({topScorer.value} goals)
            </p>
          )}

          {competition.promotedTeams?.length > 0 && (
            <p className="text-slate-300 mb-2">
              ⬆️ Promoted: {competition.promotedTeams.join(", ")}
            </p>
          )}

          {competition.relegatedTeams?.length > 0 && (
            <p className="text-slate-300 mb-2">
              ⬇️ Relegated: {competition.relegatedTeams.join(", ")}
            </p>
          )}

          {competition.europeanQualifiedTeams?.length > 0 && (
            <p className="text-slate-300 mb-2">
              🌍 Qualified for Europe: {competition.europeanQualifiedTeams.join(", ")}
            </p>
          )}
        </div>

        {/* Right Side - Stats Cards */}
        <div className="flex-1 grid grid-cols-2 gap-4">
          <div className="bg-slate-700/50 rounded-2xl p-4 flex flex-col justify-center items-center text-center shadow-lg hover:bg-slate-700/70 transition">
            <Star className="text-yellow-400 mb-2" />
            <h3 className="text-lg font-semibold mb-1">Top Scorer</h3>
            <p className="text-slate-300 text-sm">
              {topScorer?.playerName || "N/A"} ({topScorer?.value || 0} goals)
            </p>
          </div>

          <div className="bg-slate-700/50 rounded-2xl p-4 flex flex-col justify-center items-center text-center shadow-lg hover:bg-slate-700/70 transition">
            <Users className="text-green-400 mb-2" />
            <h3 className="text-lg font-semibold mb-1">European Spots</h3>
            <p className="text-slate-300 text-sm">
              {competition.europeanQualifiedTeams?.join(", ") || "N/A"}
            </p>
          </div>

          <div className="bg-slate-700/50 rounded-2xl p-4 flex flex-col justify-center items-center text-center shadow-lg hover:bg-slate-700/70 transition">
            <Trophy className="text-blue-400 mb-2" />
            <h3 className="text-lg font-semibold mb-1">Champion</h3>
            <p className="text-slate-300 text-sm">{competition.championTeam || "N/A"}</p>
          </div>

          <div className="bg-slate-700/50 rounded-2xl p-4 flex flex-col justify-center items-center text-center shadow-lg hover:bg-slate-700/70 transition">
            <Star className="text-red-400 mb-2" />
            <h3 className="text-lg font-semibold mb-1">Runner-up</h3>
            <p className="text-slate-300 text-sm">{competition.runnerUpTeam || "N/A"}</p>
          </div>
        </div>
      </div>

      {/* Footer - Extra Stats */}
      <div className="mt-10 w-full max-w-6xl grid grid-cols-3 gap-6">

        {/* Top Scorers */}
        <div className="bg-slate-800/40 rounded-2xl p-6 text-center border border-slate-700">
          <h3 className="text-xl font-bold mb-4 text-yellow-400">Top 5 Scorers</h3>
          {competition.topScorers && competition.topScorers.length > 0 ? (
            <ul className="space-y-1 text-sm">
              {competition.topScorers.slice(0, 5).map((p, i) => (
                <li key={i} className="text-slate-300">
                  {i + 1}. {p.playerName} –{" "}
                  <span className="text-yellow-400">{p.goals}</span> ⚽
                </li>
              ))}
            </ul>
          ) : (
            <p className="text-slate-400 text-sm">No scorers data</p>
          )}
        </div>

        {/* League Table */}
        <div className="bg-slate-800/40 rounded-2xl p-6 text-center border border-slate-700 overflow-x-auto">
          <h3 className="text-xl font-bold mb-4 text-green-400">League Table</h3>
          {competition.standings && competition.standings.length > 0 ? (
            <table className="w-full text-sm">
              <thead className="text-slate-400 border-b border-slate-700">
                <tr>
                  <th className="text-left p-1">#</th>
                  <th className="text-left p-1">Team</th>
                  <th className="p-1">Pts</th>
                  <th className="p-1">GD</th>
                </tr>
              </thead>
              <tbody>
                {competition.standings.slice(0, 10).map((team, i) => (
                  <tr key={i} className="border-b border-slate-800">
                    <td className="text-slate-400 p-1">{team.ranking}</td>
                    <td className="text-left p-1">{team.teamName}</td>
                    <td className="text-slate-300 p-1">{team.points}</td>
                    <td className="text-slate-400 p-1">{team.goalDifference}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          ) : (
            <p className="text-slate-400 text-sm">No standings available</p>
          )}
        </div>

        {/* Top Assists (placeholder) */}
        <div className="bg-slate-800/40 rounded-2xl p-6 text-center border border-slate-700">
          <h3 className="text-xl font-bold mb-4 text-blue-400">Top Assists</h3>
          <p className="text-slate-400 text-sm">Coming soon...</p>
        </div>
      </div>
    </div>
  );
}
