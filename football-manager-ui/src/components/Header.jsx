function Header() {
  const teamName = "FC Barcelona";
  const leagueName = "La Liga";
  const currentDate = "1 July 2025"; // временно хардкоднато
  const username = "Manager";        // временно хардкоднато
  const money = "€150M";             // временно хардкоднато

  return (
    <header className="flex justify-between items-center px-6 py-3 bg-slate-800 text-white shadow-md">
      {/* Лява част */}
      <div>
        <h1 className="text-lg font-bold">{teamName}</h1>
        <p className="text-sm text-slate-300">{leagueName}</p>
      </div>

      {/* Средна част - инфо */}
      <div className="text-sm text-slate-300 flex gap-6">
        <span>{currentDate}</span>
        <span>{username}</span>
        <span>{money}</span>
      </div>

      {/* Дясна част - бутон */}
      <button className="bg-sky-600 hover:bg-sky-700 text-white px-4 py-2 rounded-lg font-medium transition">
        Next Day →
      </button>
    </header>
  );
}

export default Header;
