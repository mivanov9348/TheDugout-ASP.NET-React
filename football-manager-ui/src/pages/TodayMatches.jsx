import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { Trophy, Play } from "lucide-react";
import TeamLogo from "../components/TeamLogo";

export default function TodayMatches() {
  const { gameSaveId } = useParams();
  const [matches, setMatches] = useState([]);
  const [userFixtureId, setUserFixtureId] = useState(null);
  const navigate = useNavigate();

  useEffect(() => {
    fetch(`/api/matches/today/${gameSaveId}`, { credentials: "include" })
      .then((res) => res.json())
      .then((data) => {
        setMatches(data.matches);

        // намери потребителския мач
        const userMatch = data.matches.find((m) => m.isUserTeamMatch);
        if (userMatch) {
          setUserFixtureId(userMatch.fixtureId);
        }
      })
      .catch((err) => console.error("Failed to fetch matches", err));
  }, [gameSaveId]);

  const handleToMatch = () => {
    if (userFixtureId) {
      navigate(`/live-match/${userFixtureId}`);
    }
  };

  const handleSimulate = async () => {
    try {
      const res = await fetch(`/api/matches/simulate/${gameSaveId}`, {
        method: "POST",
        credentials: "include",
      });
      if (!res.ok) {
        alert("Failed to simulate matches");
        return;
      }
      const updated = await fetch(`/api/matches/today/${gameSaveId}`, {
        credentials: "include",
      }).then((r) => r.json());
      setMatches(updated.matches);
    } catch (err) {
      console.error(err);
      alert("Error simulating matches");
    }
  };

  // групиране по състезание
  const grouped = matches.reduce((acc, m) => {
    if (!acc[m.competitionName]) acc[m.competitionName] = [];
    acc[m.competitionName].push(m);
    return acc;
  }, {});

  return (
    <div className="p-6 sm:p-8 space-y-10 max-w-5xl mx-auto">
      {/* Simulate / To Match button */}
      <div className="flex justify-center mb-6">
        <button
          onClick={userFixtureId ? handleToMatch : handleSimulate}
          className="flex items-center gap-2 bg-gradient-to-r from-sky-600 to-blue-700 hover:from-sky-700 hover:to-blue-800 text-white px-6 py-3 rounded-2xl shadow-md font-semibold transition transform hover:scale-105"
        >
          <Play className="w-5 h-5" />
          {userFixtureId ? "To Match" : "Simulate Matches"}
        </button>
      </div>


      {matches.length === 0 && (
        <p className="text-center text-gray-500 italic text-lg">
          No matches today.
        </p>
      )}

      {Object.entries(grouped).map(([competition, compMatches]) => (
        <div
          key={competition}
          className="bg-gradient-to-b from-white to-slate-50 rounded-2xl shadow-lg border border-slate-200 p-6 space-y-5"
        >
          {/* Competition header */}
          <h2 className="flex items-center justify-center gap-2 text-2xl font-bold text-slate-700 border-b pb-2">
            <Trophy className="w-6 h-6 text-amber-500" />
            {competition}
          </h2>

          {/* Match list */}
          <div className="space-y-4">
            {compMatches.map((m, idx) => (
              <div
                key={idx}
                className={`flex items-center justify-between px-6 py-4 rounded-xl border transition shadow-sm hover:shadow-md
                  ${
                    m.isUserTeamMatch
                      ? "bg-sky-50 border-sky-300 animate-pulse"
                      : "bg-white border-slate-100"
                  }`}
              >
                {/* Home team */}
                <div className="flex-1 flex items-center justify-end gap-2">
                  <span className="font-semibold text-slate-800">{m.home}</span>
                  <TeamLogo
                    teamName={m.home}
                    logoFileName={m.homeLogoFileName}
                    className="w-8 h-8 rounded-full shadow"
                  />
                </div>

                {/* VS */}
                <span className="px-4 text-gray-500 font-medium">vs</span>

                {/* Away team */}
                <div className="flex-1 flex items-center justify-start gap-2">
                  <TeamLogo
                    teamName={m.away}
                    logoFileName={m.awayLogoFileName}
                    className="w-8 h-8 rounded-full shadow"
                  />
                  <span className="font-semibold text-slate-800">{m.away}</span>
                </div>
              </div>
            ))}
          </div>
        </div>
      ))}
    </div>
  );
}
