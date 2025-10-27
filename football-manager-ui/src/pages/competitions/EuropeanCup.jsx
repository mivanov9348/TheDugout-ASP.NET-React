import { useEffect, useState } from "react";
import GroupStage from "./europe/GroupStage";
import Knockouts from "./europe/Knockouts";
import PlayerStats from "./europe/PlayerStats";
import { useActiveSeason } from "../../components/useActiveSeason";

export default function EuropeanCup({ gameSaveId }) {
  const { season, loading: seasonLoading, error: seasonError } = useActiveSeason(gameSaveId);
  const [cup, setCup] = useState(null);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState("group");

  useEffect(() => {
    const loadCup = async () => {
      if (!gameSaveId || !season?.id) return;
      try {
        setLoading(true);
        const res = await fetch(
          `/api/EuropeanCup/current?gameSaveId=${gameSaveId}&seasonId=${season.id}`,
          { credentials: "include" }
        );
        if (!res.ok) throw new Error("Error while loading European Cup");
        const data = await res.json();
        setCup(data);
      } catch (err) {
        console.error("❌ Error loading European Cup:", err);
      } finally {
        setLoading(false);
      }
    };

    loadCup();
  }, [gameSaveId, season]); // 👈 следи за промени в сейв и сезон

  if (seasonLoading) return <p>Loading active season...</p>;
  if (seasonError) return <p className="text-red-600">Error: {seasonError}</p>;
  if (loading) return <p>Loading European Cup...</p>;
  if (!cup?.exists) return <p>No European Cup for this season.</p>;

  const competitionLogoUrl = cup.logoFileName
    ? `/competitionsLogos/${cup.logoFileName}`
    : "/competitionsLogos/default.png";

  return (
    <div className="p-4">
      {/* Header */}
      <div className="flex items-center justify-center gap-4 mb-6">
        <img
          src={competitionLogoUrl}
          alt={cup.name}
          className="w-16 h-16 object-contain border rounded-full shadow-md"
          onError={(e) => {
            e.target.src = "/competitionsLogos/default.png";
          }}
        />
        <h2 className="text-3xl font-bold text-center">{cup.name}</h2>
      </div>

      {/* Tabs */}
      <div className="flex justify-center mb-6">
        {[
          { key: "group", label: "Group Stage" },
          { key: "knockouts", label: "Knockouts" },
          { key: "stats", label: "Player Stats" },
        ].map((tab) => (
          <button
            key={tab.key}
            onClick={() => setActiveTab(tab.key)}
            className={`px-4 py-2 mx-1 rounded-md font-medium ${
              activeTab === tab.key
                ? "bg-blue-600 text-white"
                : "bg-slate-200 text-slate-700 hover:bg-slate-300"
            }`}
          >
            {tab.label}
          </button>
        ))}
      </div>

      {/* Content */}
      {activeTab === "group" && <GroupStage cup={cup} />}
      {activeTab === "knockouts" && <Knockouts cup={cup} />}
      {activeTab === "stats" && <PlayerStats cup={cup} />}
    </div>
  );
}
