import { useState, useEffect } from "react";

export default function TeamSelectionModal({ open, onClose, onSuccess }) {
  const [teams, setTeams] = useState([]);       // <--- за списъка с отбори
  const [error, setError] = useState("");       // <--- за съобщение при грешка
  const [selected, setSelected] = useState(null);
  const [loading, setLoading] = useState(false);

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
        setTeams(data);   // <--- използваме вече дефинирания setTeams
      } catch (err) {
        console.error("Fetch error:", err);
        setError(err.message);   // <--- използваме вече дефинирания setError
      }
    };

    fetchTemplates();
  }, [open]);

  if (!open) return null;

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

          <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-3">
            {teams.map((t) => (
              <div
                key={t.id}
                onClick={() => setSelected(t.id)}
                className={`p-3 rounded cursor-pointer border ${
                  selected === t.id ? "border-green-500 shadow" : "border-gray-200"
                }`}
              >
                <div className="text-lg font-medium">{t.name}</div>
                <div className="text-sm text-gray-500">{t.abbreviation}</div>
              </div>
            ))}
          </div>
        </div>

        <div className="p-4 border-t flex justify-end gap-2">
          <button onClick={onClose} className="px-4 py-2 rounded bg-gray-200">
            Отказ
          </button>
          <button
            onClick={() => onSuccess(selected)}
            disabled={!selected || loading}
            className="px-4 py-2 rounded bg-green-600 text-white disabled:opacity-50"
          >
            {loading ? "Създавам..." : "Потвърди и започни"}
          </button>
        </div>
      </div>
    </div>
  );
}
