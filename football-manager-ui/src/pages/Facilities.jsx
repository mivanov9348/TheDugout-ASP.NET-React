import { useEffect, useState } from "react";
import { ArrowUpCircle } from "lucide-react";

export default function Facilities({ gameSaveId, teamId }) {
  const [facilities, setFacilities] = useState(null);
  const [loading, setLoading] = useState(true);

  // 🔹 Зареждане на facilities от бекенда
  useEffect(() => {
    if (!teamId) return;

    const fetchFacilities = async () => {
      try {
        setLoading(true);
        const res = await fetch(`/api/facility/team/${teamId}`, {
          credentials: "include",
        });
        if (!res.ok) throw new Error("Грешка при зареждане на Facilities");
        const data = await res.json();
        setFacilities({
  stadium: data.stadium
    ? {
        name: "🏟️ Stadium",
        level: data.stadium.level,
        capacity: data.stadium.capacity,
        ticketPrice: data.stadium.ticketPrice,
      }
    : null,
  training: data.trainingFacility
    ? {
        name: "💪 Training Facilities",
        level: data.trainingFacility.level,
        quality: data.trainingFacility.trainingQuality,
      }
    : null,
  youth: data.youthAcademy
    ? {
        name: "👶 Youth Academy",
        level: data.youthAcademy.level,
        talentPoints: data.youthAcademy.talentPointsPerYear,
      }
    : null,
});
      } catch (err) {
        console.error(err);
      } finally {
        setLoading(false);
      }
    };

    fetchFacilities();
  }, [teamId]);

  // 🔹 Upgrade бутон (пример със стадион)
  const handleUpgrade = async (key) => {
    try {
      let url;
      if (key === "stadium") url = `/api/facility/stadium/upgrade/${teamId}`;
      else if (key === "training") url = `/api/facility/training/upgrade/${teamId}`;
      else if (key === "youth") url = `/api/facility/academy/upgrade/${teamId}`;
      else return;

      const res = await fetch(url, { method: "POST", credentials: "include" });
      if (!res.ok) throw new Error("Upgrade failed");

      // след успешен upgrade презареждам данните
      const updated = await res.json();
      console.log("Upgrade success:", updated);
      // ⚠️ Може и просто да извикаш fetchFacilities() отгоре вместо ръчно сетване
    } catch (err) {
      console.error(err);
    }
  };

  const renderFacility = (key, facility) => (
    <div
      key={key}
      className="bg-white/80 backdrop-blur-sm rounded-2xl shadow-lg p-6 flex flex-col hover:scale-[1.02] transition-transform duration-200"
    >
      <h3 className="text-xl font-semibold mb-2 flex items-center gap-2">
        {facility.name}
      </h3>
      <div className="mb-4 text-sm text-gray-600 space-y-1">
        <div>
          Level: <span className="font-bold">{facility.level}</span>
        </div>
        {facility.capacity && (
          <div>
            Capacity: <span className="font-bold">{facility.capacity}</span>
          </div>
        )}
        {facility.ticketPrice && (
          <div>
            Ticket price: <span className="font-bold">{facility.ticketPrice}</span>
          </div>
        )}
        {facility.quality && (
          <div>
            Training quality: <span className="font-bold">{facility.quality}</span>
          </div>
        )}
        {facility.talentPoints && (
          <div>
            Talent points / year:{" "}
            <span className="font-bold">{facility.talentPoints}</span>
          </div>
        )}
        <div className="w-full bg-gray-200 rounded-full h-3 mt-3">
          <div
            className="bg-blue-500 h-3 rounded-full"
            style={{ width: `${facility.level * 20}%` }}
          ></div>
        </div>
      </div>
      <button
        onClick={() => handleUpgrade(key)}
        className="mt-auto flex items-center justify-center gap-2 bg-blue-600 hover:bg-blue-700 text-white font-semibold py-2 px-4 rounded-xl shadow transition"
      >
        <ArrowUpCircle size={18} />
        Upgrade
      </button>
    </div>
  );

  if (loading) {
    return <div className="p-6 text-center">Loading facilities...</div>;
  }

  if (!facilities) {
    return <div className="p-6 text-center text-red-500">No facilities found</div>;
  }

  return (
    <div className="p-6">
      <h2 className="text-2xl font-bold mb-2 text-center">🏗️ Facilities</h2>
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
  {Object.entries(facilities)
    .filter(([_, facility]) => facility !== null) 
    .map(([key, facility]) => renderFacility(key, facility))}
</div>
    </div>
  );
}
