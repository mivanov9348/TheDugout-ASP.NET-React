// src/pages/Match.jsx
import React, { useEffect, useState } from "react";
import { useParams, useNavigate, Link } from "react-router-dom";
import TeamLogo from "../../components/TeamLogo";
import {
  ArrowLeft,
  Trophy,
  Clock,
  MapPin,
  Calendar,
  Users,
  AlertCircle,
} from "lucide-react";


const placeholderLogo = "https://via.placeholder.com/128x128.png?text=No+Logo";

const formatDate = (iso) => {
  try {
    const d = new Date(iso);
    return d.toLocaleString(undefined, {
      year: "numeric",
      month: "short",
      day: "numeric",
    });
  } catch {
    return iso ?? "";
  }
};

const Match = () => {
  const { matchId } = useParams();
  const navigate = useNavigate();

  const [match, setMatch] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    let cancelled = false;

    const fetchMatch = async () => {
      setLoading(true);
      setError(null);
      try {
        const res = await fetch(`/api/matches/${matchId}`, {
          credentials: "include",
        });

        if (!res.ok) {
          const text = await res.text();
          throw new Error(text || `HTTP ${res.status}`);
        }

        const data = await res.json();
        if (!cancelled) {
          setMatch(data);
          console.log("Match data:", data);
        }
      } catch (err) {
        if (!cancelled) {
          console.error("Error loading match:", err);
          setError(err.message || "Error loading match");
        }
      } finally {
        if (!cancelled) setLoading(false);
      }
    };

    fetchMatch();

    return () => {
      cancelled = true;
    };
  }, [matchId]);

  if (loading)
    return (
      <div className="min-h-screen flex items-center justify-center bg-slate-900 text-white text-lg animate-pulse">
        Loading match...
      </div>
    );

  if (error)
    return (
      <div className="min-h-screen bg-slate-900 text-white flex items-center justify-center p-6">
        <div className="max-w-xl bg-white/5 p-8 rounded-2xl border border-white/10 text-center">
          <AlertCircle className="mx-auto mb-4 w-12 h-12 text-red-400" />
          <h2 className="text-lg font-bold">Failed to load</h2>
          <p className="mt-2 text-gray-300">{error}</p>
          <div className="mt-6">
            <button
              onClick={() => navigate(-1)}
              className="px-4 py-2 bg-blue-600 hover:bg-blue-500 rounded-md font-semibold"
            >
              Back
            </button>
          </div>
        </div>
      </div>
    );

  const dateText = match.date ? formatDate(match.date) : "—";
  const competition = match.competition ?? "—";
  const stadium = match.stadium ?? "—";
  const status = match.status ?? "Unknown";

  const home =
    match.homeTeam ?? { name: "Home", logo: placeholderLogo, goals: [], penalties: [] };
  const away =
    match.awayTeam ?? { name: "Away", logo: placeholderLogo, goals: [], penalties: [] };

  const homeGoalsCount = Array.isArray(home.goals) ? home.goals.length : 0;
  const awayGoalsCount = Array.isArray(away.goals) ? away.goals.length : 0;

  const hasPenalties =
    (Array.isArray(home.penalties) && home.penalties.length > 0) ||
    (Array.isArray(away.penalties) && away.penalties.length > 0);

  const GoalLine = ({ goal, team }) => {
    return (
      <li className="flex justify-between items-center text-gray-200 text-sm">
        <div className="truncate">
          {goal.playerId ? (
            <Link
              to={`/player/${goal.playerId}`}
              className="font-medium hover:underline hover:text-white transition-colors"
            >
              {goal.scorer}
            </Link>
          ) : (
            <span className="font-medium">{goal.scorer}</span>
          )}
        </div>
        <div className="ml-4 text-xs font-semibold">
          <span className={team === "home" ? "text-blue-400" : "text-red-400"}>
            {goal.minute}'
          </span>{" "}
          <span className="ml-1">⚽</span>
        </div>
      </li>
    );
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-900 via-blue-900 to-indigo-900 text-gray-100 flex justify-center items-start py-14 px-6 relative overflow-hidden">
      <div className="absolute inset-0 bg-[url('https://i.imgur.com/Q7m5F1W.jpg')] bg-cover bg-center opacity-8 blur-sm"></div>
      <div className="absolute inset-0 bg-gradient-to-b from-slate-900/90 via-slate-800/80 to-blue-900/90"></div>

      <div className="relative z-10 bg-white/10 backdrop-blur-lg shadow-2xl rounded-3xl max-w-5xl w-full p-6 md:p-10 border border-white/20 transition-all">
        {/* Back */}
        <button
          onClick={() => navigate(-1)}
          className="absolute top-6 left-6 flex items-center text-gray-300 hover:text-white transition-colors"
        >
          <ArrowLeft className="w-5 h-5 mr-2" />
          <span className="hidden sm:inline font-medium">Back</span>
        </button>

        {/* Top info */}
        <div className="text-center pt-6">
          <h1 className="text-2xl md:text-3xl font-extrabold text-white drop-shadow-lg flex justify-center items-center gap-3">
            <Trophy className="text-yellow-400 w-6 h-6" />
            Match Details
          </h1>

          <div className="mt-3 flex flex-col sm:flex-row items-center justify-center gap-4 text-gray-300 text-sm">
            <div className="flex items-center gap-2">
              <Calendar className="w-4 h-4 text-blue-400" />
              <span>{dateText}</span>
            </div>
            <div className="flex items-center gap-2">
              <MapPin className="w-4 h-4 text-green-400" />
              <span>{stadium}</span>
            </div>
            <div className="flex items-center gap-2">
              <Users className="w-4 h-4 text-indigo-400" />
              <span>{competition}</span>
            </div>
            <div className="flex items-center gap-2">
              <Clock className="w-4 h-4 text-emerald-400" />
              <span>{status}</span>
            </div>
          </div>
        </div>

        {/* Scoreboard */}
        <div className="mt-8 md:mt-10 flex flex-col md:flex-row justify-between items-center gap-6">
          {/* Home */}
          <div className="flex flex-col items-center w-full md:w-1/3">
            <TeamLogo
  teamName={home.name}
  logoUrl={home.logo}
  className="w-20 h-20 drop-shadow-[0_0_20px_rgba(37,99,235,0.45)]"
/>

            <h2 className="text-lg md:text-xl font-bold mt-3 text-white text-center">
              {home.name}
            </h2>
          </div>

          {/* Score */}
          <div className="text-center w-full md:w-auto">
            <div className="text-5xl md:text-6xl font-extrabold text-white tracking-wider drop-shadow-lg">
              {homeGoalsCount} <span className="text-blue-400">:</span>{" "}
              {awayGoalsCount}
            </div>
            <div className="mt-2 flex justify-center items-center gap-2 text-gray-300 text-sm">
              <Clock className="w-4 h-4 text-emerald-400" />
              <span>
                {status === "Live"
                  ? `${match.currentMinute ?? ""}'`
                  : "Full Time"}
              </span>
            </div>
          </div>

          {/* Away */}
          <div className="flex flex-col items-center w-full md:w-1/3">
            <TeamLogo
  teamName={away.name}
  logoUrl={away.logo}
  className="w-20 h-20 drop-shadow-[0_0_20px_rgba(239,68,68,0.45)]"
/>

            <h2 className="text-lg md:text-xl font-bold mt-3 text-white text-center">
              {away.name}
            </h2>
          </div>
        </div>

        {/* Goals & Penalties */}
        <div className="mt-10 grid grid-cols-1 md:grid-cols-2 gap-6">
          {/* Home goals */}
          <div className="bg-white/5 rounded-2xl p-6 border border-white/10 shadow-lg hover:bg-white/10 transition-all">
            <h3 className="text-lg font-semibold text-blue-300 mb-4 border-b border-blue-400/30 pb-2">
              {home.name} — Goal Scorers
            </h3>

            {Array.isArray(home.goals) && home.goals.length > 0 ? (
              <ul className="space-y-3">
                {home.goals.map((g, idx) => (
                  <GoalLine key={idx} goal={g} team="home" />
                ))}
              </ul>
            ) : (
              <p className="text-gray-400 italic">No goals scored.</p>
            )}

            {/* Home penalties */}
            {Array.isArray(home.penalties) && home.penalties.length > 0 && (
              <div className="mt-4">
                <h4 className="text-sm font-semibold text-gray-300 mb-2">
                  Penalties
                </h4>
                <ul className="text-sm text-gray-200 space-y-2">
                  {home.penalties.map((p, i) => (
                    <li key={i} className="flex justify-between">
                      <span>{p.shooterName ?? p.shooter ?? "Player"}</span>
                      <span
                        className={
                          p.isScored
                            ? "text-emerald-400 font-bold"
                            : "text-red-400 font-semibold"
                        }
                      >
                        {p.isScored ? "⚽" : "✖"}
                      </span>
                    </li>
                  ))}
                </ul>
              </div>
            )}
          </div>

          {/* Away goals */}
          <div className="bg-white/5 rounded-2xl p-6 border border-white/10 shadow-lg hover:bg-white/10 transition-all">
            <h3 className="text-lg font-semibold text-red-300 mb-4 border-b border-red-400/30 pb-2">
              {away.name} — Goal Scorers
            </h3>

            {Array.isArray(away.goals) && away.goals.length > 0 ? (
              <ul className="space-y-3">
                {away.goals.map((g, idx) => (
                  <GoalLine key={idx} goal={g} team="away" />
                ))}
              </ul>
            ) : (
              <p className="text-gray-400 italic">No goals scored.</p>
            )}

            {/* Away penalties */}
            {Array.isArray(away.penalties) && away.penalties.length > 0 && (
              <div className="mt-4">
                <h4 className="text-sm font-semibold text-gray-300 mb-2">
                  Penalties
                </h4>
                <ul className="text-sm text-gray-200 space-y-2">
                  {away.penalties.map((p, i) => (
                    <li key={i} className="flex justify-between">
                      <span>{p.shooterName ?? p.shooter ?? "Player"}</span>
                      <span
                        className={
                          p.isScored
                            ? "text-emerald-400 font-bold"
                            : "text-red-400 font-semibold"
                        }
                      >
                        {p.isScored ? "⚽" : "✖"}
                      </span>
                    </li>
                  ))}
                </ul>
              </div>
            )}
          </div>
        </div>

        {/* Penalties summary */}
        {hasPenalties && (
          <div className="mt-6">
            <div className="bg-white/4 p-4 rounded-xl border border-white/8 text-sm text-gray-300">
              <strong>Score after penalties:</strong>{" "}
              <span className="ml-2">
                {Array.isArray(home.penalties)
                  ? home.penalties.filter((p) => p.isScored).length
                  : 0}{" "}
                -{" "}
                {Array.isArray(away.penalties)
                  ? away.penalties.filter((p) => p.isScored).length
                  : 0}
              </span>
              <span className="ml-4 text-xs text-gray-400">(if applicable)</span>
            </div>
          </div>
        )}

        {/* Footer */}
        <div className="mt-8 text-center text-gray-400 text-sm">
        </div>
      </div>
    </div>
  );
};

export default Match;
