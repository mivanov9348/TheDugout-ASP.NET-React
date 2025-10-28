import { useEffect, useState } from "react";
import Swal from "sweetalert2";
import { ArrowLeft } from "lucide-react";

export default function AdminPage() {
  const [activeTab, setActiveTab] = useState("settings");
  const [settings, setSettings] = useState([]);
  const [editedSettings, setEditedSettings] = useState({});
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (activeTab === "settings") loadSettings();
  }, [activeTab]);

  const loadSettings = async () => {
    try {
      setLoading(true);
      const res = await fetch("/api/admin/settings", { credentials: "include" });
      if (!res.ok) throw new Error("Failed to load settings");
      const data = await res.json();
      setSettings(data);
    } catch (err) {
      console.error(err);
      Swal.fire("Грешка", "Не успяхме да заредим настройките.", "error");
    } finally {
      setLoading(false);
    }
  };

  const handleChange = (id, value) => {
    setEditedSettings((prev) => ({ ...prev, [id]: value }));
  };

  const handleSave = async () => {
    const changes = Object.entries(editedSettings).map(([id, value]) => ({
      id: Number(id),
      value,
    }));

    if (changes.length === 0) {
      Swal.fire("No Changes", "No Edited Settings.", "info");
      return;
    }

    try {
      const res = await fetch("/api/admin/settings/update", {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        credentials: "include",
        body: JSON.stringify(changes),
      });

      if (!res.ok) throw new Error("Update failed");

      Swal.fire("✅ Успех", "Settings are updated Successfully!", "success");
      setEditedSettings({});
      await loadSettings();
    } catch (err) {
      console.error(err);
      Swal.fire("Грешка", "Не успяхме да запишем промените.", "error");
    }
  };

  if (loading)
    return (
      <div className="flex justify-center items-center h-screen bg-gradient-to-b from-gray-900 to-black text-white">
        Зареждане...
      </div>
    );

  return (
    <div className="min-h-screen bg-gradient-to-b from-gray-900 to-black text-white p-6 flex flex-col items-center">
      {/* 🔙 Back Button */}
      <div className="w-full max-w-5xl mb-4 flex justify-start">
        <button
          onClick={() => window.history.back()}
          className="flex items-center gap-2 px-4 py-2 bg-gray-800 hover:bg-gray-700 rounded-xl transition"
        >
          <ArrowLeft size={18} />
          Back
        </button>
      </div>

      <h1 className="text-4xl font-bold mb-8 flex items-center gap-3">
        ⚙️ Admin Panel
      </h1>

      {/* 🔹 Tabs */}
      <div className="flex space-x-4 mb-8 bg-gray-800 rounded-full p-2 shadow-lg">
        <button
          onClick={() => setActiveTab("settings")}
          className={`px-6 py-2 rounded-full transition ${
            activeTab === "settings"
              ? "bg-green-600 text-white"
              : "bg-gray-700 text-gray-300 hover:bg-gray-600"
          }`}
        >
          Game Settings
        </button>
        <button
          onClick={() => setActiveTab("admin")}
          className={`px-6 py-2 rounded-full transition ${
            activeTab === "admin"
              ? "bg-blue-600 text-white"
              : "bg-gray-700 text-gray-300 hover:bg-gray-600"
          }`}
        >
          Admin Tools
        </button>
      </div>

      {/* 🔸 Tab Content */}
      <div className="bg-gray-800 rounded-2xl shadow-lg p-6 w-full max-w-5xl">
        {activeTab === "settings" && (
          <>
            <h2 className="text-2xl font-semibold mb-4 border-b border-gray-700 pb-2">
              🎮 Game Settings
            </h2>

            {settings.length === 0 ? (
              <p className="text-gray-400 text-center mt-4">
                Няма налични настройки.
              </p>
            ) : (
              <>
                <table className="w-full text-left border-collapse">
                  <thead>
                    <tr className="bg-gray-700 text-gray-300">
                      <th className="p-3 border-b border-gray-600 w-1/4">Key</th>
                      <th className="p-3 border-b border-gray-600 w-1/3">Description</th>
                      <th className="p-3 border-b border-gray-600 w-1/4">Category</th>
                      <th className="p-3 border-b border-gray-600">Value</th>
                    </tr>
                  </thead>
                  <tbody>
                    {settings.map((s) => (
                      <tr
                        key={s.id}
                        className="hover:bg-gray-700 transition border-b border-gray-700"
                      >
                        <td className="p-3 font-medium">{s.key}</td>
                        <td className="p-3 text-sm text-gray-300">
                          {s.description}
                        </td>
                        <td className="p-3 italic text-gray-400">{s.category}</td>
                        <td className="p-3">
                          <input
                            type="text"
                            className="w-full bg-gray-900 text-white border border-gray-600 rounded-lg px-2 py-1 focus:ring-2 focus:ring-green-500 outline-none"
                            value={
                              editedSettings[s.id] !== undefined
                                ? editedSettings[s.id]
                                : s.value
                            }
                            onChange={(e) =>
                              handleChange(s.id, e.target.value)
                            }
                          />
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>

                <div className="flex justify-end mt-6">
                  <button
                    onClick={handleSave}
                    className="px-6 py-2 bg-green-600 hover:bg-green-500 text-white rounded-xl font-semibold transition"
                  >
                    💾 Save Changes
                  </button>
                </div>
              </>
            )}
          </>
        )}

        {activeTab === "admin" && (
          <div className="flex flex-col items-center justify-center text-gray-400 py-12">
            <p className="text-lg mb-2">🔒 Admin Tools (coming soon)</p>
            <p className="text-sm text-gray-500">
              Тук ще има промотиране на потребители и управление на системата.
            </p>
          </div>
        )}
      </div>
    </div>
  );
}
