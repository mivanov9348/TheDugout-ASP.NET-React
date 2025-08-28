import { useState, useEffect } from "react";

export default function TeamSelectionModal({ open, onClose, onSuccess }) {
  const [teams, setTeams] = useState([]);
  const [error, setError] = useState("");
  const [selectedTeam, setSelectedTeam] = useState(null);
  const [loading, setLoading] = useState(false);

  const [leagues, setLeagues] = useState([]);
  const [selectedLeague, setSelectedLeague] = useState(null);

  useEffect(() => {
    if (!open) return;

    const fetchTemplates = async () => {
      try {
        const res = await fetch("/api/games/teamtemplates", {
          credentials: "include",
        });

        if (!res.ok) {
          const t = await res.text();
          throw new Error(t || "Грешка при зареждане на отборите");
        }

        const data = await res.json();
        setTeams(data);

        // извличаме уникални лиги
        const uniqueLeagues = Array.from(
          new Map(
            data.map((t) => [
              t.LeagueId,
              { id: t.LeagueId, name: t.LeagueName, tier: t.Tier },
            ])
          ).values()
        ).sort((a, b) => a.tier - b.tier || a.name.localeCompare(b.name));

        setLeagues(uniqueLeagues);

        // по подразбиране избираме първата лига
        if (uniqueLeagues.length > 0) {
          setSelectedLeague(uniqueLeagues[0].id);
        }
      } catch (err) {
        console.error("Fetch error:", err);
        setError(err.message);
      }
    };

    fetchTemplates();
  }, [open]);

  if (!open) return null;

  // филтрирани отбори според избраната лига
  const filteredTeams = selectedLeague
    ? teams.filter((t) => t.LeagueId === selectedLeague)
    : [];

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-60">
      <div className="w-11/12 max-w-4xl bg-white rounded-lg overflow-hidden">
        <div className="p-4 border-b flex justify-between items-center">
          <h2 className="text-xl font-semibold">Избери отбор</h2>
          <button onClick={onClose} className="px-3 py-1 rounded bg-gray-200">
            Отказ
          </button>
        </div>

        <div className="p-4">
          {error && <p className="text-red-500 mb-4">{error}</p>}

          {/* Tabs за лигите */}
          <div className="flex gap-2 border-b mb-4 overflow-x-auto">
            {leagues.map((l) => (
              <button
                key={l.id}
                onClick={() => {
                  setSelectedLeague(l.id);
                  setSelectedTeam(null);
                }}
                className={`px-4 py-2 whitespace-nowrap rounded-t ${
                  selectedLeague === l.id
                    ? "bg-green-600 text-white"
                    : "bg-gray-200 hover:bg-gray-300"
                }`}
              >
                {l.name} (Tier {l.tier})
              </button>
            ))}
          </div>

          {/* Отбори */}
          <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-3">
            {filteredTeams.map((t) => (
              <div
                key={t.id}
                onClick={() => setSelectedTeam(t.id)}
                className={`p-3 rounded cursor-pointer border ${
                  selectedTeam === t.id
                    ? "border-green-500 shadow bg-green-50"
                    : "border-gray-200 hover:border-gray-400"
                }`}
              >
                <div className="text-lg font-medium">{t.name}</div>
                <div className="text-sm text-gray-500">{t.abbreviation}</div>
              </div>
            ))}
            {filteredTeams.length === 0 && (
              <p className="col-span-full text-gray-500">
                Няма отбори в тази лига.
              </p>
            )}
          </div>
        </div>

        <div className="p-4 border-t flex justify-end gap-2">
          <button onClick={onClose} className="px-4 py-2 rounded bg-gray-200">
            Отказ
          </button>
          <button
            onClick={() => onSuccess(selectedTeam)}
            disabled={!selectedTeam || loading}
            className="px-4 py-2 rounded bg-green-600 text-white disabled:opacity-50"
          >
            {loading ? "Създавам..." : "Потвърди и започни"}
          </button>
        </div>
      </div>
    </div>
  );
}
