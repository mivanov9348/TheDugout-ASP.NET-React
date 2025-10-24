import { useEffect, useState } from "react";
import { ArrowUpCircle } from "lucide-react";
import Swal from "sweetalert2";
import { useGame } from "../../context/GameContext";

export default function Facilities({ gameSaveId, teamId }) {
  const [facilities, setFacilities] = useState(null);
  const [loading, setLoading] = useState(true);
  const { refreshGameStatus } = useGame();

  // 🔹 функция за зареждане на facilities
  const fetchFacilities = async () => {
    try {
      setLoading(true);
      const res = await fetch(`/api/facility/team/${teamId}`, {
        credentials: "include",
      });
      if (!res.ok) throw new Error("Грешка при зареждане на Facilities");
      const data = await res.json();
      
      // ПРОМЯНА: Добавяме upgradeCost към state-а
      setFacilities({
        stadium: data.stadium
          ? {
              name: "🏟️ Stadium",
              level: data.stadium.level,
              capacity: data.stadium.capacity,
              ticketPrice: data.stadium.ticketPrice,
              upgradeCost: data.stadium.upgradeCost, // 👈 ПРОЧЕТИ ОТ API
            }
          : null,
        training: data.trainingFacility
          ? {
              name: "💪 Training Facilities",
              level: data.trainingFacility.level,
              quality: data.trainingFacility.trainingQuality,
              upgradeCost: data.trainingFacility.upgradeCost, // 👈 ПРОЧЕТИ ОТ API
            }
          : null,
        youth: data.youthAcademy
          ? {
              name: "👶 Youth Academy",
              level: data.youthAcademy.level,
              talentPoints: data.youthAcademy.talentPointsPerYear,
              upgradeCost: data.youthAcademy.upgradeCost, // 👈 ПРОЧЕТИ ОТ API
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
    if (teamId) {
      fetchFacilities();
    }
  }, [teamId]);

  // 🔹 Upgrade бутон (ОСТАВА БЕЗ ПРОМЯНА)
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

        // 🔄 обновяваме facilities след успех
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

  const renderFacility = (key, facility) => {
    return (
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
              Capacity: <span className="font-bold">{facility.capacity.toLocaleString()}</span>
            </div>
          )}
          {facility.ticketPrice && (
            <div>
              Ticket price:{" "}
              <span className="font-bold">${facility.ticketPrice}</span>
            </div>
          )}
          {facility.quality && (
            <div>
              Training quality:{" "}
              <span className="font-bold">{facility.quality}</span>
            </div>
          )}
          {facility.talentPoints && (
            <div>
              Talent points / year:{" "}
              <span className="font-bold">{facility.talentPoints}</span>
            </div>
          )}

          {/* 👇 ПРОМЯНА: Използваме facility.upgradeCost директно */}
          {facility.upgradeCost ? (
            <div>
              Next upgrade cost:{" "}
              <span className="font-bold text-green-600">
                {/* Добавяме .toLocaleString() за форматиране (напр. 1,200,000) */}
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

        {/* 👇 ПРОМЯНА: Проверяваме срещу facility.upgradeCost (по-надеждно е) */}
        {facility.upgradeCost ? (
          <button
            onClick={() => handleUpgrade(key)}
            className="mt-auto flex items-center justify-center gap-2 bg-blue-600 hover:bg-blue-700 text-white font-semibold py-2 px-4 rounded-xl shadow transition"
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
  };

  if (loading) {
    return <div className="p-6 text-center">Loading facilities...</div>;
  }

  if (!facilities) {
    return (
      <div className="p-6 text-center text-red-500">No facilities found</div>
    );
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