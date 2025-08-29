function Header({ currentGameSave, username }) {
  if (!currentGameSave) {
    return (
      <header className="px-6 py-3 bg-slate-800 text-white shadow-md">
        <h1>No Save Loaded</h1>
      </header>
    );
  }

  const season = currentGameSave.seasons?.[0];
  const team = currentGameSave.userTeam; // трябва да дойде от бекенда
  const league = team?.league;

  return (
    <header className="flex justify-between items-center px-6 py-3 bg-slate-800 text-white shadow-md">
      <div>
        <h1 className="text-lg font-bold">{team?.name ?? "Unknown Team"}</h1>
        <p className="text-sm text-slate-300">{league?.name ?? "Unknown League"}</p>
      </div>
      <div className="text-sm text-slate-300 flex gap-6">
        <span>{season ? new Date(season.currentDate).toLocaleDateString() : ""}</span>
        <span>{username}</span>
        <span>{team?.budget ? `€${team.budget}` : "€0"}</span>
      </div>
      <button className="bg-sky-600 hover:bg-sky-700 px-4 py-2 rounded-lg">Next Day →</button>
    </header>
  );
}

export default Header;