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
    if (loading)
      return <div className="text-gray-400 italic text-center">Loading...</div>;

    switch (title) {
      case "Next Match":
        if (!dashboard?.nextMatch)
          return <div className="text-gray-400 italic text-center">No upcoming match</div>;

        const m = dashboard.nextMatch;
        return (
          <div className="flex flex-col items-center gap-2 text-center">
            <div className="text-xs uppercase tracking-wide text-gray-400">
              {new Date(m.date).toLocaleString("en-GB", {
                day: "2-digit",
                month: "short",
                year: "numeric",
                hour: "2-digit",
                minute: "2-digit",
              })}
            </div>
            <div className="text-lg font-bold text-gray-100">
              {m.homeTeam} <span className="text-gray-400">vs</span> {m.awayTeam}
            </div>
            <div className="text-sm text-gray-100 font-semibold bg-gray-700 px-2 py-1 rounded-md">
              {m.competition}
            </div>
          </div>
        );

      case "Last Fixtures":
        if (!dashboard?.lastFixtures || dashboard.lastFixtures.length === 0)
          return <div className="text-gray-400 italic text-center">No fixtures</div>;

        return (
          <ul className="space-y-2 text-sm">
            {dashboard.lastFixtures.map((f, idx) => (
              <li
                key={idx}
                className="bg-gray-700 px-3 py-2 rounded-xl shadow-sm hover:shadow-md transition flex justify-between items-center"
              >
                <div>
                  <span className="font-medium text-gray-100">
                    {f.homeTeam} <span className="text-gray-400">vs</span> {f.awayTeam}
                  </span>
                  <div className="text-xs text-gray-400">{f.competition}</div>
                </div>
                <span className="font-semibold text-gray-100">
                  {f.homeGoals}-{f.awayGoals}
                </span>
              </li>
            ))}
          </ul>
        );

      case "Standings":
        if (!dashboard?.standing)
          return <div className="text-gray-400 italic text-center">No standings</div>;

        const s = dashboard.standing;
        return (
          <div className="flex flex-col gap-2 text-sm">
            <div className="font-semibold text-gray-100">{s.league}</div>
            <div className="grid grid-cols-2 gap-2 text-gray-300">
              <span>Position: <strong className="text-gray-100">{s.ranking}</strong></span>
              <span>Points: <strong className="text-gray-100">{s.points}</strong></span>
              <span>Matches: <strong className="text-gray-100">{s.matches}</strong></span>
              <span>Wins: <strong className="text-gray-100">{s.wins}</strong></span>
              <span>Draws: <strong className="text-gray-100">{s.draws}</strong></span>
              <span>Losses: <strong className="text-gray-100">{s.losses}</strong></span>
              <span>Goals: <strong className="text-gray-100">{s.goalsFor}-{s.goalsAgainst}</strong></span>
              <span>GD: <strong className="text-gray-100">{s.goalDifference}</strong></span>
            </div>
          </div>
        );

      case "Form":
        if (!dashboard?.lastFixtures || dashboard.lastFixtures.length === 0)
          return <div className="text-gray-400 italic text-center">No form data</div>;

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
                    ? "bg-green-900 text-green-100"
                    : res === "D"
                    ? "bg-gray-700 text-gray-300"
                    : "bg-red-900 text-red-100"
                }`}
              >
                {res}
              </span>
            ))}
          </div>
        );

      case "Finances":
        if (!dashboard?.finance)
          return <div className="text-gray-400 italic text-center">No financial data</div>;

        return (
          <div className="flex flex-col gap-3">
            <div className="text-2xl font-bold text-green-400">
              💰 {dashboard.finance.currentBalance.toLocaleString()} €
            </div>
            <div className="text-sm text-gray-400 font-semibold">Recent transactions:</div>
            <ul className="space-y-2 text-sm">
              {dashboard.finance.recentTransactions.length === 0 ? (
                <li className="italic text-gray-400">No transactions</li>
              ) : (
                dashboard.finance.recentTransactions.map((t, idx) => (
                  <li
                    key={idx}
                    className="flex justify-between items-center bg-gray-700 px-3 py-2 rounded-lg shadow-sm hover:shadow-md transition"
                  >
                    <div className="flex flex-col">
                      <span className="font-medium text-gray-100">{t.description || "—"}</span>
                      <span className="text-xs text-gray-400">
                        {new Date(t.date).toLocaleDateString("en-GB")}
                      </span>
                    </div>
                    <span
                      className={`font-semibold ${
                        t.amount >= 0 ? "text-green-400" : "text-red-400"
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

      case "Transfers":
        if (!dashboard?.transfers)
          return <div className="text-gray-400 italic text-center">No transfers</div>;

        return (
          <ul className="space-y-2 text-sm">
            {dashboard.transfers.slice(0, 10).map((tr, idx) => (
              <li
                key={idx}
                className="bg-gray-700 px-3 py-2 rounded-lg shadow-sm hover:shadow-md transition flex justify-between items-center"
              >
                <div>
                  <span className="font-medium text-gray-100">{tr.player}</span>
                  <div className="text-xs text-gray-400">
                    {tr.fromTeam} → {tr.toTeam}
                  </div>
                </div>
                <span className="font-semibold text-gray-100">
                  {tr.fee.toLocaleString()} €
                </span>
              </li>
            ))}
          </ul>
        );

      default:
        return <div className="text-gray-400 italic text-center">No data available</div>;
    }
  };

  return (
    <div className="p-8 bg-gradient-to-b from-gray-900 via-gray-800 to-gray-900 text-gray-100 min-h-screen">
      <h2 className="text-3xl font-bold mb-8 text-center text-gray-100 drop-shadow-sm">
        🏟️ Dashboard
      </h2>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        {sections.map((section, index) => (
          <div
            key={index}
            className="bg-gray-700 backdrop-blur-md rounded-2xl shadow-lg p-5 flex flex-col h-64 overflow-y-auto border border-gray-600 hover:border-gray-500 hover:shadow-xl hover:scale-[1.02] transition duration-200"
          >
            <h3 className="text-lg font-semibold mb-3 text-gray-100 flex items-center gap-2">
              {section.icon} {section.title}
            </h3>
            {renderSection(section.title)}
          </div>
        ))}
      </div>
    </div>
  );
}

export default Home;