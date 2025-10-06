export default function PlayerStats({ cup }) {
  if (!cup?.playerStats || cup.playerStats.length === 0)
    return <p>No player stats yet.</p>;

  return (
    <div className="bg-white shadow rounded-2xl p-4">
      <h3 className="text-xl font-semibold mb-3">Player Statistics</h3>
      <table className="w-full text-sm border-collapse">
        <thead>
          <tr className="bg-slate-200">
            <th className="p-2 text-left">#</th>
            <th className="p-2 text-left">Player</th>
            <th className="p-2 text-left">Team</th>
            <th className="p-2 text-center">Goals</th>
            <th className="p-2 text-center">Assists</th>
            <th className="p-2 text-center">Matches</th>
          </tr>
        </thead>
        <tbody>
          {cup.playerStats.map((p, i) => (
            <tr key={p.id} className="border-b">
              <td className="p-2">{i + 1}</td>
              <td className="p-2">{p.name}</td>
              <td className="p-2">{p.teamName}</td>
              <td className="p-2 text-center">{p.goals}</td>
              <td className="p-2 text-center">{p.assists}</td>
              <td className="p-2 text-center">{p.matches}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
