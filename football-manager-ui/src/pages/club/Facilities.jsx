import { useEffect, useState } from "react";
import { ArrowUpCircle, Loader2 } from "lucide-react";
import Swal from "sweetalert2";
import { useGame } from "../../context/GameContext";

export default function Facilities({ gameSaveId, teamId, logoUrl }) {
  const [facilities, setFacilities] = useState(null);
  const [loading, setLoading] = useState(true);
  const { refreshGameStatus } = useGame();

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
              upgradeCost: data.stadium.upgradeCost,
            }
          : null,
        training: data.trainingFacility
          ? {
              name: "💪 Training Facilities",
              level: data.trainingFacility.level,
              quality: data.trainingFacility.trainingQuality,
              upgradeCost: data.trainingFacility.upgradeCost,
            }
          : null,
        youth: data.youthAcademy
          ? {
              name: "👶 Youth Academy",
              level: data.youthAcademy.level,
              talentPoints: data.youthAcademy.talentPointsPerYear,
              upgradeCost: data.youthAcademy.upgradeCost,
            }
          : null,
      });
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (teamId) fetchFacilities();
  }, [teamId]);

  const handleUpgrade = async (key) => {
    try {
      let url;
      if (key === "stadium") url = `/api/facility/stadium/upgrade/${teamId}`;
      else if (key === "training") url = `/api/facility/training/upgrade/${teamId}`;
      else if (key === "youth") url = `/api/facility/academy/upgrade/${teamId}`;
      else return;

      const res = await fetch(url, { method: "POST", credentials: "include" });

      if (res.ok) {
        const data = await res.json();

        Swal.fire({
          icon: "success",
          title: "Upgrade successful!",
          text: data.message || "Facility upgraded successfully.",
          confirmButtonColor: "#2563eb",
        });

        fetchFacilities();
        await refreshGameStatus();
      } else {
        const errText = await res.text();
        Swal.fire({
          icon: "error",
          title: "Upgrade failed",
          text: errText || "Not enough funds or invalid upgrade.",
          confirmButtonColor: "#dc2626",
        });
      }
    } catch (err) {
      console.error(err);
      Swal.fire({
        icon: "error",
        title: "Unexpected error",
        text: err.message,
        confirmButtonColor: "#dc2626",
      });
    }
  };

  const renderFacility = (key, facility) => (
    <div
      key={key}
      className="bg-gradient-to-br from-white/90 to-blue-50/70 backdrop-blur-md rounded-2xl shadow-lg border border-blue-100 p-6 flex flex-col hover:shadow-2xl hover:scale-[1.02] transition-all duration-200"
    >
      <h3 className="text-xl font-semibold mb-3 flex items-center gap-2">
        {facility.name}
      </h3>
      <div className="mb-4 text-sm text-gray-700 space-y-1">
        <div>Level: <span className="font-bold">{facility.level}</span></div>
        {facility.capacity && (
          <div>Capacity: <span className="font-bold">{facility.capacity.toLocaleString()}</span></div>
        )}
        {facility.ticketPrice && (
          <div>Ticket price: <span className="font-bold">${facility.ticketPrice}</span></div>
        )}
        {facility.quality && (
          <div>Training quality: <span className="font-bold">{facility.quality}</span></div>
        )}
        {facility.talentPoints && (
          <div>Talent points/year: <span className="font-bold">{facility.talentPoints}</span></div>
        )}
        {facility.upgradeCost ? (
          <div>
            Next upgrade cost:{" "}
            <span className="font-bold text-green-700">
              ${facility.upgradeCost.toLocaleString()}
            </span>
          </div>
        ) : (
          <div className="text-red-600 font-bold">Max level reached</div>
        )}

        <div className="w-full bg-gray-200 rounded-full h-3 mt-3">
          <div
            className="bg-blue-500 h-3 rounded-full"
            style={{ width: `${(facility.level / 10) * 100}%` }}
          ></div>
        </div>
      </div>

      {facility.upgradeCost ? (
        <button
          onClick={() => handleUpgrade(key)}
          className="mt-auto flex items-center justify-center gap-2 bg-blue-600 hover:bg-blue-700 text-white font-semibold py-2 px-4 rounded-xl shadow transition-all"
        >
          <ArrowUpCircle size={18} />
          Upgrade
        </button>
      ) : (
        <div className="mt-auto text-center text-gray-500 font-semibold">
          ✅ Max level
        </div>
      )}
    </div>
  );

  if (loading)
    return (
      <div className="p-8 text-center text-gray-600 flex flex-col items-center justify-center gap-3">
        <Loader2 className="animate-spin" size={32} />
        Loading facilities...
      </div>
    );

  if (!facilities)
    return <div className="p-6 text-center text-red-500">No facilities found</div>;

  return (
    <div className="relative p-8 min-h-screen bg-gradient-to-b from-slate-50 to-blue-100/30 rounded-3xl shadow-inner">
      {/* HEADER */}
      <div className="flex flex-col items-center justify-center mb-8">
        {logoUrl && (
          <img
            src={logoUrl}
            alt="Team Logo"
            className="w-24 h-24 object-contain mb-3 drop-shadow-md transition-transform hover:scale-110"
          />
        )}
        <h2 className="text-3xl font-extrabold text-blue-800 tracking-wide drop-shadow-sm">
          Team Facilities
        </h2>
        <p className="text-gray-500 text-sm">Manage and upgrade your club’s infrastructure</p>
      </div>

      {/* FACILITY GRID */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        {Object.entries(facilities)
          .filter(([_, facility]) => facility !== null)
          .map(([key, facility]) => renderFacility(key, facility))}
      </div>
    </div>
  );
}
