import { useEffect, useState } from "react";
import TeamLogo from "./TeamLogo";

export default function TeamSelectionModal({ saveId, onClose, onSelected }) {
  const [loading, setLoading] = useState(true);
  const [leagues, setLeagues] = useState([]);
  const [selectedLeagueId, setSelectedLeagueId] = useState(null);
  const [selectedTeamId, setSelectedTeamId] = useState(null);
  const [error, setError] = useState("");

  useEffect(() => {
    const load = async () => {
      try {
        const res = await fetch(`/api/games/${saveId}`, { credentials: "include" });
        if (!res.ok) throw new Error("Грешка при зареждане на лиги/отбори");
        const data = await res.json();
        setLeagues(data.leagues || []);
        if (data.leagues?.length > 0) {
          setSelectedLeagueId(data.leagues[0].id);
        }
      } catch (e) {
        setError(e.message);
      } finally {
        setLoading(false);
      }
    };
    load();
  }, [saveId]);

  const handleConfirm = async () => {
    if (!selectedTeamId) return;
    try {
      let res = await fetch(`/api/games/${saveId}/select-team/${selectedTeamId}`, {
        method: "POST",
        credentials: "include",
      });
      if (!res.ok) {
        const d = await res.json().catch(() => ({}));
        throw new Error(d.message || "Грешка при избор на отбор");
      }

      res = await fetch(`/api/games/current/${saveId}`, {
        method: "POST",
        credentials: "include",
      });
      if (!res.ok) {
        const d = await res.json().catch(() => ({}));
        throw new Error(d.message || "Грешка при активиране на сейфа");
      }

      const fullSave = await res.json();
      onSelected(fullSave);
    } catch (e) {
      setError(e.message);
    }
  };

  const handleCancel = async () => {
    try {
      await fetch(`/api/games/${saveId}`, { method: "DELETE", credentials: "include" });
    } catch {}
    onClose();
  };

  if (loading) {
    return (
      <div className="fixed inset-0 bg-black/70 flex items-center justify-center">
        <div className="bg-slate-800 text-white p-6 rounded-2xl w-[700px]">
          Зареждане на отборите...
        </div>
      </div>
    );
  }

  const currentLeague = leagues.find(l => l.id === selectedLeagueId);

  return (
    <div className="fixed inset-0 bg-black/70 flex items-center justify-center">
      <div className="bg-white rounded-2xl w-[900px] max-h-[90vh] overflow-hidden shadow-2xl flex flex-col">
        {/* header */}
        <div className="px-6 py-4 border-b">
          <h2 className="text-xl font-semibold">Избери отбор</h2>
          <p className="text-sm text-gray-500">Разгледай лигите и потвърди избора си.</p>
        </div>

        {/* league slider */}
        <div className="px-6 py-3 border-b bg-slate-50 overflow-x-auto flex gap-3 scrollbar-thin scrollbar-thumb-gray-300">
          {leagues.map(l => (
            <button
              key={l.id}
              onClick={() => {
                setSelectedLeagueId(l.id);
                setSelectedTeamId(null);
              }}
              className={`px-5 py-3 rounded-xl whitespace-nowrap border text-sm font-medium
                transition-colors
                ${
                  selectedLeagueId === l.id
                    ? "bg-blue-600 text-white border-blue-600"
                    : "bg-white hover:bg-gray-100"
                }`}
            >
              {l.leagueName} • {l.countryName}
            </button>
          ))}
        </div>

        {/* body */}
        <div className="flex-1 overflow-y-auto p-6">
          {error && <div className="bg-red-50 text-red-700 p-3 rounded-lg mb-4">{error}</div>}

          {currentLeague ? (
            <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-5">
              {currentLeague.teams.map(t => (
  <button
  key={t.id}
  onClick={() => setSelectedTeamId(t.id)}
  className={`p-4 rounded-xl border text-left hover:shadow transition
    ${
      selectedTeamId === t.id
        ? "ring-2 ring-blue-500 border-blue-500"
        : "hover:border-gray-400"
    }`}
>
  <div className="flex items-center gap-3">
    <TeamLogo
      teamName={t.name}
      logoFileName={t.logoFileName}
      className="w-10 h-10 flex-shrink-0"
    />
    <div className="min-w-0"> {/* <-- важно за truncate */}
      <div className="text-base font-semibold truncate">{t.name}</div>
      <div className="text-xs text-gray-500 truncate">
        {t.abbreviation} • {t.countryName}
      </div>
    </div>
  </div>
</button>

))}
            </div>
          ) : (
            <div className="text-gray-500">Няма избрана лига</div>
          )}
        </div>

        {/* footer */}
        <div className="px-6 py-4 border-t bg-slate-50 flex justify-end gap-3">
          <button
            onClick={handleCancel}
            className="px-4 py-2 rounded-xl border hover:bg-white transition"
          >
            Cancel (изтрий сейфа)
          </button>
          <button
            onClick={handleConfirm}
            disabled={!selectedTeamId}
            className={`px-4 py-2 rounded-xl text-white transition
              ${
                selectedTeamId
                  ? "bg-blue-600 hover:bg-blue-500"
                  : "bg-gray-400 cursor-not-allowed"
              }`}
          >
            Потвърди избора
          </button>
        </div>
      </div>
    </div>
  );
}
