import { useEffect, useState } from "react";
import {
  Calendar,
  Trophy,
  ListChecks,
  TrendingUp,
  Star,
  Repeat,
  DollarSign,
  Inbox,
  Newspaper,
} from "lucide-react";

function Home({ gameSaveId }) {
  const sections = [
    { title: "Next Match", icon: <Calendar size={18} /> },
    { title: "Last Fixtures", icon: <ListChecks size={18} /> },
    { title: "Standings", icon: <Trophy size={18} /> },
    { title: "Form", icon: <TrendingUp size={18} /> },
    { title: "Top Players", icon: <Star size={18} /> },
    { title: "Transfers", icon: <Repeat size={18} /> },
    { title: "Finances", icon: <DollarSign size={18} /> },
    { title: "Inbox", icon: <Inbox size={18} /> },
    { title: "News", icon: <Newspaper size={18} /> },
  ];

  const [dashboard, setDashboard] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!gameSaveId) return;

    const fetchDashboard = async () => {
      try {
        const res = await fetch(`/api/dashboard/${gameSaveId}`, {
          credentials: "include",
        });
        if (!res.ok) throw new Error("Failed to load Dashboard");
        const data = await res.json();
        setDashboard(data);
      } catch (err) {
        console.error("Dashboard error:", err);
      } finally {
        setLoading(false);
      }
    };

    fetchDashboard();
  }, [gameSaveId]);

  const renderSection = (title) => {
    if (title === "Next Match") {
      if (loading) return <div className="text-gray-400 italic">Loading...</div>;
      if (!dashboard?.nextMatch)
        return <div className="text-gray-400 italic">No upcoming match</div>;

      const m = dashboard.nextMatch;
      return (
        <div className="flex flex-col items-center gap-3 text-center">
          <div className="text-xs uppercase tracking-wide text-gray-500">
            {new Date(m.date).toLocaleString("en-GB", {
              day: "2-digit",
              month: "short",
              year: "numeric",
              hour: "2-digit",
              minute: "2-digit",
            })}
          </div>
          <div className="text-lg font-bold text-gray-800">
            {m.homeTeam} <span className="text-gray-400">vs</span> {m.awayTeam}
          </div>
          <div className="text-sm text-indigo-600 font-semibold bg-indigo-50 px-2 py-1 rounded-md">
            {m.competition}
          </div>
        </div>
      );
    }

    if (title === "Last Fixtures") {
      if (loading) return <div className="text-gray-400 italic">Loading...</div>;
      if (!dashboard?.lastFixtures || dashboard.lastFixtures.length === 0)
        return <div className="text-gray-400 italic">No fixtures</div>;

      return (
        <ul className="space-y-3 text-sm">
          {dashboard.lastFixtures.map((f, idx) => (
            <li
              key={idx}
              className="bg-gradient-to-r from-gray-50 to-gray-100 px-3 py-2 rounded-xl shadow-sm hover:shadow-md transition"
            >
              <div className="flex justify-between items-center">
                <span className="font-medium text-gray-800">
                  {f.homeTeam} <span className="text-gray-400">vs</span> {f.awayTeam}
                </span>
                <span className="text-xs text-gray-500">
                  {new Date(f.date).toLocaleDateString("en-GB")}
                </span>
              </div>
              <div className="flex justify-between text-xs mt-1">
                <span className="text-gray-500">{f.competition}</span>
                <span className="font-bold text-indigo-700">
                  {f.homeGoals} - {f.awayGoals}
                </span>
              </div>
            </li>
          ))}
        </ul>
      );
    }

    if (title === "Standings") {
      if (loading) return <div className="text-gray-400 italic">Loading...</div>;
      if (!dashboard?.standing)
        return <div className="text-gray-400 italic">No standings</div>;

      const s = dashboard.standing;
      return (
        <div className="flex flex-col gap-2 text-sm">
          <div className="font-semibold text-indigo-700">{s.league}</div>
          <div className="grid grid-cols-2 gap-2 text-gray-700">
            <span>Position: <strong>{s.ranking}</strong></span>
            <span>Points: <strong>{s.points}</strong></span>
            <span>Matches: <strong>{s.matches}</strong></span>
            <span>Wins: <strong>{s.wins}</strong></span>
            <span>Draws: <strong>{s.draws}</strong></span>
            <span>Losses: <strong>{s.losses}</strong></span>
            <span>Goals: <strong>{s.goalsFor}-{s.goalsAgainst}</strong></span>
            <span>GD: <strong>{s.goalDifference}</strong></span>
          </div>
        </div>
      );
    }

    if (title === "Form") {
      if (loading) return <div className="text-gray-400 italic">Loading...</div>;
      if (!dashboard?.lastFixtures || dashboard.lastFixtures.length === 0)
        return <div className="text-gray-400 italic">No form data</div>;

      const form = dashboard.lastFixtures.map((f) => {
        const isHome = f.homeTeam === dashboard.userTeamName;
        const gf = isHome ? f.homeGoals : f.awayGoals;
        const ga = isHome ? f.awayGoals : f.homeGoals;
        if (gf > ga) return "W";
        if (gf < ga) return "L";
        return "D";
      });

      return (
        <div className="flex gap-2 justify-center">
          {form.map((res, idx) => (
            <span
              key={idx}
              className={`px-3 py-1 rounded-lg font-semibold shadow-sm ${
                res === "W"
                  ? "bg-green-100 text-green-700"
                  : res === "D"
                  ? "bg-gray-100 text-gray-600"
                  : "bg-red-100 text-red-600"
              }`}
            >
              {res}
            </span>
          ))}
        </div>
      );
    }

    if (title === "Finances") {
      if (loading) return <div className="text-gray-400 italic">Loading...</div>;
      if (!dashboard?.finance)
        return <div className="text-gray-400 italic">No financial data</div>;

      return (
        <div className="flex flex-col gap-3">
          <div className="text-2xl font-bold text-green-700">
            💰 {dashboard.finance.currentBalance.toLocaleString()} €
          </div>
          <div className="text-sm text-gray-500 font-semibold">Recent transactions:</div>
          <ul className="text-sm space-y-2">
            {dashboard.finance.recentTransactions.length === 0 ? (
              <li className="italic text-gray-400">No transactions</li>
            ) : (
              dashboard.finance.recentTransactions.map((t, idx) => (
                <li
                  key={idx}
                  className="flex justify-between items-center bg-gray-50 px-3 py-2 rounded-lg shadow-sm hover:bg-gray-100 transition"
                >
                  <div className="flex flex-col">
                    <span className="font-medium">{t.description || "—"}</span>
                    <span className="text-xs text-gray-500">
                      {new Date(t.date).toLocaleDateString("en-GB")}
                    </span>
                  </div>
                  <span
                    className={`font-semibold ${
                      t.amount >= 0 ? "text-green-600" : "text-red-600"
                    }`}
                  >
                    {t.amount >= 0 ? "+" : ""}
                    {t.amount.toLocaleString()} €
                  </span>
                </li>
              ))
            )}
          </ul>
        </div>
      );
    }

    if (title === "Transfers") {
      if (loading) return <div className="text-gray-400 italic">Loading...</div>;
      if (!dashboard?.transfers)
        return <div className="text-gray-400 italic">No transfers</div>;

      return (
        <div className="flex flex-col gap-3 text-sm">
          {dashboard.transfers.length === 0 ? (
            <div className="italic text-gray-400">No transfers</div>
          ) : (
            <ul className="space-y-2">
              {dashboard.transfers.slice(0, 10).map((tr, idx) => (
                <li
                  key={idx}
                  className="bg-gray-50 px-3 py-2 rounded-lg shadow-sm hover:shadow-md transition"
                >
                  <div className="flex justify-between">
                    <span className="font-medium">{tr.player}</span>
                    <span className="text-xs text-gray-500">
                      {new Date(tr.gameDate).toLocaleDateString("en-GB")}
                    </span>
                  </div>
                  <div className="flex justify-between text-xs text-gray-600 mt-1">
                    <span>
                      {tr.fromTeam} → {tr.toTeam}
                    </span>
                    <span className="font-semibold text-indigo-700">
                      {tr.fee.toLocaleString()} €
                    </span>
                  </div>
                </li>
              ))}
            </ul>
          )}
        </div>
      );
    }

    return (
      <div className="flex-1 flex items-center justify-center text-gray-400 italic">
        No data available
      </div>
    );
  };

  return (
    <div className="p-8 bg-gradient-to-b from-gray-100 to-gray-200 min-h-screen">
      <h2 className="text-3xl font-bold mb-8 text-center text-gray-800 drop-shadow-sm">
        🏟️ Dashboard
      </h2>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        {sections.map((section, index) => (
          <div
            key={index}
            className="bg-white/90 backdrop-blur-md rounded-2xl shadow-lg p-5 flex flex-col h-64 overflow-y-auto border border-gray-100 hover:border-indigo-200 hover:shadow-xl hover:scale-[1.02] transition duration-200"
          >
            <h3 className="text-lg font-semibold mb-3 text-indigo-700 flex items-center gap-2">
              {section.icon}
              {section.title}
            </h3>
            {renderSection(section.title)}
          </div>
        ))}
      </div>
    </div>
  );
}

export default Home;
