import { useOutletContext } from "react-router-dom";

export default function PlayerStats() {
  const { selectedLeague } = useOutletContext();

  return (
    <div>
      <h2 className="text-xl font-bold mb-2 text-sky-700">
        {selectedLeague ? selectedLeague.name : "League"} – Player Stats
      </h2>
      <p>Тук ще се показват статистики на играчите.</p>
    </div>
  );
}
