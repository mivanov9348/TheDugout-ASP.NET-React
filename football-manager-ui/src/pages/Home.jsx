import { useEffect, useState } from "react";

function Home({ gameSaveId }) {
  const sections = [
    "Next Match",
    "Last Fixtures",
    "Standings",
    "Form",
    "Top Players",
    "Transfers",
    "Finances",
    "Inbox",
    "News",
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
        if (!res.ok) throw new Error("Грешка при зареждане на Dashboard");
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
    if (title === "Finances") {
      if (loading) return <div className="text-gray-400 italic">Зареждане...</div>;
      if (!dashboard?.finance) return <div className="text-gray-400 italic">Няма данни</div>;

      return (
        <div className="flex flex-col gap-3">
          <div className="text-2xl font-bold text-green-700">
            💰 {dashboard.finance.currentBalance.toLocaleString()} €
          </div>

          <div className="text-sm text-gray-500 font-semibold">Последни транзакции:</div>
          <ul className="text-sm space-y-2">
            {dashboard.finance.recentTransactions.length === 0 ? (
              <li className="italic text-gray-400">Няма транзакции</li>
            ) : (
              dashboard.finance.recentTransactions.map((t, idx) => (
                <li
                  key={idx}
                  className="flex justify-between items-center bg-gray-50 px-3 py-2 rounded-lg shadow-sm"
                >
                  <div className="flex flex-col">
                    <span className="font-medium">{t.description || "—"}</span>
                    <span className="text-xs text-gray-500">
                      {new Date(t.date).toLocaleDateString("bg-BG")}
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
      if (loading) return <div className="text-gray-400 italic">Зареждане...</div>;
      if (!dashboard?.transfers) return <div className="text-gray-400 italic">Няма данни</div>;

      return (
        <div className="flex flex-col gap-3 text-sm">
          {dashboard.transfers.length === 0 ? (
            <div className="italic text-gray-400">Няма трансфери</div>
          ) : (
            <ul className="space-y-2">
              {dashboard.transfers.slice(0, 10).map((tr, idx) => (
                <li
                  key={idx}
                  className="bg-gray-50 px-3 py-2 rounded-lg shadow-sm flex flex-col"
                >
                  <div className="flex justify-between">
                    <span className="font-medium">{tr.player}</span>
                    <span className="text-xs text-gray-500">
                      {new Date(tr.gameDate).toLocaleDateString("bg-BG")}
                    </span>
                  </div>
                  <div className="flex justify-between text-xs text-gray-600">
                    <span>
                      {tr.fromTeam} → {tr.toTeam}
                    </span>
                    <span className="font-semibold text-blue-700">
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

    // Default
    return (
      <div className="flex-1 flex items-center justify-center text-gray-400 italic">
        Няма данни
      </div>
    );
  };

  return (
    <div className="p-6">
      <h2 className="text-2xl font-bold mb-6 text-center">🏟️ Dashboard</h2>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        {sections.map((title, index) => (
          <div
            key={index}
            className="bg-white/80 backdrop-blur-sm rounded-2xl shadow-lg p-4 flex flex-col hover:scale-105 transition-transform duration-200 max-h-64 overflow-y-auto"
          >
            <h3 className="text-lg font-semibold mb-2 text-gray-800">{title}</h3>
            {renderSection(title)}
          </div>
        ))}
      </div>
    </div>
  );
}

export default Home;
