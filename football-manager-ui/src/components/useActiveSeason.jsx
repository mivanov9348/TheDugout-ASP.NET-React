import { useEffect, useState } from "react";

export const useActiveSeason = (gameSaveId) => {
  const [season, setSeason] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null); 

  useEffect(() => {
    if (!gameSaveId) return;

    const fetchActiveSeason = async () => {
      setLoading(true);
      setError(null);
      try {
        const res = await fetch(`/api/season/active/${gameSaveId}`, {
          credentials: "include",
        });
        if (!res.ok) throw new Error(`HTTP ${res.status}`);
        const data = await res.json();
        setSeason(data);
      } catch (err) {
        console.error("❌ Error fetching active season:", err);
        setError(err.message);
        setSeason(null);
      } finally {
        setLoading(false);
      }
    };

    fetchActiveSeason();
  }, [gameSaveId]);

  return { season, loading, error };
};
