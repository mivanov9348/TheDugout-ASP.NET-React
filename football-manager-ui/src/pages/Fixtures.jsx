import { useEffect, useState } from "react";

const Fixtures = ({ gameSaveId, seasonId }) => {
  const [fixtures, setFixtures] = useState([]);
  const [loading, setLoading] = useState(false);
  const [round, setRound] = useState("");
  const [league, setLeague] = useState("");
  const [leagues, setLeagues] = useState([]);

  useEffect(() => {
    if (!gameSaveId || !seasonId) return;

    const fetchFixtures = async () => {
      try {
        setLoading(true);
        let url = `/api/fixtures/${gameSaveId}/${seasonId}`;
        if (round) url += `?round=${round}`;

        const res = await fetch(url, { credentials: "include" });
        if (!res.ok) throw new Error("Грешка при зареждане на мачовете");
        const data = await res.json();

        setFixtures(data);
        setLeagues([...new Set(data.map((g) => g.leagueName))]);
      } catch (err) {
        console.error(err);
      } finally {
        setLoading(false);
      }
    };

    fetchFixtures();
  }, [gameSaveId, seasonId, round]);

  const filteredFixtures = league
    ? fixtures.filter((f) => f.leagueName === league)
    : fixtures;

  return (
    <div className="bg-white shadow-lg rounded-xl p-4">
      <h1 className="text-xl font-bold mb-4">Fixtures</h1>

      {/* Филтри */}
      <div className="flex flex-wrap gap-4 mb-6">
        {/* Кръгове */}
        <select
          className="border rounded-md px-3 py-2"
          value={round}
          onChange={(e) => setRound(e.target.value)}
        >
          <option value="">Всички кръгове</option>
          {[...Array(38)].map((_, i) => (
            <option key={i + 1} value={i + 1}>
              Round {i + 1}
            </option>
          ))}
        </select>

        {/* Лиги */}
        <select
          className="border rounded-md px-3 py-2"
          value={league}
          onChange={(e) => setLeague(e.target.value)}
        >
          <option value="">Всички лиги</option>
          {leagues.map((l) => (
            <option key={l} value={l}>
              {l}
            </option>
          ))}
        </select>
      </div>

      {/* Таблица с мачове */}
      {loading ? (
        <div className="flex justify-center py-6">
          <div className="animate-spin h-6 w-6 border-2 border-gray-400 border-t-transparent rounded-full"></div>
        </div>
      ) : filteredFixtures.length === 0 ? (
        <p className="text-gray-500">Няма налични мачове.</p>
      ) : (
        filteredFixtures.map((group, i) => (
          <div key={i} className="mb-6">
            <h2 className="text-lg font-semibold mb-2">
              {group.leagueName} – Round {group.round}
            </h2>
            <div className="overflow-x-auto">
              <table className="w-full border-collapse">
                <thead className="bg-gray-100">
                  <tr>
                    <th className="w-1/3 text-left p-2 font-semibold text-gray-700">Домакин</th>
                    <th className="w-1/3 text-left p-2 font-semibold text-gray-700">Гост</th>
                    <th className="w-1/3 text-left p-2 font-semibold text-gray-700">Дата</th>
                  </tr>
                </thead>
                <tbody>
                  {group.matches.map((m) => (
                    <tr key={m.id} className="border-b">
                      <td className="p-2 text-gray-600">{m.homeTeam}</td>
                      <td className="p-2 text-gray-600">{m.awayTeam}</td>
                      <td className="p-2 text-gray-600">
                        {new Date(m.date).toLocaleDateString()}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        ))
      )}
    </div>
  );
};

export default Fixtures;
